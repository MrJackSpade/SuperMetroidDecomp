namespace SuperMetroid.Core.Game;

/// <summary>Cartridge identities and operands for the four-boss statue sequence.</summary>
public static class TourianStatueRomData
{
    /// <summary>$87:833E serializes the four lock-release animations with bit 15.</summary>
    public const ushort Busy = 0x8000;
    /// <summary>$88:DBD7 latches bit 4 once all four grey-statue events exist.</summary>
    public const ushort AllReleased = 0x10;
    /// <summary>$88:DBF9 delay before lowering the statue, in handler frames.</summary>
    public const int DescentDelay = 300;
    /// <summary>$88:DC69 lowers BG2 by a quarter pixel per frame.</summary>
    public const int DescentStep = 0x4000;
    /// <summary>$88:DC69 completes at signed BG2 offset -240.</summary>
    public const int DescentDistance = 240;
    /// <summary>$87:839C eight grey target palette colors used by $87:837F.</summary>
    public const int GreyColors = 0x87839c;
    /// <summary>$86:B91E eye glow colors, four words per statue.</summary>
    public const int EyeColors = 0x86b91e;
    /// <summary>$86:B79F delete projectile instruction list.</summary>
    public const ushort DeleteProjectile = 0xb79f;
    /// <summary>$88:DC23/DC69 earthquake type 13 with timer bits $20.</summary>
    public const ushort DescentEarthquakeType = 13, DescentEarthquakeTimer = 0x20;
}

/// <summary>The bank-$86 pre-instructions that move Tourian statue unlocking effects.</summary>
internal enum TourianStatuePreInstruction : ushort
{
    /// <summary>$86:B977 splash follows water surface.</summary>
    SplashMotion = 0xb977,
    /// <summary>$86:B982 particle motion pre-instruction.</summary>
    ParticleMotion = 0xb982,
    /// <summary>$86:B9FD soul motion pre-instruction.</summary>
    SoulMotion = 0xb9fd,
}

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
