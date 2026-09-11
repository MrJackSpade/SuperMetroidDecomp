# Grounded Bomb Spread admission/free-flight audit (#414, partial)

Reference: https://wiki.supermetroid.run/Bomb_Spread . The technique description
is a lead; expectations below come from the pinned cartridge tables and bank-$90
`BombSpread` / `ProjPreInstr_SpreadBomb` code, not assumed wiki timing.

Run `SuperMetroid.Verification --grounded-bomb-spread` with the supported
`Super Metroid.smc` in the working directory. The check also runs in full
Verification. No player save or host cheats are used.

The focused production `SamusBombProjectileSystem.StepFrame` fixture starts in a
stable grounded Morph Ball pose at (512,512), with Morph Ball/Bombs and an already
earned charge of 60. Empty constructed terrain isolates projectile integration;
Samus movement, beam charging and the transition into Morph Ball are outside this
fixture. Gravity is explicitly fixed at 0:$1C00, rather than claiming all media.

Hold Down+Shoot for 0, 1, 63, 64, 127, 128, 191 or 192 calls, then release Down.
An additional 192-call case keeps Down held to test forced timeout. Assertions
check charge retention, hold-counter increments, no early allocation, five-slot
creation, charge consumption and counter clearing. Each of the five bombs is
checked for 60 calls, including its spawning call, against independent closed-form
16.16 trajectories and individual ROM fuse timers (2,700 slot observations).
Expected vertical displacement is n*v0 + acceleration*n*(n+1)/2; this tests the
native acceleration-before-integration order and fractional launch velocity.

All cases pass without a production change. This is a source/table-backed test,
not execution of the native CPU or a full controller-driven technique capture.
Do not close #414 or label it awaiting player validation based on this subset.
Remaining: native execution comparison; jump-to-unmorph charge retention;
collision/bounce trajectories; explosion no-self-launch; and complete successful/
rejected admission routes with Power Bomb selection and occupied projectile slots.
The existing bomb-charge rejection matrix is complementary, not a substitute for
those integrated technique cases.

## Retail CPU comparison (2026-09-11)

The subsequent `native-grounded-spread-probe.h` experiment executes unpatched
retail 65816 instructions for cooldown ($90:AC1C), grounded bomb production
($90:BF9D), projectile stepping ($90:AECE), and Samus/projectile overlap
($A0:9785). The shared loader restores the original ROM bytes after native
comparison-harness initialization; no SRAM, player state, or cheats are loaded.
Pinned C source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`; disassembly:
`362be646929cf8e483f692b73a6561cfc2dc1d0d`. Supported ROM SHA256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

Both implementations use a 64-by-128-block constructed room, either empty or
with a solid floor at block row 40. Samus stays at (512,512). The nine hold/release
cases above each continue for 150 calls after their hold period. All five slots
are compared every call: input, charge, hold counter, bomb count, type, integer
and fractional position/velocity, fuse, radii, instruction pointer/timer,
spritemap pointer, and bomb-jump output. **23,080 slot observations match** through
floor bounces, explosion animation and deletion; no production change was made.
Two independent native runs produced identical CSV SHA256
`59CD14A566E717D434D771B52B1EFEEC0417FA95A5957718233BDC868200FA37`.

One representation difference is explicitly excluded only when both slots are
inactive: native `ClearProjectile` ($90:ADB7) leaves `projectile_timers` scratch
memory, while the managed semantic slot zeros it. Bomb Spread initializes this
word at allocation; its active lifetime is compared without masking. Other
fields, including deletion timing and active-slot sentinel, remain asserted.

To repeat locally, apply `movement-release/native-grounded-spread-entrypoint.patch`
to the pinned native checkout (inspect existing edits first), build native
Release/x64, and invoke only its headless command:

```
sm.exe --diagnostic-grounded-spread <absolute-ROM-path> <new-private-CSV-path>
SuperMetroid.Verification --grounded-bomb-spread-native <private-CSV-path>
```

The output path must not already exist. The entrypoint bypasses SDL and suppresses
native error dialogs. Restore only these temporary hooks and rebuild the normal
native executable afterward. Keep ROM and generated traces in private ignored
storage; this repository includes only probe/comparison source.

This remains partial #414 coverage: the fixed Samus is not deliberately placed
over each bomb at the self-launch fuse boundary, so zero overlap output does not
yet prove no-self-launch. Full jump-to-unmorph retention, admission failures with
Power Bomb selection/occupied slots, and wall/slope collision cases remain. This
isolated alpha/overlap fixture does not run movement, room AI, or the full game
loop. Do not mark the whole technique ready based on these passing trajectories.
