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
