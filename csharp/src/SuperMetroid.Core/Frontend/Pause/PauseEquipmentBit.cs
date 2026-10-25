using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The upgrade one pause-menu equipment control stands for: a beam bit for the beam category, an item bit for the
/// others (#1323). Exactly one of the two is set; testing or toggling both sets therefore acts on its own.
/// </summary>
/// <param name="Beam">The beam bit, or none.</param>
/// <param name="Item">The item bit, or none.</param>
public readonly record struct PauseEquipmentBit(SamusBeamFlags Beam, SamusEquipmentFlags Item)
{
    /// <summary>A beam control's bit.</summary>
    public static PauseEquipmentBit Of(SamusBeamFlags beam) => new(beam, 0);

    /// <summary>An item control's bit.</summary>
    public static PauseEquipmentBit Of(SamusEquipmentFlags item) => new(0, item);

    /// <summary>Whether the upgrade is set in <paramref name="beams"/> or <paramref name="items"/>.</summary>
    public bool IsSetIn(SamusBeamFlags beams, SamusEquipmentFlags items) => (beams & Beam) != SamusBeamFlags.None || (items & Item) != SamusEquipmentFlags.None;
}
