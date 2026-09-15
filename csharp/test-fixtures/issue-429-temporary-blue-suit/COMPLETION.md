# Temporary Blue Suit coverage (#429)

Ready for player validation; not a whole-route or PAL-revision claim. All native
comparisons use the pinned unheadered Japan/USA ROM and production C# paths,
with gameplay cheats disabled. The per-fixture documents provide initial state,
equipment, input frames, hashes, and reproduced fixes.

| Required property | Evidence |
| --- | --- |
| Angle-held expiry, partial counters and no-angle loss | README.md / native.csv |
| Speedball unmorph | ../movement-release/TEMPORARY-BLUE.md (#470) |
| Soft-unmorph success/failure window and ordinary landing/reversal loss | CARRY.md |
| Ordinary bounce versus Spring Ball | BOUNCE.md |
| Repeated complete carry, not a stuck counter | CHAIN.md: five consecutive cycles |
| Terrain damage rather than only contact flags | TERRAIN.md: real collision/PLM mutation, full/partial controls |
| Dash and disabled-equipment effects | CANCELLATION.md and actual menu navigation/reconciliation in MENU.md |
| Sand cancellation and non-cancellation | SAND.md: body sampler and extension tiles |
| Wall versus ceiling contact | obstacle.csv, below |
| X-ray removes stored charge without clearing boost | xray.csv, below |
| Contrast with genuinely acquired persistent Blue Suit | ../issue-426-draygon-blue-suit/README.md |

## X-ray admission

Four cases: both facings and 60/140-frame run-ups. The same runway/controller
setup earns partial $0201 or full $0401, then holds R through frame 199. X-ray
Scope is equipped from the start. At checkpoint 200, native runs $91:E16D then
the $91:EB88 interrupted-pose dispatcher; C# runs the production already-selected
HUD admission boundary. No further movement/palette tick is inserted at that
boundary. This tests Samus-side charge handoff, not HUD selection or the later
scan-window/hidden-block rendering sequence.

Both preserve their earned counter. The full case has 120 shine ticks before
admission and zero afterward; the partial case never earned a shine timer.
Both end in the native X-ray crouch pose, frame two/timer $3F, palette owner 8.
The comparison reads C#'s active X-ray palette owner, not its now-inactive
shinespark palette state. All 804 observations match. No production fix needed.

Regenerate with `audit.exe ROM xray`; normalized SHA-256:
`D96C167587CD386611C715ADA147184EF9059A2B81A17591DEC1D400CF13CE19`.

## Wall/ceiling controls

Eight cases earn and expire full boost through 400 ordinary frames, then hold
Jump+forward for 100 frames. Both facings have air, wall, ceiling, and combined
controls. At frame 400, construct a solid wall 40 pixels ahead and/or a ceiling
64 pixels above the current center (aligned to the block grid). From there the
real movement/collision dispatcher owns all contact and cancellation; no result,
pose, or velocity is injected.

Ceiling contact retains $0401 through frame 420 while Samus falls. Wall contact
or subsequent ordinary landing clears it by 440. The no-obstacle control still
retains it at 440, then loses it upon landing before 499. All 4,000 position,
subpixel, pose, animation, velocity, boost/contact and timer checkpoints match.
No production fix needed.

Regenerate with `audit.exe ROM obstacle`; normalized SHA-256:
`DB526A37E9426519E0EEFA068F944E0499F3DD90C911F3EAD37407571361797B`.

## Persistent contrast

The #426 fixtures acquire persistent Blue Suit through actual Draygon death or
grab/escape callbacks after controller-earned launch. Successful cases retain
$0400 while standing and walking without an aim hold, publish walking contact
damage, can store and launch another spark, and cancel on Dash. Temporary
retention instead loses its counter on ordinary unprotected landing, reversal,
or walking out of the held-angle crouch; soft unmorph and held-angle input are
required to chain it. These are observed native outcomes, not manually assigned
"temporary" and "persistent" flags. Both share the same boost counter and
native cancellation machinery; control/handler history creates the difference.

## Run new matrices

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-xray-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/xray.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-obstacle-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/obstacle.csv
```

Only numeric traces and diagnostic code are published, never ROM/SRAM/state/art.
The ten temporary matrices total 129,332 observations. The separately documented
Draygon matrices establish the persistent-state comparison. X-ray climb and
interrupted-state exploits beyond this admission boundary retain their own tickets.
