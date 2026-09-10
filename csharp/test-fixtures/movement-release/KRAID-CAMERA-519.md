# Kraid entry-jump camera (#519)

Affected player version: 0.1.1. Camera failure reproduced and missing cartridge
integration fixed; the reported wrapped-body pixels still need focused coverage.

`--kraid-camera-audit ROM` loads the actual incoming door 83:91B6 and room A59F,
places Samus on the lower floor, restores the door's Y=256 camera, and holds Jump
for 35 frames followed by 55 neutral frames. Grounded debug placement itself
adjusts the camera, so restoring the entry position before simulation is required.
The fixture asserts that Samus actually jumps, the initial four scroll bytes,
camera distance index, and both camera Y extrema across the entire sequence.

Before: scrolls [2,2,1,1], camera minimum 226. After: [0,0,1,0], camera stays at
256 throughout. Samus moves from Y=427 to minimum 382 and ends at 395. The corrected
fixture was rerun with just the initializer scroll write temporarily removed and
failed again at Y=226; the production write was restored afterward.

Root cause: InitAI_Kraid's A7:A9E4-A9F4 camera distance and scroll writes were
missing. Growth's C0A1 release only set a diagnostic flag with no runtime consumer.
The initializer and growth now write through the existing required room-scroll
service, exactly once at their respective native phases. Living Kraid supplies
distance index 2 to the camera; an already-defeated load supplies no override.
The values live in KraidCameraDefinitions, not at functional call sites.

The full encounter checks growth's [2,2,1,1] table. The older #268 pixel assertion
is retained unchanged in substance, with an explicit upper-left observer sweep
after growth so its fixed above-head rectangle remains meaningful now that the
camera is correctly restricted before growth. This is a diagnostic viewpoint,
not a claim to have driven a controller-only route through the battle.

Validation: clean DebugRunner build; entry-jump camera test; full Kraid encounter;
900 original-CPU lint records and three runtime rides; core verification suite.
No new original-CPU camera trajectory capture or #519 wrapped-body pixel comparison
yet. Leave #519 open without awaiting-player-validation until that acceptance work
is complete. #520's hand disappearance remains separate and unresolved.
