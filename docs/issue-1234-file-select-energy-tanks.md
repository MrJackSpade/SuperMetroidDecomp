# #1234: File-select energy tanks

Affected version: 0.4.15-smoke1233. The player reported missing energy tanks on
the save-file selection screen.

DrawInstalledSlot drew the ENERGY label and health remainder, but omitted the
tank loop entirely. Restore the native loop: maximum health / 100 supplies the
owned count, current health / 100 supplies the filled count, and remaining owned
tanks use the empty tile. Draw the lower row of seven before the upper row.
The same installed-page builder covers main and copy/clear menus. Positions are
relative to the installed ENERGY anchor; palette selection follows its installed
health digits. No saved data or health values are changed.

Reference: pinned NTSC J/U 1.0 disassembly
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_81.asm,
Draw_FileSelection_Energy $81:A0A4-A111. Filled/empty tiles are $0098/$0099;
the cursor begins at anchor+$48 bytes, advances through seven cells, then
subtracts $4E bytes. Retain the native subsequent row limit of eight.

FileSelectEnergyTanksAudit uses fresh in-memory SRAM arrays and read-only
installed menu artwork. It never opens or persists player save data. A save with
899/1499 energy must draw eight filled and six empty tanks at the exact native
two-row positions. Before the fix: tank 0 expected $0098, actual blank $000F.
After the fix both the main and clear-selection pages match every cell. The
99/99 and empty slots draw no tank icons. Pixel comparisons inside filled and
empty tank bounds confirm the actual menu raster shows both kinds of icon.

Release DebugRunner build and --file-select-energy-tanks-audit passed.
Awaiting player validation.
