namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Stable visual bindings for native equipment selectors; no editable navigation table.</summary>
public static class PauseSelectorDefinitions
{
    /// <summary>Supported revision of the equipment-selector anchor, composition, animation, and palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the editable pause-equipment selector presentation.</summary>
    public const string FileName = "pause-selectors.json";
    /// <summary>Presentation limits: sixteen required equipment anchors, at most 255 named visual frames and 255 cyclic phases, and initial/phase dwell times from 1 through 254 accepted update ticks.</summary>
    public const int AnchorCount = 16, MaximumFrames = 255, MaximumPhases = 255, MaximumDuration = 254;


    /// <summary>The reserve column's two controls: the mode toggle and the transfer button.</summary>
    internal const int ReserveControlCount = 2;

    /// <summary>The stable asset anchor name of each native equipment control, by category and item.</summary>
    internal static readonly IReadOnlyDictionary<PauseEquipmentSlot, string> AnchorNames = new Dictionary<PauseEquipmentSlot, string>
    {
        [new(PauseEquipmentCategory.Reserves, 0)] = "Reserve.Mode",
        [new(PauseEquipmentCategory.Reserves, 1)] = "Reserve.Transfer",
        [new(PauseEquipmentCategory.Beams, 0)] = "Beam.Charge",
        [new(PauseEquipmentCategory.Beams, 1)] = "Beam.Ice",
        [new(PauseEquipmentCategory.Beams, 2)] = "Beam.Wave",
        [new(PauseEquipmentCategory.Beams, 3)] = "Beam.Spazer",
        [new(PauseEquipmentCategory.Beams, 4)] = "Beam.Plasma",
        [new(PauseEquipmentCategory.Suits, 0)] = "Equipment.Varia",
        [new(PauseEquipmentCategory.Suits, 1)] = "Equipment.Gravity",
        [new(PauseEquipmentCategory.Suits, 2)] = "Equipment.MorphBall",
        [new(PauseEquipmentCategory.Suits, 3)] = "Equipment.Bombs",
        [new(PauseEquipmentCategory.Suits, 4)] = "Equipment.SpringBall",
        [new(PauseEquipmentCategory.Suits, 5)] = "Equipment.ScrewAttack",
        [new(PauseEquipmentCategory.Boots, 0)] = "Boots.HiJump",
        [new(PauseEquipmentCategory.Boots, 1)] = "Boots.SpaceJump",
        [new(PauseEquipmentCategory.Boots, 2)] = "Boots.SpeedBooster",
    };

    /// <summary>$82:C196..C1D5: the native screen position of each category's first selector, including the draw routine's
    /// one-pixel bias.</summary>
    internal static readonly IReadOnlyDictionary<PauseEquipmentCategory, Position16<ScreenPixels>> CategoryOrigins =
        new Dictionary<PauseEquipmentCategory, Position16<ScreenPixels>>
        {
            [PauseEquipmentCategory.Reserves] = new(0x1b, 0x54),
            [PauseEquipmentCategory.Beams] = new(0x30, 0x84),
            [PauseEquipmentCategory.Suits] = new(0xcc, 0x4c),
            [PauseEquipmentCategory.Boots] = new(0xcc, 0x9c),
        };

    /// <summary>Vertical distance between successive items' selectors, one eight-pixel row.</summary>
    internal const int ItemRowStride = 8;
    /// <summary>The native draw routine's one-pixel bias on both selector coordinates.</summary>
    internal const int DrawBias = 1;
    /// <summary>The first suit/misc item below the gap after Gravity (Morph Ball).</summary>
    internal const int SuitsLowerGroupFirstItem = 2;
    /// <summary>Suit/misc leaves two extra eight-pixel rows between Gravity and Morph Ball.</summary>
    internal const int SuitsLowerGroupGap = 16;

    /// <summary>The visual animation role of each equipment category; suit/miscellaneous items and boots share Equipment.</summary>
    internal static readonly IReadOnlyDictionary<PauseEquipmentCategory, string> Groups = new Dictionary<PauseEquipmentCategory, string>
    {
        [PauseEquipmentCategory.Reserves] = "Reserve",
        [PauseEquipmentCategory.Beams] = "Beam",
        [PauseEquipmentCategory.Suits] = "Equipment",
        [PauseEquipmentCategory.Boots] = "Equipment",
    };
}
