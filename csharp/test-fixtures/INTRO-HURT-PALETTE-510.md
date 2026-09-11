# Intro Mother Brain hit palette (#510)

The real intro Rinka hit reproduced the reported color reset: on hurt-counter call
two, Samus palette color one became $0108 instead of the intro's $2DAD. Captures
show the sepia scene with a bright gameplay-colored Samus. After the fix, the same
frame retains the intro colors.

Cause: the flashback creates its own Samus instance but did not publish the active
cinematic-function state to the shared Samus handlers. The existing hurt palette
handler therefore took the ordinary equipped-suit restore branch. Native
$91:D8EB checks CinematicFunction and selects the intro palette when nonzero.

Fix: initialize the cinematic flag on flashback Samus. This uses the existing shared
palette branch and its timing; it also preserves native suppression of gameplay-only
sound and landing effects. No palette replacement special case was introduced.

Run DebugRunner:

```
--intro-hurt-palette-audit "Super Metroid.smc" OUTPUT_DIRECTORY
```

The audit navigates the real intro page, lets the ROM Rinka script produce its hit,
and checks all sixteen CGRAM words on each of the three restore calls (2, 4, 6)
against the cartridge intro palette. It fails before and passes after the change.
Before/after rendered restore frames were inspected locally; captures remain
ignored and are not published. Player confirmation remains required.

Windows Release build passes. The wider frontend-parity audit passes its earlier
Rinka movement checks but fails a later Ceres-haze assertion; it is not claimed
passing. That separate haze check operates on an isolated black pixel array, not
this intro Samus. Full Verification passes.
The return-jump report (#511) is not resolved by this palette fix.
