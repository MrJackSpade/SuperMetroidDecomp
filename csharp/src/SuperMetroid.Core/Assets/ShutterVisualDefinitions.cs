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
        new(Bank, GrowingFrame(0), "shutter_growing_10px"),
        new(Bank, GrowingFrame(1), "shutter_growing_20px"),
        new(Bank, GrowingFrame(2), "shutter_growing_30px"),
        new(Bank, GrowingFrame(3), "shutter_vertical_40px"),
        new(Bank, HorizontalFrame, "shutter_horizontal"),
    ];

    /// <summary><c>Spritemap_Shutters_Horizontal</c> at $A2:EDB1.</summary>
    private const ushort HorizontalFrame = 0xedb1;

    // A stage with n sprites occupies 2+5n bytes, followed by an unused
    // intermediate map with n+1 sprites occupying 2+5(n+1) bytes.
    // Sum those pairs for the preceding stages, where n starts at one.
    private static ushort GrowingFrame(int stage) => (ushort)(0xed44 + 19 * stage + 5 * stage * (stage - 1));

    internal static bool IsPresentationWord(ushort address) =>
        GrowingShutterInstructionProgramDefinitions.IsPresentationWord(address) ||
        HorizontalShutterInstructionProgramDefinitions.IsPresentationWord(address);

    internal static ushort PointerAt(ushort operandAddress)
    {
        if (GrowingShutterInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            return GrowingFrame((operandAddress - (GrowingShutterInstructionProgramDefinitions.TenPixels + 2)) / 6);
        if (HorizontalShutterInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            return HorizontalFrame;
        throw new InvalidDataException($"Shutter visual operand $A2:{operandAddress:X4} is not compiled.");
    }
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
        if (isPresentation) return PointerAt(operandAddress);
        throw new InvalidDataException(
            $"Shutter ${enemyDefinition:X4} visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
