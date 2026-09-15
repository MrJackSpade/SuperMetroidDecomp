# Boyon freeze report: release state 0

Replacement capture supplied for issue #524 on 2026-09-11, affected version 0.2.0.
Copied from the release application data directory, not the repository debugger directory.
The running application was the downloaded SuperMetroid.Game release.

The player recaptured this state for the second-closest Boyon failing to freeze
when jumping and shooting down/right; the closest Boyon freezes correctly.
Earlier Tourian/elevator captures were rejected and are not reproduction evidence.

Saved UTC: 2026-09-11 20:58:20.075631
Room header: 0xa3ae

Copy verified byte-for-byte. The current build restores this 0.2.0 graph through
explicit field-identity migrations and replays the report deterministically:

- Hold Jump + Aim Down after one neutral frame and press Shoot on replay frame 14.
- The down-right Ice/Wave shot spawns at `(01C3,004F)` and reaches `(0205,0091)`.
- That coordinate overlaps both adjacent eight-pixel-radius Boyons. The already-frozen
  nearer Boyon at X=`01F8` owns the first collision and refreshes from 265 to 399 frozen
  frames; the reported Boyon at X=`0208` correctly remains at zero.
- A same-room right-side control shoots down-left and freezes the X=`0208` Boyon to 399.

This is cartridge behavior, not a port defect. Native enemy/projectile collision at
`$A0:A143` iterates active enemies in population order, does not skip frozen enemies,
deletes a colliding non-Plasma projectile, runs that enemy's shot AI, and returns. The
directional asymmetry follows from which overlapping Boyon the shot reaches first.
