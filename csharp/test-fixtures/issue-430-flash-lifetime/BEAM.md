# Retained Flash beam palette (#430)

`beam.csv` adds a visual-data comparison to the two verified Flash/Draygon
generation orders, in both facings. It contains 1,404 original-CPU frames;
frames 300..350 include all sixteen CGRAM words at 224..239. At frame 350,
the native HUD beam producer, projectile phase and live-projectile draw emit
a real uncharged Power Beam. The C# audit calls the corresponding production
projectile producer/update and OAM draw. All four OAM streams and all 204
palette snapshots match exactly. No gameplay change was required.

The emitted ROM sprite attributes select OBJ palette six (CGRAM 224..239).
The retained Crystal Flash palette handler repeatedly overwrites this same
bank, including equal-channel gray colors in its first ten entries. This
connects the visual cue to actual beam sprite palette selection, rather than
assuming that a retained-state flag proves the beam's appearance. The test
does not cover other equipped beams or claim a full framebuffer comparison.

Native entry points: $90:B80D (beam HUD producer), $90:AECE (projectile phase),
$93:8254 (live projectile drawing), $90:AC1C (cooldown), and $91:D6F7 (Samus
palette dispatcher). Cross-checks: pinned `upstream-sm/src/sm_90.c`,
`sm_91.c` and `sm_93.c`; expected output executes original ROM CPU code.

The observer shot is injected after the frame's movement/palette checkpoint,
not as a replacement for its movement input. Both implementations use camera
(128,400) before projectile update: setting it only before drawing would let
native offscreen cleanup remove the shot. The native fixture also runs the
ordinary cooldown phase each frame, matching the runtime. Neither sprite data
nor projectile slots are fabricated. This isolated shot is not a full gameplay
input-to-render integration test.

Generate with `DraygonCrystalAudit/audit.exe "Super Metroid.smc" beam`.
Run with DebugRunner `--flash-beam-audit "Super Metroid.smc" <beam.csv>`.
Use the ROM identity pinned in the adjacent fixtures.
Normalized LF UTF-8 trace SHA-256:
`88724C419462D65CFEED282AE946DAF4730471C8C673CAD96AD707CBC2C66D55`.

Only numeric diagnostic data is published; no movie, ROM, snapshot or image.
