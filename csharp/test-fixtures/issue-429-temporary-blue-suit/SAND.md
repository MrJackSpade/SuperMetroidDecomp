# Temporary boost: sand body sampling

Partial #429. Each of eight modes in both facings first earns expired-charge
temporary boost with the same400-frame run/crouch/R sequence as README.md.
Sample400 is an isolated body-overlap phase, not a complete gameplay frame.
It changes only fixture geometry and uses Maridia's area table. Native executes
unpatched `$94:9B60`; C# calls the production `PrepareFrame` sampler with Maridia.
Neither implementation injects a boost counter, pose or position for this sample.

| Mode | Tile under the sampled body | Native counter after sampling | Extra Y |
| --- | --- | --- | --- |
| 0 | Unchanged air | 0401 | 0 |
| 1 | Surface sand at feet | 0 | 1.125 pixels |
| 2 | Surface sand at top | 0 | 0 |
| 3 | Submerging sand at feet | 0401 | 1.125 pixels |
| 4 | Slow sandfall at feet | 0401 | 1.25 pixels |
| 5 | Fast sandfall at feet | 0401 | 1.75 pixels |
| 6 | Horizontal extension to adjacent surface sand | 0 | 1.125 pixels |
| 7 | Solid control | 0401 | 0 |

Maridia inside BTS82 maps through B713 to `$84:B408`; BTS83/84/85 map to
`$84:B497/B4A8/B4B6`. Native surface setup clears boost regardless of sample point,
but only its bottom sample changes extra vertical displacement. The other three
callbacks do not clear boost. This distinction must not become a generic
"any sand cancels boost" rule. These samples retain their original position and
pose: extra displacement is a published movement input, not immediate movement.

16 cases,6,416 observations (6,400 full prefix frames plus16 isolated samplers)
match every recorded field, including extra X/Y, with no production change.
Semantic assertions check counter and exact extra-Y words. Independent repeated
capture normalized SHA-256:
`C9BDA4C0D7117C1B019129096EC184D3D182F0392CF37F1CBD562D45133F7DB6`.

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" sand > csharp\test-temp\temporary-blue-sand.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-sand-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/sand.csv
```

Build the tool per README.md. Only numeric observations are published. This is
not a complete sand traversal, jumping/sinking sequence, or terrain-damage test.
Actual breakable terrain, repeated complete carry chains, full equipment-menu
transitions and persistent Blue Suit contrasts still keep429 open.
