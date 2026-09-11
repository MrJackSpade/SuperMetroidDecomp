# Item-message spin audio (#514)

## Reproduced defect and correction

The frontend omitted the shared sound cancellation at entry to DisplayMessageBox.
An already-playing spin loop consequently survived the entire frozen message wait.
The correction detects message entry within the current frontend step, publishes
earlier gameplay sound requests, then queues the existing three-library Max6
cancellation before advancing the audio transport. It does not cancel on every
message frame, on close, or simply because an already-active message was loaded.
No new serialized flags or item-specific special cases are needed.

The actual local ROM at $85:8089 calls $82:BE17. That routine queues library one
$02, library two $71, and library three $01 through the Max6 entrypoints. The pinned
native sources agree: `sm_85.c` DisplayMessageBox_Async and `sm_82.c`
CancelSoundEffects. This establishes cancellation as cartridge behavior, not a
host policy to mute inconvenient sounds.

## Focused production-path reproduction

Run DebugRunner with:

`--item-message-audio-audit ROM standalone-assets/audio`

The fixture loads the retail Morph Ball room and initializes its real collectible.
It seeds an already-audible spin voice in ManagedSpcPlayer, sets Samus's spin pose,
then notifies the real collectible contact entry. The next production frontend step
executes the PLM pickup, message entry and audio publication. Input is locked to
isolate that boundary from navigation; this is not a controller-driven jump route.

The PCM measurement isolates library one from music/other room sounds. Other
libraries receive synthetic echo acknowledgements so unrelated requests cannot
block the cancellation transport. It does not constitute a mixed-fanfare listening
test or original-CPU whole-game replay.

- Before correction: zero cancellation commands; spin PCM audible on all 370
  checked frames after the initial 30-frame release allowance.
- After correction: exactly one cancellation per library; initially audible spin
  becomes silent and remains silent through the 400-frame unacknowledged message.
- The fixture then acknowledges/closes the message and checks that closing does
  not repeat entry cancellation.
- Full managed verification suite passes.

Native message teardown also calls QueueSamusMovementSfx. Its command-$14 spin
branch reads the low byte at $0A1E and compares it to 3 in this ROM, rather than
reading movement type at $0A1F. Do not infer an unconditional spin restart from
the routine's name. This fix does not introduce a new resumption policy.

Player confirmation of the originally reported encounter is still required.
No ROM, PCM, screenshots or debugger states are included in this fixture.
