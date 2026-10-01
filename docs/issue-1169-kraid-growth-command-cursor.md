# Kraid growth command cursor (#1169)

Affected version: `0.4.1+0d19f0216592138bb4779366849483d2e20e3074`.
The supplied diagnostic log records `Kraid head instruction $A7:96EA is not a
timed frame` during `RunKraidFootMain -> PrepareKraidFirstPhaseLunge ->
TryBeginKraidGrowth`. The archive contains no saved state or input recording.

## Native behavior and correction

Pinned disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank_A7.asm,
`HandleKraidPhase1` at $A7:C005, reads the word at current head cursor + 2 with
`LDA $0002,X` at $C026. It does not require a timed frame and does not execute
the command. The frame interpreter can legitimately leave the next cursor at
$96EA, the roar sound command, after displaying the frame at $96E2.

At $96EA, the offset-two word is the next frame's duration $0040, not its
tilemap. The native comparisons at $C02C-$C03F take the default branch,
selecting cursor $96F4 and timer $0040 at $C046-$C04F. The disassembly identifies
this as part of Kraid's quick-kill animation freeze; the correction preserves it.

The compiled reader is renamed `ReadGrowthSelectionWord` to express this raw
word contract. Timed records return their tilemap; sound and terminal records
return the next frame's duration. The last terminator borders mouth geometry,
so its adjacent word comes from the compiled hitbox definition. Existing
low-half live-memory handling and rejection of unrelated upper-ROM pointers
remain intact. No gameplay cartridge reads or new artwork are introduced.

## Confirmation

`--kraid-growth-command-cursor` executes the real head interpreter to produce
$96EA, then invokes the same foot AI phase from the report below the growth
health threshold. Before the fix it reproduced the exact exception and call
chain. Afterward it asserts $96F4, the 64-frame delay, retained head selection,
camera-release scheduling, the 180-frame function timer, and stopped foot AI.
Private-program cartridge reads are forbidden by the fixture.

The reader's offset-two values for all 28 declared head-command records are
compared with the supported NTSC J/U v1.0 ROM. This confirms the identified
conversion contract across timed, sound, and terminal forms. It is not a
whole-fight replay or player validation.
