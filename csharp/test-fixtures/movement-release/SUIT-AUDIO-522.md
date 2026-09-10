# #522 Suit transformation sound

Affected player version: 0.1.1. Pinned bank-91 disassembly confirms Varia setup
loads sound 0056 at D590 and calls QueueSound_Lib2_Max6 at D593. Gravity does
the same at D668/D66B. This is a transformation-specific request; visual
similarity to Power Bombs is not the source of the sound choice.

The managed suit initializer had no audio publication. A focused reproduction
installs a normal runtime into the frontend, starts each suit transformation,
and runs 16 real frontend frames through the sound-ring/APU-port handshake.
Before the fix both cases produced zero library-two/56 writes. Suit entry now
publishes a one-shot request, consumed outside the gameplay-publication guard
because the effect resumes from message close and has HDMA-only frames.
Both cases now produce exactly one write, not one per effect frame.

Run `--suit-pickup-audio-audit ROM` in DebugRunner. Boundary setup uses reflection
only to install the runtime/frontend state; audio collection and queue dispatch
are production code. No real item collection or decoded PCM comparison is
claimed by that initial checkpoint. The acquisition check below now completes
the production route; leave #522 open for player confirmation.

## Real Varia room acquisition and PCM

`--suit-acquisition-audit ROM AUDIO_DIRECTORY` loads retail Varia room A6E2,
starts Samus at X80 on the floor (Y171), and uses only controller input thereafter:
Shoot frames 0..59; Right+Jump 60..93; Jump 120..124; Down+Shoot 125..154;
neutral otherwise, with Shoot to acknowledge the message when allowed.
It runs 900 full frontend frames with the managed SPC/DSP and real returned
APU acknowledgements. No item, message or transformation state is injected.
Reflection installs only the initial frontend/runtime boundary. Cheats are off.

The orb breaks, Varia is collected, the message/fanfare interval runs, and the
transformation starts at frame 552. Library-two/56 reaches the APU exactly on
that frame, once only. The effect ends and gameplay input unlocks with Varia
equipped. A second audio renderer consumes identical commands except that one
sound: PCM is identical beforehand and differs on 139 frames afterward. This
isolates the effect from existing fanfare/room music instead of asserting that
any audio energy proves the sound played. The test exports a mono downmix at
`csharp/test-temp/suit-acquisition-522.wav` for listening; no subjective listening
or sample-exact native-SPC comparison is claimed.

Acceptance: production sound restored and verified, awaiting player validation.
Native trigger/library/cap/timing are source-cross-checked at D590-D593;
the full-room sequence is managed runtime evidence, not a native CPU capture.
Gravity shares the setup sound and retains its focused queue regression.
