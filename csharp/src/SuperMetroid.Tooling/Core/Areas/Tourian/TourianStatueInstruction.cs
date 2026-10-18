namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The bank-$86 instructions private to Tourian statue unlocking effects and the Chozo dust.</summary>
internal enum TourianStatueInstruction : ushort
{
    /// <summary>$86:AF36 restores a dust actor to its initial position.</summary>
    ResetDustPosition = 0xaf36,
    /// <summary>$86:B7EA spawn particle at this projectile.</summary>
    SpawnParticle = 0xb7ea,
    /// <summary>$86:B7F5 unlocking earthquake.</summary>
    Earthquake = 0xb7f5,
    /// <summary>$86:B818 spawn particle tail.</summary>
    SpawnTail = 0xb818,
    /// <summary>$86:B841 add signed word to projectile Y.</summary>
    AddY = 0xb841,
}
