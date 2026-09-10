# #466 Shinespark/combo crash investigation

Status: retained-word crash entry fixed; full combo/echo integration remains
incomplete. No awaiting-player-validation label.

## Released-radius checkpoint

Original `$90:D40D` initializes projectile X velocity (released radius) to zero,
not the 64 written to the separate speed-echo drawing field. The former C#
translation conflated these words and started departure eight updates too far
along. This also changed viewport loss timing and therefore the lifetime of the
Y word shared with crash angular travel.

`native-spark-departure-probe.h` executes original finish and echo handlers for
all six crash poses at (128,128), camera (0,0), count zero. It records both echoes
at spawn and 39 subsequent updates, including viewport deletion. The two
independent captures match byte-for-byte. Accepted CSV in
`spark-departure-466-v1.zip`, SHA-256
`DF0DDF8BDD56259EB2EBEFA8F288FA975625D35192AA6E3EF0B86E1AEFCF534A`.
`--spark-departure-audit ROM CSV` compares active state, radius, X and Y on all
480 records. Before correction: 224 mismatches across all six poses. After:
zero mismatches. The older synthetic test's incorrect 64/72 radius expectations
were replaced with 0/8, and its deletion horizon extended accordingly.

Regenerate with `native-spark-departure-entrypoint.patch` and only the bounded,
dialog-free `--diagnostic-spark-departure ROM NEW.csv` native entrypoint. Remove
the hook after capture. This fixture verifies departure geometry/lifetime, not
the outstanding shared projectile-slot ownership or successive combo sequence.

Pinned retail ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host pin `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly pin `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

The cartridge's crash entry `$90:D2BA` does not clear `$0ABC`
(`SpeedEcho_YPosition2`). Crash phase one `$90:D396` reuses that word as angular
travel and exits once its signed comparison reaches 128. Released crash-echo
creation `$90:D40D` also uses the word as a Y coordinate. Its clearing/ownership
must be traced through native echo/projectile processing; combo presence alone
is not an appropriate replacement condition.

`native-spark-retained-probe.h` executes original crash entry and handlers up to
the finish handler, with facing left/right, retained word 0/64/128/192,
position (192,192), zero subpixels/RNG, and health 29 to enter the low-energy
crash branch. No cheats, no synthesized CPU instructions or GUI. These are
explicit handler-boundary seeds, NOT proof of which combo sequence produces
each retained value. Two independent captures are byte-identical.

Accepted CSV in `spark-retained-466-v1.zip`, SHA-256
`A448393C73B278C99EE0B6C9DDCD1B450A5808289F67950479C8C2D41997599A`.

`SparkRetainedAudit` seeds the modeled counter before invoking production crash
entry, then executes the real movement step and compares each frame's handler,
radius/subphase, angular travel and both echo coordinates. Reflection is confined
to boundary setup; it does not implement the crash routine. Both cleared-counter
controls match all 70 calls. Both facings fail with retained values:

| Retained word | Cartridge calls to finish handler | C# calls |
| --- | ---: | ---: |
| 0 | 70 | 70 |
| 64 | 54 | 70 |
| 128 | 39 | 70 |
| 192 | 39 | 70 |

Those were the pre-fix results. The implementation now aliases
`CrashAngularTravel` to the first released echo's Y storage, removes the
non-native crash-entry reset, and does not clear existing echo storage when
crash-finish capacity rejects allocation. All eight groups now match every
recorded handler/subphase/travel/echo-coordinate frame and the 70/54/39 call
lengths. The diagnostic returns zero. No combo-dependent timer was introduced.

The ordinary verification fixture additionally asserts retained travel 128
after capacity-four/five finish and equality of inactive echo Y and travel.
The native `$90:D40D` capacity branch does not write slot-three echo Y when
that allocation is skipped. Existing departure/movement/deletion tests pass.
This fixes the reproduced crash-state behavior, not the whole #466 scenario.

Regenerate: apply `native-spark-retained-entrypoint.patch` to the pinned native
host, build Release x64 and run only the bounded/dialog-free
`sm.exe --diagnostic-spark-retained ROM NEW.csv`. Remove hooks afterward (done).
Compare: `SuperMetroid.DebugRunner --spark-retained-audit ROM CSV`.

Next required work: verify the unified value's native clearing on released-echo
loss and projectile reset, shared fixed-slot ownership with combos, then reproduce
successive sparks with active, just-ended and fully-cleared combos. The direct
counter fixture proves the present reset defect but does not satisfy the whole
ticket's resource/echo lifetime and successive-input acceptance requirements.

## Runtime capacity wiring checkpoint

`SparkCapacityInputAudit` uses the real runtime alpha/beta frame in a constructed
flat room. It seeds only the CrashFinish boundary, with the appropriate horizontal
spark pose, and activates each real combo through the production allocator.
The control has no combo; all five cases have zero bombs. Before the fix, every
combo case allocated two separate departing echoes despite four ordinary
projectiles being active. Runtime passed BombCounter where native `$90:D40D`
reads the ordinary ProjectileCounter. It now passes the correct counter.

`--spark-capacity-input-audit ROM`: four failing cases before, all five passing
after. The control admits two echoes; each four-particle combo admits only slot
four's echo. This checks the real caller, not a test directly supplying the
desired count. The underlying fixed projectile-slot allocation/counter ownership
is still separate in the current model and remains required work; passing the
admission test is not evidence that this later ownership is already correct.
