# Right-facing gate-glitch parity (#404)

This fixture establishes cartridge parity for the two documented vanilla
right-facing gate setups without replacing either setup with synthetic terrain or a
direct switch call. Both probes use the project ROM revision, authored room level
data, authored PLM populations, ordinary Samus movement, ordinary Super Missile
motion, and the production downward-gate projectile request path.

## Pink Brinstar Hopper Room

- Room header: `$8F:A130`
- Room dimensions: 32 by 32 blocks
- Downward gate block: `$0091` (world pixel 272,64)
- Shot-block argument: `$0002` (blue, right-facing)

The original-CPU probe exhaustively tests 24,768 stationary horizontal Super
Missile origins: X=80..271 and Y=32..160. Ten origins activate the actual gate
switch. Every other origin is a negative control. The managed runtime must reproduce
the complete positive list in `movement-release/right-gate-404-native.csv`; omitted
matrix rows must remain negative.

## East Tunnel frozen-enemy setup

- Room header: `$8F:CF80`
- Room dimensions: 64 by 32 blocks
- Downward gate block: `$0156` (world pixel 352,80)
- Shot-block argument: `$000A` (green, right-facing)
- Frozen actor: the room's real Boyon, radius 8 by 8, at world Y=139

The setup was transcribed from the published pause-assisted demonstration. Samus
starts at X=238.8191/Y=139 with Hi-Jump, Ice, and Wave, holds right+dash through
the 31 gameplay samples accepted during pause darkening, then holds
jump+dash+aim-up after gameplay resumes. The matrix varies the frozen Boyon's X
coordinate from 363 through 365 and the resumed Super Missile frame from 4 through
6. The original CPU produces exactly one activation: Boyon X=364 and shot frame 5,
with the gate switch activating on frame 7. The other eight records are adjacent
position/timing failures, not arbitrary controls.

`movement-release/east-gate-404-native.csv` records the original-CPU result. The
managed verifier repeats all nine cases through `SuperMetroidRuntime.StepFrame` and
requires exact agreement in switch state, activation frame, Samus fixed-point state,
pose, and retained projectile impact coordinates.

## Evidence boundary

The East Tunnel fixture seeds the Boyon at the frozen position shown in the
demonstration; it does not reproduce luring and freezing the enemy. The tested
property is the gate-glitch trajectory and switch activation after that setup. The
private native seed used to transport authored room collision data is intentionally
not published because it derives from the ROM. The checked-in CSV files contain only
mechanical measurements and no graphics, audio, or ROM byte ranges.
