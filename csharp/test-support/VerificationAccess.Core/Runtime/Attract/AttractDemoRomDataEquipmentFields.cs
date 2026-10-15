namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>Word offsets in a DemoSetDef record at bank $91.</summary>
internal static class AttractDemoRomDataEquipmentFields
{
    /// <summary>Collected item-bit word at the start of an equipment record.</summary>
    public const int Items = 0;
    /// <summary>Missile count word after the item bits.</summary>
    public const int Missiles = 2;
    /// <summary>Super Missile count word.</summary>
    public const int SuperMissiles = 4;
    /// <summary>Power Bomb count word.</summary>
    public const int PowerBombs = 6;
    /// <summary>Current health word.</summary>
    public const int Health = 8;
    /// <summary>Collected beam-bit word, separate from equipped beams.</summary>
    public const int CollectedBeams = 10;
    /// <summary>Equipped beam-bit word.</summary>
    public const int EquippedBeams = 12;
    /// <summary>Bank-91 demo input-object pointer word.</summary>
    public const int InputObject = 14;
}
