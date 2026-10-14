namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge OAM identities paired with the four $8B:EE5D..EE9A logo lists.</summary>
public static class EndingLogoSpriteDefinitions
{
    /// <summary>$8C:B97F, ScrewAttackSymbolUpperPart, followed by the lower S,
    /// three right-wrap and three left-wrap compositions.</summary>
    private const ushort FirstFrame = 0xb97f;
    /// <summary>Two letter sections and three stages for each side of the surrounding circle.</summary>
    private const int FrameCount = 8;
    /// <summary>The upper and lower S records each carry fourteen OAM parts.</summary>
    private const int SParts = 14;
    /// <summary>The right and left circle groups advance through matching partial and complete shapes.</summary>
    private enum CircleStage
    {
        /// <summary>First twelve-part section of one side of the circle.</summary>
        First,
        /// <summary>Second eighteen-part section of one side of the circle.</summary>
        Second,
        /// <summary>Complete 25-part section of one side of the circle.</summary>
        Complete,
    }

    /// <summary>Gets the two letter segments followed by three right-wrap and three left-wrap stages.</summary>
    public static IReadOnlyList<EndingLogoSpriteFrameDefinition> Frames { get; } = new FrameView();

    /// <summary>Walks the variable-size OAM records from $8C:B97F to locate one ending-logo frame.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 2) return (ushort)(FirstFrame + index * RecordBytes(SParts));
        int circle = (index - 2) / 3;
        int stage = (index - 2) % 3;
        int offset = 2 * RecordBytes(SParts);
        for (int preceding = 0; preceding < 3; preceding++)
            offset += circle * RecordBytes(CircleParts((CircleStage)preceding));
        for (int preceding = 0; preceding < stage; preceding++)
            offset += RecordBytes(CircleParts((CircleStage)preceding));
        return (ushort)(FirstFrame + offset);
    }

    /// <summary>Uses the cartridge's twelve, eighteen, and 25 part counts for each circle stage.</summary>
    private static int CircleParts(CircleStage stage) => stage switch
    {
        CircleStage.First => 12,
        CircleStage.Second => 18,
        CircleStage.Complete => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(stage)),
    };
    /// <summary>Accounts for the part count word and five encoded bytes per OAM part.</summary>
    private static int RecordBytes(int parts) => sizeof(ushort) + parts * 5;

    /// <summary>Pairs the calculated cartridge pointer with the JSON name and expected part count.</summary>
    private static EndingLogoSpriteFrameDefinition Get(int index)
    {
        ushort pointer = FramePointer(index);
        if (index < 2) return new(index == 0 ? "s-upper" : "s-lower", pointer, SParts);
        int stage = (index - 2) % 3;
        string side = index < 5 ? "right" : "left";
        string name = "circle-" + side + "-" + (stage + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return new(name, pointer, CircleParts((CircleStage)stage));
    }

    /// <summary>Exposes the eight calculated frame identities without a stored pointer table.</summary>
    private sealed class FrameView : IReadOnlyList<EndingLogoSpriteFrameDefinition>
    {
        /// <summary>The script uses two S sections and six circle stages.</summary>
        public int Count => FrameCount;
        /// <summary>Resolves the selected stage to its cartridge pointer and JSON key.</summary>
        public EndingLogoSpriteFrameDefinition this[int index] => Get(index);
        /// <summary>Visits all frame definitions in cinematic instruction order.</summary>
        public IEnumerator<EndingLogoSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        /// <summary>Shares the typed frame order with nongeneric collection callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
