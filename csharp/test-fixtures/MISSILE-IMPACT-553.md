# Super Missile impact shake (#553)

Affected player version: 0.1.1. Shake and impact audio now have focused coverage.

## Cartridge evidence and defect

Pinned `upstream-sm/src/sm_93.c`, `KillProjectileInner` ($93:80CF), writes
earthquake type 20 and duration 30 for Super Missile impacts. The extended enemy
collision prelude also writes these shared words before invoking the enemy callback.
The displacement table at $A0:872D specifies one-pixel diagonal displacement of
BG1, BG2 and enemies, with direction selected by timer bit 1.

Managed collisions wrote projectile-local request words, but the renderer's room
shake handler consumed the enemy system's independent words. An actual Super
Missile wall collision therefore produced zero displayed shake frames.

The fix routes each request synchronously to the shared room owner. It does not
copy stale requests at frame end or restart their timers. The routing reference
is nonserialized and rebound before frame execution after state restoration.

## Reproduction and verification

Run DebugRunner `--missile-impact-shake-audit "Super Metroid.smc"`.
The focused flat-floor fixture uses the production weapon, collision, room-shake,
display-capture and software rendering paths, with a constructed solid wall.

- Before the fix: Super Missile collision at frame 11, projectile request 20/30,
  room request 0/0, zero displayed shake frames (assertion failure).
- After: exactly 30 displayed frames, beginning one accepted NMI after collision,
  with exact alternating diagonal displacements on both background registers.
- Rendered background pixels differ from a neutral-scroll control; HUD pixels
  remain identical. This scene does not independently isolate visible BG1 pixels.
- Regular Missile control: actual wall collision, zero shake frames.
- Direct production enemy-prelude check: request overwrites an earlier quake;
  a later room producer still wins and counts down without stale-request restart.
  This is not an enemy geometry or full target-matrix playthrough test.
- Core Verification passes. Ordinary RenderVerification passes 96 ordinary and
  128 window comparisons on both hardware and WARP.

No screenshots, ROM data, or debugger-state files are published.

## Impact audio follow-up

The native impact sound is library 2, command 7 for both regular and Super
Missiles, not a louder Super-only sound. Managed `KillMissile` had no handoff.
It now publishes a separate frame-scoped impact list, retained across projectile
movement when an earlier enemy collision generated it. The gameplay publication
generation guard prevents replaying the list during NMI-only frontend frames.
Intro cinematic frames explicitly apply the native missile-sound suppression.
The new transient fields are nonserialized, preserving debugger-state schemas.

Run DebugRunner `--missile-impact-native-audio-audit AUDIO-DIRECTORY NATIVE-DLL ROM`.
The native DLL is a local diagnostic dependency, not a new production audio path.
The fixture injects a constructed room into the real frontend gameplay dispatcher.
It runs input, collision, sound publication, APU commands, acknowledgements and PCM.

- Disabling only the added impact request reproduces failure: PCM is identical
  to a control which removes every missile impact command.
- Fixed regular Missile: wall collision and impact port write both at frame 79.
- Fixed Super Missile: wall collision and impact port write both at frame 71.
- Exactly one impact command per shot; both differ audibly from the muted control.
- All 480 complete PCM/acknowledgement frames match native SPC/DSP playback fed
  the same production commands. This verifies synthesis, not independent original
  65816 gameplay queue timing; same-frame impact selection is checked separately.
- Direct enemy-impact conversion survives the subsequent projectile step; the
  same conversion in cinematic context remains silent; next-frame publication clears.

## Target, launch and explosion follow-up

The shake audit additionally runs actual ordinary enemy overlap/shot dispatch
against cartridge Zoomer ($DCFF) and Ripper ($D47F) definitions. Population shape
and overlap are constructed; their definitions and vulnerability data are retail.
Both missile types kill the 15-health Zoomer. Regular Missiles leave the Ripper's
200 health unchanged; Supers kill it. All four impacts emit one impact sound;
only Supers request 30 frames of shake. The previously documented direct extended
collision prelude covers the shared request before a custom callback, not every
boss-specific reaction or an exhaustive enemy matrix.

`MissileExplosionAnimationAudit` reads the straight-line retail instruction lists
independently and checks every post-wall-collision frame against those durations
and spritemaps. It independently decodes each spritemap's position, size, tile,
palette, priority and flip attributes and compares the production draw's OAM.
The production OBJ renderer then verifies nontransparent pixels and six distinct
images. Impact is normalized to screen center by camera subtraction for this
OBJ-only check; it is not an assertion about layering over every room background.
Regular Missile: 18 stationary frames; Super Missile: 30 stationary frames;
both delete afterward. No animation change was needed.

The frontend/native-PCM test also verifies exactly one library-one launch sound
on the fire-input frame: command 3 regular, command 4 Super, matching bank $90's
missile producer. Impact audio is intentionally shared, not intensified for Supers.

Focused checks are complete and the issue is ready for player confirmation.
No claim is made that every enemy callback, concurrent queue saturation, or
room-specific compositor has been exhaustively tested.
