# Shaktool same-frame death scheduling (#608)

Affected player build: 0.3.1.0.

The reported recording `SuperMetroid-input-20260913-120614-127.smrec`
reproduced the exact exception at recorded frame 32228, room D8C5/state D8D7.
The head had 60 health immediately before that frame. A lethal projectile cleared
its common record and the Shaktool shot tail marked all seven records deleted.

Native EnemyMain ($A0:8FD4) consumes the active list built before collisions. It
does not discard subsequent entries when another actor marks them deleted.
$AA:DC2A reads the preceding physical record's position/subposition regardless of
header. Group synchronization similarly addresses native records directly.
The port's header-identity checks wrongly rejected the cleared head during the
remaining companion AI calls. Initialized typed views now remain usable after
common-record clearing; bounds and missing-initialization checks remain.
No exception is swallowed and no additional group deletion is invented.

The focused `ShaktoolAudit.VerifyScheduledDeath` uses the retail population and
real scheduler/collision/death path. It failed with the reported exception before
the fix. It now asserts one death, all seven deletion flags, surviving companions'
final same-frame clock increments, and no further increments next frame. Existing
endpoint beam/bomb death, movement, rendering, attack and teardown tests pass.
The complete copied player recording also runs past the original failure to its
end without exceptions. The recording and diagnostic imagery remain local.

Commands:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --shaktool-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --input-replay-audit <recording.smrec> "Super Metroid.smc"
```

This addresses #608, not #605's terrain persistence or #607's Grapple report.
