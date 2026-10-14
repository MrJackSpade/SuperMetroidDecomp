namespace SuperMetroid.Core.Game;

/// <summary>The bank-$A4 shot callbacks named by Crocomire's extended hitbox records.</summary>
internal enum CrocomireHitboxShotCallback : ushort
{
    /// <summary>Crocomire no-op hitbox shot callback at $A4:B951; it still counts the hit.</summary>
    NoOp = EnemyAiCodePointers.BankA4.NoOpHitboxShot,
    /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
    Dust = EnemyAiCodePointers.BankA4.DustHitboxShot,
    /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
    Mouth = EnemyAiCodePointers.BankA4.MouthShot,
    /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
    AlternateDust = EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
    /// <summary>Records that deliberately point at the body header's RTL at $A4:B950.</summary>
    HeaderReturn = EnemyAiCodePointers.BankA4.HeaderTouch,
}
