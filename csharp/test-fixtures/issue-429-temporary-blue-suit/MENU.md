# Temporary boost through the equipment menu

Partial #429. Unlike CANCELLATION.md's isolated equipped-word changes, this
matrix executes actual equipment input and teardown reconciliation. Twelve cases
combine both facings with six menu/resume controls after controller-earned boost.

## Execution

Frames 0..399 use the established run/crouch/R sequence and synchronous gameplay
audio publication. Frame 400 presses Start+R; 401..430 hold R through the native
30-frame darkening interval. Movement checkpoint 431 resumes after the menu.

Native executes pause admission $90:EA45, fade $80:8924, initial equipment selector
$82:AB47, suit-category Down $82:B0C2 to select Speed Booster, boots-category input
$82:B150, then reconciliation $91:E633. The native harness omits rendering-only
menu frames, not these gameplay-affecting routines. No equipped-word write fakes
the toggle. Original menu selection is asserted as Morph202 then Speed Booster203.

C# uses the real frontend for admission, fade, interactive map/equipment page,
Down/A toggles, delayed Start admission, teardown and resume. It does not write
the equipment selector or equipped bits. The preceding earned run-up executes
the runtime with the same audio-publication contract as the other matrices.
Frozen menu frames are not native movement checkpoints; an additional assertion
checks position, pose, animation, base speed, counter, charge and vertical speed
remain unchanged before teardown. Collected inventory must remain intact.

## Cases and results

| Mode | Menu operation | Resume input | Native result on checkpoint 431 |
| --- | --- | --- | --- |
| 0 | No toggle | R | Counter0401 retained |
| 1 | Disable Speed Booster | R | Counter cleared |
| 2 | Disable then re-enable | R | Counter0401 retained |
| 3 | Disable | Jump+forward | Counter cleared, real jump |
| 4 | No toggle | Jump+forward | Counter0401 retained on launch |
| 5 | Disable | Dash+R+forward | Counter cleared, ordinary running |

All continue through checkpoint559. Mode4 eventually loses temporary boost on its
ordinary landing; stationary modes0/2 retain it. Disabling clears the counter in
native reconciliation even though merely changing the word while crouched did
not in CANCELLATION.md. Do not conflate those two execution paths.

12 cases /6,720 movement checkpoints match every captured movement, animation,
boost/contact, charge and equipped-item field. Independent recapture normalized
SHA-256 `5713EBE32CAC36423538C7126C312D82103629D83109A3AD6D73B083D83CC2CE`.
No production change was needed.

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" menu > csharp\test-temp\temporary-blue-menu.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-menu-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/menu.csv
```

Build per README.md. No player saves/ROM/artwork are published. This closes the
equipment-menu gap. Comparison with actually acquired persistent Blue Suit is
still outstanding before429 is ready for player validation.
