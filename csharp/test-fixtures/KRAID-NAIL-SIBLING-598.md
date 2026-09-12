# Kraid fingernail coordination (#598)

Developer-discovered at 1876de5c; no player release inferred.

Pinned bank A7:BD67..BD78 chooses the velocity row from the other nail's
vertical velocity. BDEA..BDFA similarly reads the other nail's spawn flag.
The upstream C translation agrees. The port instead read both fields from
the initializing nail, breaking the paired attack coordination.

Before production changes, the real initializer failed the focused fixture:
`Nail launch Y velocity comes from sibling sign; expected 65535, got 1`.
The correction reads the sibling while preserving writes to the current actor.
Current RNG access was already correct and remains unchanged.

The fixture covers both slots, all 65,536 RNG/velocity-sign words, and sibling
flags 0, 1, 2 and FFFF (524,288 initializations). Own velocity and spawn flags
deliberately disagree with the sibling. Assertions check actual launch phase,
X/Y whole velocities, own spawn flag and unchanged sibling fields. Reference
velocities are read through the cartridge pointer tables; RNG advance throws.
The table migration is separate and remains part of #547.

After correction: focused and full Release Verification, the complete Kraid
battle audit and Windows Release build pass. Player confirmation remains pending.
