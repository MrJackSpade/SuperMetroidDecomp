namespace SuperMetroid.Core.Game;

/// <summary>Bank-$86 addressing shared by enemy-projectile code; instructions and pre-instructions are <see cref="EnemyProjectileInstruction"/> and <see cref="EnemyProjectilePreInstruction"/>.</summary>
internal static class EnemyProjectileCodePointers
{
    /// <summary>Canonical CPU-address base for enemy-projectile code in bank $86.</summary>
    public const int BankBase = 0x860000;
}
