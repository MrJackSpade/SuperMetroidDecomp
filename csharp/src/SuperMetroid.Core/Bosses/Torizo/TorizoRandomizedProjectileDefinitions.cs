namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed randomized launch definitions shared by Bomb and Golden Torizo.</summary>
internal static class TorizoRandomizedProjectileDefinitions
{
    /// <summary>Bomb Torizo's right-facing Chozo-orb record in <c>$86:AC08-$86:AC1B</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition BombChozoOrbRight =
        new(TorizoProjectileProgram.ChozoOrbMovingRight, new(27, -40), new(400, -416));
    /// <summary>Bomb Torizo's left-facing Chozo-orb record in <c>$86:AC08-$86:AC1B</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition BombChozoOrbLeft =
        new(TorizoProjectileProgram.ChozoOrbMovingLeft, new(-27, -40), new(-400, -416));

    /// <summary>Golden Torizo's right-facing Chozo-orb record in <c>$86:AC99-$86:ACAC</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenChozoOrbRight =
        new(TorizoProjectileProgram.ChozoOrbMovingRight, new(27, -40), new(256, -448));
    /// <summary>Golden Torizo's left-facing Chozo-orb record in <c>$86:AC99-$86:ACAC</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenChozoOrbLeft =
        new(TorizoProjectileProgram.ChozoOrbMovingLeft, new(-27, -40), new(-256, -448));

    /// <summary>Golden Torizo's rightward egg record in <c>$86:B02F-$86:B042</c>; native actor parameter bit 15 selects it.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEggRight =
        new(TorizoProjectileProgram.EggBouncingRight, new(16, -1), new(128, -384));
    /// <summary>Golden Torizo's leftward egg record in <c>$86:B02F-$86:B042</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEggLeft =
        new(TorizoProjectileProgram.EggBouncingLeft, new(-16, -1), new(-128, -384));

    /// <summary>
    /// Golden Torizo's right-facing eye-beam record in <c>$86:B376-$86:B389</c>. The caller subsequently replaces both
    /// velocity words with its aimed launch.
    /// </summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEyeBeamRight =
        new(TorizoProjectileProgram.EyeBeamNormal, new(20, -30), new(1024, 1024));
    /// <summary>Golden Torizo's left-facing eye-beam record in <c>$86:B376-$86:B389</c>.</summary>
    internal static readonly TorizoRandomizedProjectileDefinition GoldenEyeBeamLeft =
        new(TorizoProjectileProgram.EyeBeamNormal, new(-20, -30), new(-1024, 1024));
}
