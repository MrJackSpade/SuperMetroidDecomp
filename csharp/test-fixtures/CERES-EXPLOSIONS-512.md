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
