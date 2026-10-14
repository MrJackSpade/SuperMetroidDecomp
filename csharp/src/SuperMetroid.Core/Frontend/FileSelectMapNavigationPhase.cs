namespace SuperMetroid.Core.Frontend;

/// <summary>Exclusive navigation phases; these names are not cartridge dispatcher addresses.</summary>
public enum FileSelectMapNavigationPhase
{
    /// <summary>Accepts area-map confirm or cancel edges.</summary>
    Area,
    /// <summary>Creates the selected area's room-map window.</summary>
    PreparingWindow,
    /// <summary>Advances the window-opening animation.</summary>
    ExpandingWindow,
    /// <summary>Installs the room-map contents after the frame reaches its endpoint.</summary>
    InitializingRoom,
    /// <summary>Accepts room-map load or return edges.</summary>
    Room,
    /// <summary>Waits for the frontend to finish returning to the area map.</summary>
    AreaReturnRequested,
    /// <summary>Requests a transition back to the file-options menu.</summary>
    OptionsRequested,
    /// <summary>Requests loading the selected save file.</summary>
    LoadRequested,
    /// <summary>Represents the frontend-owned transition into an area map.</summary>
    EnteringArea,
}
