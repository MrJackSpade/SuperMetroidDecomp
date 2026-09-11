# Mother Brain Hyper Beam audit repair (#578)

Developer diagnostic only; no gameplay behavior changed.

## Reproduction

`DebugRunner --mother-brain-audit "Super Metroid.smc"` failed on both
baseline `71f1c01f` and the signed-math migration, and was reproduced again
before this repair. One hit reduced health to the expected 35000 and selected
`SetupHyperBeamRecoil`, but the audit expected the projectile to be an explosion.

## Cartridge contract

The pinned `upstream-disassembly/src/bank_A0.asm` at $A0:A1F7-A20D
marks a projectile for deletion only if the enemy blocks Plasma or the projectile
lacks its penetration bit. Hyper Beam's produced type $9018 contains that bit.
The pinned `upstream-sm/src/sm_a9.c` at $A9:B507 calls the phase-two/three shot
reaction and then common no-death shot damage. $A9:B5A9 selects recoil and clears
the walk counter on underflow; it does not convert the beam to an explosion.

## Corrected assertion and verification

The audit now requires the projectile to remain active with unchanged type,
direction, position, damage, Hyper Beam pre-instruction, and live projectile count.
It retains the exact health, walk-counter, recoil phase and timer assertions,
including the following enemy turn's neck geometry, attack suppression,
instruction scheduling and brain shake.

The complete Release Mother Brain audit passes through its real-room load,
glass/tube destruction, phase-two attacks, rainbow beam, Baby drain/healing/death,
and produced phase-three Hyper Beam recoil. This establishes the diagnostic
repair, not a new player-visible battle fix or full original-CPU battle replay.
