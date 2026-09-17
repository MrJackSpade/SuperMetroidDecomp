# Reserve Mode / greyout (#433)

This fixture verifies the cartridge state collision behind Reserve Mode and the
two documented Shinespark-Suit continuations. It deliberately isolates the
room-independent handlers; no retail save, player SRAM, or gameplay cheat is used.

## Pinned sources

- Japan/USA rev 0 ROM SHA-256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- `upstream-sm` pin: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly pin: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Technique reference: <https://wiki.supermetroid.run/Shinespark_Suit#Reserve_Mode>.

The cartridge path is `$82:DB69` (out-of-health dispatch), `$82:DC10`
(automatic Reserve refill), `$88:8A08` (X-Ray phase-five teardown), and
`$91:DCB4` (X-Ray special palette handler). Automatic recovery installs Samus
command `$1B`; completion clears shared freeze word `$0A78`, restores normal
control with command `$10`, and resumes main gameplay. The still-live X-Ray HDMA
object then reaches `$88:8A08`. That routine performs all cleanup only when
`$0A78` is nonzero, so the cleared-word case leaves the greyout object installed.

## Native reproduction

Build the original-CPU audit, then run its `reserve-mode` matrix:

```text
cmd.exe /d /c "call C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat -arch=x64 && call csharp\native\DraygonCrystalAudit\build.cmd"
csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" reserve-mode
```

`reserve-mode-433.csv` is the accepted output. The success row begins with a
cleared shared freeze word (`0`) and retains HDMA channel bit `$04`, X-Ray phase
five, and special-palette handler eight. The adjacent ordinary-X-Ray control
begins with a set freeze word (`1`) and deletes the channel and phase. Both rows
start in standing pose `$01`, facing right, at `(256,256)`, with ordinary Samus
handlers `$E695/$E725`; there is no controller input at this teardown seam.

The native audit starts at the exact room-independent state produced by damaged
door contact reducing Samus to zero while X-Ray and automatic Reserve recovery
overlap. Door geometry and the particular damage source do not participate after
`$82:DB69` publishes state `$1B`, so they are excluded from this deterministic
fixture rather than approximated.

## C# production-path verification

Run:

```text
dotnet run --no-restore --project csharp/src/SuperMetroid.Verification/SuperMetroid.Verification.csproj -- --reserve-mode csharp/test-fixtures/movement-release/reserve-mode-433.csv
```

The managed cases use a 16-by-16 empty room, standing pose `$01`, position
`(128,128)`, zero subpositions/speeds, X-Ray equipped, automatic Reserve mode,
and no cheats. The success case begins with zero energy and five Reserve energy,
runs the production automatic-refill state to exhaustion, then advances the real
X-Ray HDMA state to phase five. It asserts refill, normal-control restoration,
retained HDMA/greyout ownership, retained phase five, and absence of the normal
X-Ray teardown sound. Its adjacent control performs an ordinary frozen X-Ray
teardown and asserts that the object is deleted.

From the retained greyout state, one case enters Crystal Flash with
Down+L+R+Shoot, ten missiles, ten super missiles, and ten power bombs. It asserts
that Crystal Flash replaces the shared palette handler while greyout survives.
The second continuation uses the real crouch posture transition with an active
speed-booster counter and asserts stored-shine state, stored-shine palette
ownership, and continued greyout. Blue Suit interruption is intentionally owned
by linked issue #428 rather than duplicated here.
