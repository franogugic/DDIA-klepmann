# Chapter 2 questions

## 1. In the social-network **home timeline** case study, explain: the read-time approach (query on demand), the write-time approach (fan-out, materialized timelines), and why the right choice depends on the follower distribution.

Read-time approach: every time a user opens the app, the system runs one database query that covers all the accounts the user follows. First it finds who the user follows (for example 200 accounts), then it fetches the newest posts of each of them and sorts them by time. If a new post must be visible within 5 seconds, this query has to run every 5 seconds for every active user, so the number of queries per second becomes very large.

Write-time approach: when someone publishes a post, the system writes it into the timeline of every user who follows that account. This way every user has their own materialized timeline, so reading is much faster. The cost moves to writing: one post causes many writes (fan-out).

There is no universally correct choice. For accounts with few followers, it is a good solution to write the post into the followers' timelines at once. But some accounts have tens of millions of followers, and for them writing into every timeline is not a good solution. The best option is a combination of both approaches. That is why big social networks often treat celebrity accounts differently from normal accounts.

## 2. What happens to fan-out-on-write for an account with tens of millions of followers? What hybrid approach handles it?

Fan-out on write works very well for accounts with few followers, because writing the post into their timelines is cheap and fast. But when an account has tens of millions of followers, one small request (publishing one post) causes 10 million writes into the timelines of different users. This is very expensive and takes a long time.

The hybrid approach solves this problem. For most accounts, the post is still written into the followers' timelines at write time. For celebrity accounts, the post is not copied into millions of timelines, but is stored separately. When a follower opens the page, the system takes their ready-made materialized timeline and merges it with the newest posts of only those celebrities that this user follows.

## 3. Distinguish **response time**, **latency**, and **service time**.

Response time is what matters most to the user: the total time from the moment the user sends a request to the moment they get the response.

Service time is the time during which the request is actually being processed.

Latency is the time during which the request is not being processed, but is waiting to be processed. It includes network delays and waiting in a queue (queueing).

The relation between them: response time = service time + latency.

## 4. Why are **percentiles** (p50, p95, p99, p999) better than the **mean** for describing performance?

The mean gives the average waiting time of all users. This looks good at first sight, but it does not show what the experience is like in extreme cases and how bad it really is. Percentiles give this insight.

The median (p50) is the time below which 50% of requests were handled, while the other 50% took longer.

P95 means that 95 of 100 requests were handled below this time, and 5 of 100 were not.

P999 (the 99.9th percentile) means 1 request in 1000. These slow requests often belong to the most valuable users, because such users usually have the most data and the most requests, so they hit the slow ones more often.

On the other hand, we need to find the limit up to which it makes sense to go with percentiles. Amazon found that optimizing p99.99 does not bring them profit, especially because at such high percentiles things outside their control often have an influence.

## 5. What is **tail latency amplification**? Why does it get worse when one request fans out to many backends?

Tail latency amplification happens when one request needs several actions, for example calls to several databases or external APIs. The request is as slow as its slowest action. For example, if there are 7 actions, and 6 of them take less than 150 ms but one takes 600 ms, the whole request takes 600 ms and looks slow to the user, even though everything else is very fast.

This effect gets worse when a request fans out to many backends. Even if each single backend is slow only rarely, for example in 1% of calls, when the request waits for many of them, the probability grows that at least one of them is slow. That is why a bigger share of user requests turns out slow than you would expect from the number for one backend. Network communication between servers slows the request down even more.

## 6. What is **head-of-line blocking**, and how does it distort measurements?

Head-of-line blocking is when one slow request holds the queue, so even fast requests behind it have to wait. They are fast by themselves, but their waiting time in the queue grows because of the slow request, so all of them look slow.

This distorts measurements because the server usually measures only processing time, not waiting in the queue. So on the server, fast requests look fast (for example 10 ms), but the user actually waited much longer. That is why response time should be measured on the client side, from sending the request to receiving the response.

## 7. How should you measure response time from the **client side**? What goes wrong if the load generator waits for each response before sending the next request?

Response time should be measured on the client side: from the moment the client sends the request to the moment it receives the whole response. This way network delay, queueing, and processing are all included, not only the processing time on the server.

If the load generator waits for each response before it sends the next request (closed-loop), a problem appears: when the server stalls, the generator stalls with it and does not send the requests that would in reality arrive in that period and be slow. So most of the slow requests are never recorded, and the tail looks much better than it really is. This is called coordinated omission.

The correct way is to use a generator that sends requests on a fixed schedule (open-loop), regardless of the responses, and to measure time from the moment the request should have been sent.

## 8. Define **SLO** and **SLA**. Give an example of each using percentiles.

An SLO (service level objective) is a document that describes the expected performance of a system. Example with percentiles: median (p50) 150 ms, p95 300 ms, p99 800 ms, and p999 2 s. An SLO can also be written for availability, for example that 99.9% of requests return a response without an error.

An SLA (service level agreement) is a contract that defines what happens if this performance is not met. Example: if p99 goes above 800 ms, part of the fee is returned to the customer.

## 9. Distinguish a **fault** from a **failure**. What is **fault tolerance**, and what is a **single point of failure**?

A fault is when one component of the system deviates from its expected behavior. A failure is when the system as a whole stops providing the service to the user. The same fault can stay just a fault or become a failure, depending on whether there is a replacement for that component and whether the work can be moved to others.

Fault tolerance means that a system can handle the fault of one component because something else takes over its work, so the fault does not turn into a failure. This is especially common in the cloud: if one instance goes down, others take over its work, and some systems even tolerate the loss of a whole data center.

A single point of failure is a part of the system that has no replacement and whose failure can bring down the whole system. A common example is a load balancer, if there is only one.

## 10. Compare hardware faults, software faults, and human error. Which are correlated, and why does correlation matter?

Hardware faults are part of everyday life. Magnetic disks fail in about 2–5% of cases per year, and SSDs in about 1–2%. Other components, such as the processor or the power supply, can also fail, but this is much rarer. In large systems hardware failures happen every day, and we should expect them and prepare for them from the start. Hardware faults are mostly uncorrelated: the failure of one part usually does not mean that the neighboring part will fail too.

Software faults are harder because they are correlated. The same software runs on many instances, so the same bug can hit all of them at once. An example is the leap second, which made systems crash.

Human errors are also correlated. Studies show that about 75% of errors come from configuration changes, and a configuration is often applied to all servers at once. People make mistakes even when they want to do something right.

Against correlated faults, these things help: thorough testing, monitoring of metrics in production (so the error is noticed quickly), and the ability to roll back quickly to the previous version (rollback), especially for configuration changes, which are good to introduce gradually.

Correlation matters because redundancy (several copies) protects well against uncorrelated faults: if the disk on one server fails, the other two still work. But when a fault is correlated, for example the same software bug or the same bad configuration, it hits all copies at once, so redundancy does not help.

## 11. What is a **blameless postmortem**, and why does the book favor it?

A blameless postmortem is a philosophy, or an approach to analysis after an incident, that does not blame an individual for the mistake. The mistake is usually not intentional, and blaming gives nothing good. So the responsibility is "moved" to the organization, because in the relationship between developing features and testing, priority is often given to developing and finishing features.

The book favors it because the goal is that a person can freely say what happened and why, without fear of being judged. This way the organization gets all the data it needs, learns from the event, and can prevent it from happening again. If people were punished, they would probably keep details to themselves and the organization could not learn.

## 12. How do you **describe load**? Why is "requests per second" often not enough?

Load is described with load parameters, and they depend on the system. For Bazaar, these would be the number of listings created per second, the number of listing reads per second, the number of new accounts per second, the read/write ratio, and the number of concurrently active users. Throughput and response time are not load but the performance of the system (the output).

Requests per second are often not enough because they do not tell how heavy each request is. 10,000 requests of 1 KB per minute is not the same as 3 requests of a hundred gigabytes in 10 minutes. Also, in the case of the social network study, the same number of posts per second can be easy or hard depending on how many followers the publishing accounts have.

That is why the most important thing is to know the needs of your own application: the expected traffic, the read/write ratio, how much traffic the current system can handle, and how resources can be increased if the number of requests grows, to keep the performance.

## 13. Compare **shared-memory**, **shared-disk**, and **shared-nothing** architectures.

Shared-memory is an architecture in which one machine has several processors or cores that use a common RAM. It scales vertically (scaling up): we increase the resources of one machine, for example with a stronger processor with more cores. Doubling the power of one machine usually costs more than double and does not bring double the performance.

Shared-disk is an architecture in which several machines have their own processors and RAM, but all use common storage over a network, for example a SAN (storage area network). The limits are the shared disk and contention for resources between the machines.

Shared-nothing is an architecture in which, instead of one stronger machine, we use several weaker ones, each with its own processor, RAM, and disk, and they coordinate with each other over the network. This is called horizontal scaling (scaling out). It is the most common form of scaling because vertical scaling is limited: at some point we cannot increase the power of one processor any more.

## 14. What do **operability**, **simplicity**, and **evolvability** mean, and how do you design for each?

If software is good enough to live for several years, it will surely need maintenance and changes, because use cases, user needs, and load change. That is why there are three principles to follow when building a system.

Operability makes it easier for the organization to run the system, even after the programmers who built it have left. Its characteristics are clear documentation, software that is not tied to one machine, clearly defined rules of behavior that an administrator can override when needed, observability and monitoring of metrics in production, and predictable behavior and prevention of errors.

Simplicity matters because other programmers will inherit the system, so the code should be simple and maintainable. The goal is to remove accidental complexity, which we add unnecessarily through our implementation, and to keep essential complexity, which the problem itself requires. The main tool for this is abstraction, for example a high-level programming language that hides machine code, or SQL that hides how data is stored.

Evolvability is how easy it is to change the system when needs change. It is achieved with simplicity and good abstractions, automated tests, compatibility when changing schemas and APIs, and small, gradual changes. This way future programmers can easily add new features.
