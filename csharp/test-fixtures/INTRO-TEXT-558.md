# Intro narration contrast investigation (#558)

Affected version: 0.1.1. Reference images remain local under
`csharp/test-temp/issue-triage/intro-text-{emulator,repro}-0.1.1.png`.
Different passages/scales prevent direct screenshot pixel subtraction.

`--intro-text-capture-audit ROM LOCAL_DIRECTORY` reaches the first illustrated
narration through the production intro dispatcher and exports a native 256x224
image plus render packet. At frame 751 the caret starts row two (8,40).
All first 32 displayed palette words match the original intro palette; this is
not evidence that every tile chooses the correct palette.

The missing behavior is the text-glow object handler:

- Native draw-character $8B:884D calls SpawnTextGlowObject ($97F7).
- Eight slots allocate descending, start timer one and palette index zero.
- $9849 updates the glyph rectangle's palette bits through $0000/$0400/$0800/
  $0C00, five frames apart; the last update releases the slot.
- The shared cinematic dispatcher calls HandleTextGlowObjects after cinematic
  BG objects. This preserves a freshly drawn character's first update ordering.
- Palette colors 1/5/9/13 retain bright green $03E0; outline colors 3/7/11/15
  change $0340 -> $0280 -> $0200 -> $0160 (green 26 -> 20 -> 16 -> 11).

Managed IntroCinematicObjectSystem copies the character and advances its caret
but has no text-glow slots or palette-bit updates. Consequently mature glyphs
retain the weak green-26 outline rather than native green-11. This matches the
reported direction of the contrast defect without applying arbitrary sharpening.

Implementation remains pending. Add the native pool/timer/rectangle behavior,
an exact glyph-age palette regression and native-resolution comparison, preserving
debugger-state compatibility. Do not label awaiting validation yet. The supplied
emulator recording's display filter still is not independently identified; do not
claim that recreating its video-processing blur is part of cartridge rendering.
