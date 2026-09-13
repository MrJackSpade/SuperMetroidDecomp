# Climb missile audio investigation (#16)

Affected report: 0.2.0, room $00/$1C. Status: unresolved audible-output report.

`--climb-missile-audio-audit ROM` loads the awakened Climb via the ordinary
runtime room loader. All eleven live actors are retail $F353 wall Pirates with
20 HP. It positions Samus 64 pixels right and eight below the first actor, shows
the encounter with camera (256,128), selects five missiles, and presses Shoot
only on frame2. It does not synthesize a projectile or edit enemy health/AI.

The selected actor dies on frame14, with four missiles remaining. The per-frame
enemy publication queue emits library2/$24 Max1 on frames23,31,39,47,55. Other
live actors emit their ordinary $66/$67 requests. These exact observations are
guarded in the diagnostic. A first diagnostic incorrectly treated the queue as
append-only across frames; it is append-only within each frame and resets at
EnemyMain. That diagnostic error was corrected, not production behavior.

This establishes a real missile kill and its death request schedule in the
reported room, improving on the previous direct Power Beam collision fixture.
It does not establish mixed audible output, original-SPC encounter parity, or
the player's exact saved room state. The loaded 20-HP population cannot supply
a surviving ordinary missile hit without changing its retail preconditions.
Do not invent higher health to represent the reported encounter. Next compare
the emitted requests through queue arbitration and playback alongside missile
impact/music; standalone sound playback alone does not settle audibility.

The fixture now enters the real frontend MainGameplay path and records its NMI
APU writes. It echoes port acknowledgements on the following frame; this is a
CPU queue diagnostic, not an emulated SPC or an audible-output assertion. The
five requests produce three port-two/$24 deliveries, on frames40,47,55. These
exact deliveries are guarded alongside the unchanged enemy/death schedule.
Room music is queued on frontend attachment; this artificial startup must not
be mistaken for the player's already-playing music state. A playback comparison
must preserve real SPC acknowledgements and include a warmed-up encounter.

No ROM, audio samples, SRAM, or captures are published with this diagnostic.

## Warmed-up managed playback

`--climb-missile-playback-audit ROM AUDIO_DIRECTORY` uses the same encounter,
but advances its actual frontend audio queue and managed SPC for 600 music-only
frames first. Gameplay actors are not advanced during that audio warmup. It
then feeds actual SPC port acknowledgements back to the frontend on every frame.
All five death requests reach port two on frames23,31,39,47,55; the diagnostic
asserts that no request was delayed or lost in this warmed-up run.

A second player receives identical uploads, music, missile-impact requests,
other enemy sounds, and port clears, suppressing only library2/$24 writes.
The full mix differs from that control on 51 frames, with maximum sample delta
19335. The diagnostic asserts a nonzero contribution, not a particular PCM
golden hash. Thus the current implementation contributes this cue to the mix;
this does not establish perceptual prominence, Windows endpoint audibility,
or original-cartridge command/mixing parity. The control intentionally does
not feed its differing acknowledgements back into the production sequence.
