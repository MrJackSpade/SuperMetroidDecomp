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

    /// <summary><c>Spritemap_Mellow_Mella_Menu_0</c> at $A2:B1E8; four one-object, seven-byte OAM frames.</summary>
    private const ushort FirstFrame = 0xb1e8;

    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new EnemySpritemapDefinition[4];
        for (int frame = 0; frame < frames.Length; frame++)
            frames[frame] = new(Bank, (ushort)(FirstFrame + frame * 7), $"fly_shared_{frame}");
        return frames;
    }
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
