namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The common enemy instructions development tools read at the same bank-local offsets in every enemy
/// bank (the bank-$A0 command set); the native-list oracles' words are in test-support.
/// </summary>
internal enum CommonEnemyInstruction : ushort
{
    /// <summary>Sleep forever on the current instruction at offset $812F.</summary>
    Sleep = 0x812f,
}
