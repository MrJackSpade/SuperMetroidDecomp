namespace SuperMetroid.Core.Game;

/// <summary>Cartridge identities and operands for the four-boss statue sequence.</summary>
public static class TourianStatueRomData
{
    /// <summary>Bank $86 owns the statue unlock projectiles and their instruction operands.</summary>
    public const int ProjectileBank = 0x860000;
    /// <summary>Bank $84 owns the clear/crumble access PLM headers and instruction-list pointers.</summary>
    public const int AccessPlmBank = 0x840000;
    /// <summary>$8F:91D7 spawn order; native allocation/iteration preserves this order.</summary>
    public static ReadOnlySpan<ushort> AnimatedObjects => [0x8558, 0x854c, 0x855e, 0x8552];
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
    /// <summary>$84:B773 crumble access to Tourian elevator, spawned at (6,12).</summary>
    public const ushort CrumbleAccess = 0xb773;
    /// <summary>$84:B777 clear access on re-entry after event $0A.</summary>
    public const ushort ClearAccess = 0xb777;
    /// <summary>$84:AB00 advances the six-row crumble PLM one row downward.</summary>
    public const ushort MoveAccessDown = 0xab00;
    /// <summary>$87:839C eight grey target palette colors used by $87:837F.</summary>
    public const int GreyColors = 0x87839c;
    /// <summary>$86:BA6A eye glow projectile definition.</summary>
    public const ushort EyeGlow = 0xba6a;
    /// <summary>$86:BA94 ascending soul projectile definition.</summary>
    public const ushort Soul = 0xba94;
    /// <summary>$86:BA78 falling unlocking particle definition.</summary>
    public const ushort Particle = 0xba78;
    /// <summary>$86:BA86 particle tail definition.</summary>
    public const ushort Tail = 0xba86;
    /// <summary>$86:BA5C water splash definition.</summary>
    public const ushort Splash = 0xba5c;
    /// <summary>$86:B90E eye/soul X positions, indexed by statue parameter.</summary>
    public const int EyeX = 0x86b90e;
    /// <summary>$86:B916 eye/soul Y positions.</summary>
    public const int EyeY = 0x86b916;
    /// <summary>$86:B91E eye glow colors, four words per statue.</summary>
    public const int EyeColors = 0x86b91e;
    /// <summary>$86:B9FD soul motion pre-instruction.</summary>
    public const ushort SoulMotion = 0xb9fd;
    /// <summary>$86:B982 particle motion pre-instruction.</summary>
    public const ushort ParticleMotion = 0xb982;
    /// <summary>$86:B977 splash follows water surface.</summary>
    public const ushort SplashMotion = 0xb977;
    /// <summary>$86:B7EA spawn particle at this projectile.</summary>
    public const ushort SpawnParticle = 0xb7ea;
    /// <summary>$86:B818 spawn particle tail.</summary>
    public const ushort SpawnTail = 0xb818;
    /// <summary>$86:B7F5 unlocking earthquake.</summary>
    public const ushort Earthquake = 0xb7f5;
    /// <summary>$86:B841 add signed word to projectile Y.</summary>
    public const ushort AddY = 0xb841;
    /// <summary>$86:B79F delete projectile instruction list.</summary>
    public const ushort DeleteProjectile = 0xb79f;
    /// <summary>$A0:B3C3, kSinCosTable8bit_Sext: signed words with magnitude 256, beginning at the negative-cosine quadrant, used by $86:B8B5 particle launch.</summary>
    public const int SignedSine = 0xa0b3c3;
    /// <summary>$86:AF84 statue descent dust definition.</summary>
    public const ushort DescentDust = 0xaf84;
    /// <summary>$86:AF36 restores a dust actor to its initial position.</summary>
    public const ushort ResetDustPosition = 0xaf36;
    /// <summary>$88:DC23/DC69 earthquake type 13 with timer bits $20.</summary>
    public const ushort DescentEarthquakeType = 13, DescentEarthquakeTimer = 0x20;
}
