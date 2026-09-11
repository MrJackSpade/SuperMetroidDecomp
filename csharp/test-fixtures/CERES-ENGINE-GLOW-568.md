# Zebes approach engine palette regression (#568)

Affected player version: 0.2.0. No private cartridge assets or rendered frames are
included in this fixture.

## Cause and cartridge evidence

The post-Ceres cinematic did not create or run its palette-FX owner. The engine
color consequently remained at its loaded value instead of animating. Pinned
bank $8B:C784 spawns PaletteFXObjects_CutsceneGunshipEngineFlicker ($8D:E1A8)
during Zebes setup. Its $8D:C87A program selects CGRAM byte offset $BE and
alternates white ($7FFF) and black ($0000), one frame each. GameState_37 runs
PaletteFxHandler after the cinematic function and objects, including the spawn
frame. The fix connects the existing RoomPaletteFxSystem interpreter, without
introducing a custom flash, recolored sprite, or screen-space effect.

References: pinned upstream-sm sm_8b.c (578f90b3cc49557bb70060ad033bb90b8cf8ac50)
and upstream-disassembly bank_8D.asm / bank_8B.asm
(362be646929cf8e483f692b73a6561cfc2dc1d0d).

## Reproduction

Run DebugRunner with:

    --ceres-engine-glow-audit "Super Metroid.smc" LOCAL-OUTPUT-DIRECTORY

The audit starts the ordinary CeresDestructionCinematicState and steps through
the entire destruction/approach flow, without seeding a later phase. It reads
the independent ROM palette definition and builds light/dark reference renders
by changing only the engine color in each captured frame. All other memory,
Mode-7 transforms, sprites, layers, and brightness are unchanged. Thus assertions
cover the actual affected pixels at each ship-relative transformed position,
including occlusion/offscreen phases, rather than just checking a timer.

Before the fix, frame 1163 (FlyingTowardZebesA), palette age 493, failed with
1,330 affected pixels: the engine color remained white on a black phase.
After the fix, 1,185 palette frames pass, including 379 visible frames and
56,001 affected-pixel observations. Direct rendering also matches snapshot
rendering. In-memory debugger round-trips during both glow phases retain the
subsequent exact sequence. Output PNGs remain local/private.

This is a ROM-program-derived pixel oracle sharing the production renderer,
not an independent emulator capture or proof of every Mode-7 camera parameter.
Player confirmation remains required.

The complete SuperMetroid.Verification suite passes. The Windows Desktop Release
build also passes with zero warnings and errors.

## Compatibility

New debugger states preserve the palette interpreter. Older states cannot
supply an animation phase that was never captured: loading explicitly warns
that the engine glow restarts at its first phase on the next approach frame.
No SRAM format changes. The synthetic cinematic timeline supplies a terminating
palette program because it deliberately contains no visible artwork; the retail
audit above checks the real alternating program.
