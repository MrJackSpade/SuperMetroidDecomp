namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Which of the pause frame's button labels are lit (#1467), as <c>SetPauseScreenButtonLabelPalettes</c> ($82:A628-$A84C)
/// sets them: on the map page, while unpausing, or on the equipment page.
/// </summary>
internal enum PauseButtonHighlight
{
    /// <summary>The map page: the Samus and Exit labels lit, the Map label dim.</summary>
    MapPage,
    /// <summary>Unpausing: the Exit label lit, the Map and Samus labels dim.</summary>
    Unpausing,
    /// <summary>The equipment page: the Map and Exit labels lit, the Samus label dim.</summary>
    EquipmentPage,
}
