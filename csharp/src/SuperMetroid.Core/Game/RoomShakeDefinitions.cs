namespace SuperMetroid.Core.Game;

/// <summary>One cartridge earthquake type's background and enemy-projectile displacement.</summary>
/// <param name="Bg1X">Horizontal offset component applied to background layer 1 for this shake type; zero means the layer is unaffected.</param>
/// <param name="Bg1Y">Vertical offset component applied to background layer 1 for this shake type; zero means the layer is unaffected.</param>
/// <param name="Bg2X">Horizontal offset component applied to background layer 2 for this shake type; zero means the layer is unaffected.</param>
/// <param name="Bg2Y">Vertical offset component applied to background layer 2 for this shake type; zero means the layer is unaffected.</param>
/// <param name="ProjectileX">Horizontal offset component for eligible enemy projectiles; zero means projectiles are not shaken.</param>
/// <param name="ProjectileY">Vertical offset component for eligible enemy projectiles; zero means projectiles are not shaken.</param>
internal readonly record struct RoomShakeDefinition(
    short Bg1X, short Bg1Y, short Bg2X, short Bg2Y, short ProjectileX, short ProjectileY);

/// <summary>Exact bounded geometry and recipient policy for the 36 rendered earthquake types.</summary>
internal static class RoomShakeDefinitions
{

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
