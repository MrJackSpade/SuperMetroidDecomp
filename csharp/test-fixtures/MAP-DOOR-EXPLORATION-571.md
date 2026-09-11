# Hellway to Caterpillar minimap investigation (#571)

Affected player version: 0.2.0. **Open: reported extra visit not reproduced.**

Command: `SuperMetroid.DebugRunner --map-door-exploration-audit <retail-ROM>`.

The fixture loads Hellway ($8F:A2F7, $01/$23), places Samus on its actual floor
near the right door, and supplies Right plus periodic Shoot. Collision selects
the real $83:908A door into Caterpillar ($8F:A322, $01/$24). The regular door
coroutine performs loading, placement, scrolling and destination fade.

It scans every Brinstar exploration bit on every accepted step, including the
door coroutine and twenty destination gameplay frames. With and without map
station data, the observed new cells are exactly:

| Step | Room | Samus center | Newly visited cell |
| --- | --- | --- | --- |
| 0 | Hellway | 701,139 | 36,10 |
| 140 | Caterpillar | 43,1419 | 37,10 |

These agree with the pinned $90:A91B map-coordinate formula and both room headers:
Hellway map origin (34,9), screen (2,0); Caterpillar origin (37,4), screen (0,5).
There is no additional written visit two rows below the entrance in this sequence.

Repeat-entry controls retain those actual exploration bits and downloaded-map
state, re-seed the same source placement, then execute the door again. No new
bits appear. This is **not** a controller-driven return through Caterpillar's
yellow Power Bomb door; that unrelated return puzzle is intentionally excluded.

Four destination PNGs are generated locally in `csharp/test-temp/map-door-571`.
The inspected downloaded/undownloaded first-entry images show the expected
visited entry and source cells. The complete lower HUD map row is blank in both;
downloaded tiles above are visibly distinguished from pink visited tiles. PNGs
and cartridge data are not committed or attached publicly.

## Limits / next evidence

This diagnostic uses constructed source placement, initially clear Brinstar
exploration, default native-backed map presentation, and direct runtime/door
stepping. It does not replay the player's earlier route, installed presentation
overrides, or full frontend/audio scheduling. It is not an independent emulator
comparison or a reproduction of the reported bug. Do not close or label the issue
as awaiting validation on this evidence. A player state/recording or broader
entry-state investigation is still needed to identify the discrepant cell.
