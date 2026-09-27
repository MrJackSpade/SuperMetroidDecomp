using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The 27 ordinary OAM compositions shared by powered and unpowered Work Robots.
/// Their 227 presentation operands are selected by compiled native bytecode;
/// walking, wall checks, laser events, and sound timing remain game mechanics.
/// </summary>
internal static class WorkRobotVisualDefinitions
{
    /// <summary>Work Robot instruction and spritemap bank, $A8.</summary>
    internal const byte Bank = 0xa8;
    internal const int FrameCount = 27;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xd1f1, "work_robot_left_00"),
        new(Bank, 0xd22f, "work_robot_left_01"),
        new(Bank, 0xd26d, "work_robot_left_02"),
        new(Bank, 0xd2ab, "work_robot_left_03"),
        new(Bank, 0xd2e9, "work_robot_left_04"),
        new(Bank, 0xd327, "work_robot_left_05"),
        new(Bank, 0xd365, "work_robot_left_06"),
        new(Bank, 0xd3a3, "work_robot_left_07"),
        new(Bank, 0xd3e1, "work_robot_left_08"),
        new(Bank, 0xd41f, "work_robot_left_09"),
        new(Bank, 0xd45d, "work_robot_left_10"),
        new(Bank, 0xd49b, "work_robot_left_11"),
        new(Bank, 0xd4d9, "work_robot_right_00"),
        new(Bank, 0xd517, "work_robot_right_01"),
        new(Bank, 0xd555, "work_robot_right_02"),
        new(Bank, 0xd593, "work_robot_right_03"),
        new(Bank, 0xd5d1, "work_robot_right_04"),
        new(Bank, 0xd60f, "work_robot_right_05"),
        new(Bank, 0xd64d, "work_robot_right_06"),
        new(Bank, 0xd68b, "work_robot_right_07"),
        new(Bank, 0xd6c9, "work_robot_right_08"),
        new(Bank, 0xd707, "work_robot_right_09"),
        new(Bank, 0xd745, "work_robot_right_10"),
        new(Bank, 0xd783, "work_robot_right_11"),
        new(Bank, 0xd7e1, "work_robot_unpowered_neutral"),
        new(Bank, 0xd7c1, "work_robot_unpowered_left"),
        new(Bank, 0xd801, "work_robot_unpowered_right"),
    ];

    /// <summary>Resolves only the 227 authored Work Robot presentation operands.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < WorkRobotInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            if (WorkRobotInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        }
        throw new InvalidDataException(
            $"Work Robot visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
