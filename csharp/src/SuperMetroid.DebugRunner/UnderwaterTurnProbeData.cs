using SuperMetroid.Core.Game;

/// <summary>Private recording/retail room identities for the #307 diagnostic.</summary>
internal static class UnderwaterTurnProbeData
{
    /// <summary>$8F:CEFB, the Maridia tube room next to the exported FILE C save station.</summary>
    public const ushort BrokenTubeRoom = 0xcefb;

    /// <summary>Recorded equipment $3105: Varia, Morph, Hi-Jump, Bombs, Speed Booster; no Gravity Suit.</summary>
    public const ushort ComparisonEquipment = (ushort)(SamusEquipmentFlags.VariaSuit |
        SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots |
        SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster);
}
