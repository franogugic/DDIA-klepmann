# DDIA 2nd Edition — Practical Mastery Guide

A chapter-by-chapter working script for *Designing Data-Intensive Applications*,
2nd Edition (Martin Kleppmann & Chris Riccomini).

This guide contains **no source code**. It gives you instructions, projects,
investigations, and success criteria. You build, measure, break, and explain
things yourself.

---

## Table of Contents

- [How This Guide Works](#how-this-guide-works)
- [The Main Project: Bazaar](#the-main-project-bazaar)
- [Companion File: Lab Library](#companion-file-lab-library)
- [Lab Setup (One-Time)](#lab-setup-one-time)
- **Part I — Foundations of Data Systems**
  - [Chapter 1 — Trade-offs in Data Systems Architecture](#chapter-1--trade-offs-in-data-systems-architecture)
  - [Chapter 2 — Defining Nonfunctional Requirements](#chapter-2--defining-nonfunctional-requirements)
  - [Chapter 3 — Data Models and Query Languages](#chapter-3--data-models-and-query-languages)
  - [Chapter 4 — Storage and Retrieval](#chapter-4--storage-and-retrieval)
  - [Chapter 5 — Encoding and Evolution](#chapter-5--encoding-and-evolution)
- **Part II — Distributed Data**
  - [Chapter 6 — Replication](#chapter-6--replication)
  - [Chapter 7 — Sharding](#chapter-7--sharding)
  - [Chapter 8 — Transactions](#chapter-8--transactions)
  - [Chapter 9 — The Trouble with Distributed Systems](#chapter-9--the-trouble-with-distributed-systems)
  - [Chapter 10 — Consistency and Consensus](#chapter-10--consistency-and-consensus)
- **Part III — Derived Data**
  - [Chapter 11 — Batch Processing](#chapter-11--batch-processing)
  - [Chapter 12 — Stream Processing](#chapter-12--stream-processing)
  - [Chapter 13 — A Philosophy of Streaming Systems](#chapter-13--a-philosophy-of-streaming-systems)
  - [Chapter 14 — Doing the Right Thing](#chapter-14--doing-the-right-thing)
- [Final: Packaging Your Portfolio](#final-packaging-your-portfolio)
- [Progress Tracker](#progress-tracker)

> **Edition note:** The chapter titles and topics follow the 2nd Edition's
> structure. Early-release and final printings differ slightly. If a section
> name in your copy is different, match tasks to the topic, not the title.

---

## How This Guide Works

### Three phases per chapter

| Phase | What | Size | You end up with |
|---|---|---|---|
| **1. Knowledge Check** | Closed-book questions | 1–2 hours | Written answers, self-graded |
| **2. Mini-Project** | A very small, independent project on this chapter only | 1–3 evenings | A throwaway lab plus a short write-up |
| **3. Bazaar Milestone** | Apply the chapter to the one large project | 1–2 weeks | Working features, decision records, measured evidence |

- The **mini-project** is where you learn the concept in isolation, where it's
  cheap to break things. Throw it away when you're done.
- The **milestone** is where you prove you can apply the concept to a real
  system with competing requirements. That's what you'll show other people.
- Each chapter also lists **optional deep-dive labs** from the companion
  Lab Library, for when you want to go further.

### The study loop (repeat for every chapter)

1. **Read** the chapter once, end to end, without taking notes.
2. **Phase 1, closed book:** answer every question in writing.
   - Use your own words. Do not copy phrasing from the book.
   - Mark each answer as *confident*, *shaky*, or *don't know*.
3. **Grade yourself:** re-read only the sections behind your *shaky* and
   *don't know* answers. Rewrite those answers.
   - You can also paste your answers to me (Claude) and ask me to grade them
     strictly against the 2nd Edition.
4. **Phase 2:** do the mini-project. Write a one-page lab report.
5. **Phase 3:** do the Bazaar milestone. Record decisions and evidence.
6. **Gate check:** don't move on until you meet every *Mastered when*
   criterion, or you've written down why you skipped one.

### Scoring the Knowledge Check

| Score | Meaning | Action |
|---|---|---|
| 90–100% confident and correct | Ready | Go to Phase 2 |
| 70–89% | Gaps | Re-read the weak sections, re-answer, then go to Phase 2 |
| Below 70% | Not ready | Re-read the whole chapter before Phase 2 |

### The "Explain It" rule

For every concept in a *Focus concepts* list, you should be able to:

- Explain it in two minutes to a junior engineer, with no notes.
- Give one real-world system that uses it.
- Name one situation where it is the **wrong** choice.

---

## The Main Project: Bazaar

**Bazaar** (rename it if you like) is a peer-to-peer marketplace with a
built-in wallet. You start it in Chapter 1 as a single-node app and grow it
through all 14 chapters.

### Why this project best demonstrates your knowledge

- **It touches every chapter for a real reason.** Nothing is added just to
  tick a box. Each layer solves a problem the previous design created.
- **Money makes correctness tangible.** "No money created or destroyed" and
  "never sell the last item twice" are invariants anyone understands. You can
  check them after every crash, partition, and failover.
- **It mixes very different workloads:** transactional checkout, search,
  feeds, analytics, streaming signals, and offline editing. Those workloads
  force you to make and defend trade-offs, which is the core skill DDIA
  teaches.
- **It's recognizable.** Interviewers and reviewers understand a marketplace
  immediately, so they focus on your design decisions rather than on
  learning your domain.

Why not the alternatives:

- A social network is weak on transactions and correctness.
- A pure banking ledger is weak on search, feeds, and offline editing.

### What Bazaar does by the end

- **Accounts and seller profiles**, with unique usernames.
- **Listings** with category-specific attributes and photos.
- **Search:** keyword search plus semantic "find similar" search.
- **Follow sellers**, with a home feed of new listings.
- **Checkout:** inventory reservation, wallet payment, order lifecycle.
- **Wallet:** a double-entry ledger, transfers between users, and payouts.
- **Offline drafts:** edit a listing on two devices offline, then sync.
- **Analytics:** seller dashboards and marketplace metrics.
- **Recommendations** (batch) and **trending listings** (streaming).
- **Fraud signals** computed from event streams.
- **Privacy:** export my data, and delete my data everywhere.

### Core invariants (tested from Chapter 3 onward)

1. **Ledger balance:** all ledger entries always sum to zero.
2. **No overselling:** inventory never goes below zero.
3. **No double charge:** one checkout request causes at most one payment.
4. **Unique usernames:** no two accounts ever share a username.
5. **Convergence:** every derived store (search, feed, cache, analytics)
   eventually matches its source of truth.
6. **No lost acknowledged writes:** if Bazaar told a user "done", the change
   survives any single failure you test.

### Ground rules for the main project

- **Start simple on purpose.** Use a single PostgreSQL node until a chapter
  gives you a measured reason to add something.
- **Every significant decision gets an ADR** (Architecture Decision Record).
  You should have 25–40 by the end.
- **Measure before and after** every change that is meant to improve
  performance or reliability.
- **Invariant checks are permanent.** Once you write a check, it runs after
  every experiment for the rest of the project.
- **Everything derived must be rebuildable.** If you can't delete it and
  rebuild it from a source of truth, it isn't derived; it's a second source
  of truth, and you must document it as one.
- **One-command startup.** The whole system, at every milestone, must start
  from a clean machine with one documented command using containers.

### Evidence folder structure

Keep a `docs/` folder in the Bazaar repository:

- `docs/adr/` — numbered decision records
- `docs/reports/` — benchmarks, load tests, drills, with raw numbers
- `docs/postmortems/` — blameless write-ups of every failure you find
- `docs/diagrams/` — the architecture diagram, one version per milestone
- `docs/invariants.md` — each invariant, how it's checked, last result

### ADR template (use for every decision)

- **Title and number**
- **Status:** proposed, accepted, or superseded by ADR-X
- **Context:** the problem and the forces at play, with numbers
- **Options considered:** at least two, with pros and cons
- **Decision**
- **Consequences:** what gets easier, what gets harder
- **DDIA reference:** chapter and concepts applied
- **Revisit when:** the measurable condition that would change this decision

---

## Companion File: Lab Library

The file **`DDIA-2e-Lab-Library.md`** (next to this guide) contains the full
catalogue of standalone experiments for every chapter. Each has detailed
*Do / Focus concepts / Mastered when* sections.

- Mini-projects in this guide point to Lab Library tasks for detailed
  procedures, written as **"Lab 4.1"**.
- The **optional deep-dive labs** at the end of each chapter are the Lab
  Library tasks not covered by the mini-project or milestone.

---

## Lab Setup (One-Time)

Install tools as each chapter needs them, not all at once.

### Baseline environment

- A container runtime (Docker or Podman) with Compose support.
- A terminal you're comfortable in, plus standard Unix tools
  (`sort`, `uniq`, `awk`, `grep`, `head`, `wc`, `xxd` or `hexdump`).
- One general-purpose programming language you know well. You'll use it for
  Bazaar and for small storage engines and simulators.
- A spreadsheet tool, for back-of-envelope models and simple simulations.
- Git, with Bazaar in its own repository from day one.

### Systems you'll use across chapters

| System | Mini-projects | Bazaar milestones |
|---|---|---|
| PostgreSQL (with `pageinspect`, logical decoding, `pgvector`) | 3, 6, 8 | 2–14 |
| Metrics and dashboards (Prometheus + Grafana, or similar) | 2 | 2–14 |
| An open-loop load generator (k6, wrk2, or similar) | 2 | 2, 4, 7 |
| SQLite | 4 | — |
| DuckDB plus Parquet files | — | 4, 11 |
| A graph database (Neo4j or Memgraph) | 3 | 3 (optional) |
| Protocol Buffers or Avro tooling | 5 | 5, 12 |
| A durable-execution engine (Temporal or Restate) | — | 5 |
| A CRDT library (Automerge or Yjs) | — | 6 |
| A sharded SQL system (Citus, CockroachDB, or Vitess) | — | 7 |
| A fault injector (Toxiproxy, or Linux `tc netem`) | 9 | 9, 10 |
| etcd (3-node cluster) | 10 | 10 |
| Object storage (MinIO) | — | 4, 11 |
| Apache Spark (local mode) | 11 | 11 (optional) |
| A workflow scheduler (Airflow, Dagster, or Prefect) | — | 11 |
| Kafka or Redpanda, plus Debezium | 12, 13 | 12–14 |
| A stream processor (Flink, or your broker's streams library) | — | 12 |

### Lab report template (for every mini-project)

- **Title**
- **Hypothesis:** what I expect to happen, and why (cite the chapter section)
- **Setup:** versions, configuration, data size
- **Procedure:** steps actually taken
- **Observations:** raw numbers, screenshots, logs
- **Conclusion:** did the theory hold? Where did reality differ?
- **One-sentence takeaway**

---

# Part I — Foundations of Data Systems

---

## Chapter 1 — Trade-offs in Data Systems Architecture

### Phase 1: Knowledge Check

Answer in writing, closed book.

1. What are the main differences between **operational (OLTP)** and
   **analytical (OLAP)** systems in terms of:
   - read pattern
   - write pattern
   - who the users are
   - what data is represented
   - typical dataset size
2. What is a **data warehouse**? Why do organizations keep analytics in a
   separate system instead of querying OLTP databases directly?
3. How does a **data lake** differ from a data warehouse? What problem does a
   **lakehouse** or open table format try to solve?
4. What is **ETL** vs **ELT**? What is "reverse ETL", and why does it exist?
5. Define **system of record** and **derived data**. Give two examples of each.
   Why does the distinction matter when something goes wrong?
6. List the main trade-offs between **cloud services** and **self-hosting**.
   Include cost, control, operational burden, and vendor lock-in.
7. What does **separation of storage and compute** mean in cloud-native
   databases? What does it make possible, and what does it cost you?
8. How does the role of operations change in the cloud era? What does the
   book mean by the shift toward capacity planning and cost management?
9. Give three good reasons to **distribute** a system across machines. Then
   give three problems that distribution introduces.
10. What are **microservices** and **serverless**? What problem are they
    mainly solving: technical, organizational, or both?
11. Why does the book discuss **law and society** in an architecture chapter?
    Explain **data minimization** and how the GDPR "right to erasure"
    affects system design.

### Phase 2: Mini-Project — "Data Landscape Teardown"

> **Goal:** Take apart the data architecture of a product you didn't build.
> **Time box:** 1–2 evenings.
> **Deliverable:** A two-page teardown with one diagram.

#### Step 1.A — Map every data store

- **Do:**
  1. Pick a well-known product you can reason about, for example a
     food-delivery or ride-hailing app.
  2. List every data store it likely has: primary DB, cache, search,
     warehouse, logs, object storage, ML features.
  3. Draw the data flow between them.
  4. Label each store **OLTP or analytical**, and **system of record or
     derived**.
  5. Detailed procedure: Lab 1.1.
- **Focus concepts:** OLTP vs OLAP, systems of record vs derived data,
  ETL, warehouses and lakes.
- **Mastered when:**
  - Every store has both labels and you can defend them.
  - Every derived store names its source and how it would be rebuilt.

#### Step 1.B — "Does this need to be distributed?"

- **Do:**
  1. Estimate the product's storage, peak throughput, and working set.
  2. Compare with one large modern server.
  3. Name which reason to distribute actually applies: fault tolerance,
     scale, latency, elasticity, or legal.
  4. Detailed procedure: Lab 1.4.
- **Focus concepts:** reasons to distribute and their costs.
- **Mastered when:**
  - Every assumption is written down and the estimate is within an order
    of magnitude.
  - You don't use "scale" as the reason when the numbers fit on one machine.

#### Step 1.C — Trace one user's deletion

- **Do:**
  1. Pick one user's personal data. Trace every store it lands in,
     including backups, logs, and the warehouse.
  2. Write the deletion procedure. Detailed procedure: Lab 1.6.
- **Focus concepts:** data minimization, GDPR, right to erasure, deletion
  in derived data.
- **Mastered when:**
  - Your procedure names the difficulty with backups and immutable logs.
  - You name at least one piece of data the product shouldn't collect.

### Phase 3: Bazaar Milestone M1 — "Charter and Architecture v0"

> **Goal:** Define what Bazaar is, what must never go wrong, and where data
> will live. No features yet.
> **Deliverables:** Product brief, invariants doc, target data map, first
> ADRs, repository skeleton.

#### M1.1 — Product brief and invariants

- **Do:**
  1. Write a one-page brief: users, core features, what's out of scope.
  2. Copy the six core invariants into `docs/invariants.md`. Add any of your
     own.
  3. For each invariant, write in plain words how you would check it
     automatically.
- **Focus concepts:** functional requirements vs guarantees, systems of
  record.
- **Mastered when:**
  - Every invariant is checkable by a query or script, not by judgment.

#### M1.2 — Target data map

- **Do:**
  1. Draw Bazaar's **end-state** data landscape, even though you'll build it
     gradually: primary database, search, feed, cache, warehouse, event log,
     object storage.
  2. Mark each store as system of record or derived, and OLTP or analytical.
  3. Save it as `docs/diagrams/v0-target.png`, or any format you like.
- **Focus concepts:** derived data, OLTP vs analytics, data integration.
- **Mastered when:**
  - The ledger, listings, and accounts are clearly systems of record.
  - Search, feed, cache, analytics, and recommendations are clearly
    derived, each with its source named.

#### M1.3 — Deployment and "single node first" ADRs

- **Do:**
  1. Write **ADR-001**: where Bazaar would run in production. Compare
     managed cloud vs self-hosted, using the method from Lab 1.2.
  2. Write **ADR-002**: start on one PostgreSQL node. Include the
     measurable condition that would make you add replicas or shards.
- **Focus concepts:** cloud vs self-hosting, separation of storage and
  compute, distributed vs single-node trade-offs.
- **Mastered when:**
  - Both ADRs include a concrete "Revisit when" condition with a number.

#### M1.4 — Repository skeleton and data inventory

- **Do:**
  1. Create the Bazaar repository with the `docs/` structure from the
     ground rules.
  2. Create a personal-data inventory table: every personal field Bazaar
     will collect, with purpose, retention period, and the stores it will
     reach.
  3. Delete from the plan any field that has no clear purpose.
- **Focus concepts:** data minimization, law and society.
- **Mastered when:**
  - Every personal field has a purpose and a retention period.
  - You removed at least one field you were initially going to collect.

#### Optional deep-dive labs

- **Lab 1.3** — investigate a cloud database's storage/compute separation.
- **Lab 1.5** — microservices split analysis.

---

## Chapter 2 — Defining Nonfunctional Requirements

### Phase 1: Knowledge Check

1. In the social-network **home timeline** case study, explain:
   - the read-time approach (query on demand)
   - the write-time approach (fan-out, materialized timelines)
   - why the right choice depends on the follower distribution
2. What happens to fan-out-on-write for an account with tens of millions of
   followers? What hybrid approach handles it?
3. Distinguish **response time**, **latency**, and **service time**.
4. Why are **percentiles** (p50, p95, p99, p999) better than the **mean**
   for describing performance?
5. What is **tail latency amplification**? Why does it get worse when one
   request fans out to many backends?
6. What is **head-of-line blocking**, and how does it distort measurements?
7. How should you measure response time from the **client side**? What goes
   wrong if the load generator waits for each response before sending the
   next request?
8. Define **SLO** and **SLA**. Give an example of each using percentiles.
9. Distinguish a **fault** from a **failure**. What is **fault tolerance**,
   and what is a **single point of failure**?
10. Compare hardware faults, software faults, and human error. Which are
    correlated, and why does correlation matter?
11. What is a **blameless postmortem**, and why does the book favor it?
12. How do you **describe load**? Why is "requests per second" often not
    enough?
13. Compare **shared-memory**, **shared-disk**, and **shared-nothing**
    architectures.
14. What do **operability**, **simplicity**, and **evolvability** mean, and
    how do you design for each?

### Phase 2: Mini-Project — "Latency Lab"

> **Goal:** See with your own eyes why averages lie and tails dominate.
> **Time box:** 2 evenings.
> **Deliverable:** A lab report with percentile charts.

#### Step 2.A — Measure a latency distribution correctly

- **Do:**
  1. Run a tiny HTTP service with one database-backed endpoint. Make a small
     fraction of requests artificially slow.
  2. Load it with an **open-loop** generator at a fixed arrival rate. Record
     p50, p95, p99, p999, max, and mean.
  3. Repeat with a closed-loop generator and compare.
  4. Detailed procedure: Lab 2.2.
- **Focus concepts:** percentiles, tail latency, queueing, client-side
  measurement, coordinated omission.
- **Mastered when:**
  - You can show a case where the mean looks fine and p99 doesn't.
  - You can explain why the closed-loop test underestimated the tail.

#### Step 2.B — Tail latency amplification

- **Do:**
  1. Compute the probability that a request is slow when it fans out to
     1, 10, 50, and 100 backends, each slow 1% of the time.
  2. Confirm with a 10,000-trial spreadsheet simulation.
  3. Detailed procedure: Lab 2.3.
- **Focus concepts:** tail latency amplification, parallel fan-out.
- **Mastered when:**
  - Your formula and simulation agree.

#### Step 2.C — Write SLOs for the lab service

- **Do:**
  1. Write two latency SLOs and one availability SLO, with measurement
     window and definition of a "good" request.
  2. Compute a monthly error budget.
  3. Detailed procedure: Lab 2.4.
- **Focus concepts:** SLOs, SLAs, error budgets.
- **Mastered when:**
  - Each SLO can be checked mechanically from your measurements.

### Phase 3: Bazaar Milestone M2 — "Vertical Slice with SLOs"

> **Goal:** Build the first working version of Bazaar on a single node, and
> be able to measure it properly from day one.
> **Deliverables:** Running slice, metrics dashboard, load model, baseline
> load-test report, feed ADR, SLO document.

#### M2.1 — Minimal vertical slice

- **Do:**
  1. Build the smallest useful Bazaar on one PostgreSQL node:
     - sign up
     - create and view a listing
     - follow a seller
     - a home feed of followed sellers' new listings, computed at read time
  2. Make it start with one command.
- **Focus concepts:** simplicity, evolvability, starting from a single node.
- **Mastered when:**
  - A fresh clone starts the whole slice with one command.
  - You can explain which parts will hurt first under load, and why.

#### M2.2 — Observability from day one

- **Do:**
  1. Record a latency **histogram** per endpoint, plus error rate and
     request rate.
  2. Build a dashboard showing p50, p95, and p99 per endpoint.
  3. Make sure percentiles across instances are computed by merging
     histograms, never by averaging percentiles.
- **Focus concepts:** response time vs latency, percentiles, operability.
- **Mastered when:**
  - You can explain where your numbers are measured (client or server) and
    what that hides.

#### M2.3 — Load model and baseline test

- **Do:**
  1. In a spreadsheet, define Bazaar's load parameters: users, listings
     created per second, feed reads per second, follower distribution
     (long-tailed, with a few very popular sellers).
  2. Generate synthetic data that matches it.
  3. Run an open-loop load test at 1×, 3×, and 10× your expected load.
  4. Save the results as `docs/reports/M2-baseline.md`.
- **Focus concepts:** describing load, load parameters, scalability.
- **Mastered when:**
  - You know which endpoint breaks its SLO first, and at what load.

#### M2.4 — The feed decision

- **Do:**
  1. Using the method from Lab 2.1, model fan-out-on-read vs
     fan-out-on-write for Bazaar's follower distribution.
  2. Implement the better approach, or a hybrid with a celebrity-seller
     threshold.
  3. Re-run the load test and compare.
  4. Write an ADR with the threshold and its justification.
- **Focus concepts:** fan-out, materialized timelines, skew, read/write
  trade-offs.
- **Mastered when:**
  - Your ADR includes before/after p99 numbers.
  - You can explain what a seller with 1 million followers does to your
    design.

#### M2.5 — SLO document

- **Do:**
  1. Write SLOs for: view listing, home feed, checkout (future), search
     (future).
  2. Add a short runbook: what to look at first when each SLO is breached.
- **Focus concepts:** SLOs, operability, blameless culture.
- **Mastered when:**
  - Every SLO can be evaluated from your dashboard right now, except the
    future ones.

#### Optional deep-dive labs

- **Lab 2.5** — fault drill and blameless postmortem.
- **Lab 2.6** — maintainability audit of a codebase you know.

---

## Chapter 3 — Data Models and Query Languages

### Phase 1: Knowledge Check

1. What is the **object-relational mismatch**? What do ORMs solve, and
   what problems do they introduce?
2. Explain **normalization** vs **denormalization**. When is denormalizing
   the right call?
3. Why do **many-to-one** and **many-to-many** relationships favor the
   relational model over documents?
4. Describe a **star schema** and a **snowflake schema**. What is a fact
   table, and what is a dimension table? What is "one big table"?
5. When does the **document model** fit best? Use the idea of a tree of
   one-to-many relationships loaded all at once.
6. Explain **schema-on-read** vs **schema-on-write**. Why is "schemaless"
   misleading?
7. What is **data locality** in document storage? What is its cost when
   you update one small field?
8. What is a **property graph**? How does it differ from a
   **triple store**?
9. Why are **recursive / variable-length path** queries natural in
   Cypher or SPARQL and awkward in SQL?
10. What is **Datalog**? How do its rules build complex queries out of
    simple ones?
11. What is **GraphQL** designed for, and what does it deliberately **not**
    allow?
12. Explain **event sourcing** and **CQRS**. Why are events stored as
    immutable facts, and how are read models derived?
13. How do **DataFrames**, **matrices**, and **arrays** relate to the
    relational model? When would you use them instead?

### Phase 2: Mini-Project — "One Domain, Three Models"

> **Goal:** Feel the strengths and pains of each data model on the same data.
> **Time box:** 2–3 evenings.
> **Deliverable:** A comparison table and lab report.

#### Step 3.A — Model a profile domain three ways

- **Do:**
  1. Model professional profiles (people, jobs, schools, companies,
     connections) as relational tables, JSON documents, and a property graph.
  2. Load the same 20–50 entities into each.
  3. Detailed procedure: Lab 3.1.
- **Focus concepts:** relational, document, and graph models; data
  locality; joins.
- **Mastered when:**
  - All three models hold equivalent data and you can query each.

#### Step 3.B — Run the same four queries everywhere

- **Do:**
  1. In each model: fetch one full profile, find everyone at a company,
     rename a company everywhere, find people within three degrees.
  2. Break a denormalized rename halfway and describe the result.
  3. Detailed procedure: Labs 3.1 and 3.2.
- **Focus concepts:** normalization, many-to-many relationships, update
  anomalies.
- **Mastered when:**
  - You can name the best and worst model for each query, with reasons.

#### Step 3.C — Recursive query showdown

- **Do:**
  1. Write the "three degrees" or "nested locations" query in Cypher and as
     a recursive SQL query. Optional: in Datalog.
  2. Compare length, readability, and query plans.
  3. Detailed procedure: Lab 3.5.
- **Focus concepts:** graph queries, recursive SQL, Datalog.
- **Mastered when:**
  - All versions return identical results.
  - You can explain why graph languages express this more naturally.

### Phase 3: Bazaar Milestone M3 — "The Data Model"

> **Goal:** Give Bazaar a deliberate data model, choosing the right model
> for each part rather than one model for everything.
> **Deliverables:** Schema diagram, ledger design, model ADRs, first
> invariant checks.

#### M3.1 — Relational core

- **Do:**
  1. Design normalized tables for users, sellers, listings, inventory,
     orders, and order items.
  2. Use IDs for references, never names.
  3. Migrate the M2 slice to this schema without losing data.
- **Focus concepts:** normalization, many-to-one relationships, schema
  migrations.
- **Mastered when:**
  - You can explain each foreign key and what anomaly it prevents.

#### M3.2 — Flexible listing attributes

- **Do:**
  1. Store category-specific attributes (for example size for clothes,
     mileage for cars) as a JSON document inside each listing.
  2. Write down your schema-on-read rules: how the app handles missing or
     old-format attributes.
  3. Decide the rule for when an attribute gets promoted to a real column.
- **Focus concepts:** document model, schema-on-read vs schema-on-write,
  hybrid models.
- **Mastered when:**
  - Old and new attribute formats coexist and both display correctly.
  - Your promotion rule is written in an ADR.

#### M3.3 — The ledger as immutable events

- **Do:**
  1. Design the wallet as a **double-entry ledger**: every transfer is
     recorded as immutable entries that debit one account and credit
     another.
  2. Treat balances as **derived** from entries.
  3. Write the first invariant check: all entries sum to zero.
  4. Delete the derived balances and rebuild them by replaying entries.
- **Focus concepts:** event sourcing, immutability, derived state, CQRS.
- **Mastered when:**
  - Balances rebuilt from entries match exactly.
  - You can explain why a mistaken transfer is corrected by a new entry,
    never by editing an old one.

#### M3.4 — The social graph

- **Do:**
  1. Implement "sellers followed by people you follow" as a recursive query
     in PostgreSQL.
  2. Write an ADR on whether Bazaar needs a dedicated graph database. Use
     your Step 3.C experience.
- **Focus concepts:** graph-like data, recursive queries, choosing models.
- **Mastered when:**
  - The ADR states what query pattern or scale would change the decision.

#### M3.5 — Analytics schema (design only)

- **Do:**
  1. Design a star schema: a fact table for order lines, and dimensions
     for time, buyer, seller, listing category, and region.
  2. State the grain of the fact table.
  3. You'll build it in Chapters 4 and 11.
- **Focus concepts:** star and snowflake schemas, facts and dimensions.
- **Mastered when:**
  - You can list three seller-dashboard questions and the tables each uses.

#### Optional deep-dive labs

- **Lab 3.3** — design an analytics schema for another domain.
- **Lab 3.4** — schema evolution both ways.
- **Lab 3.6** — event-sourced shopping cart with CQRS.
- **Lab 3.7** — DataFrames and matrices.

---

## Chapter 4 — Storage and Retrieval

### Phase 1: Knowledge Check

1. Why is an **append-only log** a good base for storage? What problem
   does an **index** solve, and what does every index cost?
2. Describe a **hash index** over a log file. What are its limitations?
3. What is an **SSTable**? Why does sorting enable efficient merging and
   sparse indexes?
4. Walk through an **LSM-tree** write path and read path: memtable, WAL,
   flush, SSTable levels, compaction.
5. What is a **Bloom filter**, and why does an LSM-tree need one?
6. Compare **size-tiered** and **leveled** compaction.
7. How does a **B-tree** organize data? What happens on a page split?
8. Why does a B-tree need a **write-ahead log**? What other techniques,
   such as copy-on-write, give crash safety?
9. Compare B-trees and LSM-trees on read performance, write throughput,
   **write amplification**, space usage, and latency predictability.
10. Distinguish clustered indexes, covering indexes, and secondary indexes.
    Also explain multi-column (concatenated) indexes.
11. Why can **in-memory databases** be faster, even though disk-based
    databases also cache data in memory?
12. Why is **column-oriented storage** better for analytics? Explain column
    compression: bitmap encoding and run-length encoding.
13. How does **sort order** in a column store help both compression and
    queries?
14. How do column stores handle **writes**?
15. Explain **query compilation** vs **vectorized processing**.
16. What are **materialized views** and **data cubes**? What is the
    trade-off?
17. How does a **full-text** inverted index work?
18. What are **vector embeddings**? Explain approximate nearest neighbor
    indexes such as HNSW and IVF.

### Phase 2: Mini-Project — "Build a Tiny LSM Store"

> **Goal:** Build a miniature storage engine in your language, so storage
> internals stop being abstract.
> **Time box:** 3 evenings.
> **Deliverable:** Your engine, plus a benchmark report against SQLite.

#### Step 4.A — Log plus hash index

- **Do:**
  1. Build an append-only key-value store with an in-memory hash index,
     segments, compaction, tombstones, and checksummed crash recovery.
  2. Detailed procedure: Lab 4.1.
- **Focus concepts:** append-only logs, hash indexes, compaction, crash
  recovery.
- **Mastered when:**
  - Killing the process at any moment never returns corrupt data.

#### Step 4.B — Turn it into an LSM-tree

- **Do:**
  1. Add a sorted memtable, SSTable flushes with sparse indexes, merge
     compaction, a write-ahead log, and per-file Bloom filters.
  2. Detailed procedure: Lab 4.2.
- **Focus concepts:** SSTables, memtables, Bloom filters, read
  amplification.
- **Mastered when:**
  - Range scans work, and you've measured the Bloom filter's benefit.

#### Step 4.C — Benchmark against a B-tree

- **Do:**
  1. Run the same workloads against your engine and SQLite: sequential
     writes, random writes, random reads, range scans.
  2. Measure bytes written to disk vs bytes written by the app.
- **Focus concepts:** B-trees vs LSM-trees, write amplification,
  sequential vs random I/O.
- **Mastered when:**
  - You can explain each result using the chapter's comparison of the two
    designs.

### Phase 3: Bazaar Milestone M4 — "Indexes, Search, and Analytics"

> **Goal:** Make Bazaar's queries fast on purpose, add search, and separate
> analytics from OLTP.
> **Deliverables:** Index review report, search feature with recall
> numbers, analytics v0, storage ADR.

#### M4.1 — Index review

- **Do:**
  1. From your M2 dashboard, take the ten slowest or most frequent queries.
  2. For each, view the query plan, design an index (single, multi-column,
     or covering), and measure again.
  3. Measure the write slowdown each index adds.
  4. Save the results in `docs/reports/M4-indexes.md`.
- **Focus concepts:** secondary indexes, multi-column indexes, covering
  indexes, the write cost of indexes.
- **Mastered when:**
  - You can predict the plan before running it for most queries.
  - You rejected at least one index because its write cost was too high.

#### M4.2 — Listing search: keyword plus semantic

- **Do:**
  1. Add full-text search over listing titles and descriptions, with
     ranking and stemming.
  2. Add semantic search: embeddings for each listing, stored with
     `pgvector`, with an approximate (HNSW) index.
  3. Measure recall at 10 of the approximate index against exact search.
     Tune one parameter.
  4. Combine both into one ranked result list.
- **Focus concepts:** inverted indexes, vector embeddings, approximate
  nearest-neighbor search, recall vs speed.
- **Mastered when:**
  - You have recall and latency numbers for at least two index settings.
  - You can show a query where keyword wins and one where semantic wins.

#### M4.3 — Analytics v0

- **Do:**
  1. Export orders and listings to Parquet files in object storage (MinIO).
     For now, a manual step is fine.
  2. Build the M3.5 star schema in DuckDB on top of the files.
  3. Answer three seller-dashboard questions. Compare speed with running
     the same questions on the OLTP database.
  4. Add one materialized aggregate. Note its staleness.
- **Focus concepts:** column-oriented storage, compression, separating
  OLTP from analytics, materialized views.
- **Mastered when:**
  - You can explain the speed difference in terms of bytes read.
  - Analytics queries no longer touch the production database.

#### M4.4 — Storage engine ADR

- **Do:**
  1. Write an ADR covering: why a B-tree engine for OLTP, why columnar for
     analytics, and whether any Bazaar data (for example click and view
     events) would suit an LSM-based store better.
- **Focus concepts:** B-trees vs LSM-trees, OLTP vs OLAP storage.
- **Mastered when:**
  - The ADR uses numbers from your mini-project benchmark.

#### Optional deep-dive labs

- **Lab 4.3** — look inside a real B-tree.
- **Lab 4.4** — RocksDB vs B-tree engine benchmark.
- **Lab 4.6** — columnar storage on a large public dataset.

---

## Chapter 5 — Encoding and Evolution

### Phase 1: Knowledge Check

1. Define **backward compatibility** and **forward compatibility**. Which
   one is harder, and why?
2. Why are **language-specific serialization formats** (for example Java
   serialization or Python pickle) a bad idea for storage and interchange?
3. What are the weaknesses of **JSON, XML, and CSV**? Consider numbers,
   binary strings, and schemas.
4. How does **Protocol Buffers** encode a record? What role do field tags
   play in schema evolution?
5. How does **Avro** differ? Explain writer's schema vs reader's schema
   and how they are resolved.
6. Why does Avro not need field tags? What does that make easier, for
   example generating schemas from a database?
7. What are the benefits of schemas, as listed in the book?
8. Describe the hazard where **old code reads a new record, updates it,
   and writes it back**. How do you prevent data loss?
9. What does "data outlives code" mean in database dataflow?
10. Compare **REST** and **RPC**. Why is a network call fundamentally
    different from a local function call?
11. How should service APIs be **versioned**?
12. What is **durable execution** (workflow engines)? What does it
    require of your code, for example determinism?
13. How do **message brokers** and **event-driven architectures** change
    the compatibility problem?
14. What is a **distributed actor framework**, and how does it handle
    message encoding across versions?

### Phase 2: Mini-Project — "Schema Evolution Torture Test"

> **Goal:** Find out by experiment which schema changes break which readers.
> **Time box:** 2 evenings.
> **Deliverable:** A fully tested compatibility matrix and lab report.

#### Step 5.A — Decode bytes by hand

- **Do:**
  1. Encode one record as JSON, MessagePack, Protocol Buffers, and Avro.
     Compare sizes.
  2. Annotate the Protobuf and Avro hex dumps byte by byte.
  3. Detailed procedure: Lab 5.1.
- **Focus concepts:** binary encodings, field tags, variable-length
  integers.
- **Mastered when:**
  - You can decode a small Protobuf message by hand.

#### Step 5.B — Compatibility matrix

- **Do:**
  1. Test six schema changes in both directions in both Protobuf and Avro.
  2. Fill every cell from a real test. Detailed procedure: Lab 5.2.
- **Focus concepts:** forward and backward compatibility, defaults, schema
  resolution.
- **Mastered when:**
  - You can write each format's safe evolution rules from memory.

#### Step 5.C — Reproduce the lost-field hazard

- **Do:**
  1. Let an old app version read, modify, and write back a record created
     by a new version. Check whether the new field survives.
  2. Apply a fix. Detailed procedure: Lab 5.3.
- **Focus concepts:** dataflow through databases, rolling upgrades.
- **Mastered when:**
  - You reproduced the loss and prevented it.

### Phase 3: Bazaar Milestone M5 — "Contracts, Events, and Durable Checkout"

> **Goal:** Make Bazaar safe to evolve and safe to retry, and add checkout.
> **Deliverables:** API contract with versioning policy, event schemas,
> idempotent writes, durable checkout workflow, rolling-upgrade drill report.

#### M5.1 — API contract and versioning policy

- **Do:**
  1. Write a formal API description for every endpoint (OpenAPI, or gRPC
     service definitions).
  2. Write the versioning policy: what counts as a breaking change, and how
     clients migrate.
  3. Add an automatic breaking-change check to your build pipeline, using a
     schema-diff tool for your format.
- **Focus concepts:** REST vs RPC, API versioning, compatibility.
- **Mastered when:**
  - A deliberately breaking change fails the build.

#### M5.2 — Domain event schemas

- **Do:**
  1. Define schemas for Bazaar's domain events: listing created, order
     placed, payment captured, transfer completed, and so on.
  2. For now, write each event to an **outbox table** in the same
     transaction as the business change. Chapter 12 will stream it.
  3. Choose a compatibility mode for each event type, as in Lab 5.6.
- **Focus concepts:** schemas, event-driven dataflow, compatibility modes.
- **Mastered when:**
  - Every event type has a schema and a stated compatibility mode.

#### M5.3 — Idempotency keys

- **Do:**
  1. Require an idempotency key on every mutating endpoint, especially
     payments and transfers.
  2. Test: send the same request 10 times concurrently. Exactly one effect
     must happen, and all 10 must get the same response.
  3. Test: commit the request, then drop the response, then retry.
- **Focus concepts:** network calls vs local calls, retries, idempotence.
- **Mastered when:**
  - Both tests pass, and the "no double charge" invariant check passes.

#### M5.4 — Durable checkout workflow

- **Do:**
  1. Build checkout in a durable-execution engine: reserve inventory, debit
     wallet, create order, notify seller.
  2. Add compensation steps: release inventory and refund if a later step
     fails.
  3. Kill the worker in the middle of each step. Verify that checkout
     finishes or fully compensates, never stops halfway.
- **Focus concepts:** durable execution, workflow determinism, side
  effects, compensation.
- **Mastered when:**
  - Every kill point ends in a consistent state, and all invariants pass.

#### M5.5 — Rolling-upgrade drill

- **Do:**
  1. Run two versions of the Bazaar service side by side against the same
     database. The new version adds a listing field.
  2. Let both versions edit the same listings.
  3. Write up the drill in `docs/reports/`.
- **Focus concepts:** rolling upgrades, forward compatibility, data
  outliving code.
- **Mastered when:**
  - No field is lost, and you can explain the mechanism that prevents it.

#### Optional deep-dive labs

- **Lab 5.4** — REST vs RPC API design exercise.
- **Lab 5.5** — durable execution tutorial on its own.

---

# Part II — Distributed Data

---

## Chapter 6 — Replication

### Phase 1: Knowledge Check

1. Why replicate data? List the main goals.
2. Explain **single-leader replication**. Compare **synchronous** and
   **asynchronous** followers. What is semi-synchronous replication?
3. How do you add a **new follower** without downtime?
4. Explain **follower failure: catch-up recovery** and
   **leader failure: failover**. List the things that can go wrong in
   failover, including split brain and lost writes.
5. Compare replication log implementations:
   - statement-based
   - WAL shipping
   - logical (row-based)

   Which one decouples replication from the storage engine?
6. Define **read-after-write (read-your-writes)**, **monotonic reads**,
   and **consistent prefix reads**. Give a user-visible bug for each.
7. When is **multi-leader** replication useful? Consider geo-distribution
   and offline clients.
8. What are **sync engines** and **local-first software**?
9. Name ways to handle **conflicting writes**:
   - conflict avoidance
   - last write wins
   - manual resolution
   - CRDTs
   - operational transformation

   What is lost with LWW?
10. How does **leaderless** replication work? Explain read repair and
    anti-entropy.
11. Explain **quorums** (*w + r > n*). List the situations where stale
    reads still happen despite a quorum.
12. How do you detect **concurrent writes**? Explain the
    **happens-before** relation and **version vectors**.
13. Compare single-leader and leaderless replication for performance and
    for multi-region operation.

### Phase 2: Mini-Project — "Break Replication"

> **Goal:** Lose data and see stale reads on purpose, so you recognize them
> forever.
> **Time box:** 2–3 evenings.
> **Deliverable:** A lab report with timelines of each anomaly.

#### Step 6.A — Lose an acknowledged write

- **Do:**
  1. Run a PostgreSQL primary and replica. Write continuously while logging
     acknowledged writes.
  2. With async replication, kill the primary and promote the replica.
     Find the lost writes.
  3. Repeat with sync replication. Detailed procedure: Lab 6.1.
- **Focus concepts:** sync vs async replication, failover, durability.
- **Mastered when:**
  - You found an acknowledged write that disappeared, and can explain why.

#### Step 6.B — Reproduce three lag anomalies

- **Do:**
  1. Add an artificial replica delay. Reproduce read-your-writes,
     monotonic-reads, and consistent-prefix violations.
  2. Fix each one. Detailed procedure: Lab 6.3.
- **Focus concepts:** replication lag, eventual consistency, session
  guarantees.
- **Mastered when:**
  - All three anomalies were reproduced and fixed, with costs explained.

#### Step 6.C — Version vectors on paper

- **Do:**
  1. Trace the concurrent shopping-cart example with two clients, then
     three replicas. Detailed procedure: Lab 6.6.
- **Focus concepts:** happens-before, concurrency, version vectors.
- **Mastered when:**
  - Given two version vectors, you can instantly say how they're related.

### Phase 3: Bazaar Milestone M6 — "Replicated Bazaar"

> **Goal:** Add replicas without breaking user expectations, and support
> offline editing.
> **Deliverables:** Read-routing policy, session guarantees with tests,
> failover runbook and drill report, offline drafts.

#### M6.1 — Read-routing policy

- **Do:**
  1. Add two read replicas.
  2. Classify every read endpoint: must read from the leader, may read from
     a replica, or needs a session guarantee.
  3. Route reads according to the table.
- **Focus concepts:** single-leader replication, replication lag,
  read scaling.
- **Mastered when:**
  - Every endpoint has a classification and a written reason.

#### M6.2 — Session guarantees

- **Do:**
  1. Implement read-your-writes for a seller's own listings and the user's
     wallet balance.
  2. Implement monotonic reads for the home feed.
  3. Write tests that inject replica lag and verify each guarantee.
- **Focus concepts:** read-your-writes, monotonic reads, consistent prefix.
- **Mastered when:**
  - The tests fail with guarantees off and pass with them on.

#### M6.3 — Failover runbook and drill

- **Do:**
  1. Write an ADR choosing sync or async replication for the ledger. Use
     your Step 6.A results.
  2. Write a failover runbook.
  3. Run a failover drill under load. Measure data loss (RPO) and downtime
     (RTO).
  4. Run all invariant checks afterwards.
- **Focus concepts:** failover, split brain, durability vs availability.
- **Mastered when:**
  - Your drill report states RPO and RTO with numbers.
  - The ledger invariant holds after failover, or you documented exactly
    why not.

#### M6.4 — Offline listing drafts with a CRDT

- **Do:**
  1. Store listing drafts as CRDT documents (Automerge or Yjs).
  2. Edit the same draft on two offline clients, then sync.
  3. Decide what happens when a draft is published: it becomes a
     server-authoritative listing.
  4. Document the merge results that might surprise users.
- **Focus concepts:** multi-leader replication, local-first software,
  CRDTs, conflict resolution.
- **Mastered when:**
  - Both clients always converge.
  - You can explain why drafts use CRDTs but the ledger never does.

#### Optional deep-dive labs

- **Lab 6.2** — inspect replication log formats.
- **Lab 6.4** — CRDT vs last-write-wins comparison.
- **Lab 6.5** — leaderless quorum experiments with Cassandra or ScyllaDB.

---

## Chapter 7 — Sharding

### Phase 1: Knowledge Check

1. What is **sharding**, and what synonyms do different systems use?
   Why combine sharding with replication?
2. What are the **pros and cons** of sharding? When should you avoid it?
3. How does sharding help with **multitenancy**?
4. Explain **key-range sharding**. What are its strengths, such as range
   scans, and its risks, such as hot spots on timestamps?
5. Explain **hash-based sharding**. Why is *hash mod N* a bad
   rebalancing strategy?
6. Compare these approaches:
   - a fixed number of shards larger than the node count
   - dynamic shard splitting
   - **consistent hashing**
7. What causes **skewed workloads** and **hot spots**? How can you relieve
   a hot key?
8. Should rebalancing be **automatic or manual**? What can go wrong with
   fully automatic rebalancing?
9. Describe three approaches to **request routing**. What role does a
   coordination service play?
10. Compare **local (document-partitioned)** and **global
    (term-partitioned)** secondary indexes. Which needs scatter/gather,
    and which makes writes more complex?

### Phase 2: Mini-Project — "Rebalancing Simulator"

> **Goal:** See with numbers how sharding strategies behave on real keys.
> **Time box:** 2 evenings.
> **Deliverable:** A spreadsheet or small simulator, plus charts.

#### Step 7.A — Range vs hash sharding

- **Do:**
  1. Assign time-stamped rows to 8 shards by range, by hash, and by
     compound key. Chart the write load per shard.
  2. Detailed procedure: Lab 7.1.
- **Focus concepts:** key-range vs hash sharding, hot spots.
- **Mastered when:**
  - Your chart clearly shows the time-range hot spot.

#### Step 7.B — Count the keys that move

- **Do:**
  1. Place 100,000 keys on 10 nodes using hash mod N, fixed shards, and
     consistent hashing. Add and remove a node, and count moved keys.
  2. Detailed procedure: Lab 7.2.
- **Focus concepts:** rebalancing, consistent hashing, fixed shard counts.
- **Mastered when:**
  - Your numbers match what the chapter predicts.

#### Step 7.C — Hot key salting

- **Do:**
  1. Design salting for one key receiving 500,000 writes per minute.
  2. Detailed procedure: Lab 7.3.
- **Focus concepts:** skew, hot keys, the read cost of salting.
- **Mastered when:**
  - You can compute per-shard load for your chosen salt count.

### Phase 3: Bazaar Milestone M7 — "Sharding Decision"

> **Goal:** Decide, with evidence, what to shard and how. "Not yet" is a
> valid answer if the numbers support it.
> **Deliverables:** Sharding ADR, shard-key design, one sharded dataset with
> a rebalancing demo, hot-seller mitigation.

#### M7.1 — Do we need to shard?

- **Do:**
  1. Project your M2 load model to 10× and 100×.
  2. For each dataset, find the point where a single node plus replicas
     stops being enough.
  3. Write an ADR with the threshold for each dataset.
- **Focus concepts:** pros and cons of sharding, when to avoid it.
- **Mastered when:**
  - The ADR names which dataset hits its limit first, with numbers.

#### M7.2 — Shard key design

- **Do:**
  1. Choose a shard key for each dataset: listings, orders, ledger
     accounts, feeds, view events.
  2. List every operation that would span shards, for example a transfer
     between users on different shards.
  3. Mark which of those will be solved in Chapter 8 and which in
     Chapter 13.
- **Focus concepts:** shard keys, cross-shard operations, request routing.
- **Mastered when:**
  - Every cross-shard operation is listed with a planned solution.

#### M7.3 — Shard one dataset and rebalance it

- **Do:**
  1. Shard the dataset that hits its limit first, using a sharded SQL
     system or application-level sharding.
  2. Under load, add a node and let it rebalance.
  3. Record p99 latency before, during, and after.
- **Focus concepts:** rebalancing, request routing, operations.
- **Mastered when:**
  - Your report shows the latency impact of rebalancing.
  - You can explain how a request finds the right shard.

#### M7.4 — Hot-seller mitigation

- **Do:**
  1. Simulate a viral listing receiving a huge share of views and likes.
  2. Apply the salting design from Step 7.C to its counters.
  3. Re-run the skewed load test.
- **Focus concepts:** skew, hot spots, salting.
- **Mastered when:**
  - The hot shard's load drops by roughly the salt factor.

#### M7.5 — Search index sharding strategy

- **Do:**
  1. Decide whether Bazaar's search would use local or global secondary
     indexes when sharded.
  2. Estimate the scatter/gather cost and connect it to your Chapter 2
     tail-latency results.
- **Focus concepts:** local vs global secondary indexes.
- **Mastered when:**
  - Your ADR quantifies the expected p99 for a scatter/gather search.

#### Optional deep-dive labs

- **Lab 7.4** — investigate a real sharded system in depth.
- **Lab 7.5** — local vs global index tracing.
- **Lab 7.6** — multitenant sharding plan.

---

## Chapter 8 — Transactions

### Phase 1: Knowledge Check

1. Explain each letter of **ACID** precisely. Why does the book say the
   "C" doesn't really belong?
2. Compare single-object and multi-object operations. Why do multi-object
   transactions matter?
3. What does **aborting and retrying** a transaction safely require? When
   is a retry dangerous?
4. What does **read committed** prevent? How is it typically implemented?
5. What anomaly does read committed still allow (read skew /
   nonrepeatable read)? Why does it matter for backups and analytics?
6. How does **snapshot isolation** work with **MVCC**? Explain the
   visibility rules.
7. Why is "repeatable read" an ambiguous name across databases?
8. What is a **lost update**? List the ways to prevent it:
   - atomic writes
   - explicit locking
   - automatic detection
   - compare-and-set
   - conflict resolution in replicated systems
9. Define **write skew** and **phantoms** with an example. Why doesn't
   `SELECT ... FOR UPDATE` always help? What is materializing conflicts?
10. Explain three ways to get **serializability**:
    - actual serial execution
    - two-phase locking (2PL)
    - serializable snapshot isolation (SSI)
11. What are **predicate locks** and **index-range locks**?
12. How does SSI detect conflicts? Explain stale MVCC reads and writes
    that affect prior reads.
13. Explain **two-phase commit (2PC)**. What happens if the coordinator
    crashes after prepare? What is an in-doubt transaction?
14. Compare database-internal distributed transactions with heterogeneous
    (XA) transactions.
15. How can **exactly-once message processing** be achieved with atomic
    commit or with idempotence?

### Phase 2: Mini-Project — "Isolation Anomaly Zoo"

> **Goal:** Reproduce every classic anomaly with your own hands.
> **Time box:** 2–3 evenings.
> **Deliverable:** A timeline diagram for each anomaly, plus its fix.

> Use **two or three separate PostgreSQL sessions** in different terminal
> windows. Step through each interleaving by hand.

#### Step 8.A — Read skew and lost updates

- **Do:**
  1. Reproduce read skew under read committed. Detailed procedure: Lab 8.1.
  2. Reproduce a lost update and fix it four ways. Detailed procedure:
     Lab 8.2.
- **Focus concepts:** read committed, read skew, lost updates, atomic
  operations, compare-and-set.
- **Mastered when:**
  - Each anomaly and fix is drawn as a timeline and verified.

#### Step 8.B — Write skew and phantoms

- **Do:**
  1. Reproduce the on-call doctors write skew and the SSI abort. Detailed
     procedure: Lab 8.3.
  2. Reproduce a meeting-room double booking and fix it three ways.
     Detailed procedure: Lab 8.4.
- **Focus concepts:** write skew, phantoms, SSI, materializing conflicts.
- **Mastered when:**
  - You can explain why each is not a lost update.

#### Step 8.C — In-doubt 2PC

- **Do:**
  1. Play the coordinator by hand and "crash" after prepare. Detailed
     procedure: Lab 8.6.
- **Focus concepts:** two-phase commit, in-doubt transactions, blocking.
- **Mastered when:**
  - You observed locks held by prepared transactions surviving a restart.

### Phase 3: Bazaar Milestone M8 — "Correct Money and Inventory"

> **Goal:** Make Bazaar provably correct under concurrency.
> **Deliverables:** Isolation matrix, concurrency test suite, write-skew
> fixes, retry policy, cross-shard transfer design.

#### M8.1 — Isolation matrix

- **Do:**
  1. List every write operation in Bazaar.
  2. For each, name the anomaly it's exposed to (lost update, write skew,
     phantom, none) and the technique you'll use (atomic update, row lock,
     compare-and-set, serializable isolation, constraint).
  3. Save it as `docs/isolation-matrix.md`.
- **Focus concepts:** isolation levels, choosing protection per operation.
- **Mastered when:**
  - Every operation has an anomaly analysis, even if the answer is "none".

#### M8.2 — Concurrency test suite

- **Do:**
  1. Write tests that hit Bazaar's API concurrently:
     - 50 buyers try to buy the last item; exactly one succeeds
     - 20 concurrent transfers from one wallet can't overdraw it
     - 10 concurrent sign-ups with the same username; exactly one succeeds
  2. Run them at a weak isolation level first and confirm they fail. Then
     apply your matrix and confirm they pass.
- **Focus concepts:** race conditions, lost updates, write skew.
- **Mastered when:**
  - Each test fails without the protection and passes with it, every time
    over 100 runs.

#### M8.3 — Fix Bazaar's write skew cases

- **Do:**
  1. Find at least two write-skew-shaped rules in Bazaar. Examples:
     - "a seller must keep at least one payout method"
     - "no overlapping pickup time slots for a seller"
  2. Fix them using serializable isolation or materialized conflicts, and
     test them.
- **Focus concepts:** write skew, phantoms, SSI.
- **Mastered when:**
  - You can explain why `SELECT ... FOR UPDATE` alone was or wasn't enough
    in each case.

#### M8.4 — Retry policy

- **Do:**
  1. Retry serialization failures automatically, with a limit and backoff.
  2. Use idempotency keys from M5.3 so retries are always safe.
  3. Measure abort rates under rising contention.
- **Focus concepts:** aborts and retries, when retries are dangerous.
- **Mastered when:**
  - You know the contention level at which abort rates become a problem.

#### M8.5 — Cross-shard transfers

- **Do:**
  1. If you sharded the ledger in M7, implement transfers between shards
     using two-phase commit (prepared transactions).
  2. Crash the coordinator between phases. Document recovery.
  3. If you didn't shard the ledger, design this on paper and note it as a
     future problem for Chapter 13.
- **Focus concepts:** distributed transactions, 2PC, coordinator failure.
- **Mastered when:**
  - The ledger invariant holds after every crash point you test.

#### Optional deep-dive labs

- **Lab 8.5** — isolation levels across databases (Hermitage).
- **Lab 8.7** — effectively-once processing design.

---

## Chapter 9 — The Trouble with Distributed Systems

### Phase 1: Knowledge Check

1. What is a **partial failure**? Why does it make distributed systems
   different from single computers?
2. List the things that can go wrong when you send a request over an
   **asynchronous network**. Why can't the sender tell them apart?
3. What does **TCP** guarantee, and what are its limits?
4. What do studies show about **network faults in practice**?
5. How do you choose a **timeout**? Why are network delays unbounded?
   Explain queueing at switches, the OS, the hypervisor, and TCP.
6. Compare **synchronous** (circuit-switched) and **asynchronous**
   (packet-switched) networks. Why don't datacenters use bounded-delay
   networks?
7. Compare **time-of-day clocks** and **monotonic clocks**. Which should
   you use to measure elapsed time?
8. How accurate is **NTP synchronization** in practice? What goes wrong?
9. Why is **last write wins** with wall-clock timestamps dangerous?
10. What are **clock confidence intervals**? How does Spanner's TrueTime
    use them?
11. List the causes of **process pauses**. Why do they break lease-based
    code?
12. What does "**the majority rules**" mean? Why can't a node trust its
    own judgment about being the leader?
13. What is a **fencing token**? How does it prevent a paused process
    from corrupting data?
14. What are **Byzantine faults**? When do you need Byzantine fault
    tolerance and when don't you?
15. Name the **system models** for timing and for node failure. Define
    **safety** vs **liveness** properties.
16. What do **model checking**, **fault injection**, and
    **deterministic simulation testing** each give you?

### Phase 2: Mini-Project — "Lease and Fencing Demo"

> **Goal:** Watch a paused process corrupt data, then make it impossible.
> **Time box:** 2–3 evenings.
> **Deliverable:** A small toy system and a lab report.

#### Step 9.A — Network faults

- **Do:**
  1. Put a fault-injection proxy between a client and a database. Inject
     latency, packet loss, and a partition.
  2. Find a case where the client times out but the write committed.
  3. Detailed procedure: Lab 9.1.
- **Focus concepts:** unreliable networks, ambiguous failures, timeouts.
- **Mastered when:**
  - You can explain why the client can't tell "request lost" from
    "response lost".

#### Step 9.B — Lease violation and fencing tokens

- **Do:**
  1. Build a toy lock service with leases and a toy storage service.
  2. Pause a lease holder past expiry and corrupt data.
  3. Add fencing tokens and show the stale write is rejected.
  4. Detailed procedure: Lab 9.4.
- **Focus concepts:** process pauses, leases, fencing tokens.
- **Mastered when:**
  - You reproduced the corruption and then prevented it.

#### Step 9.C — Clock skew vs last write wins

- **Do:**
  1. Compare wall-clock and monotonic measurements while changing the
     system time.
  2. On paper, show a causally later write lost under LWW with skewed
     clocks. Detailed procedure: Lab 9.3.
- **Focus concepts:** time-of-day vs monotonic clocks, clock skew.
- **Mastered when:**
  - Your LWW example shows the lost write with exact timestamps.

### Phase 3: Bazaar Milestone M9 — "Chaos Bazaar"

> **Goal:** Assume everything fails, and prove Bazaar stays correct.
> **Deliverables:** Fault scenario catalog, tuned timeouts and retries,
> clock audit, fenced payout job, game-day postmortem.

#### M9.1 — Fault scenario catalog

- **Do:**
  1. Put fault proxies between every Bazaar component and its dependencies.
  2. Write a catalog of at least ten fault scenarios: latency, loss,
     partition, slow-closing connection, process pause, disk full, and so
     on.
- **Focus concepts:** partial failure, unreliable networks.
- **Mastered when:**
  - Each scenario can be triggered with one documented step.

#### M9.2 — Timeouts, retries, and circuit breakers

- **Do:**
  1. Measure RTT distributions between Bazaar's components.
  2. Set every timeout from those numbers, not by guessing.
  3. Add retries with exponential backoff and jitter, plus circuit
     breakers.
  4. Prove with the M5.3 tests that retries never double-charge.
- **Focus concepts:** timeouts, unbounded delays, cascading failures.
- **Mastered when:**
  - Every timeout in the system has a written justification.

#### M9.3 — Clock audit

- **Do:**
  1. Search Bazaar's code for every use of wall-clock time.
  2. Classify each: display only (fine), measuring duration (use a
     monotonic clock), or ordering events (use database order, sequence
     numbers, or logical clocks).
  3. Fix the dangerous ones. Re-run tests with a skewed clock on one node.
- **Focus concepts:** unreliable clocks, ordering, LWW dangers.
- **Mastered when:**
  - No ordering or correctness decision depends on wall-clock time.

#### M9.4 — Fenced payout job

- **Do:**
  1. Build a daily payout job that must run on exactly one worker at a
     time, protected by a lease.
  2. Pause the worker past its lease and show a double payout.
  3. Add fencing tokens checked by the ledger. Show the double payout is
     now rejected.
- **Focus concepts:** leases, process pauses, fencing tokens.
- **Mastered when:**
  - The ledger, not the worker, enforces the token check.

#### M9.5 — Game day

- **Do:**
  1. Run every scenario from the catalog under load.
  2. Run all invariant checks after each scenario.
  3. Write a blameless postmortem for the worst failure you found, and fix
     it.
- **Focus concepts:** fault tolerance, system models, safety vs liveness.
- **Mastered when:**
  - You found at least one real bug and fixed it.
  - For each scenario, you can say whether safety or liveness was affected.

#### Optional deep-dive labs

- **Lab 9.2** — choosing a timeout from data.
- **Lab 9.5** — system model classification.
- **Lab 9.6** — Jepsen reports, deterministic simulation, TLA+.

---

## Chapter 10 — Consistency and Consensus

### Phase 1: Knowledge Check

1. Define **linearizability** precisely. What does "recency guarantee"
   mean?
2. How does linearizability differ from **serializability**? What is
   strict serializability?
3. List situations where you **rely on linearizability**:
   - locking and leader election
   - constraints and uniqueness guarantees
   - cross-channel timing dependencies
4. Which replication methods can be linearizable, and which can't? Why
   isn't a leaderless quorum enough?
5. State the **CAP theorem** properly. Why does the book consider it
   unhelpful as usually quoted?
6. Why is linearizability slow, beyond fault tolerance?
7. What are **Lamport timestamps**? What do they guarantee, and what
   can't they guarantee (for example uniqueness checks)?
8. What are **hybrid logical clocks**, and why are they useful?
9. How do you build a **linearizable ID generator**? Compare it with
   Snowflake-style IDs and UUIDv7.
10. List the **many faces of consensus**, problems equivalent to
    consensus:
    - single-value consensus
    - compare-and-set
    - shared log (total order broadcast)
    - fetch-and-add
    - atomic commitment
11. Explain how **single-leader replication** turns into consensus. What
    are epochs or terms, and why do two rounds of voting overlap?
12. What subtleties exist in consensus: reconfiguration, leader leases,
    and so on? What are its costs?
13. What do **coordination services** (ZooKeeper, etcd) provide?
    Explain linearizable operations, ephemeral nodes/leases, watches,
    and fencing.

### Phase 2: Mini-Project — "Consensus Playground"

> **Goal:** Watch consensus work, and stop working, in front of you.
> **Time box:** 2–3 evenings.
> **Deliverable:** Annotated diagrams and an etcd lab report.

#### Step 10.A — Linearizability by diagram

- **Do:**
  1. Draw five histories and decide whether each is linearizable.
  2. Detailed procedure: Lab 10.1.
- **Focus concepts:** linearizability, linearization points.
- **Mastered when:**
  - You can judge a new history in under two minutes.

#### Step 10.B — Raft in a visualization

- **Do:**
  1. Reproduce election, leader crash, minority partition, and log repair
     in an interactive Raft visualization.
  2. Detailed procedure: Lab 10.4.
- **Focus concepts:** consensus, terms/epochs, quorums, log replication.
- **Mastered when:**
  - You can explain why a minority partition can't commit.

#### Step 10.C — etcd under partition

- **Do:**
  1. Run a 3-node etcd cluster. Use compare-and-swap, leases, and watches.
  2. Partition one node and test both sides. Detailed procedure: Lab 10.5.
- **Focus concepts:** coordination services, CAP in practice.
- **Mastered when:**
  - The minority side refused linearizable operations.

### Phase 3: Bazaar Milestone M10 — "Coordination and Linearizability"

> **Goal:** Use consensus exactly where Bazaar needs it, and nowhere else.
> **Deliverables:** Linearizability inventory, etcd-based leader election,
> ID strategy ADR, invariant checker under partitions.

#### M10.1 — Linearizability inventory

- **Do:**
  1. For every Bazaar operation, decide whether it needs linearizability.
     Candidates: username uniqueness, last-item purchase, wallet balance,
     feed, search.
  2. For each "yes", name the mechanism that provides it.
  3. For each "no", name the weaker guarantee that's enough.
- **Focus concepts:** relying on linearizability, the cost of
  linearizability, CAP.
- **Mastered when:**
  - Feed and search are explicitly "no", with reasons.

#### M10.2 — Replace the homegrown lease with etcd

- **Do:**
  1. Move the payout job's leader election to etcd.
  2. Use the etcd revision as the fencing token for the ledger.
  3. Partition the leader from the etcd majority. Verify that a new leader
     takes over and the old one's writes are rejected.
- **Focus concepts:** coordination services, leader election, fencing.
- **Mastered when:**
  - Your partition test never produces a double payout.

#### M10.3 — ID strategy

- **Do:**
  1. Compare database sequences, UUIDv4, UUIDv7, and Snowflake-style IDs for
     Bazaar's tables. Use the method from Lab 10.3.
  2. Write an ADR, and apply the decision.
- **Focus concepts:** ID generators, ordering, Lamport timestamps, hybrid
  logical clocks.
- **Mastered when:**
  - The ADR explains what ordering guarantees your IDs do **not** give.

#### M10.4 — Jepsen-style invariant checker

- **Do:**
  1. During a run with partitions and failovers, record a history of every
     transfer request, response, and balance read.
  2. Afterwards, check:
     - every acknowledged transfer exists in the ledger
     - the ledger sums to zero
     - no balance read shows a state that never existed
- **Focus concepts:** linearizability checking, safety properties.
- **Mastered when:**
  - The checker runs automatically after each chaos scenario from M9.

#### Optional deep-dive labs

- **Lab 10.2** — Lamport and vector clocks by hand.
- **Lab 10.6** — reductions between consensus problems.

---

# Part III — Derived Data

---

## Chapter 11 — Batch Processing

### Phase 1: Knowledge Check

1. What is **batch processing**, and how does it differ from online
   services and stream processing?
2. What design principles from the **Unix philosophy** carry over to
   batch systems? Consider uniform interfaces, composition, and immutable
   inputs.
3. Why does sorting scale better than an in-memory hash table when data
   exceeds memory?
4. Compare **distributed filesystems** and **object stores** for batch
   jobs. What semantics differ? Consider renames and appends.
5. What does **distributed job orchestration** involve? Cover resource
   allocation and scheduling.
6. How do workflow schedulers (for example Airflow) chain jobs into DAGs?
7. How do batch systems **handle faults**? Why is re-running tasks safe?
8. Explain the **MapReduce** model and its limitations.
9. How do **dataflow engines** (Spark, Flink batch) improve on MapReduce?
10. Explain **shuffling**. Compare join strategies:
    - sort-merge joins
    - broadcast hash joins
    - partitioned hash joins
11. How do batch jobs deal with **skew** in join keys?
12. Why did **SQL and DataFrame** APIs largely replace hand-written
    map/reduce code?
13. Describe batch use cases:
    - ETL
    - analytics
    - machine learning
    - **serving derived data** by bulk-loading built indexes
14. What is the advantage of building a new derived dataset and swapping
    it in, rather than writing results into a live database row by row?

### Phase 2: Mini-Project — "Logs to Insights"

> **Goal:** Process data that doesn't fit in memory, and see joins and skew
> at work.
> **Time box:** 2–3 evenings.
> **Deliverable:** A lab report with query plans and Spark UI screenshots.

#### Step 11.A — Unix pipeline at scale

- **Do:**
  1. Find the top URLs in a large access log using only a Unix pipeline,
     including a file bigger than RAM. Detailed procedure: Lab 11.1.
- **Focus concepts:** the Unix philosophy, sort-based processing.
- **Mastered when:**
  - The pipeline works on the oversized file and you can explain why.

#### Step 11.B — Join strategies

- **Do:**
  1. In Spark local mode, force a sort-merge join and a broadcast hash
     join. Compare shuffle bytes and runtime. Detailed procedure: Lab 11.3.
- **Focus concepts:** shuffling, join algorithms, query planning.
- **Mastered when:**
  - You can predict the planner's choice from table sizes.

#### Step 11.C — Skew and stragglers

- **Do:**
  1. Join on a heavily skewed key. Find the straggler, then fix it two
     ways. Detailed procedure: Lab 11.4.
- **Focus concepts:** skew, stragglers, salting.
- **Mastered when:**
  - The fix is visible in the task timeline.

### Phase 3: Bazaar Milestone M11 — "Batch Layer"

> **Goal:** Give Bazaar a reliable, rerunnable batch layer.
> **Deliverables:** Scheduled pipeline, 90-day backfill, reconciliation job,
> batch-built recommendations with atomic swap.

#### M11.1 — Nightly analytics pipeline

- **Do:**
  1. Replace the manual M4.3 export with a scheduled DAG: extract from a
     replica, write Parquet to MinIO partitioned by date, build the star
     schema, refresh dashboards.
  2. Generate 90 days of synthetic history and run a backfill.
- **Focus concepts:** workflow scheduling, ETL, immutable inputs,
  partitioned outputs.
- **Mastered when:**
  - Rerunning any day replaces its output, never duplicates it.

#### M11.2 — Ledger reconciliation job

- **Do:**
  1. Nightly, recompute every balance from ledger entries and compare it
     with the stored balance.
  2. Raise an alert on any mismatch.
  3. Deliberately corrupt one balance and confirm the alert fires.
- **Focus concepts:** batch processing for correctness, derived data.
- **Mastered when:**
  - The injected corruption is caught on the next run.

#### M11.3 — Batch-built recommendations

- **Do:**
  1. Build "similar listings" in batch, either from embeddings or from
     co-purchases.
  2. Write the result to a **new** serving table or index, then swap it in
     atomically.
  3. Ship a deliberately bad version, then roll back by swapping back.
- **Focus concepts:** serving derived data, bulk loading, human fault
  tolerance.
- **Mastered when:**
  - The rollback takes seconds and needs no recomputation.

#### M11.4 — Fault tolerance

- **Do:**
  1. Kill the pipeline in the middle of a run. Rerun it.
  2. Verify the outputs are identical to a clean run.
- **Focus concepts:** batch fault tolerance, idempotent outputs,
  determinism.
- **Mastered when:**
  - Outputs match exactly, by checksum or row-level comparison.

#### Optional deep-dive labs

- **Lab 11.2** — object storage semantics and table formats.
- **Lab 11.5** — killing Spark workers mid-shuffle.
- **Lab 11.6** — workflow DAG and backfills on their own.

---

## Chapter 12 — Stream Processing

### Phase 1: Knowledge Check

1. What is an **event stream**? How do producers, consumers, and topics
   relate?
2. Compare **direct messaging**, **traditional message brokers**
   (AMQP/JMS style), and **log-based message brokers**.
3. Compare **load balancing** and **fan-out** consumption. How does each
   broker type support them?
4. Why can traditional brokers reorder messages on redelivery?
5. How does a log-based broker use **partitions** and **consumer
   offsets**? What limits parallelism?
6. What happens when a log-based consumer falls behind? How does disk
   space affect this?
7. Why is it hard to keep **heterogeneous systems in sync** with dual
   writes? Describe the race condition.
8. What is **change data capture (CDC)**? How do you take the initial
   snapshot? What is **log compaction**?
9. Compare **event sourcing** with CDC. Distinguish commands from events.
10. Explain the relationship between **state, streams, and
    immutability**. What is the "log as source of truth" idea?
11. What are the limits of immutability? Consider privacy deletion.
12. What are typical **uses of stream processing**? Consider complex
    event processing, stream analytics, and maintaining materialized
    views.
13. Distinguish **event time** and **processing time**. What are
    stragglers, and how do you know when a window is complete?
14. Compare window types: tumbling, hopping, sliding, and session.
15. Explain the three **stream joins**:
    - stream-stream (window join)
    - stream-table (enrichment)
    - table-table (materialized view maintenance)

    What is a slowly changing dimension problem in joins?
16. How do stream processors achieve **fault tolerance**?
    - microbatching
    - checkpointing
    - atomic commit
    - idempotence
    - rebuilding state after failure

### Phase 2: Mini-Project — "CDC in a Box"

> **Goal:** Build the smallest possible change-data-capture pipeline and
> break it.
> **Time box:** 2–3 evenings.
> **Deliverable:** A running pipeline and a lab report.

#### Step 12.A — Dual writes vs CDC

- **Do:**
  1. On paper, show dual writes leaving two systems permanently different.
  2. Build PostgreSQL to Debezium to Kafka to a derived store. Make
     concurrent updates and verify convergence. Rebuild the derived store
     from scratch.
  3. Detailed procedure: Lab 12.2.
- **Focus concepts:** dual writes, CDC, initial snapshots.
- **Mastered when:**
  - The CDC pipeline converges every time.

#### Step 12.B — Duplicates on crash

- **Do:**
  1. Kill a consumer after writing but before committing its offset. See
     the duplicate. Fix it two ways. Detailed procedure: Lab 12.6.
- **Focus concepts:** at-least-once delivery, idempotence, transactional
  offsets.
- **Mastered when:**
  - Both fixes eliminate the duplicate.

#### Step 12.C — Windows on paper

- **Do:**
  1. Compute processing-time and event-time windows on 50 out-of-order
     events, with a watermark. Detailed procedure: Lab 12.4.
- **Focus concepts:** event time vs processing time, watermarks, window
  types.
- **Mastered when:**
  - You can explain every difference between the two sets of counts.

### Phase 3: Bazaar Milestone M12 — "Streaming Bazaar"

> **Goal:** Make the event log Bazaar's integration backbone, and add
> real-time features.
> **Deliverables:** CDC pipeline, derived stores fed from the log, fraud
> signals, trending listings, crash-tested consumers.

#### M12.1 — CDC replaces the outbox poller

- **Do:**
  1. Stream the M5.2 outbox table and core tables through Debezium into
     Kafka topics.
  2. Remove every dual write in Bazaar.
- **Focus concepts:** CDC, keeping systems in sync, log-based brokers.
- **Mastered when:**
  - No request handler writes to more than one store.

#### M12.2 — Derived stores from the log

- **Do:**
  1. Feed the search index, home feed, and cache from Kafka consumers.
  2. For each one: delete it, rebuild it from the log, and verify it
     matches.
  3. Add a convergence check to `docs/invariants.md`.
- **Focus concepts:** derived data, log compaction, rebuilding state.
- **Mastered when:**
  - Every derived store can be rebuilt with one documented step.

#### M12.3 — Real-time fraud signals

- **Do:**
  1. Compute signals with a stream processor, for example:
     - more than five transfers out of one wallet in ten minutes
     - logins from new devices in one session window
  2. Use event time and watermarks. Decide how late events are handled.
  3. Publish signals to a risk topic that checkout consults.
- **Focus concepts:** stream analytics, windows, event time, stragglers.
- **Mastered when:**
  - Replaying the same events always produces the same signals.

#### M12.4 — Trending listings

- **Do:**
  1. Count listing views in hopping windows.
  2. Enrich them with listing category using a stream-table join.
  3. Decide what happens when a listing's category changes mid-window.
- **Focus concepts:** stream-table joins, time-dependent joins.
- **Mastered when:**
  - You can state how much state the join keeps and how it's bounded.

#### M12.5 — Effectively-once consumers

- **Do:**
  1. Make every consumer idempotent, or commit offsets in the same
     transaction as its output.
  2. Kill each consumer at random points under load.
  3. Run all invariant and convergence checks.
- **Focus concepts:** fault tolerance, checkpointing, idempotence.
- **Mastered when:**
  - No consumer produces duplicates or gaps across 20 random kills.

#### Optional deep-dive labs

- **Lab 12.1** — log-based vs traditional brokers.
- **Lab 12.3** — log compaction and event sourcing.
- **Lab 12.5** — stream join design on paper.

---

## Chapter 13 — A Philosophy of Streaming Systems

### Phase 1: Knowledge Check

1. Why is **data integration** across specialized tools the central
   problem in many architectures?
2. How does **deriving data** through an ordered log help integration?
   What are the limits of a total order?
3. How do **batch and stream processing** complement each other? How do
   they help with gradual schema evolution and reprocessing?
4. What does "**unbundling the database**" mean? Which database features
   (indexes, materialized views, replication, triggers) map to which
   external components?
5. Compare federated databases (unifying reads) with unbundled databases
   (unifying writes).
6. What does it mean to **design applications around dataflow**? Explain
   the idea of application code as a derivation function.
7. Explain the **write path** vs the **read path**. How does moving the
   boundary between them change caching and precomputation?
8. How can you push **derived state to clients** (for example via
   subscriptions) and make the read path stateful too?
9. State the **end-to-end argument** and apply it to duplicate
   suppression.
10. How can you **enforce constraints** such as uniqueness using a
    partitioned log instead of distributed transactions?
11. Distinguish **timeliness** and **integrity**. Why is integrity usually
    more important?
12. What are **loosely interpreted constraints** and apologies or
    compensating transactions?
13. What does "**trust, but verify**" mean? What role do auditing and
    end-to-end checks play?

### Phase 2: Mini-Project — "Username Registry Without Transactions"

> **Goal:** Enforce a hard constraint using only a partitioned log.
> **Time box:** 2 evenings.
> **Deliverable:** A small working registry and a lab report.

#### Step 13.A — Uniqueness via a partitioned log

- **Do:**
  1. Route claims for each username to one log partition. A
     single-threaded processor accepts the first claim and rejects the rest.
  2. Detailed procedure: Lab 13.5.
- **Focus concepts:** enforcing constraints with logs, total order within
  a partition.
- **Mastered when:**
  - Thousands of concurrent claims never produce a duplicate.

#### Step 13.B — End-to-end request IDs

- **Do:**
  1. Have the client generate a request ID, and check it at the final write.
  2. Inject retries at every hop. Detailed procedure: Lab 13.4.
- **Focus concepts:** the end-to-end argument, duplicate suppression.
- **Mastered when:**
  - No hop's retry produces a duplicate.

#### Step 13.C — Audit job

- **Do:**
  1. Write a periodic check that detects a deliberately injected violation.
     Detailed procedure: Lab 13.6.
- **Focus concepts:** "trust, but verify", integrity checks.
- **Mastered when:**
  - The audit catches the injected violation.

### Phase 3: Bazaar Milestone M13 — "Log-Centric and Self-Verifying"

> **Goal:** Make Bazaar's final architecture coherent, and have it check
> itself continuously.
> **Deliverables:** Final architecture diagram, log-based transfers,
> end-to-end request IDs, continuous audits, live client updates.

#### M13.1 — Final architecture diagram

- **Do:**
  1. Draw Bazaar's architecture with the log as the backbone.
  2. Label every store as system of record or derived, with its consumer.
  3. Mark where total ordering holds and where it doesn't.
- **Focus concepts:** data integration, unbundling the database, ordering
  limits.
- **Mastered when:**
  - Someone new can understand the whole data flow from the diagram alone.

#### M13.2 — Transfers as multi-stage log processing

- **Do:**
  1. Re-implement cross-account transfers without distributed
     transactions: a request ID, a debit stage in the sender's partition, and
     a credit stage in the receiver's partition.
  2. Crash between stages. Verify money is never created or destroyed.
  3. Write an ADR comparing this approach with the M8.5 approach.
- **Focus concepts:** constraints without atomic commit, multi-partition
  processing.
- **Mastered when:**
  - The ledger invariant holds at every crash point.

#### M13.3 — End-to-end request IDs everywhere

- **Do:**
  1. Generate a request ID in the client for every mutating action, and
     carry it through to the final write.
  2. Inject duplicates at the load balancer, API, broker, and consumer.
- **Focus concepts:** the end-to-end argument.
- **Mastered when:**
  - Duplicates at any hop never reach the ledger twice.

#### M13.4 — Continuous verification

- **Do:**
  1. Run audits continuously, not just after experiments:
     - ledger sums to zero
     - inventory never negative
     - derived stores match source, by sampled checksums
  2. Put audit results on the dashboard.
- **Focus concepts:** "trust, but verify", integrity.
- **Mastered when:**
  - A deliberate corruption in any store shows up on the dashboard.

#### M13.5 — Live updates to clients

- **Do:**
  1. Push order status and feed changes to clients over a live connection.
  2. Let a client reconnect after being offline and catch up from its last
     known position.
- **Focus concepts:** extending dataflow to the client, read path vs write
  path.
- **Mastered when:**
  - An offline client catches up with no gaps and no duplicates.

#### M13.6 — Timeliness vs integrity

- **Do:**
  1. Classify each invariant as "must hold in real time" or "may be
     briefly violated, then corrected".
  2. For one loosened constraint, build the apology flow. Example: a rare
     oversell triggers an automatic refund and a message to the buyer.
- **Focus concepts:** timeliness vs integrity, loosely interpreted
  constraints, compensation.
- **Mastered when:**
  - Every invariant is classified, with the business cost of loosening it.

#### Optional deep-dive labs

- **Lab 13.2** — unbundling mapping exercise.
- **Lab 13.3** — shifting the read/write boundary.

---

## Chapter 14 — Doing the Right Thing

### Phase 1: Knowledge Check

1. Why does the book argue that engineers carry responsibility for the
   **consequences** of the systems they build?
2. How can **predictive analytics** cause harm? Explain bias and
   discrimination arising from training data.
3. What does **responsibility and accountability** mean when an algorithm
   makes decisions? What recourse do affected people have?
4. What are **feedback loops** in automated decisions? Give an example of
   a self-reinforcing loop.
5. What is the difference between **data collection** and
   **surveillance**? Why does the book use that reframing?
6. Why is **consent** often not meaningful in practice? What makes
   choice not truly free?
7. What does **privacy** mean as a decision right, beyond secrecy?
8. In what sense is **data an asset and a source of power**? What risks
   come with holding large amounts of it?
9. What lesson does the book draw from the **industrial revolution**
   analogy?
10. What role do **legislation** and **self-regulation** play? What is a
    culture of treating data with respect, and what practices support it,
    such as data minimization and purging?

### Phase 2: Mini-Project — "Personal Data Audit"

> **Goal:** Look at data systems from the side of the people inside them.
> **Time box:** 1–2 evenings, plus waiting for a data export.
> **Deliverable:** A short audit report and your ethics checklist.

#### Step 14.A — Request your own data

- **Do:**
  1. Request a copy of your data from one service you use, through its own
     export feature. List what's inside, including inferred data.
  2. Detailed procedure: Lab 14.3.
- **Focus concepts:** surveillance, consent, data as power.
- **Mastered when:**
  - You found at least three data categories you didn't expect.

#### Step 14.B — Algorithmic harm audit

- **Do:**
  1. Audit one publicly analyzed automated decision system. Detailed
     procedure: Lab 14.1.
- **Focus concepts:** bias, accountability, recourse.
- **Mastered when:**
  - You can explain discrimination through proxy variables.

#### Step 14.C — Your ethics checklist

- **Do:**
  1. Write a personal statement and a five-to-eight-question pre-launch
     checklist. Detailed procedure: Lab 14.5.
- **Focus concepts:** responsibility, self-regulation.
- **Mastered when:**
  - You can apply it to a feature in under ten minutes.

### Phase 3: Bazaar Milestone M14 — "Responsible Bazaar"

> **Goal:** Make Bazaar respectful of the people whose data it holds.
> **Deliverables:** Fraud fairness review, feedback-loop analysis,
> retention purges, end-to-end erasure, data export.

#### M14.1 — Fraud signal fairness review

- **Do:**
  1. Apply your checklist to the M12.3 fraud signals.
  2. Identify proxy variables and who is harmed by a false positive, for
     example legitimate users blocked from checkout.
  3. Add an appeal path for blocked users.
- **Focus concepts:** predictive analytics, discrimination,
  accountability.
- **Mastered when:**
  - Every automated block has a human-reviewable reason and an appeal path.

#### M14.2 — Feedback loops in trending and recommendations

- **Do:**
  1. Draw the feedback loop: trending listings get more views, which makes
     them trend more.
  2. Add a mitigation, such as exploration slots for new sellers.
  3. Define the metric that would show whether the mitigation works.
- **Focus concepts:** feedback loops.
- **Mastered when:**
  - Your metric would actually reveal the loop if it were still there.

#### M14.3 — Retention and minimization

- **Do:**
  1. Revisit the M1.4 data inventory against what Bazaar actually stores
     now.
  2. Implement automatic purge jobs for each retention period.
- **Focus concepts:** data minimization, retention.
- **Mastered when:**
  - Data older than its retention period is provably gone.

#### M14.4 — Erasure through derived data

- **Do:**
  1. Implement "delete my account" across every store: database, search,
     feed, cache, warehouse, and the Kafka topics.
  2. For immutable logs, use crypto-shredding: encrypt personal fields with
     a per-user key, and delete the key.
  3. Run an audit that confirms the user's data is unreadable everywhere.
- **Focus concepts:** right to erasure, immutability vs privacy, derived
  data.
- **Mastered when:**
  - The audit passes for a test user, including logs and backups policy.

#### M14.5 — Data export

- **Do:**
  1. Build "export my data", producing everything Bazaar holds about a user
     in a readable format.
  2. Compare its contents against the data inventory.
- **Focus concepts:** privacy, user rights.
- **Mastered when:**
  - Nothing in the inventory is missing from the export.

#### Optional deep-dive labs

- **Lab 14.2** — feedback loop analysis of another system.
- **Lab 14.4** — privacy-by-design exercise.

---

## Final: Packaging Your Portfolio

Bazaar is now your proof of mastery. Package it so others can see that
quickly.

### Step F.1 — README

- **Do:**
  1. Write a README with what Bazaar is, the final architecture diagram,
     the invariants, and the one-command startup.
  2. Add a "Design highlights" section: five decisions you're proudest of,
     each linking to its ADR.
- **Mastered when:**
  - A reviewer understands the system in five minutes.

### Step F.2 — Evidence index

- **Do:**
  1. Create `docs/README.md` that links every ADR, report, and postmortem.
  2. Make sure these key reports exist:
     - latency baseline and SLOs (M2)
     - index review and search recall (M4)
     - failover drill with RPO and RTO (M6)
     - rebalancing under load (M7)
     - concurrency test results (M8)
     - game-day postmortem (M9)
     - invariant checker under partitions (M10)
     - pipeline backfill and reconciliation (M11)
     - derived-store rebuild (M12)
     - erasure audit (M14)
- **Mastered when:**
  - Every claim in the README links to measured evidence.

### Step F.3 — Chapter-to-evidence map

- **Do:**
  1. Fill in a table like the one below. It's what you'll use in interviews.

| Chapter | Question you can now answer | Evidence in Bazaar |
|---|---|---|
| 2 | "How do you measure performance?" | M2 report, dashboards |
| 3 | "Relational or document?" | M3 ADRs, ledger design |
| 4 | "Why is this query slow?" | M4 index review |
| 5 | "How do you evolve APIs safely?" | M5 contract checks, upgrade drill |
| 6 | "What happens when the primary dies?" | M6 failover drill |
| 7 | "How would you shard this?" | M7 ADR, rebalancing report |
| 8 | "How do you prevent double spending?" | M8 isolation matrix and tests |
| 9 | "What if the network is slow?" | M9 fault catalog and postmortem |
| 10 | "When do you need consensus?" | M10 inventory and checker |
| 11 | "How do you backfill safely?" | M11 pipeline and reconciliation |
| 12 | "How do you keep search in sync?" | M12 CDC and rebuild test |
| 13 | "How do you know your data is correct?" | M13 continuous audits |
| 14 | "How do you handle deletion requests?" | M14 erasure audit |

- **Mastered when:**
  - Every row has real evidence, not plans.

### Step F.4 — Design review talk and demo

- **Do:**
  1. Prepare a 15-minute design review presentation. Structure it as:
     problem, invariants, architecture, three hardest trade-offs, what
     broke and how you fixed it.
  2. Record a 5-minute demo: normal flow, then a live failure with the
     invariants still holding.
  3. Present it to an experienced engineer and invite them to find holes.
- **Mastered when:**
  - You can defend every major trade-off by citing the relevant chapter.
  - The reviewer can't find a correctness hole you hadn't already
    documented as an accepted trade-off.

---

## Progress Tracker

Copy this table into your Bazaar repository and update it as you go.

| Ch. | Title | Quiz score | Mini-project | Milestone | ADRs added |
|---|---|---|---|---|---|
| 1 | Trade-offs in Data Systems Architecture | | ☐ Data Landscape Teardown | ☐ M1 Charter | |
| 2 | Defining Nonfunctional Requirements | | ☐ Latency Lab | ☐ M2 Vertical Slice | |
| 3 | Data Models and Query Languages | | ☐ Three Models | ☐ M3 Data Model | |
| 4 | Storage and Retrieval | | ☐ Tiny LSM Store | ☐ M4 Indexes & Search | |
| 5 | Encoding and Evolution | | ☐ Schema Torture Test | ☐ M5 Contracts & Checkout | |
| 6 | Replication | | ☐ Break Replication | ☐ M6 Replicated | |
| 7 | Sharding | | ☐ Rebalancing Simulator | ☐ M7 Sharding | |
| 8 | Transactions | | ☐ Anomaly Zoo | ☐ M8 Correct Money | |
| 9 | The Trouble with Distributed Systems | | ☐ Lease & Fencing | ☐ M9 Chaos | |
| 10 | Consistency and Consensus | | ☐ Consensus Playground | ☐ M10 Coordination | |
| 11 | Batch Processing | | ☐ Logs to Insights | ☐ M11 Batch Layer | |
| 12 | Stream Processing | | ☐ CDC in a Box | ☐ M12 Streaming | |
| 13 | A Philosophy of Streaming Systems | | ☐ Username Registry | ☐ M13 Self-Verifying | |
| 14 | Doing the Right Thing | | ☐ Personal Data Audit | ☐ M14 Responsible | |
| — | Final | — | — | ☐ Portfolio packaged | |

---

*Tip: after each chapter, paste your Phase 1 answers, your lab report, and
your milestone ADRs into a chat with Claude and ask for a strict review
against the 2nd Edition. Ask: "What did I get wrong, what did I miss, and
what would a senior reviewer challenge?"*
