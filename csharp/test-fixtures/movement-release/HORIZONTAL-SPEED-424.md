# Horizontal momentum, Dash and medium parity (#424)

Status: the translated movement paths match the pinned NTSC cartridge in the
available original-CPU corpus. This audit found no horizontal-physics production
defect. It does not add a technique-specific speed adjustment.

## Cartridge evidence

The comparison source is the Japan/USA NTSC revision-zero ROM, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
The native captures execute the original 65816 routines after restoring cartridge
bytes changed by the native harness; the managed side runs production frame paths.
The pinned native source is `578f90b3cc49557bb70060ad033bb90b8cf8ac50` and the
pinned disassembly is `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

The current build replays these retained numeric-only captures with zero mismatches:

| Matrix | Cases / frames | Horizontal properties exercised |
|---|---:|---|
| Stop on a dime | 96 / 15,360 | walking and Dash release tails, aim stops, base/extra folding, air/water/Gravity |
| Arm pumping | 120 / 14,400 | one-pixel posture motion, slopes, walls, charge and shoulder transitions |
| Downback | 1,360 / 152,320 | turns, compact posture collision, falling/wall-jump/unmorph/damage entry |
| Mockball | 336 / 67,200 | controller-acquired Dash carry, morph timing, landing and tunnel traversal |
| Speedball sequence | 400 / 120,000 | Speed Booster stages, ball transition, retained momentum and adjacent failures |
| Waterball | 162 / 32,400 | suitless water versus Gravity, animation timing, base/extra speed and collision |
| Morph bounce | 256 / 24,576 | grounded/aerial ball transitions, bounce phases and horizontal carry |

That is 2,730 cases and 426,256 exact frame comparisons. Depending on the matrix,
each frame asserts fixed-point X/Y, pose and movement type, base and extra run
components, acceleration mode, vertical state, animation state and collision
witnesses. An endpoint or no-crash result is not accepted as parity.

Additional retained cartridge comparisons cover the parts not isolated by the
seven-command replay above:

- `JUMP-TURN.md`: 176 cases / 31,680 frames across air/water, both facings,
  tapped/held reversal, raised ledges and eleven turn timings.
- `QUICKSAND.md`: 16 cases / 2,880 frames across shallow/submerging sand,
  Gravity/no-Gravity, takeoff, landing and sinking.
- `SPEEDBALL-FAMILIES.md`: 114 original-CPU block contacts plus 96 vertical
  controls, preserving movement while checking Speed Booster terrain reactions.
- `README.md`: 720 short-tap samples and 400 running-release samples across dry
  and submerged movement, plus the host input-catch-up regression.

The full verification executable also passes. Its horizontal gates compare all
492 authored words from the air/water/lava tables, 768 indexed reads, 3,145,728
standalone speed calculations, the exact base acceleration/deceleration recurrence,
ordinary-Dash and Speed-Booster caps, signed comparison boundaries, divisor/clamp
behavior, liquid table selection and momentum cancellation. These expectations are
read from the pinned cartridge or independently compiled from those bytes, not from
wiki speed values.

## Current replay commands

Extract the named CSV from each archive in this directory, build DebugRunner in
Release, and run:

```text
SuperMetroid.DebugRunner --stop-on-dime-comparison-audit ROM stop-on-dime-453-v1.csv
SuperMetroid.DebugRunner --arm-pump-comparison-audit ROM arm-pump-452-v2.csv
SuperMetroid.DebugRunner --downback-audit ROM downback-461-v2.csv
SuperMetroid.DebugRunner --mockball-comparison-audit ROM mockball-469-v2.csv
SuperMetroid.DebugRunner --speedball-comparison-audit ROM speedball-sequence-470-v3.csv
SuperMetroid.DebugRunner --waterball-audit ROM waterball-450-v1.csv
SuperMetroid.DebugRunner --morph-bounce-comparison-audit ROM morph-bounce-449-v3.csv
```

The downback diagnostic now reads the current pose's collision radius at the
native post-alpha comparison boundary. The later managed pose-ownership refactor
made the cached runtime radius intentionally lag at that external boundary; using
it caused 3,281 radius-only false mismatches while every position, pose, speed and
collision field still agreed. This is a test-boundary correction, not a gameplay
change.

## Limits

This is an NTSC revision-zero result. No PAL timing claim is made. Technique-specific
admission rules, enemy boosts, door transitions and visual-only animation artwork
remain owned by their focused tickets even when they consume the same speed words.
Player confirmation is still required before closing the issue.
