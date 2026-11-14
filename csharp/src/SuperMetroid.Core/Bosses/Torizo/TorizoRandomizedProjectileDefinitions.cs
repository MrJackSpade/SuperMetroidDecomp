namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed randomized launch definitions shared by Bomb and Golden Torizo.</summary>
internal static class TorizoRandomizedProjectileDefinitions
{
    /// <summary>Bomb Torizo's right-facing Chozo-orb record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition BombChozoOrbRight =
        new(TorizoProjectileSegment.ChozoOrbMovingRight, new(27, -40),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(400, -416));
    /// <summary>Bomb Torizo's left-facing Chozo-orb record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition BombChozoOrbLeft =
        new(TorizoProjectileSegment.ChozoOrbMovingLeft, new(-27, -40),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-400, -416));

    /// <summary>Golden Torizo's right-facing Chozo-orb record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenChozoOrbRight =
        new(TorizoProjectileSegment.ChozoOrbMovingRight, new(27, -40),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(256, -448));
    /// <summary>Golden Torizo's left-facing Chozo-orb record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenChozoOrbLeft =
        new(TorizoProjectileSegment.ChozoOrbMovingLeft, new(-27, -40),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-256, -448));

    /// <summary>Golden Torizo's rightward egg record; the actor parameter's rightward flag selects it.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEggRight =
        new(TorizoProjectileSegment.EggBouncingRight, new(16, -1),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(128, -384));
    /// <summary>Golden Torizo's leftward egg record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEggLeft =
        new(TorizoProjectileSegment.EggBouncingLeft, new(-16, -1),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-128, -384));

    /// <summary>
    /// Golden Torizo's right-facing eye-beam record. The caller subsequently replaces both
    /// velocities with its aimed launch.
    /// </summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEyeBeamRight =
        new(TorizoProjectileSegment.EyeBeam, new(20, -30),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(1024, 1024));
    /// <summary>Golden Torizo's left-facing eye-beam record.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEyeBeamLeft =
        new(TorizoProjectileSegment.EyeBeam, new(-20, -30),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-1024, 1024));
}
