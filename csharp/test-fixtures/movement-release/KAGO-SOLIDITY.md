# Kago platform solidity boundary (#455, partial)

This capture isolates original CPU routine `$A0:A8F0`, the shared directional
solid-enemy probe. It is **not** a controller-driven moving-platform reproduction
and does not complete #455. It rules out a proposed blanket solidity exception
for turning/morphing: the native probe does not inspect animation, health or
invincibility. It skips an enemy when Samus's current leading edge has already
penetrated it, but stops at a touching edge. Which platform motion/pose timing
creates that penetration still needs a separate integrated comparison.

## Provenance and capture

- Japan/USA revision 0, 3 MiB ROM SHA-256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host `upstream-sm` at `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Cross-check: `upstream-disassembly` at
  `362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank A0 enemy headers and
  `$A0:A8F0-$AB18`. Pose radii come from original `$90:EC22` execution.
- `native-kago-solidity-entrypoint.patch` registers only the explicit
  `--diagnostic-kago-solidity ROM OUTPUT.csv` entrypoint before SDL startup and
  suppresses both explicit SDL error/warning dialogs for that mode.
- `native-kago-solidity-probe.h` uses the bounded original-CPU runner and restores
  the unmodified ROM through the existing movement fixture loader. No translated
  native C collision routine is substituted for CPU execution.
- Two independent captures had identical SHA-256:
  `BC8C4237CCB67A556647F567E6429B0AD71E8A0F9D0FF25152B6143DD95B8C15`.
- `kago-solidity-native-capture.zip` contains the accepted numeric CSV. Temporary
  native host hooks were removed after capture; the reusable patch remains here.

## Matrix and observations

8,640 independent probes, not game frames:

- Four original header radii: vertical Kamer `$D5FF`, horizontal Kamer `$D83F`,
  Kzan top `$DFFF`, Kzan bottom `$E03F`.
- Twelve pose records, both facings: standing, morph ball, standing turns,
  crouches, crouching transitions and standing-up transitions.
- Four movement directions; current leading-edge gaps -2 through +2 pixels;
  zero, half and maximal subpixel positions; requested displacement 2.5 pixels.
- Three explicitly constructed property conditions: solid/unfrozen,
  non-solid/unfrozen, and non-solid/frozen. These are admission controls, **not**
  a claim that every actor naturally uses every property condition. In particular
  retail Kzan bottom is not solid, and its top is solid.

C# compares collision result, accepted displacement whole/fraction, Y fraction
side effect and collided enemy index against every native row. It also requires
2,304 existing-overlap cases to pass through and 1,152 touching cases to stop.
The latter clear Y fraction even for horizontal movement, preserving the native
quirk. Non-solid/unfrozen controls must not collide. All **8,640 match**.

Run after extracting the archive:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --kago-solidity-audit 'Super Metroid.smc' PATH/kago-solidity-455-v1.csv
```

No production fix was needed or made for this boundary. Enemy AI, rider carry,
touch damage dispatch, pose expansion and the controller timeline are outside
this capture. The ordinary-enemy contact comparison in `KAGO-CONTACT.md` covers
damage/knockback separately; combining these tests is not proof that moving
platform Kagos work end to end. Keep #455 open without a validation label until
the remaining integrated cases are verified.
