# Ridley player recordings (#1266)

These are the three original Snes9x SMV files supplied by the playtester for [issue #1266](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1266). They are behavior references, not verified examples of a specific port divergence. The files were copied without modification.

| File | SMV version | Recorded frames | SHA-256 |
| --- | ---: | ---: | --- |
| `Ridley fight showcase.smv` | 5 | 10890 | `7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66` |
| `Ridley fight showcase 2.smv` | 5 | 7164 | `CDC0BE9A956234CAE9C59B9F9C0C471F9E6FC821ED0CB83F1CCFDF79D97A04D1` |
| `Ridley fight showcase 3.smv` | 5 | 5035 | `A18F8CB7DC42420117A98F0C2BB0687807A86AC91AC2C0F9AADCA108C65CBBBC` |

## Native opening trajectory / acceleration regression

`opening-native.csv` contains numeric expectations from movie 1, frames 375–744.
Frame 375 is the destination-ready start of gameplay in room `$B32E`.
The regression runs the production enemy dispatcher and instruction machinery,
feeding the recorded Samus pose/position and random values to isolate Ridley's
trajectory. It asserts function, both positions including subpixels, and both
velocities on every frame. It is not an independent whole-game input replay.

The first pre-fix divergence is frame 707: horizontal velocity `$FFD6` in the port,
`$FFD5` on the cartridge. `$A6:D5D8..D611` chains ADC/SBC carry/borrow; the original
port combined the instructions into integer arithmetic. The same chain applies
to Y at `$A6:D559..D5A5`. The bug dates to `d486f1cacbf1df242959ea04d1a22d6989723d75`
(2026-08-29), the initial Lower Norfair combat translation.

Source cartridge SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Assembly reference: InsaneFirebat revision
`362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank A6.

Capture used Snes9x 1.60 source `913b75d07c6e8d54e966e2c4a79d7c55428007df`,
its libretro host, and a no-window console wrapper calling `S9xMovieOpen` on the
original unmodified movie and `retro_run` until playback ended. WRAM was captured
before requested movie frames and after terminal playback. The complete 10,890-frame
movie reached room `$B32E`, Ridley function `$C600`, health zero. No speed hacks or
port-derived expectations were used. The only compatibility source edit made two
configuration comparators const for the current C++ compiler.

The older supplied 1.51 capture executable cannot read this movie: SMV v5 embeds
snapshot version 11, while that executable supports SMV v4 / snapshot 1510.
ROM, snapshots, and full WRAM dumps stay in ignored private diagnostic storage.
Only these selected numeric fields are published.

A separate target-arrival mismatch starts at frame 734, tracked in #1268. The
cartridge calls `$A9:EF06`: compare absolute center separation against the sum of
actor radius, target radius, and one. The port used only the target radius and
excluded the touching edge. With actual Ridley radii 8/8 and target radii 8/8,
frame 733's position (74, 270) already overlaps target (64, 256). On the next frame
the cartridge enters `$B6DD`; the port previously remained in `$B2F3`.

The regression was extended only after recording this distinct failure. It now
confirms the exact transition and ten following frames through 744. Separate edge
assertions cover the shared helper's existing 2-, 4-, and 8-radius callers. The
center-only check originated in the Ceres translation `9d81b919c4` (2026-08-27),
then became shared with Norfair in `d486f1cacb` (2026-08-29).
