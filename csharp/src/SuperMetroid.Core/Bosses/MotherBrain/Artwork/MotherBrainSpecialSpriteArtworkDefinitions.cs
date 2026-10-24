namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The four sheets whose pages Mother Brain's encounter shows its sprites, where each comes from and where it shows.</summary>
public static class MotherBrainSpecialSpriteArtworkDefinitions
{
    /// <summary>$B7:9000-$9FFF, eight phase-two leg pages shown from sprite character 320 ($A9:8F8F).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Legs =
        new("mother-brain-leg-tiles.png", 0xb79000, 8, 320);

    /// <summary>$B1:8800-$8FFF, four Baby Metroid pages shown from sprite character 448 ($A9:8FE5).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid =
        new("mother-brain-baby-tiles.png", 0xb18800, 4, 448);

    /// <summary>$B7:A000-$A7FF, four Mother Brain attack pages shown again from sprite character 448 ($A9:8FC7).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack =
        new("mother-brain-attack-tiles.png", 0xb7a000, 4, 448);

    /// <summary>$AB:F400-$F7FF, the two exploded escape-door pages shown from sprite character 256 ($A9:902F).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor =
        new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 256);

    /// <summary>All four separate editable source sheets, in the order the encounter first shows them: legs, Baby Metroid, attack, escape door.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } = [Legs, BabyMetroid, Attack, ExplodedDoor];
}
