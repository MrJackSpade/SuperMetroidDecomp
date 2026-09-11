# Crystal Flash parity investigation (#408)

Status: incomplete technique audit. Do not label the entire issue ready for player
validation based on the timer correction below.

## Hit-immunity reset omission

Pinned bank-$90 disassembly explicitly clears SamusInvincibilityTimer at:

- $90:D66C: successful activation, followed by knockback timer/direction clears.
- $90:D6A1: tenth raise call, immediately after switching movement handlers.
- $90:D6D6: after every ammo-handler call, followed by knockback timer at $D6D9.

The third clear is outside the individual ammo handlers' mod-eight NMI checks.
It applies even when no ammunition is consumed, including the final drain that
changes phase. The first nine raise calls do not execute it. The pinned C
translation agrees at `Hdmaobj_CrystalFlash`, `SamusMoveHandler_CrystalFlashStart`
and `SamusMoveHandler_CrystalFlashMain`.

The C# translation omitted all three immunity clears and the main-loop knockback
timer clear. Existing tests started with zero hit immunity, concealing the defect.
The production implementation now performs those exact writes at those boundaries,
without changing the INI health floor or granting Crystal Flash immunity.

The synthetic production-state regression now starts with immunity 96 and proves
rejected activation preserves it, while successful activation clears it. Before
the fix the latter assertion fails (expected 0, actual 96). It injects immunity
77 across the raising phase, proves nine calls preserve it and the tenth clears
it, and injects both timers for a no-drain main frame and all thirty drain calls.
All must clear on those calls, including phase completion. Existing resource,
palette, HDMA bubble, pose/history, and finish-animation assertions also pass.

Focused command:

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --crystal-flash
```

This is a first-pass, mechanically explicit source correction reproduced with a
constructed fixture. It is not a cartridge-CPU trace or a complete real-room damage
comparison. Do not conflate these timers with proof of every contact-damage path.

## Remaining acceptance work

Compare actual bank-$88 Power Bomb cleanup entry and the whole sequence against
the pinned cartridge, with cheats off. Cover every resource/input threshold and
adjacent failure, exact explosion-center positioning and vertical speed, the
ten-capacity refill-after-placement route, contact damage, animation and control
ownership through completion. Capture native/managed initial states and identical
input schedules. Wiki page retrieval timed out during this pass; no unverified
wiki claim was made an expected value.

## Original-CPU cleanup admission matrix

The new `movement-release/native-crystal-flash-probe.h` executes original
$88:8B4E, including its call into $90:D5A2, for 18 boundary cases in both facings.
The native loader restores the retail cartridge bytes after the upstream harness's
startup patches. CPU/RAM reset before every case; no cheats, player save or GUI.

Cases include health 49/50/51/0; each ammo family just below ten; nonempty reserve;
whole/fractional vertical velocity; one-pixel X/Y offsets; differing X/Y subpixels;
missing Down; extra Jump; an empty owned reserve; and current Power Bombs ten with
capacity ten. That last case establishes the admission check uses current ammo;
it does NOT yet reproduce collecting the refill after placement.

Managed comparison runs the real Power Bomb animation to cleanup via
`SamusBombProjectileSystem.StepFrame`, not direct Crystal Flash initiation.
It compares pose, armed flag, both hit timers, health and all three ammo counts.
All **36** cases match. In particular, whole-pixel offsets fail, subpixel offsets
do not; failed admission retains immunity 96/knockback 5, while success clears
them and retains the Power Bomb armed flag until the later Crystal Flash handoff.

Native CSV SHA256:
`CCD507BE8423FEC78122CD95458577F21F58624510578184BF51EE25FF22A5F3`.
The comparator checks this accepted capture identity and every row. Trace remains
local at `csharp/test-temp/crystal-flash-408-native.csv`, not published.

Apply `movement-release/native-crystal-flash-entrypoint.patch` inside the pinned
`upstream-sm` checkout. Build `src/sm.vcxproj` Release/x64 with v145 and absolute
SolutionDir ending in `upstream-sm\`. Then run from repository root:

```
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-crystal-flash "Super Metroid.smc" NEW_NATIVE.csv
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --crystal-flash-native "Super Metroid.smc" NEW_NATIVE.csv
```

The native output is exclusive-create. Temporary main/sm_rtl integration was
removed and normal native executable rebuilt after capture. This slice adds no
production fix; the prior timer correction now agrees with actual CPU activation.
Still outstanding: full input/resource boundary sweep, frame-by-frame entire
technique, real refill collection and contact-damage/animation/control comparison.

## Original-CPU movement and animation lifetime

`--diagnostic-crystal-flash-lifetime` now extends the native probe through the
installed movement handler, animation ($90:8000) and pose dispatcher ($91:EB88),
until normal movement returns. Both facings and all eight initial NMI phases
produce **4088** compared frames. Health starts at 49 with maximum 1499, each ammo
family at ten. The trace compares phase, pose, animation frame/delay, whole Y,
health/ammo, and the two hit timers on every frame.

Nonzero immunity 77 and knockback 5 are injected before each handler call to expose
its writes independently of any earlier clear. This found an additional omission:
the final normal-input completion should clear immunity ($90:D78C). The baseline
case failed at frame 257, native zero versus managed 77. The production finish
handler now performs the clear, and all 4088 frames agree. The standard focused
Crystal Flash test also checks this final clear without requiring the native file.

The first native fixture incorrectly left pending-pose words zero after RAM reset;
that requests pose zero, not 'no transition'. It was rejected. The accepted fixture
initializes all three pending-pose words to $FFFF and liquid heights to $FFFF.
Earlier incomplete local captures are not accepted by the comparator.

Accepted lifetime CSV SHA256:
`CA77210D138C654AEF79E44AAA897B5BE0F79244E2F72AB362D46303521BC043`.
Commands after applying the same headless entrypoint patch:

```
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-crystal-flash-lifetime "Super Metroid.smc" NEW_NATIVE.csv
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --crystal-flash-lifetime "Super Metroid.smc" NEW_NATIVE.csv
```

The native and managed probes call movement/animation owners directly; they do
not execute the whole room, contact damage, input-handler interruption, palette
or bubble rendering in this lifetime comparison. Native demo input has a distinct
completion branch and is not covered by this normal-gameplay fixture. Actual
refill collection and contact-damage/full-runtime ownership checks remain open.
No issue completion/validation label follows from this partial comparison.

## Full runtime placement and refill integration

`--crystal-flash-runtime` (also in standard verification) initializes a flat
constructed clearing in Landing Site and runs `SuperMetroidRuntime.StepFrame`.
Samus starts grounded in Morph Ball, cheats off, with 49/1499 energy, ten missiles
and supers. Normal Shoot placement consumes one Power Bomb. The exact chord is
held until cleanup; after activation, held Right/Jump challenges input ownership.

Three routes pass:

- Eleven-capacity/no refill: placement leaves ten; activation at fixture frame
  245, completion at 502; final health 1499 and all three ammo counts zero.
- Ten-capacity/refill: placement leaves nine; a Power Bomb drop is collected at
  frame 60, restoring one without changing capacity. Same activation/completion.
- Ten-capacity/no refill: no activation in 1000 frames, no healing or extra
  consumption, and the Power Bomb armed lock is released.

The refill is not a direct ammo assignment: the existing constructed enemy-drop
fixture collides a real pickup projectile with the runtime's Samus, checking its
collection identity, sound and effect. That drop uses a separate constructed enemy
owner rather than a naturally killed enemy in the runtime's room. This bounds the
integration claim; enemy generation and original-CPU refill timing remain separate.

Successful routes assert the Crystal Flash bubble phase, ammo-drain phase, exact
two-pixel-per-frame ten-call rise, fixed X despite held Right/Jump, full resource
result, and resumed movement after completion. No production correction was needed
in this slice. This is runtime integration evidence, not a native full-room replay
or a visual pixel comparison. Complete boundary sweep and native full-runtime
ordering still need coverage before closing the technique audit.

## Actual contact during ammo drain

The eleven-capacity route now inserts a stationary retail Ripper at Samus's current
position thirty frames after activation. The population and position are constructed;
the header, AI, spritemap, contact dispatcher, damage and runtime update order are
production paths. The first authored map is seeded because contact precedes the
actor's first AI update. This is deliberately a controlled contact, not a naturally
encountered enemy or a native full-room replay.

On this frame health changes from 149 to 144 (retail damage five). The test checks
contact-before-refill ordering, zero immunity and knockback timers, absence of active
normal knockback, continued fixed Crystal Flash trajectory, and eventual completion
at frame 502 with health 1499 and ammo zero. The conditional expected refill follows
the normal every-eighth-NMI handler; this particular contact frame does not refill.

Mutation verification: temporarily removing the already-implemented main wrapper's
two timer clears makes this actual-contact test fail: expected immunity zero, got 95.
Restoring the production clears passes all three runtime routes. The mutation is not
retained and no new gameplay change is claimed. Header damage is sampled before the
frame because later projectile processing may delete/recycle the actor. The fixture's
nonzero RNG avoids an invalid zero-only drop-selection loop.

This closes the managed full-runtime contact coverage gap, not the remaining native
contact/timing comparison or visual acceptance scope. Issue #408 remains open without
an awaiting-validation label. No private captures are required for this regression.

## Expanded cleanup admission boundaries

The original-CPU cleanup probe now runs 44 cases in both facings (88 comparisons).
Alongside the original eighteen cases it exhausts all sixteen subsets of held
Down/L/R/Shoot, remaps Shoot to B with a correct-button and old-button control,
checks one-pixel offsets on the opposite side of each explosion-center axis, and
checks zero and eleven units independently for all three ammo families. Only
the complete required chord is admitted. Adjacent whole-pixel offsets fail;
ammo eleven succeeds and zero fails, with the original nine/ten boundary retained.

For the remapped cases, the managed fixture uses the production controller
normalizer before bomb alpha, matching the runtime boundary; it does not bypass
cleanup or change Crystal Flash's canonical input argument. Native executes the
original configurable Shoot-word lookup. No gameplay cheats are enabled.

Two independent captures have identical SHA256:
`4A6DEB28F4CE497B73D45EFFC7164C78D62F4A6B12A113FE8CE581A30AE3A223`.
The existing `--crystal-flash-native ROM CSV` comparison validates either this
expanded capture or the earlier 36-case capture, checking identity, row count,
and every result. All 88 expanded cases pass without a production change.
Private outputs are `csharp/test-temp/crystal-flash-408-expanded-a.csv` and `-b.csv`.
Use the same committed headless entrypoint patch and capture command described
above to regenerate. Temporary hooks were removed and the normal native binary
rebuilt after the experiment; unrelated native checkout changes were preserved.

This improves activation-boundary evidence, not full-game contact ordering or
rendered bubble/palette acceptance. Those remaining requirements keep #408 open.

## Original-CPU contact followed by refill

The headless `--diagnostic-crystal-flash-contact ROM CSV` probe extends the
movement/animation lifetime with one deliberately admitted normal touch at frame
30, before the beta movement handler. It executes original `$A0:A4A1` using the
retail Ripper damage header, then the installed Crystal Flash movement, animation
and pose-transition routines. Three suit states (none, Varia, Gravity), both
facings and eight initial NMI phases produce 12,264 compared frames through
completion. Unlike the timer-injection lifetime probe, no artificial hit timers
are inserted during these sequences.

Managed comparison command: `--crystal-flash-contact-native ROM CSV`. Its
constructed overlapping Ripper enters the real ordinary contact dispatcher;
the production interactive list is built in a frozen enemy pass so AI does not
advance before this intentionally admitted contact. It asserts immediate damage
of five/two/one, immunity 96 and pending knockback five before movement. Every
post-movement frame then compares phase, pose, animation frame/delay, Y, health,
ammo and both hit timers against the native trace. All 12,264 frames match,
including refill and no-refill timing phases, timer clearing and normal completion.
No production change was required.

Native trace SHA256:
`A9E43DA426DD832D9DFBBCF820A374CDA109A2E715764E5012B6AF9174CCB5F3`.
Private output: `csharp/test-temp/crystal-flash-408-contact-a.csv`. The shared
entrypoint patch includes this command and passes `git apply --check`; temporary
hooks were removed and the normal native executable rebuilt after capturing.

This compares the admitted touch/handler sequence, not the native enemy broadphase
or whole-room scheduling. The separate full-runtime contact fixture exercises
managed room ordering. The rendered bubble/palette and original-game integrated
presentation remain unverified; #408 is still open without a validation label.
