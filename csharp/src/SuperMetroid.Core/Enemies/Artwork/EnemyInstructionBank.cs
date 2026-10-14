namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The cartridge banks whose enemy and enemy-projectile instruction lists carry calculated visual selectors; each
/// enemy family's code, instruction lists and spritemaps live in one bank.
/// </summary>
internal enum EnemyInstructionBank : byte
{
    /// <summary>Bank $86: enemy projectile instruction lists and spritemaps.</summary>
    Bank86 = 0x86,



    /// <summary>Bank $A6: enemy AI bank (Boulder, Mini-Kraid and others).</summary>
    BankA6 = 0xa6,

}
