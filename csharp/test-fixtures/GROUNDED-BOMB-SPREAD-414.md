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
