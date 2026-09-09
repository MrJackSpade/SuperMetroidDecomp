# Speedball collision bomb-block contact (#470)

Status: contact defects reproduced and corrected. The full ticket remains open:
broader block-family contrasts and temporary Blue Suit conversion still need
complete sequence coverage. Controller-acquired active boost through Mockball is
now separately covered by [SPEEDBALL-SEQUENCE.md](SPEEDBALL-SEQUENCE.md).
Do not treat the seeded contact counter alone as proof of acquiring or retaining it.

## Native reproduction

`native-speedball-block-probe.h` runs untouched cartridge CPU instructions for
radius setup ($90:EC22) and grounded-ball movement ($90:A521). The C# comparison
calls the real `SamusMorphBallMovement.StepGrounded`, shared block collision and
`RoomPlmSystem`, not a replacement block predicate.

Room is 16 x 32 blocks, floor row 16. Samus starts moving-ball pose $1E/$1F,
Y249 with zero subpixels, X124 right / X132 left. Base speed 3, extra speed 2,
acceleration mode zero, momentum flag one. Morph Ball and Speed Booster are
equipped; health 99; air; no enemies or cheats. The collision target is row 15,
column 8 right / 7 left, original level word $F123. Sweep all eight area-independent
collision bomb-block BTS values (0..7) and counters 0, $0300, $0400. This produces
48 one-call contact cases; no PLM instruction-handler frame has run yet.

CSV compares complete X/Y subpositions, base/extra velocity, live level word,
boost counter, and active PLM count. All 16 active-stage-four cases failed before
the fix: C# clipped to the block, cleared velocity/boost and spawned no PLM;
native passed through, retained velocity/stage and spawned one actor. The other
32 cases correctly remained solid.

## Root causes and corrections

1. Ball movement never supplied the explicit bomb-breaking admission used by
   Screw Attack/Shinespark. Native setup $84:CE83 tests active boost stage
   independently of pose. Shared horizontal **and vertical** bomb-block dispatch
   now reads the owning Samus's live stage in addition to the existing explicit
   admissions. This covers grounded, airborne and morph-transition callers
   without a Speedball-only override or modifying their movement arithmetic.
2. Setup CE83 synthesizes live air-type word $0058 immediately, and preserves
   original collision type with visual $058 in the restore word. C# instead
   cleared only the original word's collision nibble, leaving $0123 visible until
   the later actor draw. The PLM and ownerless setup-only path now write $0058.
   The visual identity is named in `RoomPlmVisualBlockIndexes`; restore composition
   uses `RoomLevelWord`. Three old tests that expected preserved visual bits were
   corrected using this native capture, including Screw Attack and Shinespark.

Post-fix all 48 native cases match. Active stage four accepts the complete
5.C000-pixel horizontal movement, retains base 3.C000 / extra 2.0000 and boost
$0400, writes $0058 and allocates one PLM. Stage zero/three remain solid, retain
$F123 and allocate none. The ordinary mover cancels their speed after collision.

An additional 48 focused production vertical scans cover both vertical directions,
all eight BTS values and the same three stages. They assert full accepted movement
and immediate mutation/allocation for stage four, solid/unchanged/unallocated
otherwise. These vertical assertions are synthetic checks of the shared rule,
**not** separate native vertical traces. The remaining ticket work must not be
inferred from them.

## Capture and replay

Include `native-release-probe.h`, then `native-speedball-block-probe.h`, after
`StateRecorder` in `sm_rtl.c`. Temporarily dispatch
`DiagnosticSpeedballBlocks(romPath, newCsvPath)` before SDL. The shared loader
restores the unpatched cartridge bytes; output creation refuses overwrites.
Remove temporary hooks after capture. Player SRAM/debugger slots are untouched.

`speedball-block-native-capture.zip` contains the independently repeated CSV,
SHA256 `3377B142CDD6F388CBBAB1EC1D8A4389CE2774463A13EAE77CDAEF2508310FF0`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --speedball-block-comparison-audit "Super Metroid.smc" path/to/speedball-block-470.csv
```

ROM: Japan/USA NTSC revision zero, SHA256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
Key native predicate/setup: `$84:CE83`; bank-$94 bomb collision dispatch `$94:932D`.
No PAL-specific result is claimed.

## Verification

The complete bank-$80 verification executable passes. The focused audit passes
all 48 native contact cases and 48 synthetic vertical checks. Additional movement
comparisons pass 289,920 frames: Mockball (67,200), ceiling-steering bomb chains
(138,240), impact bounce (24,576), falling morph timing (13,824), and run/jump
morph acquisition (46,080). Temporary native capture hooks were removed, and the
archived CSV payload hash matches the independently repeated capture.
