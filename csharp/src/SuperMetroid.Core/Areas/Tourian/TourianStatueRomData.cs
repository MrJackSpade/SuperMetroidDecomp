namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge identities and operands for the four-boss statue sequence.</summary>
public static class TourianStatueRomData
{
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
    /// <summary>$86:B7F5 release earthquake: type 1, with timer bit $20 so the shake lasts at least 32 frames.</summary>
    public const ushort ReleaseEarthquakeType = 1, ReleaseEarthquakeTimer = 0x20;
}
