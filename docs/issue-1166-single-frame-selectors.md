# Single-frame enemy visual omissions (#1166)

Affected player build: `0.4.0+c1f51a02badbdb9ce80a4c493e97097949cb547b`.
Fix branch starts at release `v0.4.0`. The player diagnostic contains the exact
exception `Enemy $DFFF has no compiled visual selector $A6:$8B2B`.

The existing `--kzan-instruction-mechanics` fixture reproduced that exception
through the real initializer and instruction interpreter before the fix.
No private state or cartridge payload is included in this change.

## Cause and static scope

`InspectEnemyVisualSelectors` discovered indexed `PresentationWordAddress`
methods but skipped a singular `PresentationWord` constant. Source inspection
identified four such owners without an indexed method:

| Owner | Operand | Native target | Disposition |
| --- | --- | --- | --- |
| Kzan | A6:8B2B | A6:8CE5 | Missing runtime selector and ordinary composition; added both. |
| Polyp | A2:B51C | A2:B5FB | Same omission; added selector and ordinary composition. |
| Polyp rock | 86:BBD7 | 8D:9340 | Same omission; added selector inventory entry and installed projectile binding/composition. |
| Dead Torizo | A9:D6DE | A9:D6E2 | Existing specialized selector and composition already work; generator now includes the declaration. |

Horizontal Shutter also declares a singular constant, but already exposes the
indexed interface; it remains inventoried once. The generator now accepts both
shapes and preserves all existing entries. Its four additions produce 5,073
distinct addresses from 5,218 occurrences.

The other instruction-program owners without indexed presentation methods were
reviewed from source: Ceres Baby has explicitly typed sprite/palette operands;
Mother Brain body/head have dedicated visual resolvers; common projectile delete,
Golden Torizo eye-beam/jump-landing/stunned, and the live Tourian entrance statue
programs contain no timed spritemap operands. The inventory's three pre-existing
unresolved reflection shapes are not this defect: Dead Tourian corpses and
Skree/Metaree have specialized frame resolvers, and Mother Brain hand-beam
projectiles use installed presentation bindings. This is a scoped source audit,
not a claim of whole-game completeness.

## Native evidence

The local Japan/USA NTSC v1.0 image matches the supported SHA-256
`12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`.
The pinned InsaneFirebat disassembly checkout is
`362be646929cf8e483f692b73a6561cfc2dc1d0d`; banks A6, A2, 86 and 8D identify the
lists and targets above. The Kzan program is one frame followed by sleep.
Cross-references: [InsaneFirebat bank A6](https://github.com/InsaneFirebat/sm_disassembly/blob/362be646929cf8e483f692b73a6561cfc2dc1d0d/src/bank_A6.asm)
and [Patrick Johnston bank A6](https://patrickjohnston.org/bank/A6#f8B29).

## Confirmation and compatibility

`--single-frame-enemy-visuals` runs only these three instruction fixtures and the
new artwork check. It confirms exact Kzan/Polyp native frame selection, terminal
sleep, Polyp-rock presentation selection and deletion, with instruction/selector
ROM reads forbidden. It extracts the required presentation data and compares
complete low/high OAM bytes against the independent native decoder. Kzan's four
parts and Polyp's one part draw through the production enemy rendering helper.

Ordinary composition schema 68 accepts schema-67 overrides with current stock;
projectile schema 13 accepts schema-12 overrides. Focused assertions confirm
existing edits survive and new stock frames are inherited. Incomplete old stock
is rejected. Enemy bundle schema 68 requests asset refresh through existing
installation handling; a retained installed ROM permits re-extraction, otherwise
the player must select their supported ROM again.

The inventory-only command confirms regeneration against declared operands.
Player confirmation remains pending; no exploratory gameplay was performed.
