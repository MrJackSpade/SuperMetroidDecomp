using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable Mama/Baby Turtle OAM compositions selected by the compiled tatori
/// programs. Movement, shell callbacks, durations and collision remain in code.
/// </summary>
internal static class MamaTurtleVisualDefinitions
{
    /// <summary>Tatori instruction and OAM bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Twenty-nine selected Spritemap_BabyTurtle/MamaTurtle records at $A2:94D9..983F.</summary>
    internal const int FrameCount = 29;

    /// <summary>Builds one spritemap definition for each distinct frame selected by the compiled Mama/Baby Turtle programs.</summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seen = new HashSet<ushort>();
        for (int index = 0; index < MamaTurtleInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort pointer = FrameAt(MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index));
            // Crawling, shell and spinning programs reuse native compositions.
            // Export each actual selection once, leaving repetitions in mechanics.
            if (seen.Add(pointer))
                frames.Add(new(Bank, pointer, $"tatori_{frames.Count:D2}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Tatori programs select {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return [.. frames];
    }

    /// <summary>Resolves a compiled Mama/Baby Turtle presentation operand to its bank-$A2 spritemap pointer.</summary>
    /// <param name="operandAddress">The instruction operand address within one of the compiled presentation lists.</param>
    /// <returns>The selected spritemap pointer.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled visual operand.</exception>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < MamaTurtleInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            if (operandAddress != MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index))
                continue;
            if (CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort pointer))
                return pointer;
            break;
        }
        throw new InvalidDataException($"Tatori visual operand $A2:{operandAddress:X4} is not compiled.");
    }
}
