using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Zero crawler OAM identities for its four compiled surface-orientation loops.
/// Orientation changes, velocity, timers and collision stay in the crawler code.
/// </summary>
internal static class ZeroVisualDefinitions
{
    /// <summary>Zero instruction and OAM bank $A3.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>Sixteen selected Spritemap_Zero records at $A3:995B..9B6A.</summary>
    internal const int FrameCount = 16;

    /// <summary>Builds the distinct spritemap list selected by Zero's presentation words, preserving each frame's first-use order.</summary>
    /// <returns>The sixteen OAM frame definitions used by Zero's compiled orientation loops.</returns>
    /// <exception cref="InvalidDataException">The compiled instruction words do not select the expected number of distinct frames.</exception>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seen = new HashSet<ushort>();
        for (int index = 0; index < ZeroInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort pointer = FrameAt(ZeroInstructionProgramDefinitions.PresentationWordAddress(index));
            // Each six-step loop repeats two intermediate frames. Keep that
            // repetition in the compiled timing, not in the editable export list.
            if (seen.Add(pointer))
                frames.Add(new(Bank, pointer, $"zero_crawler_{frames.Count:D2}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Zero programs select {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return [.. frames];
    }

    /// <summary>Resolves a presentation-word operand address to the selected Zero spritemap pointer.</summary>
    /// <param name="operandAddress">Address of a Zero presentation word whose selector has been compiled.</param>
    /// <returns>The OAM pointer selected by the operand.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled Zero visual operand.</exception>
    internal static ushort FrameAt(ushort operandAddress)
    {
        for (int index = 0; index < ZeroInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            if (operandAddress != ZeroInstructionProgramDefinitions.PresentationWordAddress(index))
                continue;
            if (CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort pointer))
                return pointer;
            break;
        }
        throw new InvalidDataException($"Zero visual operand $A3:{operandAddress:X4} is not compiled.");
    }
}
