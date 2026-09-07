# Independent Snes9x startup reference (#321)

Captured September 7, 2026 using the approved visible, isolated Snes9x session.
`1994.png` is Snes9x's own unedited 256x224 PNG, not a desktop screenshot or managed
renderer output. It shows red `1994` text on an otherwise black screen.

| Artifact | SHA-256 |
| --- | --- |
| `1994.png` | `A13C7D914853025D79D0F3CBB61C6E249CE0AEFA323EF07FC21E914D602EF35D` |
| `Reference.000` | `8713BFF0A36755B70BDE5CAE14ED4DEA2CAF2DFCA7767116BEBB3331E2F9FCC9` |
| ROM, copied unchanged from repository `Super Metroid.smc` | `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72` |
| Snes9x 1.60 `snes9x-x64.exe` from the player's download | `B9FE59605EB0773A0B50F4166E42984FE7A724FEF949A43A86F4A7B722A550B7` |

The emulator reported checksum OK, NTSC, CRC32 D63ED5F8. It used a newly created
directory without SRAM, cheats or patches. Sound output was muted, not the emulated
sound CPU. The minimal configuration is `csharp/tools/snes9x-reference.ini`.

## Capture procedure

`capture-snes9x-reference.ps1` creates an isolated emulator/ROM copy, launches visibly
with approval, waits eight host seconds, pauses, requests a native screenshot,
advances one frame to fulfill that request, then saves isolated slot zero and closes
its own process. Host startup duration is not deterministic: this command is a
capture aid, not a guarantee of the same emulated frame on every run.

The freeze was requested after the captured frame while paused. Reload was verified
in a second isolated session: pause, F1 load, request screenshot and advance one
frame. The resulting PNG has the identical SHA-256 listed above.
To use it in Snes9x 1.60, name the matching ROM `Reference.smc` and place the freeze
in that isolated emulator's `Saves` directory; F1 loads default slot zero. Do not
overwrite the player's active emulator saves.

## Scope

Tracked separately as [issue #335](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/335).

Follow-up diagnostic verifies explicit R/G/B/A bytes equal the record's memory
layout on every tested frame, ruling out that hash-encoding mistake. The maximum
red value across the whole managed YearText phase is 198, so no frame in that phase
reaches the reference's red 255. Production rendering has not been changed.

`RenderVerification --reference-startup` is a separate, currently failing independent
reference diagnostic, not part of the GPU/software parity suite. It searches only
the managed YearText phase for the decoded reference RGBA hash
`6F57F4B36C71E9112DE91312B6EDAAD0004D676C4C38B9EB61F0D399C88771A8`.
No exact match was found. Its last YearText frame differs in 152 red glyph pixels:
the emulator reference has R=255, managed has R=198, with matching positions and
otherwise black background. This observation does not yet isolate whether the
cause is palette, phase timing or another native-state difference. Do not change
the reference to make it pass. The diagnostic writes its last managed image on
failure. Native startup frame timing is not inferred from the host delay.

This provides independent evidence for the historical report about the background
when `1994` appears. It does not yet assert a matched managed capture, all startup
frames, or cartridge correctness of doors, Ceres, Kraid and other scenes. It must
not be used to replace the same-packet GPU/software comparison suite.

Two unsuccessful capture attempts preceded this artifact: a relative ROM path was
not loaded; then the paused screenshot request was not serviced until frame advance.
Neither attempt is counted as reference evidence. Only disposable copies were
removed after preserving this successful capture; original ROM, emulator and player
saves were untouched. This fixture contains private ROM-derived emulator state.
