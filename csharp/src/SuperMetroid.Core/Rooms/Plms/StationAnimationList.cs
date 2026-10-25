namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The seven station animations. The values are arbitrary and nothing reads them; each station's layout names the
/// animations it shows (#1414).
/// </summary>
internal enum StationAnimationList
{
    /// <summary>The map station's idle cycle.</summary>
    MapIdle = 1,
    /// <summary>The map station's cycle once its area map is held.</summary>
    MapAcquired,
    /// <summary>The energy station's idle and recharged cycle.</summary>
    Energy,
    /// <summary>The missile station's idle and recharged cycle.</summary>
    Missile,
    /// <summary>The save station's idle draw, shown once before its trigger sleeps.</summary>
    SaveIdle,
    /// <summary>The save station's first active frame.</summary>
    SaveFirstFrame,
    /// <summary>The save station's second active frame.</summary>
    SaveSecondFrame,
}
