# Ridley death acceleration argument mix-up (#591)

## Source and reproduction

Developer-discovered at 67459c38 while tracing the remaining #547 parity question.
Pinned bank A6 `MoveRidleyToDeathSpot` at $C601 loads Y=0 ($C60E), then A=$10
($C611), and calls `RidleyDeathSpotAcceleration` at $D526. The pinned C equivalent
is `Ridley_Func_104(0, 0, 0x10, rect.x, rect.y)`. The table index and extra
deceleration are separate arguments.

Both port callers ($C538 approach and $C551 roar) instead passed index 16 and
default extra deceleration zero. Before the lookup migration this read the $B9
opcode immediately after the table; the migration preserved it temporarily.
That was a port mistake, not a cartridge overread to retain.

`Program.RidleyDeathAcceleration.cs` executes both real phase helpers without a
bus, across nine signed distances and seven initial velocities. The independent
expected calculation uses divisor 16 and extra reversal deceleration 16.
Before the production correction, full Verification failed on the first case:
expected 64312 (-1224), actual 64266 (-1270). The fixture also checks pinned ROM
argument opcodes/bytes and preserves the shared movement integration boundary.

## Fix

Both phase callers now use named DeathDivisorIndex=0 and DeathReversalBoost=16.
Removed the obsolete adjacent-opcode exception from the compiled catalog.
The shared acceleration algorithms and ordinary-fight callers are unchanged.

## Verification

- Both death callers: 126 cases, reversal/same-direction/zero distance and limits.
- Shared inertia: all 32 native bytes and 2,097,152 real two-axis calls.
- Complete Norfair Ridley audit: reveal, 4,096 combat frames, damage, grab,
  breakup actors, drops and persisted defeat; death now completes in 738 frames
  in this fixture instead of 896 with the wrong parameters.
- Complete Ceres Ridley audit and Windows Release build.
- Full Release Verification.

The exact native arguments are proven by pinned source and ROM bytes; the
synthetic expected values are not presented as a full emulator death recording.
Player confirmation remains pending. #547's broader migration remains open.
