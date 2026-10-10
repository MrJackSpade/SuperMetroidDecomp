using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Ripper-family visual selection from direction and the neutral/first/neutral/second wing cycle.</summary>
internal static class RipperVisualDefinitions
{
    /// <summary>$A2:E3C5, first shared GRipper/Ripper II direction, followed by four/three/four-part compositions.</summary>
    private const ushort SharedFirstDirectionFrame = 0xe3c5;
    /// <summary>$A2:E527, the three left-facing ordinary Ripper two-part compositions, followed by right-facing ones.</summary>
    private const ushort OrdinaryLeftFrame = 0xe527;

    internal static ushort FrameAt(EnemyDefinitionId enemyDefinition, ushort address)
    {
        bool ordinary = enemyDefinition switch
        {
            EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 => false,
            EnemyDefinitionId.Ripper => true,
            _ => throw new InvalidDataException(
                $"Enemy ${(int)enemyDefinition:X4} has no compiled Ripper visuals."),
        };
        int offset = address - (ordinary ? RipperInstructionProgramDefinitions.RipperMovingRight :
            RipperInstructionProgramDefinitions.GRipperMovingLeft) - 2;
        if (!ordinary && !IsVisualOffset(offset))
            offset = address - RipperInstructionProgramDefinitions.Ripper2MovingRight - 2;
        if (!IsVisualOffset(offset))
            throw new InvalidDataException(
                $"Ripper-family ${(int)enemyDefinition:X4} visual operand ${address:X4} is not compiled.");

        int direction = offset / 20;
        int timedFrame = (offset % 20) / 4;
        int drawing = (timedFrame & 1) == 0 ? 0 : (timedFrame + 1) / 2;
        if (ordinary)
        {
            const int compositionBytes = 2 + 5 * 2;
            return (ushort)(OrdinaryLeftFrame + ((1 - direction) * 3 + drawing) * compositionBytes);
        }
        const int fullCompositionBytes = 2 + 5 * 4;
        const int shortCompositionBytes = 2 + 5 * 3;
        return (ushort)(SharedFirstDirectionFrame + direction * (2 * fullCompositionBytes + shortCompositionBytes) +
            drawing * fullCompositionBytes - (drawing / 2) * (fullCompositionBytes - shortCompositionBytes));
    }

    private static bool IsVisualOffset(int offset) =>
        offset >= 0 && offset <= 32 && offset % 20 <= 12 && (offset % 20 & 3) == 0;
}