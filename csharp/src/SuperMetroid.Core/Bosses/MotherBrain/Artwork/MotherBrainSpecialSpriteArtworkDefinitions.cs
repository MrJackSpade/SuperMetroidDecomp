namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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

    /// <summary>All four separate editable source sheets, in the native transfer lists' order: legs, Baby Metroid, attack, escape door.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } = [Legs, BabyMetroid, Attack, ExplodedDoor];

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
}
