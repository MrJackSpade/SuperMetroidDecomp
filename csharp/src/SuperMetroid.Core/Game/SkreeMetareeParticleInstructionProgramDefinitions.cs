namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the visually distinct Skree and Metaree death-particle programs.
/// Their interleaved spritemap operands are visual identities: diagnostic sessions
/// still read the cartridge, while installed sessions select the matching editable
/// compositions through <see cref="SuperMetroid.Core.Assets.SkreeMetareeParticleVisualDefinitions"/>.
/// </summary>
internal abstract class SkreeMetareeParticleInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABD.</summary>
    internal const ushort Skree = 0x8abd;

    /// <summary><c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC5.</summary>
    internal const ushort Metaree = 0x8ac5;
    /// <summary>Checks whether a projectile kind uses one of the compiled Skree or Metaree death-particle programs.</summary>
    /// <param name="kind">Enemy-projectile identity to classify.</param>
    /// <returns><see langword="true"/> for a directional Skree or Metaree particle; otherwise <see langword="false"/>.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.SkreeParticleDownRight or
        RoomEnemyProjectileKind.SkreeParticleUpRight or
        RoomEnemyProjectileKind.SkreeParticleDownLeft or
        RoomEnemyProjectileKind.SkreeParticleUpLeft or
        RoomEnemyProjectileKind.MetareeParticleDownRight or
        RoomEnemyProjectileKind.MetareeParticleUpRight or
        RoomEnemyProjectileKind.MetareeParticleDownLeft or
        RoomEnemyProjectileKind.MetareeParticleUpLeft;

    /// <summary>Resolves a particle program's duration, vertical-loop opcode, or loop target.</summary>
    /// <param name="address">Bank-relative address of a candidate mechanics word in either particle list.</param>
    /// <returns>The compiled mechanics value at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a mechanics word in either list.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Skree;
        if ((uint)offset < 16)
        {
            ushort start = offset < 8 ? Skree : Metaree;
            switch (offset % 8)
            {
                case 0: return 16;
                case 4: return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
                case 6: return start;
            }
        }
        throw new InvalidDataException(
            $"Skree/Metaree particle instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }
}
