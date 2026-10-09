using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireProjectileInstructionProgramDefinitions))]
internal abstract class CrocomireProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of sprite-frame operands across Crocomire's projectile instruction programs.</summary>
    public static int PresentationWordCount => CrocomireProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the address of one interleaved spritemap operand in the compiled projectile programs.</summary>
    /// <param name="index">Zero-based position among the projectile programs' presentation operands.</param>
    /// <returns>The bank-local address whose word selects the spritemap for a frame.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => CrocomireProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the number of duration and control words exposed for Crocomire's projectile programs.</summary>
    public static int MechanicsWordCount => 22;
    /// <summary>Enumerates each program's timed frames followed by its control
    /// trailer: a self-loop, or the shot program's drop/goto/delete sequence.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = index < 8 ? CrocomireProjectileInstructionProgramDefinitions.MouthProjectile : index < 11 ? CrocomireProjectileInstructionProgramDefinitions.BridgeFragment
            : index < 14 ? CrocomireProjectileInstructionProgramDefinitions.SpikeWallPiece : CrocomireProjectileInstructionProgramDefinitions.MouthProjectileShot;
        int field = index < 8 ? index : index < 11 ? index - 8
            : index < 14 ? index - 11 : index - 14;
        var layout = CrocomireProjectileInstructionProgramDefinitions.Layout(start);
        int offset = field < layout.Frames ? 4 * field
            : 4 * layout.Frames + 2 * (field - layout.Frames);
        ushort address = (ushort)(start + offset);
        return new(address, CrocomireProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Tests whether a full SNES address belongs to a compiled mechanics word in these projectile programs.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> for duration-word bytes or the contiguous control trailer; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        var layout = CrocomireProjectileInstructionProgramDefinitions.Layout(bankAddress);
        int offset = bankAddress - layout.Start;
        int trailer = 4 * layout.Frames;
        int length = trailer + (layout.Start == CrocomireProjectileInstructionProgramDefinitions.MouthProjectileShot ? 6 : 4);
        return offset >= 0 && offset < length && (offset >= trailer || offset % 4 < 2);
    }
}
