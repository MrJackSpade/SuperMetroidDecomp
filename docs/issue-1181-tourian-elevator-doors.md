# Tourian entrance collision errors (#1181-#1184)

Affected version: 0.4.8.0, reported from the local 0.4.8 smoke-build session.

Four automatic reports came from room $8F:DAAE (RoomId $05/$00): standing,
turning, and running touched BTS $02; running right touched BTS $03.
The compiled door list at $8F:DAD5 contained only two entries.

The pinned J/U NTSC 1.0 cartridge and InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d have four entries:
$A984, $A990, $A18A, $A99C. $83:A18A is a second zero-destination elevator
sentinel, immediately before the second physical door-header block. The catalog
recognized only the first sentinel, $83:88FC. Its completeness check consequently
accepted $A18A as the end of the list and missed the subsequent save-room door.
The same native sentinel is entry 3 of the Maridia elevator list at $8F:D332.

Added the second sentinel and restored both lists. The existing strict invalid-BTS
failure remains. Updated the existing catalog oracle to recognize both sentinels
so it no longer treats $A18A as a valid list terminator.

Native collision at $94:938B/$93CE masks BTS with $7F, indexes the list, and uses
the destination high bit to choose a room transition or solid elevator collision.
Elevator contact is published only for poses below $09. This agrees with the
[Patrick Johnston bank-$94 reference](https://patrickjohnston.org/bank/94#938B)
and the pinned local disassembly.

`--tourian-elevator-doors` first failed with the exact reported exception in
production MoveVertical. After the fix it passes: the two native four-entry lists
and full headers match ROM bytes; the production collision seam clips elevator
blocks, preserves pose gating, and requests the correct save-room door. A bus
that rejects every read/write proves collision uses compiled definitions. The
case also checks the BTS high-bit mask and rejection of index 4. This is a focused
fixture, not a replay of the player's whole recording. Player validation remains
pending.

The separate auto-report #1179 (captured build 0.4.0.0) duplicates #1171's demo
startup missing-palette binding. Commit 27da732e, shipped in v0.4.3, already fixes
it. The focused `--attract-runtime-bindings` reproduction/confirmation passed again.
