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

The freeze was requested after the captured frame while paused. It is retained for
follow-up, but reload-to-identical-PNG verification has not yet been performed.
To use it in Snes9x 1.60, name the matching ROM `Reference.smc` and place the freeze
in that isolated emulator's `Saves` directory; F1 loads default slot zero. Do not
overwrite the player's active emulator saves.

## Scope

This provides independent evidence for the historical report about the background
when `1994` appears. It does not yet assert a matched managed capture, all startup
frames, or cartridge correctness of doors, Ceres, Kraid and other scenes. It must
not be used to replace the same-packet GPU/software comparison suite.

Two unsuccessful capture attempts preceded this artifact: a relative ROM path was
not loaded; then the paused screenshot request was not serviced until frame advance.
Neither attempt is counted as reference evidence. Only disposable copies were
removed after preserving this successful capture; original ROM, emulator and player
saves were untouched. This fixture contains private ROM-derived emulator state.
