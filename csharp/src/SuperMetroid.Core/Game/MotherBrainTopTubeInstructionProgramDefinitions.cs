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

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xcc43),
        Entry(TopRight),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(TopLeft),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(TopMiddleLeft),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(TopMiddleRight),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep));
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainTopRightTube or
        RoomEnemyProjectileKind.MotherBrainTopLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube;

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Mother Brain ceiling-tube mechanics pointer $86:{address:X4} is not compiled.");
}
