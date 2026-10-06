# Speed Booster palette overrun, issue #1254

The player's v0.4.22 log reports a rejected pointer read at $91:DAC7.
Screw Attack uses the shared phase word through offset10, while active Speed
Booster normally uses offsets0..6. Leaving Screw Attack with boosted momentum
can therefore reach Gravity list $DABF +8 or +10. The original routine copies
before clamping the next phase to6. The C translation in upstream-sm/src/sm_91.c
contains an explicit pre-read bug fix; it is not the oracle used here.

probe.cpp executes the unmodified ROM routine $91:D9B2 on the pinned 65816 core.
The probe maps only the routine's WRAM, ROM and undriven $9B:6000..7FFF accesses;
unexpected peripheral accesses fail. It neither plays through the game nor edits
the ROM. Its process boundary disables Windows error dialogs and reports failures
with a nonzero exit code. Build probe.vcxproj with MSBuild Release/x64, then run
csharp/test-temp/issue-1254-speed-palette/probe.exe upstream-sm/sm.smc.

Supported ROM SHA256: 12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72
CPU source SHA256: A5D88B0F2E0798482A2CAE9DDDAF602FEC69A8FD26C55B167417C4C92EEC30A6

native.csv contains the two resulting palettes, next phase and timer. Phase8
interprets the first LDA instruction word as pointer $68AD; the unrolled palette
copy's absolute-indexed operands have high byte zero, producing black through
open bus. Phase10 interprets the next instruction-boundary word as $C90A and
copies the first32bytes there from bank9B, masking CGRAM's unused bit15.

Verification --speed-palette-overrun reproduces the transition through the real
Screw Attack/Speed Booster handlers, compares all16colors with both raw ROM data
and native.csv, checks the four-frame hold, then the normal phase6 recovery.
The gameplay address space has no cartridge data. Before the fix this fixture
failed with the same $DAC7 exception as the player report.

The archive contains no recording or palette-phase fields, so it does not prove
the exact player inputs or preceding Screw Attack timing. This is a faithful
bounded reproduction of the observed read and a cartridge-verified fix.

References: [bank91 assembly](https://github.com/InsaneFirebat/sm_disassembly/blob/master/src/bank_91.asm)
and [annotated bank91](https://patrickjohnston.org/bank/91), routines D9B2 and DD5B.