# Temporary boost: Dash and equipment cancellation

Partial #429. This extends [initial retention](README.md) with eight post-expiry
controls in both facings. The native CPU tool and C# runtime start with the same
earned counter0401 at399, zero remaining charge, and zero base/extra speed.

All modes hold R from the crouch at140 through399. From400:

| Mode | Equipment operation | Held input |
| --- | --- | --- |
| 0 | None | R |
| 1 | None | R + forward |
| 2 | None | R + forward + Dash |
| 3 | Disable Speed Booster | R |
| 4 | Disable Speed Booster | R + forward |
| 5 | Disable Speed Booster | R + forward + Dash |
| 6 | Disable on400, re-enable on410 | R |
| 7 | None | R + Dash |

Each case runs460 frames. Equipment writes only change the equipped word and
preserve collected inventory. This isolates the gameplay consumer; it does not
claim pause-menu input or resume-handler coverage.

## Native result

Modes0/3/6/7 retain0401 throughout. Dash without running is not cancellation;
neither is the isolated equipment toggle while remaining stationary.
Modes1/2/4/5 keep0401 on400 while changing to standing, then clear on401 while
entering running. Mode2 begins fresh accumulation on402 and ends at0201 on459.
Mode5 has no boost accumulation and caps extra speed at2.0000; mode2 reaches
3.A000 by459. Every post-expiry frame in this matrix has contact damage zero,
including stationary cases retaining0401. A counter alone is not a terrain-hit
assertion; actual breakable-block interactions remain separate work.

Pinned native `Samus_HandleExtraRunspeedX` ($90:973E) requires both Running and
Dash for its acceleration/init branch. Its non-equipped initialization clears
the counter only when beginning momentum. The complete CPU movement/input/pose
phases determine the earlier401 cancellation; the test does not manually clear it.

16 cases / 7,360 frames match all recorded fields without any production change.
An independent second capture matches normalized SHA-256:
`E8AD5317A45B4692FFACA07F660D38250F7A5707BD2D2DFE7427C718FAD43B7E`.
Semantic assertions additionally enforce cancellation boundaries, new buildup's
terminal counter, and zero post-expiry contact damage.

## Reproduce

Build the native tool per README.md, then:

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" cancel > csharp\test-temp\temporary-blue-cancel.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-cancel-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/cancel.csv
```

The previous bounce matrix also passes after the shared runner extension.
No player state, ROM bytes or artwork is included. Sand, actual terrain damage,
repeated complete soft-unmorph chains, full equipment-menu transitions and
persistent Blue Suit contrasts remain open. Do not treat this as completion of429.
