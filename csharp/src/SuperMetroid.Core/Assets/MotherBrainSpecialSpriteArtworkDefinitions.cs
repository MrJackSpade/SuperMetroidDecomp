namespace SuperMetroid.Core.Assets;

/// <summary>Native visual sources and destinations of the three fixed Mother Brain lists.</summary>
public static class MotherBrainSpecialSpriteArtworkDefinitions
{
    /// <summary>$B7:9000-$9FFF, eight phase-two leg pages loaded by $A9:8F8F.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Legs =
        new("mother-brain-leg-tiles.png", 0xb79000, 8, 0x7400);

    /// <summary>$B1:8800-$8FFF, four Baby Metroid pages loaded by $A9:8FE5.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid =
        new("mother-brain-baby-tiles.png", 0xb18800, 4, 0x7c00);

    /// <summary>$B7:A000-$A7FF, four Mother Brain attack pages restored by $A9:8FC7.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack =
        new("mother-brain-attack-tiles.png", 0xb7a000, 4, 0x7c00);

    /// <summary>$AB:F400-$F7FF, the two exploded escape-door pages from $A9:902F.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor =
        new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 0x7000);

    /// <summary>All four separate editable source sheets.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } =
        new SheetList();

    /// <summary>Looks up the sheet containing one native source address.</summary>
    public static bool TryForSource(uint sourceAddress,
        out MotherBrainSpecialSpriteSheetDefinition definition)
    {
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in All)
        {
            if (!sheet.ContainsSource(sourceAddress)) continue;
            definition = sheet;
            return true;
        }
        definition = default;
        return false;
    }
    /// <summary>Keeps the four fixed transfer sources in their native list order without a mutable backing array.</summary>
    private sealed class SheetList : IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition>
    {
        /// <summary>The leg, Baby Metroid, attack, and escape-door lists each have one source sheet.</summary>
        public int Count => 4;
        /// <summary>Maps a native list index to its corresponding fixed source sheet.</summary>
        public MotherBrainSpecialSpriteSheetDefinition this[int index] => index switch
        {
            0 => Legs,
            1 => BabyMetroid,
            2 => Attack,
            3 => ExplodedDoor,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Visits the fixed sheets in the same order as the native transfer lists.</summary>
        public IEnumerator<MotherBrainSpecialSpriteSheetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same sheet order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
