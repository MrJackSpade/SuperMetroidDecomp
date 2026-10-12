using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The twelve distinct body and wing OAM compositions selected by Dragon's six
/// compiled bank-$A2 programs. Animation timing and attack callbacks stay in
/// <see cref="DragonInstructionProgramDefinitions"/>.
/// </summary>
internal static class DragonVisualDefinitions
{
    /// <summary>Native Dragon spritemap bank $A2.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>$A2:E80C / Spritemap_Dragon_0: four eight-object body maps precede two one-object wing maps.</summary>
    private const ushort FirstBody = 0xe80c;
    internal static EnemySpritemapDefinition[] Frames()
    {
        var result = new EnemySpritemapDefinition[12];
        for (int side = 0; side < 2; side++)
        {
            string facing = side == 0 ? "left" : "right";
            int body = FirstBody + side * (4 * 42 + 2 * 7);
            result[side * 3] = new(Bank, (ushort)body, $"dragon_body_idle_{facing}");
            for (int frame = 0; frame < 2; frame++)
                result[side * 3 + 1 + frame] = new(Bank, (ushort)(body + 4 * 42 + frame * 7), $"dragon_wing_{facing}_{frame}");
            for (int frame = 0; frame < 3; frame++)
                result[6 + side * 3 + frame] = new(Bank, (ushort)(body + (frame + 1) * 42), $"dragon_body_attack_{facing}_{frame}");
        }
        return result;
    }
    /// <summary>Resolves one of Dragon's sixteen compiled presentation operands.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0;
             index < DragonInstructionProgramDefinitions.PresentationWordCount;
             index++)
            if (DragonInstructionProgramDefinitions.PresentationWordAddress(index) ==
                    operandAddress &&
                CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                    out ushort frame))
                return frame;
        throw new InvalidDataException(
            $"Dragon visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
