namespace SuperMetroid.Core.Game;

/// <summary>The five mutually exclusive enemy-death animation variants.</summary>
internal enum EnemyDeathAnimation : ushort
{
    /// <summary>Native selector zero; starts the small-explosion program at $86:ED69.</summary>
    SmallExplosion = 0,
    /// <summary>Native selector one; starts the Samus-contact death program at $86:EDFF.</summary>
    KilledBySamusContact = 1,
    /// <summary>Native selector two; starts the normal-explosion program at $86:ED4B.</summary>
    NormalExplosion = 2,
    /// <summary>Native selector three; starts Mini-Kraid's explosion program at $86:ECC5.</summary>
    MiniKraidExplosion = 3,
    /// <summary>Native selector four; starts the big-explosion program at $86:ECAB.</summary>
    BigExplosion = 4,
}
