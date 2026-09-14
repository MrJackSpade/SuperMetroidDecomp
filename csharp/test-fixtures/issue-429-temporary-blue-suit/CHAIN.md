# Five consecutive temporary-boost carry cycles

Partial #429. Extends CARRY.md without resetting Samus between cycles. The first
400 frames earn and retain expired-charge boost through run/crouch/R. Frames
400..999 repeat the120-frame controller cycle five times. Each cycle jumps,
releases forward during ascent, uses two Down edges to morph, soft-unmorphs with
Up+R, lands holding R, releases Jump, and begins the next jump.

Eight modes use carry timing IDs18,19,20,26,27,28,38,39 respectively. These place
Up at cycle offsets84,85,86,92,93,94,104,105. Both facings run independently.
No state, velocity, pose or boost is injected between cycles. Geometry and
equipment remain unchanged. Native runs the original input/movement/animation/
pose/charge phases; the port runs its normal room gameplay dispatcher.

## Observations

Modes1/2/3/5/6/7 retain0401 through all five cycles. At each cycle's phase40 Samus
is airborne; at119 she has returned to the exact ground center01F0.FFFF in
diagonal-up-aim crouch71/72. These properties have explicit semantic assertions,
in addition to exact per-frame position/subpixel, pose, animation/timer, velocity,
boost/contact and charge-field comparisons. Charge stays expired throughout.

Modes0/4 lose boost at494, during their first cycle. They continue the same input
schedule afterward and do not reacquire boost. These controls cover both too-early
unmorph and the first-bounce-frame timing that ignores the unmorph input. A test
that only covered the nearby successful window would miss the latter failure.

16 cases /16,000 complete gameplay frames match without production changes.
Independent recapture normalized SHA-256:
`B5097BFAA561907059B142478386125AD185DCC969F250FD2086A27987334957`.

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" chain > csharp\test-temp\temporary-blue-chain.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-chain-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/chain.csv
```

Build per README.md. No ROM or player-state content is published. This closes
the multiple-complete-cycle evidence gap, not full equipment-menu transitions
or comparison against an actually acquired persistent Blue Suit. Those remain
outstanding before429 is ready for player validation.
