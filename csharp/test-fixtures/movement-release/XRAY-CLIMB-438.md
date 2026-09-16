# X-Ray Climb embedding and prerequisites (#438)

Source: [X-Ray Climb](https://wiki.supermetroid.run/X-Ray_Climb), revision 9951.
The article explicitly marks its depth measurements as provisional. This audit
therefore checks cartridge routines and composed collision behavior; it does not
turn the article's one-through-sixteen descriptions into gameplay constants.

## Native stand-up primitive

`xray-climb-438-v3.zip` contains 384 original-CPU records. The decompressed CSV
SHA-256 is `215B1880DD5E235F967214B6393C70219089D3594B31788221EB407FA5295DCE`.
The matrix covers:

- both final facings;
- air, suitless water and lava/acid physics;
- every nominal door embed depth from one through sixteen pixels; and
- crouched turn, stable crouched X-Ray, standing turn and stable standing X-Ray.

The probe executes `$91:E2AD` (`ResponsibleForXrayStandupGlitch`) on the original
65816 CPU, followed by the ordinary `$90:EC22` radius publication boundary. It
compares exact fixed-point X/Y, final pose, radii, facing and movement type with
the production X-Ray activation/turn/teardown path.

All 384 cases match. Every crouched-turn case changes radius 16 to 21 and moves
Samus's center upward exactly five pixels. All three controls retain Y. X, embed
depth and medium are untouched. This proves the important architecture: X-Ray
Climb is not a room-specific collision bypass. Teardown classifies turning type
`$0E` as standing, expands the body, and applies the unsigned radius difference
directly. Door, slope, gate and enemy behavior comes from ordinary systems before
or after that primitive.

```text
SuperMetroid.DebugRunner --xray-climb-audit ROM xray-climb-438-v3.zip
```

Regenerate by applying `native-xray-climb-entrypoint.patch` to the pinned native
tree, building Release x64, and running:

```text
sm.exe --diagnostic-xray-climb ROM NEW.csv
```

The native hook is bounded, runs before SDL, and creates output exclusively. It
contains no ROM, SRAM, player recording or rendered asset.

## Composed prerequisite coverage

The surrounding mechanisms are already retained as independent original-CPU
comparisons. They remain separate because combining every route would obscure
which native owner produced an embed:

- `DOOR-ALIGNMENT-448.md` verifies all four door directions, exact camera-alignment
  calls, transition completion, and retained Samus subpixels/momentum.
- `HORIZONTAL-SPEED-424.md` verifies Dash carry, turns, release tails, liquid
  restrictions, Mockball/Speedball carry and exact fixed-point motion.
- `DAMAGEBOOST-MATRIX.md` verifies damage-entry velocity, direction, hurt pose and
  collision state through 616,608 original-CPU samples.
- the shinespark captures verify launch/storage, dry/water/corrosive/sand travel,
  surface crossings, collision and energy boundaries.
- `GATE-GLITCH-403.md` verifies blue/green gates in both orientations and moving
  success/adjacent-failure windows against the original CPU.
- `FROZEN-GATE-407.md` and the frozen-enemy pipe fixture verify strict-overlap
  entry, tangency failures, collision priority and frozen/nonfrozen controls.
- the exhaustive shared-slope gates compare 12,288 Samus samples and 55,296
  collision/alignment cases against cartridge slope definitions. The climb adds
  no slope branch of its own.

Thus shallow/deep/very-deep outcomes remain emergent results of exact position,
subpixels, entry momentum and ordinary geometry. No `depth >= N`, room identity,
door direction, or X-Ray-specific slope exception was introduced.

## Revisions and limits

ROM: Japan/USA NTSC revision zero, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

No PAL timing claim is made. Direct/indirect G-Mode and X-Ray release input windows
are tracked separately; this ticket establishes the embed/depth primitive and the
ordinary systems that supply it. Ready for player validation, not automatic closure.
