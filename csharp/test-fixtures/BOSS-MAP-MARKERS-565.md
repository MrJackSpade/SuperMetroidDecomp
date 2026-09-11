# #565 boss map marker audit

Affected version: 0.1.1. Partial implementation fix; wider validation remains open.

## Reproduced pause omission

Pinned bank82 DrawMapIcons ($B672) and DrawFileSelectMapIcons ($B6DD) both
call DrawBossMapIcons ($B892), using the same lists at $C7CB. The port's
file-select renderer had this routine, but PauseMenuState.PrepareRenderOam
called only the player indicator. No boss sprites reached pause OAM.

The regression creates the actual pause renderer using each of the four native
boss records, with its downloaded area map. It centers the authored coordinate
through the scroll fields, renders alive, changes only that boss bit in the
shared progression owner, and renders again. Before the fix the first record
(Brinstar bit0) produced identical pixels. After the fix all four records have
changed pixels confined to the centered marker rectangle (within16 pixels).
No debug map reveal is enabled. Tests change the progression byte directly;
they do not claim to replay four battles or save/reload through the frontend.

The shared boss drawing method is now called after the player indicator on the
pause map, preserving native draw order. File select calls the same method as
before. Native logic retains the map-download gate, defeated cross overlay and
palette change, unused-record bit shifts, and absence of invented markers.
Equipment pages are unchanged. No new serialized pause owner was introduced.

Run Verification `--pause-boss-markers`; it is also part of the standard suite.

## Still required

Independent native OAM/pixel comparison of both display states and visibility
conditions, first subsequent opening, area/room re-entry and real save/reload
persistence, and confirmation of map-surface differences. This initial pixel
regression proves the missing pause invocation, not the complete #565 contract.
Keep the issue open without awaiting-player-validation until those checks finish.
