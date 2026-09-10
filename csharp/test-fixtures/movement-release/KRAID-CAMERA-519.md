# Kraid entry-jump camera (#519)

Affected player version: 0.1.1. Camera movement and the reported visible body wrap
are reproduced, with a rendered regression that fails when the fix is removed.

`--kraid-camera-audit ROM` loads the actual incoming door 83:91B6 and room A59F,
places Samus on the safe entrance ledge at X=48, restores the door's Y=256 camera,
and repeats 35 Jump frames followed by 55 neutral frames ten times (900 frames).
Grounded debug placement itself
adjusts the camera, so restoring the entry position before simulation is required.
The fixture asserts NormalJumping movement and actual upward travel, the initial
four scroll bytes, camera distance index, and both camera Y extrema.

This supersedes the initial 90-frame fixture: its floor search placed Samus beneath
the spikes, potentially mixing damage recoil into the jump. The safe-ledge fixture
has Samus Y=395 -> 323 -> 395. With only the initializer scroll write removed,
scrolls are [2,2,1,1] and camera Y reaches 166. With the write restored, initial
scrolls are [0,0,1,0] and camera Y remains exactly 256 throughout.

Every frame is rendered through GameplayDisplayCapture and the software layered
snapshot renderer. A second render of the identical immutable packet disables
only BG2, isolating Kraid's visible body contribution. The first 16 gameplay
scanlines below the HUD (Y=32..47) must have no body pixels throughout this
undamaged first-phase sequence. Without the initializer write, the new pixel
assertion fails: 250425 pixel-frame differences, first visible at frame 13.
With the write restored, there are zero differences across all 900 frames.

`kraid-camera-519-v1.zip` preserves the visually inspected frame-13 captures:
`wrapped.png` shows the erroneous green body strip above the ceiling; `frame13.png`
shows the same jump frame with the camera locked and no body strip. Full filenames
carry the `kraid-camera-519-` prefix. These are production-renderer captures from
the real room with diagnostic placement, not screenshots from an original emulator.

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
The added 900-frame rendered audit builds without warnings, fails on the removed
write, and passes after restoration. Camera constraints and activation/release
timing are cross-checked against the pinned cartridge sources above; no new
original-CPU camera trajectory capture is claimed. Keep #519 open for player
confirmation with awaiting-player-validation. #520's hand disappearance remains
separate and unresolved.
