# Ceres explosion spawner timing (#512)

The reported duplicate-looking explosion sets remain open for visual confirmation.
This change repairs a demonstrated timing defect, not a claim of whole-cinematic
pixel equivalence to an emulator recording.

## Evidence and correction

The pinned bank-$8B disassembly's list at CE35 waits $80 frames, creates five
staggered explosions, waits another $50 frames, installs pre-instruction C489,
and runs it during the final $40-frame wait. C489 repeats every $0C frames and
disables itself when the cinematic enters the gunship-flying-away function.
CE47 still creates the four terminal explosions independently.

The port instead began repeating explosions immediately after the first group,
during the intervening wait. Its range guard also lacked the departure cutoff.
The correction moves that range after the wait and stops repeating on departure.

## Reproduction

Run DebugRunner with:

```
--ceres-explosion-timing-audit "Super Metroid.smc" OUTPUT_DIRECTORY
```

The audit runs the production cinematic, observes new actor identities, and reads
the expected wait durations and repeat period independently from the retail ROM.
It excludes the separate actor created at departure. It does not emulate native
CPU execution or establish absolute frame alignment with a music-driven capture.

Before: initial group at 128; repeats at 129,141,153,165,177,189,201; terminal
group at 272. The assertion fails.

After: initial group at 128; repeats at 209,221; departure at 222; terminal group
at 272. The assertion passes. Captures stay in ignored local diagnostic folders.
The frame-129 before/after images themselves look identical: birth timing is not
the same as the first visible sprite frame, so those images are not visual proof
of the entire reported defect being resolved.

Windows Release build passes. The broader `--ceres-destruction-audit` reaches the
Landing Site but fails its later fresh-frontend gunship-save assertion with state
MainGameplayFadeIn; that end-to-end audit is not reported as passing.

Follow-up: that assertion assumed the controller helper returned in state eight,
but it returns as soon as room loading creates the runtime, during state-seven
fade-in. The audit now validates exact restored checkpoint fields at that boundary
and separately requires both state eight and restored movement after the native
appearance sequence. No production behavior changed. The complete audit then
passes: 1678 cinematic calls, 641 landing frames, and checkpoint 10.41 restored;
the Landing Site left-travel checks also finish. This corrects the test failure
above, without upgrading the unconfirmed visual report to a verified fix.

## Follow-up: duplicated approach scale increment

The production approach handler called a drift helper that incremented scale,
then incremented scale again itself. Native `$8B:C345` increments once; the
preceding fade handler `$8B:C2F1` also increments once. Scale advancement now
belongs to those individual phase handlers, not their shared position helper.

`--ceres-zoom-timing-audit "Super Metroid.smc" OUTPUT_DIRECTORY` reproduced
177 incorrect per-call scale increments before the fix and zero afterward.
Departure moved from zero-based step 221 to 398. These step indices differ from
the explosion actor clock used above; they are not emulator recording indices.
Same-step 220 captures show the changed station scale. This is timing and scale
evidence, not a claim that every explosion pixel matches native rendering.

With the corrected approach duration, the explosion audit passes with the
initial five actors at 128, repeats at 209/221/233/245/257/269, and terminal four
at 272. The formerly premature departure no longer truncates that repeat range.
The complete Ceres/Zebes audit passes at 1854 calls, including landing and save
restoration. Full Verification and the Windows Release build also pass.

The duplicate-looking explosion report remains open: native visual comparison
of the actor sets is still required before claiming the entire report resolved.

## Original-CPU actor comparison

`movement-release/native-ceres-actors-probe.h` runs the retail `$8B:938A`
initializer and `$8B:93EF` sprite handler for all parameter variants of CEBB,
CEC1 and CEC7, independently for 260 calls each. The bounded headless entrypoint
patch suppresses native dialogs. Use the existing native Release build workflow
after applying `native-ceres-actors-entrypoint.patch` with `--unidiff-zero`, run:

```
sm.exe --diagnostic-ceres-actors "Super Metroid.smc" NEW_PRIVATE_CSV
```

Reverse only that patch after the run. Output creation is exclusive; do not
overwrite an earlier trace. No trace, ROM, screenshot or compiled binary belongs
in the public commit. The loader restores original ROM bytes before CPU execution.

Then run DebugRunner:

```
--ceres-actor-native-audit "Super Metroid.smc" PRIVATE_CSV
```

The managed audit observes actors spawned by the real production scene, retaining
their identities after deletion. Native origins use a zero camera; subtracting
the production camera at each birth compares exact initial offsets and subsequent
integer/subpixel motion. All 15 actual spawner children match across 3,900 states:
active/deleted status, X/Y and subpositions, and selected spritemap. The native
trace also contains the two secondary parameters not reached by this schedule;
those extra variants are not claimed as production-scene comparisons.

This eliminates actor animation/lifetime/motion discrepancies for the tested
sequence. It does not compare final OAM or composed pixels, does not exercise
native full-scene scheduling, and excludes the separate departure explosion.
Those boundaries remain important to the still-open visual report.

Follow-up draw coverage: the native probe now additionally runs the original
`$8B:9746` draw routine after each sprite-handler call and records the used low
OAM bytes and all high-table bytes. Regenerate the private trace with this probe
version before running the managed audit; the CSV now requires `oam,high` columns.
The managed comparison invokes each production actor's `Draw` with an inverse
birth-camera offset to restore the native zero-camera origin. All 3,900 frames
also match exact OAM bytes, including component order, tile/attribute words,
coordinates, size and X-high bits. Invisible/deleted frames emit no low OAM.

This supersedes the earlier absence of an OAM check, but only at that normalized
origin. Real-scene clipping, interaction with other actors, VRAM/CGRAM contents,
Mode 7 composition and full-scene scheduling are not established by this test.
No production draw change was justified by these results.

## Whole-population reproduction and correction

The next comparison exposed what the isolated checks could not. Use the same
headless native entrypoint/build workflow with:

```
sm.exe --diagnostic-ceres-scene "Super Metroid.smc" NEW_PRIVATE_CSV
--ceres-scene-native-audit "Super Metroid.smc" PRIVATE_CSV
```

The native probe seeds the documented C11B scalar/actor initialization, runs
original CPU phase handlers, the complete sprite handler and the original draw
routine. It deliberately fixes the initial music wait at fourteen calls to match
the no-audio managed fixture; it is not an SPC or full machine boot comparison.
It does not initialize native graphics memory or render native pixels.

Before correction: the first OAM mismatch is zero-based frame 127, with 69
managed sprites versus 68 native. The port omitted the initial call that fetches
CF33's first instruction duration. Adding that call moves the one-based birth
schedule to 129; 210/222/234/246/258/270; and 273. This supersedes the earlier
ROM-operand-only audit's absolute schedule, which shared that missing fetch.

After correcting timing, unordered sprite components match every compared frame,
but OAM order still differs on 270 of the first 398 frames. Production used list
creation order, whereas native allocation reuses the highest free numbered slot
and draws in descending slot order. Track those native slots for Ceres, reserve
the invisible spawner's slot through its last instruction, release deleted slots,
and draw in native slot order. Full-slot allocation returns failure as native
938A does; its cinematic callers intentionally ignore that carry result.

After both corrections: all 600 compared frames match phase, scale, camera,
brightness, the complete unordered component population and exact low/high OAM
bytes. This includes departure and the post-explosion hold. Full Verification,
Windows Release build, and the complete Ceres/Zebes landing/save audit pass.

These are reproduced timing and composition fixes, now ready for player
confirmation. Whole-renderer pixel parity and real audio-queue timing remain
outside the evidence; the issue should stay open with awaiting-player-validation,
not be closed solely on this diagnostic.
