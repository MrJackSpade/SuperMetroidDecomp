using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Five distinct bank-$A2 shutter OAM compositions. The forty-pixel frame is
/// shared by the growing and plain vertical shutter programs; motion, collision,
/// shot reactions and program timing remain compiled gameplay behavior.
/// </summary>
internal static class ShutterVisualDefinitions
{
    /// <summary>Native shutter sprite-map bank.</summary>
    internal const byte Bank = 0xa2;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xed44, "shutter_growing_10px"),
        new(Bank, 0xed57, "shutter_growing_20px"),
        new(Bank, 0xed74, "shutter_growing_30px"),
        new(Bank, 0xed9b, "shutter_vertical_40px"),
        new(Bank, 0xedb1, "shutter_horizontal"),
    ];

    /// <summary>Accepts only a native visual operand for the specified shutter family.</summary>
    internal static ushort FrameAt(ushort enemyDefinition, ushort operandAddress)
    {
        bool isPresentation = enemyDefinition switch
        {
            RoomEnemySystem.GrowingShutterDefinition =>
                GrowingShutterInstructionProgramDefinitions.IsPresentationWord(operandAddress),
            RoomEnemySystem.ShootableVerticalShutterDefinition or
                RoomEnemySystem.DestroyableVerticalShutterDefinition =>
                VerticalShutterInstructionProgramDefinitions.IsPlainShutterPresentationWord(
                    operandAddress),
            RoomEnemySystem.ShootableHorizontalShutterDefinition =>
                HorizontalShutterInstructionProgramDefinitions.IsPresentationWord(operandAddress),
            _ => false,
        };
        if (isPresentation &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Shutter ${enemyDefinition:X4} visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
