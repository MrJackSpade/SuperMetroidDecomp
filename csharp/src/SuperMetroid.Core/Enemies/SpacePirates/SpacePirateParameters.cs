namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Reads the options walking, wall and ninja Space Pirates take from population parameter one: bit 0 picks the starting
/// side, bit 15 the slow laser (and, for wall pirates, slow jump) timing. No other bit of the word is read.
/// </summary>
internal static class SpacePirateParameters
{
    /// <summary>The parameter bit that picks the starting side.</summary>
    private const int StartingSideBit = 0;

    /// <summary>The parameter bit that picks the slow timing.</summary>
    private const int SlowTimingBit = 15;

    /// <summary>The walking pirate's options in <paramref name="parameter"/>, its population parameter one (or a laser's inherited copy).</summary>
    internal static WalkingSpacePirateParameterFlags Walking(int parameter) =>
        (Has(parameter, StartingSideBit) ? WalkingSpacePirateParameterFlags.StartsFacingRight : WalkingSpacePirateParameterFlags.None) |
        (Has(parameter, SlowTimingBit) ? WalkingSpacePirateParameterFlags.SlowLaserAndProjectileFlinch : WalkingSpacePirateParameterFlags.None);

    /// <summary>The wall pirate's options in <paramref name="parameter"/>, its population parameter one.</summary>
    internal static WallSpacePirateParameterFlags Wall(int parameter) =>
        (Has(parameter, StartingSideBit) ? WallSpacePirateParameterFlags.StartsOnRightWall : WallSpacePirateParameterFlags.None) |
        (Has(parameter, SlowTimingBit) ? WallSpacePirateParameterFlags.SlowJumpAndLaser : WallSpacePirateParameterFlags.None);

    /// <summary>The ninja pirate's options in <paramref name="parameter"/>, its population parameter one.</summary>
    internal static NinjaSpacePirateParameterFlags Ninja(int parameter) =>
        Has(parameter, StartingSideBit) ? NinjaSpacePirateParameterFlags.StartsAtLeftPostFacingRight : NinjaSpacePirateParameterFlags.None;

    /// <summary>Whether bit <paramref name="bit"/> of <paramref name="parameter"/> is set.</summary>
    private static bool Has(int parameter, int bit) => (parameter >> bit & 1) != 0;
}
