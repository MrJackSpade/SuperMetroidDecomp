namespace SuperMetroid.Core.Game;

/// <summary>One cartridge earthquake type's background and enemy-projectile displacement.</summary>
internal readonly record struct RoomShakeDefinition(
    short Bg1X, short Bg1Y, short Bg2X, short Bg2Y, short ProjectileX, short ProjectileY);

/// <summary>Exact bounded geometry and recipient policy for the 36 rendered earthquake types.</summary>
internal static class RoomShakeDefinitions
{
    /// <summary>$A0:872D BGShakeDisplacements.BG1X/BG1Y, at eight-byte record stride.</summary>
    internal const int Bg1ReferenceAddress = 0xa0872d;
    /// <summary>$A0:8731 BGShakeDisplacements.BG2X/BG2Y, at eight-byte record stride.</summary>
    internal const int Bg2ReferenceAddress = 0xa08731;
    /// <summary>$86:846B Get_Values_for_Screen_Shaking.horizontalX/Y, at four-byte stride.</summary>
    internal const int ProjectileReferenceAddress = 0x86846b;

    /// <summary>Returns the physical displacement record for earthquake type 0..35.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A0/86.asm.
    /// Type encodes group=type/9, magnitude=(type/3)%3+1 and direction=type%3:
    /// horizontal, vertical, diagonal. BG1 is enabled except group 3; BG2 except group 0;
    /// projectiles only in groups 2/3. All 216 original words follow this Cartesian
    /// product exactly. The three recipient vectors have separate complete-domain proofs.
    /// Validate before division/modulo; types 36+ remain caller-owned non-rendered effects.
    /// Timer-controlled sign alternation, freezing and lifetime updates remain in callers.
    /// </remarks>
    internal static RoomShakeDefinition ForType(ushort earthquakeType)
    {
        if (earthquakeType >= 36)
            throw new InvalidDataException(
                $"Earthquake type ${earthquakeType:X4} exceeds the 36 rendered definitions.");
        int group = earthquakeType / 9;
        short magnitude = (short)(earthquakeType / 3 % 3 + 1);
        int direction = earthquakeType % 3;
        short x = direction == 1 ? (short)0 : magnitude;
        short y = direction == 0 ? (short)0 : magnitude;
        return new(
            group == 3 ? (short)0 : x, group == 3 ? (short)0 : y,
            group == 0 ? (short)0 : x, group == 0 ? (short)0 : y,
            group < 2 ? (short)0 : x, group < 2 ? (short)0 : y);
    }
}
