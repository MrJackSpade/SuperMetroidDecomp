using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Four shared bank-$A2 OAM compositions selected by the Mellow, Mella and Memu
/// flight loop. The loop's cadence and movement remain compiled gameplay data.
/// </summary>
internal static class FlyVisualDefinitions
{
    /// <summary>Native bank for the three fly-family enemy definitions.</summary>
    internal const byte Bank = 0xa2;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xb1e8, "fly_shared_0"),
        new(Bank, 0xb1ef, "fly_shared_1"),
        new(Bank, 0xb1f6, "fly_shared_2"),
        new(Bank, 0xb1fd, "fly_shared_3"),
    ];

    /// <summary>Accepts only a visual operand in the native shared flight loop.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (FlyInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Fly-family visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
