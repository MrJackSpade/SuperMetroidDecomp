namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Map/equipment page fade and load phases owned by the pause menu.</summary>
internal enum PauseMenuTransition
{
    /// <summary>Dim the map before loading equipment graphics.</summary>
    MapToEquipmentFadeOut,
    /// <summary>Brighten the newly loaded equipment page.</summary>
    MapToEquipmentFadeIn,
    /// <summary>Dim equipment before restoring the map.</summary>
    EquipmentToMapFadeOut,
    /// <summary>Brighten the restored map page.</summary>
    EquipmentToMapFadeIn,
    /// <summary>Swap the map page for the equipment page while the screen is black.</summary>
    MapToEquipmentLoad,
    /// <summary>Restore the map page after the equipment page has faded out.</summary>
    EquipmentToMapLoad,
}
