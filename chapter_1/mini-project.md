# OLX System Analysis

## 1.A

For this assignment, I chose "OLX.ba", also known as PIK. It is a well-known platform for buying and selling various items, ranging from second-hand to brand new products. The sellers vary from individual users to large companies.

The storage structure of this application consists of:

* **Primary Database:** Used for storing user profile data, messages between users, and listings, which take up the most storage space. This is an *OLTP* system and our *System of Record*.

* **Object Storage:** Used for storing images uploaded with listings. This is also an *OLTP* component and a *System of Record*.

* **Warehouse:** Used to gather data for business analytics. This is the first *OLAP* component in the system, representing *Derived Data* constructed from the primary database and logs. Refilling with repeated ETL or backfill.

* **Lake:** Used to store all potentially useful data because it is a cheaper format. It is used for Machine Learning (ML), operating as an *OLAP* component with *Derived Data* coming from the primary database and logs. same as WH, refilling with repeated ETL or backfill.

* **Search Engine:** Used for running searches. It is an *OLTP* system populated via events or batch processing, also acting as *Derived Data* since it ingests data from the primary database. And a refill is executed by replaying batches or events, which can take hours, while in the meantime search operates in a degraded state or falls back to the DB.

* **Cache:** Used to improve performance. It is an *OLTP* layer containing *Derived Data* that originates from the primary database, similar to the other components. And the data is populated via lazy loading; After a cache loss it starts cold, so all requests hit the primary database until it refills.

* **Logs:** Used for system logging to monitor system health and stability. It functions as an *OLAP*, but it is not usual OLAP system, it is append-only  and a *System of Record*.

* **Standby Replica:** A continuously updated copy of the primary database, fed by its replication log, used to take over if the primary fails and to serve part of the read traffic. It is an *OLTP* system and *Derived Data* coming from the primary database, rebuilt by taking a fresh base backup from the primary and resuming replication.

* **Backup Database:** A periodic point-in-time copy of the primary database, stored separately together with the archived write-ahead log, so the database can be restored to any moment before a failure or human error. It is an OLTP-type, derived copy of the primary database in its original schema, rebuilt by taking a new snapshot from the primary, although past points in time cannot be recreated.

### Data Flow

```
                           ┌────────────────────────┐
                           │     USER / CLIENT      │
                           │  (Mobile / Web Browser)│
                           └───────────┬────────────┘
                                       │
                                       │ HTTP Request (Write / Read / Images)
                                       ▼
                           ┌────────────────────────┐
                           │   APPLICATION SERVER   │
                           │         (.NET)         │
                           └─┬───────┬────────┬───┬─┘
                             │       │        │   │
         ┌───────────────────┘       │        │   └───────────────────────┐
         │                           │        │                           │
         ▼ (Image upload)            │        ▼ (System logs)             ▼ (Read only)
    ┌──────────────────┐             │  ┌──────────────────┐      ┌──────────────────┐
    │  OBJECT STORAGE  │             │  │  LOGGING STACK   │      │   CACHE SYSTEM   │ 
    │ (S3 / Blob / S2) │             │  │   (Loki / ELK)   │      │     (Redis)      │
    └──────────────────┘             │  └──────────────────┘      └──────────────────┘
                                     │             |
                                     │             |
                                     │             |
                                     ▼             |
                           ┌──────────────────┐    |
                           │ PRIMARY DATABASE │ Primary Writes & ACID Transactions
                           │ (PostgreSQL SoR) │    |
                           └─────────┬────────┘    |
                                     │   __________| 
                     Synchronous / Asynchronous Sync
                     ├───────────────┼───────────────┬──────────────────┐
                     │               │  |            │                  │
                     ▼               ▼  ▼            ▼                  ▼
           ┌──────────────────┐ ┌───────────┐ ┌──────────────┐ ┌─────────────────┐
           │  SEARCH ENGINE   │ │ LAKEHOUSE │ │   BACKUPS    │ │ STANDBY REPLICA │
           │  (Elasticsearch) │ │  / DWH    │ │(S3 Snapshots)│ │ (PG streaming)  │
           └──────────────────┘ └───────────┘ └──────────────┘ └─────────────────┘
```

## 1.B

### Assumptions

| Quantity | Value | Source |
| ----- | ----- | ----- |
| Stored listings | ~30 million | my estimate |
| Size of one listing in the database (without images) | ~5 KB | my estimate |
| User profiles, chats, analytics | ~30 GB | my estimate |
| Average traffic in busy hours | 1k-2k RPS | my estimate |
| Peak traffic | 15k RPS | my estimate |
| Working set (active and frequently read listings) | ~20% of the database, about 36 GB | my assumption, to be verified |
| One modern server | 4 TB NVMe, 512 GB RAM, ~20k RPS for simple indexed read queries | assumed specification |

Images are stored in object storage, not on the database server, so they are excluded from the database estimate.

### Calculation

Listings: 30M x 5 KB = 150 GB. Together with the additional 30 GB, the database needs about **180 GB**.

| Resource | Requirement | Server | Utilization |
| ----- | ----- | ----- | ----- |
| Storage | 180 GB | 4 TB | ~4.5% |
| RAM | ~36 GB working set (the whole 180 GB database would be ~35%) | 512 GB | ~7% |
| Traffic, average | 1.5k RPS | 20k RPS | ~7.5% |
| Traffic, peak | 15k RPS | 20k RPS | ~75% |

Most reads can be served from the buffer pool without disk I/O. I do not claim a specific response time, because it also depends on the network and query processing.

### Reasons to distribute

* **Scale:** does not apply. Both the data and the traffic fit on one server.
* **Latency (geography):** does not apply. The users are mostly in one region.
* **Elasticity:** the peak is about 10x the average, but one server still handles it at 75%, so automatic scaling is not needed.
* **Legal:** does not apply to this case.
* **Fault tolerance:** the only one that applies. It is solved with a standby replica and backups.

The standby is still **a second machine with replication**. This means there is a failover, and with asynchronous replication the RPO is not zero. This is a deliberately accepted cost, not "no distribution".

### Conclusion

One PostgreSQL primary with one standby replica and backups is enough for the current scale.

## 1.C
When a user creates a profile on our application, the data is saved in the database. From there, it is immediately recorded into backups and logs, and later sent to the warehouse, lake, search engine, and potentially cached. The lake, backups and logs are immutable... We solve the lake issue by placing a lakehouse layer in front of it, which handles the immutability problem of the lake.

Regarding backups and logs, we have two options:

1. The first option is to encrypt all personal information stored in the warehouse, backups, and logs. If a user requests data deletion, we simply delete that user's own encryption key (one key per user) from the KMS (Key Management System). The data remains there, but it is rendered empty and mathematically impossible to decrypt.

2. The second approach is to send pseudo-identifiers like a `user_id` instead of raw personal information. If a deletion request is made, we delete the corresponding entry from the PII vault.

Data minimization means avoiding the storage of unnecessary personal details, such as exact home addresses or national ID numbers, that are not required for the application's core functionality.

### Trace of one user's personal data

For each cell: **Yes** or **No**. In the last row write how the data is deleted from that store.

| Data | Primary DB | Object storage | Search | Cache | Warehouse | Lake | Logs | Backups | Standby replica |
| ----- | ----- | ----- | ----- | ----- | ----- | ----- | ----- | ----- | ----- |
| Email | Yes | No | No | No | No | No | No | Yes | Yes |
| Phone number | Yes | No | No | Yes | No | No | No | Yes | Yes |
| Name / username | Yes | No | No | Yes | No | No | No | Yes | Yes |
| Password hash | Yes | No | No | No | No | No | No | Yes | Yes |
| IP address | Yes | No | No | No | No | Yes | Yes | Yes | Yes |
| Device ID / user agent | Yes | No | No | No | No | Yes | Yes | Yes | Yes |
| Location (city) | Yes | No | Yes | Yes | Yes | Yes | No | Yes | Yes |
| Listing text (title, description) | Yes | No | Yes | Yes | No | Yes | No | Yes | Yes |
| Listing images | Yes (reference only) | Yes | No | No | No | No | No | Yes | Yes (reference only) |
| Messages between users | Yes | No | No | No | No | No | No | Yes | Yes |
| **How deleted** | DELETE rows | DELETE objects | DELETE document | Invalidate / TTL | DELETE by `user_id` | DELETE via lakehouse | Log `user_id` only, short retention | Retention + crypto-shredding | Automatic replication |

Assumptions behind the table: the cache holds data that is shown publicly (phone and name on a listing, city, listing text); the warehouse and the lake use `user_id` instead of email, phone and name; the raw IP and device ID only reach the lake through the logs, and the warehouse ETL does not carry them; message content is not needed for analytics.
