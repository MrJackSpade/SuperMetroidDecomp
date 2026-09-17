namespace SuperMetroid.Core.Assets;

/// <summary>Semantic identities and native extraction sources for pause equipment labels.</summary>
public static class PauseEquipmentLabelDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-equipment-labels.json";
    public const int TilemapColumns = 32;
    public const int TilemapRows = 32;
    public const int BeamWords = 5;
    public const int EquipmentWords = 9;
    public const int DisabledPalette = 3;

    /// <summary><c>kEquipmentScreenTilemap_Blank</c> at $82:C01A.</summary>
    public const ushort BlankSource = 0xc01a;

    /// <summary><c>kHyperBeamWeaponsTilemaps</c> at $82:C0A8.</summary>
    public const int HyperPointerTable = 0x82c0a8;

    public static readonly string[][] Keys =
    [
        [],
        ["Beam.Charge", "Beam.Ice", "Beam.Wave", "Beam.Spazer", "Beam.Plasma"],
        ["Equipment.Varia", "Equipment.Gravity", "Equipment.MorphBall", "Equipment.Bombs", "Equipment.SpringBall", "Equipment.ScrewAttack"],
        ["Boots.HiJump", "Boots.SpaceJump", "Boots.SpeedBooster"],
    ];

    public const string HyperKey = "Beam.Hyper";
    public const string PlasmaKey = "Beam.Plasma";
    public const string VariaKey = "Equipment.Varia";
    /// <summary>Hyper mode's pointer table places its only nonblank patch in the Wave slot.</summary>
    public const int HyperBeamItem = 2;
    /// <summary>Native Plasma label destination at equipment-page cell $284.</summary>
    public const int NativePlasmaDestinationCell = 0x284;
    /// <summary>The ninth Boots-length Plasma word is subsequently owned by the wireframe at cell $28C.</summary>
    public const int NativePlasmaWireframeOverlapCell = 0x28c;

    public static string Key(int category, int item)
    {
        if ((uint)category >= Keys.Length || (uint)item >= Keys[category].Length)
            throw new ArgumentOutOfRangeException(nameof(item), $"No pause equipment label exists for category {category}, item {item}.");
        return Keys[category][item];
    }

    public static int WordCount(int category) => category == 1 ? BeamWords : EquipmentWords;
}
