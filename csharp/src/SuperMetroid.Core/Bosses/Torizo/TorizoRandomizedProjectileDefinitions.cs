namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed randomized launch definitions shared by Bomb and Golden Torizo.</summary>
internal static class TorizoRandomizedProjectileDefinitions
{
    /// <summary>
    /// Bomb Torizo Chozo-orb records at <c>$86:AC08-$86:AC1B</c>, selected by facing.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition BombChozoOrb(bool facingRight) =>
        facingRight
            ? new(TorizoProjectileProgram.ChozoOrbMovingRight,
                new(27, -40), new(400, -416))
            : new(TorizoProjectileProgram.ChozoOrbMovingLeft,
                new(-27, -40), new(-400, -416));

    /// <summary>
    /// Golden Torizo Chozo-orb records at <c>$86:AC99-$86:ACAC</c>, selected by facing.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenChozoOrb(bool facingRight) =>
        facingRight
            ? new(TorizoProjectileProgram.ChozoOrbMovingRight,
                new(27, -40), new(256, -448))
            : new(TorizoProjectileProgram.ChozoOrbMovingLeft,
                new(-27, -40), new(-256, -448));

    /// <summary>
    /// Golden Torizo egg records at <c>$86:B02F-$86:B042</c>, selected by horizontal
    /// launch direction. Native actor parameter bit 15 selects the rightward record.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEgg(bool movingRight) =>
        movingRight
            ? new(TorizoProjectileProgram.EggBouncingRight,
                new(16, -1), new(128, -384))
            : new(TorizoProjectileProgram.EggBouncingLeft,
                new(-16, -1), new(-128, -384));

    /// <summary>
    /// Golden Torizo eye-beam records at <c>$86:B376-$86:B389</c>, selected by facing.
    /// The caller subsequently replaces both velocity words with its aimed launch.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEyeBeam(bool facingRight) =>
        facingRight
            ? new(TorizoProjectileProgram.EyeBeamNormal,
                new(20, -30), new(1024, 1024))
            : new(TorizoProjectileProgram.EyeBeamNormal,
                new(-20, -30), new(-1024, 1024));
}
