using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for the four falling ceiling-tube poses in Mother Brain's fake-death
/// sequence. Their four spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class MotherBrainTopTubeInstructionProgramDefinitions
{
    /// <summary>Top-right ceiling tube instruction list at $86:CC43.</summary>
    internal const ushort TopRight = 0xcc43;

    /// <summary>Top-left ceiling tube instruction list at $86:CC49.</summary>
    internal const ushort TopLeft = 0xcc49;

    /// <summary>Top-middle-left ceiling tube instruction list at $86:CC4F.</summary>
    internal const ushort TopMiddleLeft = 0xcc4f;

    /// <summary>Top-middle-right ceiling tube instruction list at $86:CC55.</summary>
    internal const ushort TopMiddleRight = 0xcc55;

    /// <summary>Native program bank $86.</summary>
    internal const byte Bank = 0x86;

    /// <summary>
    /// Describes the four ceiling-tube entries, each of which displays one frame before sleeping;
    /// presentation operands are resolved from extracted art while mechanics stay compiled here.
    /// </summary>
    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xcc43),
        Entry(TopRight),
        Frame(1),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        Entry(TopLeft),
        Frame(1),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        Entry(TopMiddleLeft),
        Frame(1),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        Entry(TopMiddleRight),
        Frame(1),
        Op(EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep));
    /// <summary>Number of presentation operands referenced by the ceiling-tube entries.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    /// <summary>Returns the native address of a presentation operand in the compiled layout.</summary>
    /// <param name="index">Zero-based operand index.</param>
    /// <returns>The bank-local address read from the operand's instruction slot.</returns>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    /// <summary>Determines whether the projectile kind is one of the four Mother Brain ceiling tubes.</summary>
    /// <param name="kind">Projectile kind to check.</param>
    /// <returns><see langword="true"/> when this definition owns the kind; otherwise, <see langword="false"/>.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainTopRightTube or
        RoomEnemyProjectileKind.MotherBrainTopLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube;

    /// <summary>Reads a compiled mechanics word by its bank-local instruction address.</summary>
    /// <param name="address">Address of a mechanics word in bank <see cref="Bank"/>.</param>
    /// <returns>The compiled word stored at the requested address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word in this layout.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Mother Brain ceiling-tube mechanics pointer $86:{address:X4} is not compiled.");
}
