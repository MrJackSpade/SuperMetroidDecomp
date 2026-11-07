using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The four installed sheets whose pages Mother Brain's encounter shows its sprites, and their page counts (#1527).</summary>
public static class MotherBrainSpecialSpriteSheetDefinitions
{
    /// <summary>Eight phase-two leg pages.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Legs = new("mother-brain-leg-tiles.png", MotherBrainSpecialSpriteArtworkDefinitions.LegPageCount);

    /// <summary>Four Baby Metroid pages.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid = new("mother-brain-baby-tiles.png", MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroidPageCount);

    /// <summary>Four Mother Brain attack pages.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack = new("mother-brain-attack-tiles.png", MotherBrainSpecialSpriteArtworkDefinitions.AttackPageCount);

    /// <summary>The two exploded escape-door pages.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor = new("mother-brain-exploded-door-tiles.png", MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoorPageCount);

    /// <summary>All four separate editable source sheets, in the order the encounter first shows them: legs, Baby Metroid, attack, escape door.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } = [Legs, BabyMetroid, Attack, ExplodedDoor];
}
