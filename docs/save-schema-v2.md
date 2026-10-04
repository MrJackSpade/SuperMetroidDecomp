# Named save schema v2

`preservedUntranslatedSram` in v1 was the **entire 8 KiB SRAM**, not merely unknown
bytes. It duplicated the named slot data and checksums in 32 hexadecimal pages.
Version 2 removes it. The named state is authoritative and native SRAM is rebuilt.

The four previously omitted meanings are:

| JSON field | Native meaning | Runtime owner |
| --- | --- | --- |
| `slots[].resources.reserveMissiles` | `$09D8`, excess missile accumulator | `SamusState.ReserveMissiles` |
| `slots[].japaneseText` | `$09E2`, alternate text option | Options and runtime language setting |
| `slots[].loadedItemCount` | `$7E:D91A`, cumulative item PLM setup count, wrapping at 65536 | `Bank80SystemState`; incremented on item setup, including already collected items |
| `gameCompleted` | Exact twelve-byte `supermetroid` signature at `$70:1FE0` | Ending final-message transition; fourth attract-demo set requires this and a valid slot |

The item-load counter is **not** collected item count or completion percentage.
The signature has no NUL terminator: selected-slot metadata starts immediately after it.

## Complete native storage inventory

Offsets are hexadecimal, ranges inclusive. Slot starts are `0010`, `066C`, `0CC8`;
each occupies `065C` bytes. The per-slot ranges below are relative to its start.

| Slot range | Representation |
| --- | --- |
| `0000–0007` | Equipped/collected items and beams |
| `0008–000F` | Four fixed directional bindings; regenerated |
| `0010–001D` | Seven named controller bindings |
| `001E–0035` | Reserve mode, health/ammo capacities and current values, HUD selection, reserve energy |
| `0036–0037` | Reserve missiles |
| `0038–003F` | Game time |
| `0040–0049` | Language, moonwalk, debug flag, new-file marker, icon cancellation |
| `004A–005F` | Unused player-mirror allocation; zeroed |
| `0060–0067` | Event bit plane |
| `0068–006F` | Boss flags by area |
| `0070–00AF` | Chozo orb bits |
| `00B0–00EF` | Collected item bits |
| `00F0–012F` | Opened door bits |
| `0130–0137` | Native `neverReadD8F0` allocation; zeroed |
| `0138–0147` | Used save station/elevator bits |
| `0148–0153` | Acquired map-station plane (including its existing reserved bits) |
| `0154–0159` | Loading state, save station, area |
| `015A–015B` | Item-load counter |
| `015C–02A2` | Six packed map planes, represented as explored coordinates |
| `02A3–065B` | Unused remainder of map allocation; zeroed |

| Absolute SRAM range | Representation |
| --- | --- |
| `0000–0005`, `0008–000D` | Three checksum/complement pairs; rebuilt |
| `0006–0007`, `000E–000F` | Unused directory entries; zeroed |
| `0010–1323` | Three slots as above; invalid slots represented by `null` |
| `1324–1FDF` | Unused SRAM; zeroed |
| `1FE0–1FEB` | Named completion Boolean |
| `1FEC–1FEF` | Selected slot and derived complement |
| `1FF0–1FF5`, `1FF8–1FFD` | Redundant checksums; rebuilt |
| `1FF6–1FF7`, `1FFE–1FFF` | Unused directory entries; zeroed |

Evidence: pinned InsaneFirebat disassembly revision
`362be646929cf8e483f692b73a6561cfc2dc1d0d`, NTSC J/U 1.0:
`memory.asm` player/SRAM mirror allocations; bank `$81` save/load and map packing;
`$84:EE64`/`$84:EE8E` item setup; `$80:824F`/`$80:8261` completion signature write/check;
`$8B:E797` ending completion. `SaveRamLayout` and `ExploredMapPackingDefinitions`
encode those layouts locally. Padding, invalid-slot remnants, corrupt checksums,
and noncanonical Boolean encodings are not gameplay state and are not perpetuated.

## Migration and confirmation

V1 JSON imports the four missing fields from its raw image, keeping previously named
fields authoritative. It validates before applying and automatically writes v2 through
the atomic file writer; `.bak` initially holds the exact original v1 JSON. Later normal
writes rotate that backup as before. Unknown fields and malformed pages fail loudly.
Legacy `.srm` imports remain supported and leave the source untouched. V2 is not readable
by older builds; keep the original backup when downgrading.

Focused command: `dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --game-save-json`.
It checks canonical round trips, missing field restoration, real frontend completion
production/consumption, item setup counting, strict errors, named-over-raw precedence,
legacy imports, atomic persistence, and the v1 backup. Fixtures use fresh SRAM arrays and
temporary files, never player saves.
