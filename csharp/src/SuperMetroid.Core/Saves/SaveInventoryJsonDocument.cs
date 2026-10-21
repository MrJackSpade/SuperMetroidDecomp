using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>Collected and currently enabled equipment bit sets.</summary>
/// <param name="CollectedItems">All acquired suit, mobility, explosive, and exploration item flags.</param>
/// <param name="EquippedItems">The acquired item flags currently enabled for gameplay.</param>
/// <param name="CollectedBeams">All acquired beam-component flags.</param>
/// <param name="EquippedBeams">The acquired beam components currently enabled; retail UI excludes Spazer with Plasma.</param>
public sealed record SaveInventoryJsonDocument(
    SamusEquipmentFlags CollectedItems,
    SamusEquipmentFlags EquippedItems,
    SamusBeamFlags CollectedBeams,
    SamusBeamFlags EquippedBeams);
