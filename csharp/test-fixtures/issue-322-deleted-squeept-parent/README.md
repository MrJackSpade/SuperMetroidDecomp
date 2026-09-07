# Issue 322: deleted Squeept parent

The original recording `input-recordings/SuperMetroid-input-20260906-163115-467.smrec`
failed at frame 52923 in room AFA3. Follower slot 16 read preceding slot 15:
header 0000, health 012C, parameter 0000, properties 2200. The header had been
deleted without clearing the other physical-record words.

Native `Function_Squeept_Flame` (bank A2, BEDC–BF19) reads preceding health,
freeze timer, velocity and Y directly. It does not revalidate that record's header.
The fix retains population-time pair validation and removes the invalid runtime
identity requirement. This is not a catch-and-ignore exception workaround.

## Regression commands (repository root)

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --deleted-squeept-parent-audit 'Super Metroid.smc'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --input-replay-audit input-recordings/SuperMetroid-input-20260906-163115-467.smrec 'Super Metroid.smc'
```

The focused fixture loads the retail pair, marks the parent deleted with nonzero
health, and uses the real enemy scheduler to clear its header. It failed at the
same invariant before the fix. Afterward it asserts retained Y following and
freeze-driven invisibility, not merely absence of an exception. The original
recording subsequently completes all 53686 frames, including the reported crash.

The separate legacy `--norfair-lava-jumper-audit` is **not** claimed passing. Its
freeze fixture lacks Ice equipment, its death assertion expects a deleted flag
instead of native record clearing, and its power-bomb immunity expectation does
not match current behavior. Those expectations need their own cartridge audit;
they were not weakened to qualify this fix.
