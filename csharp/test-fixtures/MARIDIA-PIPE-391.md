# #391 Maridia elevatube descent investigation

## Status

The residual terrain-band defect is reproduced on the current code, not fixed.
The historical camera correction still carries Samus through the tube and into
Oasis. Do not mark this issue awaiting validation on the basis of state restoration
or camera coordinates alone.

## Restoring the preserved capture

The local, untracked `maridia-041b-pipe-entry/slot-0-named.smstate` predates several
state-layout changes. It is read through the production debugger state reader in
a disposable directory, never through the player's current slot. No state, ROM,
audio, or rendered images are included in this commit.

The initial current-build replay failed on the frontend's 46/47-field mismatch.
After that was reconciled, it exposed additional known historical layouts. Git
history from `b944f1b5` identifies the added/changed members; the reader continues
checking the declaring type, field name, and order of every surviving field.

| Historical layout | Explicit restoration |
| --- | --- |
| Frontend before `3a891459` | Missing pause-fade counter is zero: the next fade step is eligible, then follows native cadence. |
| Enemy owner before `833cc4ee` | Missing per-frame projectile dependency is null until EnemyMain supplies the live owner; composes with the older two-field statue migration. |
| PLM slots before `571b9a42` | Missing Samus Eater held coordinates are zero until an actual plant capture setup. |
| Samus before the independent bomb lock | Omit the nine additions proven against `b944f1b5`; retain the existing previous-draw input rather than mistaking the missing bomb lock for that older latch. |
| Kinematics before prospective-pose contact snapshot | Missing nullable contact mode retains lookup through the saved live Samus owner. |
| Projectile owner/slot/result before charge-combo integration | Restore no pending combo, auxiliary phase zero, and no additional sound-request list; preserve all existing projectile, charge, trajectory, and sound fields. |
| Suit pickup before its entry sound latch | No pending transformation sound; saved transformation phase remains intact. |
| DSP voice before silent-release BRR fallback | Missing nullable fallback retains the saved PCM/envelope state until a normal loop handoff. |
| PCM bank before cross-source loop routing | Construct an empty routing dictionary, preserving the old self-loop behavior until the next bank upload. This limitation is warned about, not presented as exact modern routing. |

The old nineteen-field Shinespark layout needs a different adapter: it stored
crash angular travel/delta separately from the echo coordinates now aliasing them.
The adapter reads the exact old field sequence and retains both removed values
until all echo records are restored. In a live crash phase it transfers those
values to the shared words, sign-extending the old signed byte delta. Outside a
crash it preserves the echo coordinates. Conflicting historical copies cannot
both occupy one native word; restoration warns about this limitation.

Synthetic tests cover the precise surviving field sequences, eight legacy spark
graphs (inactive plus three crash phases, both delta signs), current-format round
trips, rejection of an unknown legacy field, and initialization of the old PCM
self-loop map. The actual saved graph now loads and executes 420 frontend frames.

## Current visual reproduction

```text
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --maridia-pipe-from-north
```

The diagnostic loads the preceding tube through its real northern door setup,
starts Samus at `(128,64)`, then supplies neutral input. The frontend owns the
single transition from `$04/$18` (D408) to `$04/$1B` (D48E).

At frame120 Samus is Y1064 and camera Y938; at frame210 they are Y2335 and Y2209.
The exit reaches Oasis and returns to ordinary gameplay. Captured descent frames
90 and 180 visibly contain horizontal bands across the sand/tube background;
frame180 includes retained purple architectural strips in the surrounding sand.
These frames are local under `csharp/test-temp/issue-391-pipe-north`.

Next: capture each descent frame, compare visible streamed terrain rows with
their authored source and the pinned cartridge behavior, and make the actual
band/timing assertion fail before any rendering correction. The camera/exit
control must remain green. The snapshots alone do not yet identify the faulty
producer or prove that every architectural row is erroneous.
