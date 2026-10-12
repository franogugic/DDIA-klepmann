# Chapter 1 questions

## 1. OLTP and OLAP 

The difference between OLTP and OLAP is visible in every aspect. In the context of read patterns, OLTP uses reads to give a user information about their profile, a product, or something they are interested in, depending on the application's purpose, and most often those are very light, cheap, and simple queries. On the other hand, OLAP uses read patterns primarily to execute large-scale analytical queries, for example, to calculate all sales of one product in one month.

When it comes to writing in OLTP, all data which a user has access to is written into the database at a given moment, and just like with reads, these are small, frequent writes. Conversely, OLAP is like a read-only copy of the OLTP database, the data is derived from priamry database and logs into this one, so it doesn't have writes at all from the user perspective, but data in OLAP is usually written in batches through ETL/ELT processes.

Users of OLTP are mainly end-users of our product or application, while data analysts and BI engineers are the ones who work on OLAP systems.

With that being said, typical data in OLTP systems is things like user profiles, products, etc., depending on the application's purpose. On the other hand, OLAP contains combined data in an analytics-friendly schema, well suited and optimized for analytical queries. The main difference is that OLTP represents current data, while OLAP keeps historical data.

In large systems, OLAP has bigger datasets because it combines data from many services of the application. Typically, the size of OLTP is in gigabytes, while OLAP is in terabytes or even petabytes.

## 2. Data Warehouse

The data warehouse is a database for OLAP - Snowflake, Google BigQuery, Amazon Redshift, and ClickHouse. It contains historical data from many OLTP systems of one application, and it is used by BI and analytics engineers. The primary purpose of a data warehouse is to enable engineers to find patterns in data.

There are many reasons for separating these two systems, and the main ones are performance issues in case of executing heavy analytical operations in OLTP. Another reason lies in the database schema: if one schema is good for OLTP, it doesn't mean it is good for OLAP. Instead, even though OLAP is a relational database like OLTP, it has an analytics-friendly schema optimized for large-scale queries. Another reason lies in the fact that large systems have a lot of OLTP databases, so it would be very hard to run analytics with one query, and we could also face a problem with different networks, making it inaccessible to some users.

Furthermore, we could find the same data in different forms, which is totally fine for warehouses but unacceptable for OLTP. That is the main reason why we are filtering data in the ETL process, and it is the reason why OLTP uses a normalized database and OLAP uses a denormalized database.

## 3. Data Warehouse and Data Lake 

The main difference lies in who uses it and for what purpose. While the data warehouse is used by data engineers and BI engineers, and the focus is on past events, on the other hand, a data lake is used in data science and machine learning, and their goal is to predict behaviors from collected data. There is a difference in the type of storage as well: while a warehouse is a relational DB, A data lake stores raw files (CSV, JSON, Parquet, Avro, images) in object storage without enforcing a schema on write; structure is applied only when the data is read (schema-on-read), collecting all data that could be useful one day.

The data size is much bigger, but it's cheaper because it uses object storage such as AWS S3, Azure Data Lake Storage, or Google Cloud Storage. But the data lake has a few serious problems, like a lack of ACID transactions, input control, updating and deleting rows, and slow performance. All of that is a reason why lakehouses exist

A lakehouse adds an open table format (Apache Iceberg, Delta Lake, Apache Hudi) on top of the files in object storage, which gives the lake warehouse-like features: ACID transactions, schema enforcement, row-level updates and deletes, and time travel.

## 4. ETL, ELT, and Reverse ETL 

ETL/ELT is a process of loading a data warehouse with data from OLTP systems, APIs, logs, or other sources. It consists of three parts: first is extracting data from sources; second is transforming that data to an OLAP schema, which could also be the third part depending on whether it is ETL or ELT; and the last step in ETL is load, which basically involves loading the warehouse with that transformed data, or with raw data if we are transforming it inside the warehouse itself during the ELT process.

ELT/ETL could have one extra station in its process, and that is loading a data lake. On the other hand, reverse ETL is a process of bringing the results of executed analytical operations back to operational analytics tools or even into the OLTP system itself. It is used for product analytics where we need to deliver these results directly to end-users in OLTP.

## 5. System of Record and Derived Data 

A System of Record keeps the official version of data; every piece of information in our system has a written official version. Primarily, that is a database where data is written first. If another system has a different version of some piece of information, the System of Record is the reliable source of truth, such as the main database, or the payment provider’s transaction records, which are authoritative for whether a charge happened.

On the other hand, derived data is data which is derived from another system or even a few of them. So actually, that data is redundant, but that is the point, because we can combine data in a way that suits us. Caches and database indexes are examples of derived data. In case of a failure, losing this data is irrelevant for us because we can always make a new one, while the System of Record is the main source of truth.

## 6. Self-Hosting and Cloud Hosting 

The biggest problem with self-hosting can be the lack of consistency in terms of traffic. When a system has huge oscillations in traffic, like an OLAP system, it is idle most of the time, but it requires high performance when it needs to execute high-scale analytical operations. Because of that, we have to pay the whole time for high-performance resources, even though they are only used for a small fraction of the time.

On the other hand, cloud hosting offers a solution: it scales the system very easily so we are not wasting resources when we don't need them. In this sense, if we know that we will have constant traffic, self-hosting could be a better option, but if we are not sure, the cloud is better. Of course, the cloud is not ideal in terms of control. In a self-hosted setup, we have much more freedom: we get our own OS with allocated resources and we handle deployments ourselves. In the cloud, through SaaS, we can deploy our app very easily and the cloud provider will manage everything related to infrastructure, though you can get more control with IaaS.

In a self-hosted environment, our team is responsible for monitoring, logging, and CI/CD, cloud providers offer managed monitoring, logging, and CI/CD services, but the team still configures and uses them. However, there is a catch: in case of an error or performance degradation, it is much harder to find the root cause because we don't have access to the hardware and we are very limited with information about the underlying infrastructure.

The main problem with the cloud is that if we end up in a situation where we need a service they currently don't offer, we can only politely ask them and hope they add it soon. Furthermore, if they change something in a way we don't like, we are at their mercy - we can't keep using an older version. Even worse, if we want to move to another provider, it is a long and painful process, and we can also run into issues due to political events or compliance regulations in our country.

Additional:

In self-hosted monitoring, we set up tools like Prometheus, Grafana, and ELK ourselves. The cost is fixed because we only pay for our servers. The main benefit is that we have full access to the server with SSH. We can easily check if the CPU is too busy, if the hard drive is failing, if the memory is full, or if the network has problems using simple tools like htop or iostat.

The bad side is that we must manage everything ourselves. If our log tool crashes or runs out of disk space, we lose our log data while the app keeps running. Also, if the monitoring tool is on the same server as the application and the server crashes, both go down at the same time.

On the other hand, cloud services like AWS CloudWatch are very easy to set up and stay running even if our main app crashes. However, the cloud works like a locked box because we do not have SSH access to check or fix real server issues, and cloud costs can jump very fast if our app gets stuck in a loop and sends too many logs. To solve this locked box problem in the cloud, we use application observability tools like distributed tracing and OpenTelemetry directly inside our code. This tracks each request step by step, showing us exact query times and bottlenecks in the application so we do not need SSH access to find the root cause.

## 7. Separation of Storage and Compute 

Separation of storage and compute in cloud-native architecture means using independent services for storage and compute rather than coupling them on a single machine. For example, storing data in AWS S3 and executing analytical queries using a separate engine like Amazon Athena requires transferring data over the network.

This approach enables independent scaling, allowing us to pay only for cheap storage continuously and spin up compute resources on demand, which is ideal for OLAP workloads. It also allows multiple compute workloads to access the same storage simultaneously, while making compute nodes stateless so a node failure causes no data loss.

However, the trade-offs include potential network bottlenecks from transferring data across services and higher costs associated with aggressive local memory or disk caching required to reduce network latency and maintain fast query performance.

## 8. Operations in the Cloud Era 

Operations is still very relevant, but its role has changed in the cloud era. Before, operations was about managing physical hardware, watching remaining disk space, adding new disks, and setting up physical servers.

Today, the focus is on cloud architecture - choosing the best service for a task, connecting services together, and managing cloud migrations. Capacity planning is no longer about physical hardware limits, because cloud capacity is practically unlimited.

Instead, capacity planning has shifted to cost management, where we decide if adding another instance actually brings business profit, or if we are just paying for extra performance that we do not really need.

## 9. Distributed Systems 

There are many reasons to choose distributed systems, and one of those is high availability and failure tolerance. We could have redundancy of our instances, so if one goes down, we could just move to another one. Other one is scalability, the ability of a system to keep its performance acceptable as load grows (more data, more requests, more users), typically by spreading the work across more machines instead of one.Latency plays a key role in big services; you can have instances across continents so every user can go to the nearest instance.

Of course, distributed systems have bad sides as well. They are very dependent on the network, and in case of slow performance or some other problem, it's much harder to find a problem than in single-node systems. We can also face data consistency problems. One of the solutions is distributed transactions, but it's the opposite of the logic behind microservices. And there is more dowside like a data privacy (GDPR)

## 10. Microservices and Serverless 

Microservices are an architecture where one system has several smaller services, where each has a very defined goal. Every service has a separate team working on it. This approach enables each system to update independently and decreases the need for team coordination.

Every system can allocate exact resources for its needs, and all logic is behind an API so you can always make changes without touching clients. This is a distributed system, so it has all the problems of distributed systems, but microservices also drastically increase the complexity of an application. In a lot of cases, there is no need for it.

To test one service, you need all the services you depend on, and for every update, you need adaptation of allocated resources, plus of course the metrics and logs problem. Changing some models can be very complicated because other services expect exact data, so it could cause other services to break.

On the other hand, serverless or FaaS (function as a service) is a different deployment model, where you completely rely on a vendor and you are just paying for what you use. Because of this, many vendors limit function execution time, and the biggest problem here is the cold start.

The term serverless has become even synonymous with automatic scaling; Kafka, for example, uses this term to emphasize that they are auto-scaling their services and you pay per use. Microservices mainly solve organizational problems, not technical ones.

## 11. Data Minimization and Right to Erasure

As we saw, it is not just about technical aspects; organizational and human needs play a very important role in the architecture of data systems. We are not just fulfilling requirements for our business, we have responsibilities for society and their data.

Data minimization is the opposite of the "big data" concept of collecting data, which was very popular. With the advent of GDPR in 2018, things changed a lot. The law says data can be collected for one purpose and has to be used for that exact purpose, and just for the time you need it for that.

With that being said, we moved from the logic of collecting all data - even data that just might be used at some point - to the main question: is it worth collecting some data? It is not just the cost of S3 or some other storage; now come all the risks that storing data brings with new laws.

The right to erasure allows a user to ask for the deletion of all of their data. The problem is that a lot of modern data systems are immutable, so we are just adding into them and can't just delete one row. Furthermore, a lot of data goes to derived data for ML, etc., and we need to delete it from there as well.
