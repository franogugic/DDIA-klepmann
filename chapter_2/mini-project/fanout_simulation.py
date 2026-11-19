"""Tail latency amplification: simulation of a request that fans out to n calls.

Each call is slow with probability P_SLOW, independently of the others.
The request waits for all calls (parallel fan-out), so it is slow when at
least one call is slow.

The simulated share of slow requests is compared with the formula
1 - (1 - P_SLOW) ** n.

Run:
    python3 fanout_simulation.py
"""

import random

P_SLOW = 0.01
FAN_OUT_SIZES = (1, 10, 50, 100)
TRIALS = 10_000
SEED = 42


def formula(n: int, p_slow: float = P_SLOW) -> float:
    """Probability that a request with n parallel calls is slow."""
    return 1 - (1 - p_slow) ** n


def simulate(n: int, trials: int, p_slow: float, rng: random.Random) -> float:
    """Share of slow requests in `trials` simulated requests with n calls."""
    slow_requests = sum(
        1
        for _ in range(trials)
        if any(rng.random() < p_slow for _ in range(n))
    )
    return slow_requests / trials


def main() -> None:
    rng = random.Random(SEED)
    print(f"{'n':>4} | {'formula':>8} | {'simulation':>10} | {'difference':>10}")
    for n in FAN_OUT_SIZES:
        expected = formula(n)
        simulated = simulate(n, TRIALS, P_SLOW, rng)
        print(
            f"{n:>4} | {expected * 100:>7.2f}% | {simulated * 100:>9.2f}% "
            f"| {(simulated - expected) * 100:>+9.2f}"
        )


if __name__ == "__main__":
    main()
