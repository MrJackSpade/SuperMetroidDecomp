namespace SuperMetroid.Core.Game;

/// <summary>
/// One bank-$86 Torizo launch tuple whose two velocity words receive independent signed
/// random-byte displacements when the projectile is initialized. The native record interleaves
/// X offset, X velocity, Y offset and Y velocity words.
/// </summary>
internal readonly record struct TorizoRandomizedProjectileDefinition(
    ushort InstructionList,
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> BaseVelocity);

/// <summary>Fixed randomized launch definitions shared by Bomb and Golden Torizo.</summary>
internal static class TorizoRandomizedProjectileDefinitions
{
    /// <summary>
    /// Bomb Torizo Chozo-orb records at <c>$86:AC08-$86:AC1B</c>, selected by facing.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition BombChozoOrb(bool facingRight) =>
        facingRight
            ? new(0xab1d,
                new(27, -40), new(400, -416))
            : new(0xab15,
                new(-27, -40), new(-400, -416));

    /// <summary>
    /// Golden Torizo Chozo-orb records at <c>$86:AC99-$86:ACAC</c>, selected by facing.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenChozoOrb(bool facingRight) =>
        facingRight
            ? new(0xab1d,
                new(27, -40), new(256, -448))
            : new(0xab15,
                new(-27, -40), new(-256, -448));

    /// <summary>
    /// Golden Torizo egg records at <c>$86:B02F-$86:B042</c>, selected by horizontal
    /// launch direction. Native actor parameter bit 15 selects the rightward record.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEgg(bool movingRight) =>
        movingRight
            ? new(GoldenTorizoEggInstructionProgramDefinitions.BouncingRight,
                new(16, -1), new(128, -384))
            : new(GoldenTorizoEggInstructionProgramDefinitions.BouncingLeft,
                new(-16, -1), new(-128, -384));

    /// <summary>
    /// Golden Torizo eye-beam records at <c>$86:B376-$86:B389</c>, selected by facing.
    /// The caller subsequently replaces both velocity words with its aimed launch.
    /// </summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEyeBeam(bool facingRight) =>
        facingRight
            ? new(GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal,
                new(20, -30), new(1024, 1024))
            : new(GoldenTorizoEyeBeamInstructionProgramDefinitions.Normal,
                new(-20, -30), new(-1024, 1024));
}
