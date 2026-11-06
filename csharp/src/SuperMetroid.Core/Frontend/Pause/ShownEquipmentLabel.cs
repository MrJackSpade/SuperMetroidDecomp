using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>How the equipment page shows one ordinary item label (#1467, #1515): in the state its item is in now.</summary>
/// <param name="Label">The label.</param>
/// <param name="State">Whether the item is uncollected, collected and equipped, or collected and not equipped.</param>
internal readonly record struct ShownEquipmentLabel(PauseEquipmentLabelId Label, PauseEquipmentLabelState State);
