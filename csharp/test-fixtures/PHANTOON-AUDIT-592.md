# Phantoon encounter diagnostic repair (#592)

The unchanged rage-direction checks expected PAL angular speeds +3/-3. The
pinned Japan/USA cartridge's immediate operands at $86:9885/$988D are +2/-2.
Production already used the correct NTSC values. The full audit reproduced the
failure after 2,243 rage-route frames, with both observed-direction booleans false.

The diagnostic now checks the LDA-immediate opcodes and reads their literal
operands from the cartridge. It does not borrow production constants, accept
either value indiscriminately, or weaken the remaining encounter assertions.
There are no gameplay changes in this repair.

Afterwards the complete audit passes all 5,906 frames through combat, eight rage
waves, ten death fades, 29 explosions and Wrecked Ship activation, including the
persisted boss bit, door and music. DebugRunner Release builds without warnings.
The focused native flame-region and eight-wave cadence/audio audits also pass.
