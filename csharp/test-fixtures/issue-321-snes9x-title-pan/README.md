# Independent native Mode-7 title-pan reference (#321)

Captured September 7, 2026 with Snes9x 1.60 in the isolated approved session using
`capture-snes9x-reference.ps1 -Seconds 25`. Native screenshot output, not a desktop
crop or a generated managed image. It shows the vertical machinery column and
baby Metroid during the pre-title pan, including clipped content at the viewport.

| Artifact | SHA-256 |
| --- | --- |
| `title-pan.png` | `3EC67A32D45C1E3A327512A6E0438F1A42BEDF89900A72F46ED1B76A0AEF1A91` |
| `Reference.000` | `4039A5651071BC409F9BCA7937294144B18F655713AD5A6E1FA1D7ED01D9720E` |

The unchanged source ROM SHA-256 is
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`;
the emulator executable SHA-256 is
`B9FE59605EB0773A0B50F4166E42984FE7A724FEF949A43A86F4A7B722A550B7`.
It reported checksum OK, NTSC, CRC32 D63ED5F8. No SRAM, patches or cheats were
provided. Configuration is checked in as `csharp/tools/snes9x-reference.ini`.

`Reference.000` was saved after the screenshot's frame advance while paused.
Reload screenshot parity is not yet verified. The 25-second host wait is not an
emulated frame number and must not be used as a managed timing assertion. This is
a preserved independent scene reference, not evidence of byte equality against a
corresponding managed frame or of the entire title pan/zoom trajectory.

Only the isolated test process was closed. Its disposable ROM/executable copies
were removed after retaining these artifacts; player saves and emulator settings
were untouched. Keep this private ROM-derived fixture in the private repository.

## Independent comparison diagnostic (#336)

`title-pan.rgba` is the PNG decoded without alteration into row-major RGBA8 by
`csharp/tools/decode-reference-png.ps1`. Its SHA-256 is
`D3B61C24CF7D6D4FECC0E44CBC29D46A92CD9606EA4787A191151E6EAAAE045C`.
The decoder refuses to overwrite an existing fixture. The comparison checks this
digest before using the data.

`RenderVerification --reference-title-pan` currently fails after searching 2500
managed frames. Closest candidate: tick 1146, 23,205 different pixels. The exact
candidate packet passes GPU/software parity before the native comparison fails.
Failure emits native expected, actual and difference images plus a replayable
packet through the ordinary comparison artifact pipeline.

Native background bands contain green/blue 8, 16 and 24 where managed is black;
other pixels similarly suggest additive green/blue. A missing title gradient or
color-math state is a hypothesis, not a confirmed diagnosis. Track separately in
[issue #336](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/336). Do not
weaken equality or modify the native golden. Closest-image selection is diagnostic,
not proof of native frame/timing correspondence.

## Native source diagnosis

The gradient is absent from the shared title implementation, including its
pre-migration version at `8e0b7b7`, not just from the GPU shader. That version's
Render method composes Mode 7, OBJ and master brightness without the following
native streams:

- `$88:EB58` spawns HDMA objects writing COLDATA ($2132) and CGADSUB ($2131).
- `$88:EB95` supplies CGADSUB $A1 for the first $7A scanlines and $31 afterward:
  subtract on BG1/backdrop above, add on BG1/eligible OBJ/backdrop below.
- `$8B:A00A` selects the fixed-color table at `$8C:BC5D` using the Mode-7 zoom
  high nibble and copies it to the live HDMA table. The `$C1/$C2/$C3` lower-screen
  color writes explain the observed green/blue bands.
- HDMA pre-instructions maintain the table and remove the actors on title reload.

These addresses are verified in the local bank-$88/$8B/$8C disassembly and the
corresponding C reconstruction. #336 must implement native scanline state and
preserve main-screen provenance/OBJ color-math eligibility, not add a flat tinted
overlay. It remains separate pre-existing visual work under #321's scope boundary;
the captured evidence and known defect must remain visible in the handoff.
