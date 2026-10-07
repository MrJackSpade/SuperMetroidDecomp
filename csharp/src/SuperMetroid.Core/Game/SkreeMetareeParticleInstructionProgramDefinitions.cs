namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the visually distinct Skree and Metaree death-particle programs.
/// Their interleaved spritemap operands are visual identities: diagnostic sessions
/// still read the cartridge, while installed sessions select the matching editable
/// compositions through <see cref="SuperMetroid.Core.Assets.SkreeMetareeParticleVisualDefinitions"/>.
/// </summary>
internal abstract class SkreeMetareeParticleInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABD.</summary>
    internal const ushort Skree = 0x8abd;

    /// <summary><c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC5.</summary>
    internal const ushort Metaree = 0x8ac5;

    public static int MechanicsWordCount => 6;
    public static int PresentationWordCount => 2;

    /// <summary>Each eight-byte loop is a sixteen-frame drawing followed by goto-self.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = index < 3 ? Skree : Metaree;
        int field = index % 3;
        return field switch
        {
            0 => new(start, 16),
            1 => new((ushort)(start + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            _ => new((ushort)(start + 6), start),
        };
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index == 0 ? Skree : Metaree) + 2);
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.SkreeParticleDownRight or
        RoomEnemyProjectileKind.SkreeParticleUpRight or
        RoomEnemyProjectileKind.SkreeParticleDownLeft or
        RoomEnemyProjectileKind.SkreeParticleUpLeft or
        RoomEnemyProjectileKind.MetareeParticleDownRight or
        RoomEnemyProjectileKind.MetareeParticleUpRight or
        RoomEnemyProjectileKind.MetareeParticleDownLeft or
        RoomEnemyProjectileKind.MetareeParticleUpLeft;

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

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
