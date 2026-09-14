# Temporary boost through aerial soft unmorph

Extends [the initial expired-charge fixture](README.md), still partial #429.
This branch covers a jump, two Down edges to morph, a timed Up edge to unmorph,
angle-held crouched landing and a second jump. Both facings include the successful
window, neighboring failures and a failure on the exact first-bounce frame.
Controls release all input, reverse direction, or land normally without morphing.

## Native evidence

Same ROM, CPU adapter, floor, starting state, equipment and audio-publication
contract as the initial fixture. Run forward+B for140 frames, crouch with R on140,
and hold R until400. At399 boost is `$0401` with charge expired; it is not injected.
The `aim` CSV column now names a carry mode, while `stop` remains140.

Modes0..3: jump+forward at400, then at415 respectively continue, release everything,
reverse while holding Jump, or retain Jump alone. These establish distinct native
cancellation boundaries. Neutral input cancels the counter on415 but keeps that
frame's contact damage. Reversal enters the turn pose on415; its movement cancels
the counter on416 but retains contact until417. Ordinary landing loses the counter
on493 in these samples, even when Jump remains held.

Modes4..39: hold Jump+forward through410, Jump alone411..419, Jump+Down420,
Jump421, Jump+Down422, then Jump+forward. On frame `466+mode` press
Jump+Up+R+forward. Then hold Jump+R until510, R alone510..519, and Jump+forward
from520. The Up sweep is470..505 inclusive. Every mode runs620 frames.

On540, modes19..26 and28..39 have retained `$0401`, contact damage one, and are
actually airborne on their second jump. Modes4..18 unmorph too early and lose
boost. Mode27 presses Up on493, the first floor-bounce frame: the ball does not
unmorph, and removal of forward input on494 cancels boost. Thus a continuous
"earliest through latest" timing approximation would be wrong. Both facings
match independently; this is not inferred by mirroring the right-facing trace.

80 cases, 49,600 frames. Exact comparison fields and semantic assertions include
position/subpixels, pose, animation/timer, base/extra speed, boost/contact, charge
timer/type and vertical speed/direction. Independent repeated capture has the
same normalized SHA-256:
`C7486E42C4A0247665E4ABC1FD076A9E1D4D6305056E72C181136B6F5BB2B414`.

## Reproduced omission and fix

Only two frames differed before the production fix: frame416 of each reversal
case had contact zero instead of one. Native `$90:A790/$90:A7AD` call the shared
X mover `$90:8E64`, which calls extra-run-speed processing `$90:973E` before
performing X movement. The port went directly to base-speed calculation, omitting
that common epilogue. It now runs the same update before movement and the existing
post-movement boost cancellation. No synthetic one-frame grace period is added.

After the fix all 49,600 frames and semantic gates match. No other carrying rule
was changed. Grounded/airborne bounce and Spring Ball coverage, repeated complete
soft-unmorph chains beyond this second jump, sand/equipment/X-Ray resets and
persistent Blue Suit contrasts remain open. This is not complete #429 validation.

## Reproduce

Build the native tool as described in README.md, then generate an independent
trace and compare the committed reference:

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" carry > csharp\test-temp\temporary-blue-carry.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-carry-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/carry.csv
```

The command redirection preserves the generated ASCII CSV as ordinary diffable
text, avoiding Windows PowerShell's UTF-16 redirection default. No ROM or player
state is published. Native routine identities were cross-checked against pinned
`upstream-sm/src/sm_90.c` and the bank references linked in README.md.

Verification also passes the earlier 12,800-frame retention matrix and full
Release Verification. Windows Release builds with zero warnings/errors.
