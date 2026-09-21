namespace SuperMetroid.Core.Game;

/// <summary>Compiled gameplay selectors for Golden Torizo's reflected projectiles.</summary>
internal static class GoldenTorizoProjectileDefinitions
{
    /// <summary>Returns the cartridge instruction list selected by Golden Torizo's facing.</summary>
    internal static ushort GetReflectedSuperMissileInstruction(bool facingRight) =>
        facingRight
            ? GoldenTorizoSuperMissileInstructionProgramDefinitions.RightInitial
            : GoldenTorizoSuperMissileInstructionProgramDefinitions.LeftInitial;
}
