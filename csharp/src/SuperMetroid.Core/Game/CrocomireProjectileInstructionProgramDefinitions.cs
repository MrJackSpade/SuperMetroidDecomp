namespace SuperMetroid.Core.Game;

/// <summary>One compiled Crocomire projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct CrocomireProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Crocomire's mouth projectile, bridge fragments, and spike-wall
/// pieces. Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CrocomireProjectileInstructionProgramDefinitions
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
    /// one-pose loop is retained as three authored control words; an address
    /// classifier would only restate them. $8FF3 starts the spike-wall list.
    /// </summary>
    internal const ushort BridgeFragment = 0x8feb;

    /// <summary>
    /// <c>InstList_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:8FF3.
    /// The pinned NTSC J/U v1.0 ROM has $7FFF here, a live spritemap operand
    /// at $8FF5, goto-Y $81AB at $8FF7, and target $8FF3 at $8FF9. Retain
    /// these three authored control words for the eight spawned fragments;
    /// the following unused list at $8FFB is outside their one-pose loop.
    /// </summary>
    internal const ushort SpikeWallPiece = 0x8ff3;

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> at $86:9007.
    /// The five mechanics words at $9007 + 4*i (i=0..4) are exactly four-frame
    /// durations in the pinned NTSC J/U v1.0 ROM. Retain the authored
    /// side-effect order: $901B calls drop opcode $9270, $901D is goto-Y
    /// $81AB, and $901F targets shared delete program $84FC. The physical
    /// $8154 word at $9021 is skipped by that jump, not a sixth frame or
    /// compiled word. Interleaved explosion spritemaps remain live reads.
    /// </summary>
    internal const ushort MouthProjectileShot = 0x9007;

    private static readonly CrocomireProjectileInstructionMechanicsWord[] Words =
    [
        new(MouthProjectile, 0x0003),
        new(0x8fd3, 0x0003),
        new(0x8fd7, 0x0003),
        new(0x8fdb, 0x0003),
        new(0x8fdf, 0x0003),
        new(0x8fe3, 0x0003),
        new(0x8fe7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8fe9, MouthProjectile),
        new(BridgeFragment, 0x7fff),
        new(0x8fef, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8ff1, BridgeFragment),
        new(SpikeWallPiece, 0x7fff),
        new(0x8ff7, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8ff9, SpikeWallPiece),
        new(MouthProjectileShot, 0x0004),
        new(0x900b, 0x0004),
        new(0x900f, 0x0004),
        new(0x9013, 0x0004),
        new(0x9017, 0x0004),
        new(0x901b,
            EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithCrocomireChances),
        new(0x901d, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x901f, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
    ];

    /// <summary>
    /// Entries 0..5 identify mouth-projectile presentation operands at
    /// $86:8FD1 + 4*i. In the pinned NTSC J/U v1.0 ROM their stock pointers
    /// are exactly $802A + $16 * min(i, 6-i) for i=0..5: a four-level pose
    /// sweep that returns through levels two and one before the loop. These
    /// operands remain live cartridge reads so installed presentation works.
    /// Entry 6 is the bridge-fragment operand at $86:8FED: stock pointer
    /// $8109, followed by goto-Y at $8FEF. Retain this single authored
    /// spritemap identity as a live read; no indexed formula clarifies it.
    /// Entry 7 is the spike-wall operand at $86:8FF5: stock pointer $8110,
    /// followed by goto-Y at $8FF7. Its eight spawned actors all keep this
    /// separately authored one-pose presentation operand live.
    /// Entries 8..12 identify shot operands at $86:9009 + 4*i. Their stock
    /// pointers are $8D9C at i=0 and $8DA3 + $16*(i-1) for i=1..4; the
    /// first stride is only $0007. $901B starts the drop/delete control.
    /// These shot operands also remain live presentation reads.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        0x8fd1, 0x8fd5, 0x8fd9, 0x8fdd, 0x8fe1, 0x8fe5,
        0x8fed, 0x8ff5,
        0x9009, 0x900d, 0x9011, 0x9015, 0x9019,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CrocomireProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CrocomireProjectile or
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CrocomireProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Crocomire projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
