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
claimed by this checkpoint. Full acquisition/fanfare playback validation remains
before #522 is marked awaiting player validation. Do not close on this test alone.
