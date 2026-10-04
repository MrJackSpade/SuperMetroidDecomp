# #1178: Crateria Power Bomb Room rock background

Affected version: v0.4.7. Player confirmed RoomId $00/$03 ($8F:93AA).

The supported NTSC J/U 1.0 room state $93B7 uses rock library background $B8B4,
acid FX $8100, setup RTS $91D3, and main callback $C116. The cartridge really does
call the land-sky tilemap updater here. That callback ($88:AF8D / $88:AFA3)
updates BG2 Y and offscreen tilemap rows, but does not spawn horizontal sky HDMA.
FX $20 / sky setup $88:A7D8 installs that object; its $88:ADC2 pre-instruction
advances horizontal strips and selects BG2SC=$4A (32x64).

Cross-checked against pinned InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_8F.asm and bank_88.asm,
and the project's compiled room/state/FX definitions.

The port inferred the HDMA object and tilemap geometry from the main callback
alone. The fix retains the cartridge's updater, but derives horizontal HDMA and
vertical tilemap geometry from the selected FX and room setup independently.
Both ordinary display capture and the direct software renderer use this decision.
There is no room-ID exception.

## Focused confirmation

Run the guarded DebugRunner with:

    --crateria-acid-background-audit <installed-content-directory>

This loads the actual reported room and fixes the camera position. Before the
change it failed at frame 2: horizontal HDMA scroll 2, incorrect map 32x64.
Afterward, 512 frames retain BG2 X=$00C0, no horizontal HDMA, unchanged visible
rock tilemap words after initial NMI synchronization, and the correct 64x32 map.
Acid still rises/falls through Y=160..224. A Landing Site control retains its
horizontal sky HDMA and 32x64 map.

Release DebugRunner build and PLM, enemy visual, and door static audits pass.
The queued VRAM audit is blocked by the pre-existing TourianStatueSequence.StepTiles
certificate mismatch introduced with the tester-option change (7eab2d0c); the
sky producer contract is unchanged. Player confirmation remains pending.
