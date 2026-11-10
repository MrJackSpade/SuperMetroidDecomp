namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Stable visual bindings for equipment selectors; no editable navigation table.</summary>
public static class PauseSelectorDefinitions
{
    /// <summary>The reserve column's two controls: the mode toggle and the transfer button.</summary>
    internal const int ReserveControlCount = 2;

    /// <summary>The stable asset anchor name of each equipment control, by category and item.</summary>
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
}
