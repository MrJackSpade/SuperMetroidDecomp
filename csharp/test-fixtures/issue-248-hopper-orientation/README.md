# Issue 248: floor-stranded ceiling Sidehoppers

This directory preserves the exact debugger state supplied by the player after the
hopper-orientation bug survived earlier test passes. It is deliberately a copy of live
debug slot 2 so later state saves cannot destroy the reproduction.

## Reproduction

1. Load `slot-2.smstate` against commit `5691b67` and the private Japan/USA v1.0 ROM.
2. Observe room `$01/$25` (`$8F:A37C`) immediately.
3. Three giant Sidehoppers authored as ceiling actors are stranded on the floor while
   still using their ceiling-oriented spritemaps.

The captured runtime reports all three as enemy `$D9BF`, parameter one `$8000`, with
ceiling instruction lists. Their current Y positions are near `$008C`; the cartridge
population starts them at Y=`$0058` under the ceiling. This evidence led to the focused
cartridge-room movement regression in `HopperAudit`.

## Integrity

- Size: 2,276,604 bytes
- SHA-256: `12EACE98DED9F6DA32B5139060265FA2ECDE0A0FDC47F2181910970A7CD5E975`
