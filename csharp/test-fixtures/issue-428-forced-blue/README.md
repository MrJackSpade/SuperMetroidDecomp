# Forced-state Blue Suit parity (#428)

This fixture compares four physical movement-handler replacements and one adjacent
non-replacement control against the original 65816 CPU. Every case begins from the
same active, uncrashed horizontal shinespark. The translated cases exercise the real
production owner which replaces (or retains) that handler; no post-setup boost word,
pose, palette, or speed is injected.

## Pinned evidence

- Japan/USA revision-zero ROM SHA-256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- `upstream-sm` pin: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly pin: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Accepted numeric trace: `../movement-release/forced-blue-428-v1.csv`, SHA-256
  `1C520EB9D2F5632A922B7463D203ED6377E2612FE0410552392F134E0B6DE5DA`.
- Technique inventory: <https://wiki.supermetroid.run/Blue_Suit_Glitch>.

The committed trace contains no ROM bytes, screenshots, save state, or other private
cartridge asset.

## Cases and boundaries

- `drained-f7`: Super Metroid terminal drain and Mother Brain's rainbow-release
  sequence converge on controller zero plus animation command `$F7`. The probe invokes
  the shared `$91:E4F8 -> $90:8360` physical write; it does not replay either boss route.
- `ceres-ridley`: `$90:E119` replaces only the movement handler. The constructed
  Speed Booster inventory is intentionally impossible in normal Ceres progression, as
  required by the ticket; the native routine itself is room independent.
- `elevator-command`: command seven at `$90:F1C8` restores normal movement/input and
  clears extra run speed while retaining boost counter `$0400`. C# reaches the same seam
  through the real `$A3:D73F` elevator actor and Down input. It does not model the visual
  corruption or traversal required to reach an elevator transition block out of bounds.
- `xray-teardown`: X-Mode can leave the visor HDMA dispatcher alive after an active spark
  owns movement. Releasing Dash eventually reaches `$88:8A08 -> $91:E2AD`; the audit
  runs the translated visor through that real phase-five teardown.
- `reserve-unlock-control`: automatic Reserve completion's command `$10` restores the
  outer alpha/beta pair but does **not** write the movement-handler word. Native and C#
  therefore retain `$90:D106`. This is a failing/adjacent control, not a claim that bare
  Reserve completion produces Blue Suit. The successful R-Mode forced-stand timing is a
  separate remaining sequence under issue #428.

All successful direct interruption cases retain the native stage-four boost counter;
the exact retained/cleared extra-speed and input-handler words are asserted per row.

## Reproduction

Build and run the original-CPU probe:

```text
cmd.exe /d /c "call C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat && csharp\native\DraygonCrystalAudit\build.cmd"
csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" forced-blue
```

Compare the same states through production C# paths:

```text
dotnet run -c Release --project csharp/src/SuperMetroid.DebugRunner/SuperMetroid.DebugRunner.csproj -- --forced-blue-audit "Super Metroid.smc" csharp/test-fixtures/movement-release/forced-blue-428-v1.csv
```

The expected result is five cases and zero mismatches. This fixture covers state and
mechanical retention, not a renderer-specific attempt to preserve incidental graphical
corruption from out-of-bounds elevator travel.
