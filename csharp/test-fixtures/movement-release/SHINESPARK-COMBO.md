# #466 Shinespark/combo crash investigation

Status: ready for player validation. Retained-word entry, departure, slot ownership
and reset fixes are verified, including successive crashes through the full
gameplay frame dispatcher. Sections below preserve chronological evidence.

## Runtime acceptance

`--spark-runtime-sequence-audit ROM CSV` runs the same 180-crash matrix through
`SuperMetroidRuntime.StepFrame(0)`, using `FlatFloorMovementFixture` with no
water and no room transitions during the samples. Both gameplay cheats are
explicitly rejected by the audit. This exercises ordinary input sampling, pose
handling, alpha projectile/instruction dispatch, beta crash movement and camera
processing. Each sampled frame must actually dispatch the installed shinespark
handler. All **11,688 records match** the accepted original-CPU sequence capture.

The initial crash/contact and combo allocation boundaries are still explicitly
seeded, as documented below. Pre-aging and inter-crash gaps run only projectile
updates to preserve the oracle's stationary setup; they are not falsely labeled
as a whole controller route. The runtime frame test covers the affected recovery
interval end to end. Input is zero throughout that interval. Normal charge-combo
activation has its own real-input fixtures under #416; launch/storage is #465.

The verified defects were shared Y/angular travel reset on entry, wrong capacity
input, conflated radius/drawing speed, independent projectile ownership, missing
reentry drawing/X-delta writes, and incomplete whole-reset clearing. None were
replaced with combo-specific timing shortcuts. Original unusual lifetime and
counter behavior is retained. Supported evidence is for the pinned NTSC revision
only; no claim of PAL/revision equivalence. Leave #466 open for player confirmation.

## Ordered successive-crash comparison

`native-spark-sequence-probe.h` uses original HandleProjectile ($90:AECE),
including projectile instruction processing, before each installed crash handler.
The 60 initial setups cover no combo plus all four combos, both facings, ages
0/580/640 updates, and with/without prior departing echoes. Each performs three
crashes, separated by 20 projectile updates. Samus is stationary at (128,128),
subpixels/RNG initially zero, camera (0,0), Charge plus the named beam, PB two,
HUD index three, no controller input. Each crash contact boundary is explicitly
seeded with the appropriate horizontal spark pose and health 29; there is no
invincibility or infinite ammo. These are handler-sequence fixtures, not a
controller-driven launch demonstration.

The managed audit invokes production projectile StepFrame with the producer
disabled (matching native HandleProjectile rather than its input caller), then
the real shinespark movement step. It compares every frame's handler, projectile
count/types, orbit index, shared echo X/Y words and drawing enables, plus exact
call counts. All **11,688 records across 180 crashes match**. The original CPU
confirms that prior retained history changes the first recovery; combo presence
alone is not a fixed-duration switch. Adjacent no-history/no-combo controls are
included. These results required no further production change.

Accepted CSV in `spark-sequence-466-v2.zip`, SHA-256
`ABF726C92E5A57AEA03FB9F3A015BF42C4400ED7C0DC6C8C00C55B3AE7827856`.
Two captures are byte-identical. Regenerate with
`native-spark-sequence-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-sequence ROM NEW.csv`. Compare using hash-gated
`--spark-sequence-audit ROM CSV`. Temporary native hooks removed.

That checkpoint's remaining full-frame integration check is now completed in
the runtime acceptance section above.

## Whole-reset checkpoint

`native-spark-reset-probe.h` finishes a spark, overwrites slot three with each
combo in both facings, then executes original ResetProjectileData ($90:AD22).
It records the owners/count and independent drawing enable/X/Y before and
after reset. Two captures are byte-identical; accepted CSV is archived in
`spark-reset-466-v1.zip`, SHA-256
`FF69264A9DF1E7BAE216203F2648A9177699D7CF2B436404CD78AC67D58DFFB9`.

Before correction, all eight post-reset records failed: ordinary projectile
slots cleared but Samus's shared echo words survived. Gameplay reset calls now
supply their Samus owner, clearing both ordinary speed-echo arrays and crash
echo drawing storage, including the aliased angular delta/travel and orbit
index/angles. Unrelated movement handler phase and timers are not reset.
Standalone projectile-only hosts retain a reset without an associated Samus.
All 16 before/after records now match; full core verification passes.

Regenerate with `native-spark-reset-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-reset ROM NEW.csv`; compare using
`--spark-reset-audit ROM CSV`. Temporary native hooks removed after capture.
Full successive-spark alpha/beta/controller sequences remain outstanding.

## Crash reentry drawing/delta checkpoint

Original-CPU `native-spark-reentry-probe.h` finishes a spark, activates each of
the four combos, then enters another low-energy crash in both facings. It
records allocation, entry, and five orbit updates, including the first angular
step. Only handler boundaries are seeded; all shared-word writes execute the
original ROM. The remaining slot-four echo is deliberately not advanced during
the sampled crash-handler calls, so this does not claim a full alpha/beta run.

Retail entry clears the first departing echo drawing enable, stores signed
angular delta +/-4 in its X word ($0AB4), and retains its Y word ($0ABC).
C# previously kept drawing enabled and stored delta in an independent field.
Before correction the managed comparison disagreed on 48 of 56 records. The
implementation now disables only that drawing flag and aliases the complete
signed delta word to echo X. It does not delete the owning projectile or clear
the retained Y/angular-travel word.

After correction all 56 records match. The 176-record ownership matrix,
480-record departure matrix, eight retained-word timing cases and full core
verification also pass.

Two original captures are byte-identical. CSV in `spark-reentry-466-v1.zip`,
SHA-256 `5C6C1FB23CA978493EEE047164CD9C97E749E5BECA0190F96592B5864316A53D`.
Regenerate with `native-spark-reentry-entrypoint.patch`, bounded/dialog-free
`--diagnostic-spark-reentry ROM NEW.csv`. Compare with
`--spark-reentry-audit ROM CSV`. Temporary native hooks removed after capture.

Whole-reset clearing and complete successive-spark alpha/beta sequences remain
required before marking #466 ready for player confirmation.

## Shared-ownership reproduction and implementation

`native-spark-ownership-probe.h` executes both boundary orders for all four
ordinary combo families: FireSBA then crash finish, and crash finish then
FireSBA. It records each allocation and 20 updates of only the still-installed
departing echo handlers. Combo motion is deliberately not advanced in this
boundary fixture; the family motion audits cover it separately. This is not
yet an end-to-end controller reachability proof.

The original CPU confirms three distinct properties:

- Combo first: count four admits only echo slot four and increments count to
  five. Viewport loss decrements it back to four.
- Echo first: FireSBA overwrites slot three and resets count to four, while
  leaving slot four's echo allocated. Losing that echo decrements count to
  three. The counter is not derived from the number of nonempty slots.
- Overwriting slot three does NOT clear its separate echo drawing enable or
  X/Y words. Its old echo handler no longer runs, so retained Y remains 128 in
  this fixture. Drawing enable is not synonymous with projectile ownership.

`--spark-ownership-audit ROM CSV` exercises the corresponding production
allocation/update boundaries and compares counter, slot-three/four types,
drawing enables and coordinates. Before implementation: **152 mismatches / 176
records**, affecting both orders and all four families. The initial checkpoint
committed that failing reproduction without a production change.

The implementation now allocates slots three/four through the ordinary
projectile system, initializes their ROM instruction data, maintains the native
counter, and dispatches their echo pre-instructions in the ordinary descending
alpha pass. The independent runtime echo update was removed. Combo replacement
therefore stops the old handler naturally, without clearing independent echo
drawing words. The matrix now matches all 176 records. The six-pose departure
audit also uses these owned slots rather than the standalone helper.

The runtime capacity fixture additionally checks actual allocated slot owners,
counts two/five for no-combo/active-combo cases, and exactly one radius update
in the next real alpha pass (including instruction-list processing).

Two captures are byte-identical. Accepted CSV in `spark-ownership-466-v1.zip`,
SHA-256 `25FFBFE7013658F9123ED41F67928C3A410EDD256E450B160432654C0F251D5F`.
Use `native-spark-ownership-entrypoint.patch` and the bounded/dialog-free
`--diagnostic-spark-ownership ROM NEW.csv` entrypoint to regenerate. Temporary
native hooks were removed after capture. The managed audit is hash-gated.

Whole-reset clearing and full successive crash sequences remain additional
required coverage. The crash-entry drawing/delta checkpoint above covers its
entry writes, but not the complete successive-spark lifecycle.

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
