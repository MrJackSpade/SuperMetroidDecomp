namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The four sheets whose pages Mother Brain's encounter shows its sprites, and where each shows.</summary>
public static class MotherBrainSpecialSpriteArtworkDefinitions
{
    /// <summary>Eight phase-two leg pages shown from sprite character 320 ($A9:8F8F).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Legs =
        new(MotherBrainSpritePageSheet.Legs, "mother-brain-leg-tiles.png", 8, 320);

    /// <summary>Four Baby Metroid pages shown from sprite character 448 ($A9:8FE5).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid =
        new(MotherBrainSpritePageSheet.BabyMetroid, "mother-brain-baby-tiles.png", 4, 448);

    /// <summary>Four Mother Brain attack pages shown again from sprite character 448 ($A9:8FC7).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack =
        new(MotherBrainSpritePageSheet.Attack, "mother-brain-attack-tiles.png", 4, 448);

    /// <summary>The two exploded escape-door pages shown from sprite character 256 ($A9:902F).</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor =
        new(MotherBrainSpritePageSheet.ExplodedDoor, "mother-brain-exploded-door-tiles.png", 2, 256);

    /// <summary>All four separate editable source sheets, in the order the encounter first shows them: legs, Baby Metroid, attack, escape door.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } = [Legs, BabyMetroid, Attack, ExplodedDoor];
}
