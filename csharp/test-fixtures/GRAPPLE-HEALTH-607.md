# Grapple health audit (#607)

Affected player version: v0.3.1. The universal instant-kill report remains
unreproduced; this change improves diagnostic assertions, not gameplay.

Run `SuperMetroid.DebugRunner --retail-enemy-grapple-audit <ROM>`.
The audit loads authored populations, waits for natural interactive activation,
isolates the endpoint target with temporary invincibility on companions, and
executes the real endpoint dispatcher followed by the real enemy frame.

The stale death assertion expected Deleted to survive. Native EnemyGrappleDeath
($A0:9FC4) calls EnemyDeathAnimation ($A0:A3AF), which clears the record and
optionally installs the respawn placeholder. The assertion now verifies that
identity, zero health, and exactly one kill. Every nonlethal reaction must retain
the original health and definition without becoming Deleted.

Result: 657 variants inspected; 451 reactions across 119 definitions passed:
None 11, Attach 9, Kill 139, Cancel 274, AttachWithoutInvincibility 1,
HurtSamus 17. Forty-one definitions never became interactive within 2048
fresh-load frames. AttachAndParalyze was not exercised. This is not proof of
every later encounter, frozen variant, or player input sequence.

The pinned upstream sm_a0.c endpoint and reaction dispatch select instant death
only for the header's Kill pointer; that reaction is independent of health.
No production damage behavior was changed. A specific unexpected victim or
recording is still needed to reproduce the player report.
