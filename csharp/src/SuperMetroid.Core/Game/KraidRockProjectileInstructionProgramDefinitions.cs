using System.Collections.Frozen;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Kraid's rock projectiles and the initial Kago-bug pose they share.
/// Their interleaved spritemap operands select compiled presentation identities.
/// </summary>
internal abstract class KraidRockProjectileInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_EnemyProjectile_KraidRocks_KagoBug</c> at $86:9C7D.
    /// </summary>
    internal const ushort SharedRockAndKagoBug = 0x9c7d;

    /// <summary>
    /// <c>InstList_EnemyProjectile_KraidFloorRocks_Right</c> at $86:9C83.
    /// </summary>
    internal const ushort RisingRockRight = 0x9c83;

    /// <summary>
    /// <c>InstList_EnemyProjectile_Shot_KraidRockSpit</c> at $86:9C89.
    /// </summary>
    internal const ushort SpitRockShot = 0x9c89;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the shared list at $86:9C81.
    /// </summary>
    internal const ushort SharedRockAndKagoBugSleep = 0x9c81;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the right-rock list at $86:9C87.
    /// </summary>
    internal const ushort RisingRockRightSleep = 0x9c87;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Delete</c> ending the spit-rock shot list at $86:9CA1.
    /// </summary>
    internal const ushort SpitRockShotDelete = 0x9ca1;

    public static int MechanicsWordCount => 12;
    public static int PresentationWordCount => 7;

    /// <summary>Two fixed poses end in Sleep. The shot sequence installs its
    /// palette pre-instruction, emits five four-tick frames, then deletes itself.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 4)
        {
            bool sleep = (index & 1) != 0;
            return new((ushort)(SharedRockAndKagoBug + 6 * (index / 2) + (sleep ? 4 : 0)),
                sleep ? (ushort)EnemyProjectileInstruction.Sleep : (ushort)0x7fff);
        }
        return index switch
        {
            4 => new(SpitRockShot, (ushort)EnemyProjectileInstruction.PreInstructionInY),
            5 => new(SpitRockShot + 2, (ushort)EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0),
            < 11 => new((ushort)(SpitRockShot + 4 + 4 * (index - 6)), 4),
            _ => new(SpitRockShotDelete, (ushort)EnemyProjectileInstruction.Delete),
        };
    }

    /// <summary>Two fixed-pose operands followed by five shot-frame operands,
    /// each two bytes after its duration; skip the pre-instruction command pair.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 2 ? SharedRockAndKagoBug + 6 * index + 2
            : SpitRockShot + 6 + 4 * (index - 2));
    }
    /// <summary>The projectile kinds whose instruction programs live in this allocation.</summary>
    private static readonly FrozenSet<RoomEnemyProjectileKind> OwnerKinds =
    [
        RoomEnemyProjectileKind.KraidSpitRock, RoomEnemyProjectileKind.KraidCeilingRock,
        RoomEnemyProjectileKind.KraidRisingRockLeft, RoomEnemyProjectileKind.KraidRisingRockRight,
        RoomEnemyProjectileKind.KagoBug,
    ];

    internal static bool Owns(RoomEnemyProjectileKind kind, ushort address) => OwnerKinds.Contains(kind) && kind switch
    {
        RoomEnemyProjectileKind.KraidSpitRock =>
            IsSharedProgramAddress(address) || IsSpitShotProgramAddress(address),
        RoomEnemyProjectileKind.KraidCeilingRock or
        RoomEnemyProjectileKind.KraidRisingRockLeft or
        RoomEnemyProjectileKind.KagoBug => IsSharedProgramAddress(address),
        RoomEnemyProjectileKind.KraidRisingRockRight => IsRisingRightProgramAddress(address),
        _ => throw new InvalidOperationException($"{kind} is not a Kraid rock owner."),
    };

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Kraid-rock projectile instruction mechanics pointer $86:{address:X4} " +
            "is not compiled.");
    }

    private static bool IsSharedProgramAddress(ushort address) =>
        address is SharedRockAndKagoBug or SharedRockAndKagoBugSleep;

    private static bool IsRisingRightProgramAddress(ushort address) =>
        address is RisingRockRight or RisingRockRightSleep;

    private static bool IsSpitShotProgramAddress(ushort address) =>
        address >= SpitRockShot && address <= SpitRockShotDelete && (address & 1) != 0;
}
