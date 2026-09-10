# #450: actual liquid-entry Morph Ball trajectories

## Reproduced defect and fix

Movement-medium and animation-FX selection sampled the exclusive `Y + radius`
boundary. Original $90:EC3E reads the pose radius, adds Y, then decrements before
returning the occupied bottom pixel. At exact surface equality, managed physics
and animation became submerged one frame early. This changed friction/gravity,
animation timing, and lava's boost cancellation, then accumulated position error.

Before the correction: 11,067 frame mismatches. Correcting only animation FX left
10,983. Correcting both production callers to use `BottomPixel` gives **32,400
frames / zero mismatches** across 108 independent trajectories.
The exclusive collision boundary is retained for callers with different native
semantics; this is not a global subtraction from every collision calculation.

Older standalone liquid tests had treated equality as submersion. Their wet
fixtures now place the surface one pixel higher, and an explicit equality test
asserts dry movement. Their original cancellation, damage and splash assertions
remain, including the splash's actual new surface coordinate.

## Provenance

- Reference: https://wiki.supermetroid.run/Morphball_bounce, revision 7711.
- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `4826266B38AC11C60A7556B8DA35385DAC9736FAE3DEE00192113164FCFEE4EC`.
- Two independent native captures are byte-identical.

Cross-check $90:EC3E's DEC in pinned bank_90.asm and the $90:8000 FX dispatch.
The comparison executes original CPU instructions, not translated native wrappers.
It uses the production C# StepFrame plus its normal audio publication callback.

## Actual setup and inputs

144x80-block synthetic room, upper runway row 32, basin floor row 40, liquid
surface Y560, outer walls. The rightward runway ends at column 56; the leftward
mirror begins at column 88. Starting positions X128/X2176, Y491, at rest.
Morph Ball and Speed Booster only, no Gravity Suit. Health is 1499 in both
implementations to survive the bounded acid trajectory; invincibility is off.

Both directions, water/lava/acid, 64/96 running frames, and nine morph timings
produce 108 cases of 300 frames. Hold forward+dash until launch. Jump while
holding forward; release jump on launch+8 then hold it again. First Down is
launch+10; second Down is launch+20 plus 0/2/4/.../16 frames. Release forward
between Down presses, then hold it again. No counters or medium flags are forced.
Liquid state is sampled from the actual surface and evolving position. Some
short-run/late-morph controls lose their carry before reaching the basin.

The CSV compares X/Y including subpixels, pose/type, animation/timer, base and
extra speed, acceleration mode, boost, bounce, Y speed/direction and liquid flag
on every frame. Input timeline, hash, matrix order and frame count are enforced.

## Explicit trajectory witnesses

The 96-frame run with second Down at launch+32, in both directions:

- Frame 146: occupied bottom equals the surface, FX medium remains air.
- Frame 147: liquid entered; base speed is 1.0. Water/acid retain extra 5.9375
  and counter $0401; lava clears both, as its own original FX handler specifies.
- Frame 163: underwater floor contact starts bounce state one.
- Water apex: frame 195, Y=$0269.7FFF; bounce ends frame 233.
- Lava/acid apex: frame 192, Y=$026B.45FF; bounce ends frame 225.
- Terminal Y=$0279.FFFF, bounce state zero and base speed zero.

These properties have dedicated assertions in addition to complete trace
comparison. The lava distinction is intentionally retained rather than treating
all liquids as generic water. Native behavior qualifies the wiki's broad wording.

## Re-run

Extract `waterball-450-v1.zip`, then run:

```text
SuperMetroid.DebugRunner --waterball-audit "Super Metroid.smc" waterball-450-v1.csv
```

To regenerate, apply `native-waterball-entrypoint.patch` in the pinned native
tree, build Release x64, and invoke the bounded, dialog-free diagnostic:

```text
sm.exe --diagnostic-waterball "Super Metroid.smc" NEW.csv
```

Remove temporary native hooks afterward. This verifies pinned NTSC synthetic
air-to-liquid trajectories, not PAL, every terrain route, or all liquid callers.
The issue remains open for player confirmation once the verified fix is published.
