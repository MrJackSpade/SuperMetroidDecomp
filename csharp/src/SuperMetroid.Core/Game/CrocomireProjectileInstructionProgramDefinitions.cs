namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Crocomire's mouth projectile, bridge fragments, and spike-wall
/// pieces. Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class CrocomireProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomiresProjectile</c> at $86:8FCF.
    /// In the pinned NTSC J/U v1.0 ROM, each mechanics word at
    /// $8FCF + 4*i for i=0..5 is exactly a three-frame duration. $8FE7
    /// holds goto-Y $81AB and $8FE9 targets $8FCF, closing the six-pose
    /// loop. The interleaved spritemap pointers remain live presentation;
    /// $8FEB begins the separate bridge-fragment program.
    /// </summary>
    internal const ushort MouthProjectile = 0x8fcf;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomireBridgeCrumbling</c> at $86:8FEB.
    /// The pinned NTSC J/U v1.0 ROM has $7FFF here, a live spritemap operand
    /// at $8FED, goto-Y $81AB at $8FEF, and target $8FEB at $8FF1. This
    /// one-pose loop uses a duration, goto opcode and self-target.
    /// $8FF3 starts the spike-wall list.
    /// </summary>
    internal const ushort BridgeFragment = 0x8feb;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:8FF3.
    /// The pinned NTSC J/U v1.0 ROM has $7FFF here, a live spritemap operand
    /// at $8FF5, goto-Y $81AB at $8FF7, and target $8FF3 at $8FF9;
    /// the following unused list at $8FFB is outside their one-pose loop.
    /// </summary>
    internal const ushort SpikeWallPiece = 0x8ff3;

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> at $86:9007.
    /// The five mechanics words at $9007 + 4*i (i=0..4) are exactly four-frame
    /// durations in the pinned NTSC J/U v1.0 ROM. After those frames,
    /// $901B calls drop opcode $9270, $901D is goto-Y
    /// $81AB, and $901F targets shared delete program $84FC. The physical
    /// $8154 word at $9021 is skipped by that jump, not a sixth frame or
    /// compiled word. Interleaved explosion spritemaps remain live reads.
    /// </summary>
    internal const ushort MouthProjectileShot = 0x9007;

    public static int MechanicsWordCount => 22;
    public static int PresentationWordCount => 13;

    /// <summary>Enumerates each program's timed frames followed by its control
    /// trailer: a self-loop, or the shot program's drop/goto/delete sequence.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = index < 8 ? MouthProjectile : index < 11 ? BridgeFragment
            : index < 14 ? SpikeWallPiece : MouthProjectileShot;
        int field = index < 8 ? index : index < 11 ? index - 8
            : index < 14 ? index - 11 : index - 14;
        var layout = Layout(start);
        int offset = field < layout.Frames ? 4 * field
            : 4 * layout.Frames + 2 * (field - layout.Frames);
        ushort address = (ushort)(start + offset);
        return new(address, ReadMechanicsWord(address));
    }

    /// <summary>Spritemap operands are two bytes after each frame duration.
    /// Enumerate six mouth frames, one bridge frame, one spike frame and five
    /// shot frames. These are operand positions, not stored artwork identities.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 6 ? MouthProjectile + 2 + 4 * index
            : index == 6 ? BridgeFragment + 2
            : index == 7 ? SpikeWallPiece + 2
            : MouthProjectileShot + 2 + 4 * (index - 8));
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CrocomireProjectile or
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces;

    /// <summary>Dispatches timed frames and named control instructions at their
    /// exact native positions. Adjacent unused programs and sprite operands are
    /// excluded, including the skipped $86:9021 delete opcode.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        var layout = Layout(address);
        int offset = address - layout.Start;
        int trailer = 4 * layout.Frames;
        if (offset >= 0 && offset < trailer && offset % 4 == 0) return layout.Duration;
        bool shot = layout.Start == MouthProjectileShot;
        if (offset == trailer)
            return shot ? EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithCrocomireChances
                : EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (offset == trailer + 2)
            return shot ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : layout.Start;
        if (shot && offset == trailer + 4)
            return CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        throw new InvalidDataException(
            $"Crocomire projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        var layout = Layout(bankAddress);
        int offset = bankAddress - layout.Start;
        int trailer = 4 * layout.Frames;
        int length = trailer + (layout.Start == MouthProjectileShot ? 6 : 4);
        return offset >= 0 && offset < length && (offset >= trailer || offset % 4 < 2);
    }

    // Select the preceding program; the caller validates its exact field domain.
    private static (ushort Start, int Frames, ushort Duration) Layout(ushort address) =>
        address < BridgeFragment ? (MouthProjectile, 6, (ushort)3)
        : address < SpikeWallPiece ? (BridgeFragment, 1, (ushort)0x7fff)
        : address < MouthProjectileShot ? (SpikeWallPiece, 1, (ushort)0x7fff)
        : (MouthProjectileShot, 5, (ushort)4);
}
