using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The Landing Site gunship's two hull and eleven entrance-pad compositions.
/// These identities follow the compiled bank-$A2 instruction selectors; opening,
/// closing, flight timing and actor positions remain gameplay-owned.
/// </summary>
internal static class GunshipVisualDefinitions
{
    /// <summary>Gunship instruction/OAM bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Thirteen distinct Spritemap_Ship_0..C records at $A2:AD81..AFDD.</summary>
    internal const int FrameCount = 13;
    /// <summary>The two hull frames; the other eleven selected frames are entrance-pad poses.</summary>
    private enum HullFrame : ushort
    {
        /// <summary>Spritemap_Ship_0 at $A2:AD81, selected by InstList_ShipTop.</summary>
        Top = 0xad81,
        /// <summary>Spritemap_Ship_1 at $A2:ADDD, selected by InstList_ShipBottom.</summary>
        Bottom = 0xaddd,
    }

    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seen = new HashSet<ushort>();
        int padIndex = 0;
        for (int index = 0; index < GunshipInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = GunshipInstructionProgramDefinitions.PresentationWordAddress(index);
            if (!CompiledEnemyVisualSelectors.TryGet(Bank, operand, out ushort pointer))
                throw new InvalidDataException($"Gunship visual selector $A2:{operand:X4} is not compiled.");
            // The closing program reuses the opening frames in reverse. Export
            // each native identity once, keeping the actual timing in the program.
            if (!seen.Add(pointer)) continue;
            string name = !Enum.IsDefined((HullFrame)pointer)
                ? $"gunship_entrance_{padIndex++:D2}"
                : (HullFrame)pointer switch
                {
                    HullFrame.Top => "gunship_top_hull",
                    HullFrame.Bottom => "gunship_bottom_hull",
                    _ => throw new InvalidOperationException($"Undefined {nameof(HullFrame)} {pointer:X4}."),
                };
            frames.Add(new(Bank, pointer, name));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Gunship programs select {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return [.. frames];
    }
}
