using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Stable visual bindings for native equipment selectors; no editable navigation table.</summary>
public static class PauseSelectorDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-selectors.json";
    public const int AnchorCount = 16, MaximumFrames = 255, MaximumPhases = 255, MaximumDuration = 254;
    /// <summary>$82:C18E equipment-selector positions: tanks, weapons, suit/misc, boots.</summary>
    public const int PositionPointers = PauseMenuRomData.EquipmentSelectorPositionPointerTable;
    /// <summary>$82:C0EC points to selector timing entries, (duration, unused, sprite offset).</summary>
    public const int AnimationPointer = PauseMenuRomData.ItemSelectorAnimationPointer;
    /// <summary>$82:AB98 initializes the selector timer from the first byte at $82:C10C.</summary>
    public const int InitialTimer = PauseMenuRomData.ItemSelectorAnimationTimer;
    /// <summary>$82:C0DA identifies the low-byte category variable at WRAM $0755.</summary>
    public const int VariantPointer = PauseMenuRomData.ItemSelectorAnimationVariantPointer;
    /// <summary>$7E:0755 PauseMenu_EquipmentScreenCategoryIndex; the animation reads only its low byte.</summary>
    public const ushort CategoryVariable = 0x0755;
    /// <summary>$82:C1E8 points to the four category sprite bases.</summary>
    public const int BasePointer = PauseMenuRomData.EquipmentSelectorBaseTablePointer;
    /// <summary>$82:C100 is the native caller palette word, selecting OBJ palette three.</summary>
    public const int PaletteSource = PauseMenuRomData.SelectedItemSpritemapPointer;
    /// <summary>Bank $82 owns selector positions, timing and sprite-pointer data.</summary>
    public const int Bank = 0x820000;
    private static readonly string[][] anchors =
    [
        ["Reserve.Mode", "Reserve.Transfer"],
        ["Beam.Charge", "Beam.Ice", "Beam.Wave", "Beam.Spazer", "Beam.Plasma"],
        ["Equipment.Varia", "Equipment.Gravity", "Equipment.MorphBall", "Equipment.Bombs", "Equipment.SpringBall", "Equipment.ScrewAttack"],
        ["Boots.HiJump", "Boots.SpaceJump", "Boots.SpeedBooster"],
    ];
    public static IEnumerable<(int Category, int Item, string Name)> Anchors()
    {
        for (int category = 0; category < anchors.Length; category++)
        for (int item = 0; item < anchors[category].Length; item++) yield return (category, item, anchors[category][item]);
    }
    public static string Anchor(int category, int item) => (uint)category < anchors.Length && (uint)item < anchors[category].Length
        ? anchors[category][item] : throw new ArgumentOutOfRangeException(nameof(item), "Invalid equipment selector category/item.");
    /// <summary>$82:C202 bases: Reserve=$14, Beam=$15, Suit and Boots=$16. Retained as diagnostic identities.</summary>
    public static ushort NativeSpriteId(int category) => category switch
    { 0 => 0x14, 1 => 0x15, 2 or 3 => 0x16, _ => throw new ArgumentOutOfRangeException(nameof(category)) };
    public static string Group(int category) => category switch
    { 0 => "Reserve", 1 => "Beam", 2 or 3 => "Equipment", _ => throw new ArgumentOutOfRangeException(nameof(category)) };
}
