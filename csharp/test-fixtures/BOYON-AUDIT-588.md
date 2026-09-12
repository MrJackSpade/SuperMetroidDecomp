# Boyon diagnostic repair (#588)

The complete Alpha Power Bomb Boyon audit reproduced two stale expectations.
No production code changes were needed.

1. Body contact correctly removed ten energy and queued knockback, but the audit
   required immediate `KnockbackActive`. It now uses the shared domain helper to
   assert the unchanged pose/position, exact damage and side, pending timer,
   frozen-time rejection and single later bank-$90 admission.
2. The Crateria Super Ice check expected the frozen handler bit to clear on call
   400. Native `NormalEnemyFrozenAI` tests zero before decrementing: call 400
   reaches zero, call 401 clears the frozen handler. The audit now checks timer,
   handler bit and all four position words on every one of the 400 frozen calls,
   then checks the next-call thaw.

Verification: DebugRunner Release build passes with zero warnings/errors, and
`--alpha-power-bomb-boyon-audit "Super Metroid.smc"` passes completely. This covers
four retail Alpha Power Bomb actors, the 44-frame arc from Y $00A8 to $007A,
seven animation maps, contact, beam immunity, Super Missile/Power Bomb damage,
Grapple cancellation, death, OBJ output, and the separate $00/$1D Ice puzzle.

This repairs developer diagnostics only. It is not player confirmation or a
resolution of the deferred bad-capture report #524.
