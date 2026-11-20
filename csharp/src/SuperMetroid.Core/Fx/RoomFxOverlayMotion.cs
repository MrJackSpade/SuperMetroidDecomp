namespace SuperMetroid.Core.Fx;

/// <summary>What a room effect shows on the effect overlay and how it moves there.</summary>
internal enum RoomFxOverlayMotion
{
    /// <summary>Nothing on the overlay.</summary>
    None,

    /// <summary>Lava or acid, which hazes, rises with quakes and draws on the shared random number as it moves.</summary>
    LavaOrAcid,

    /// <summary>Water, whose surface and background waves move.</summary>
    Water,

    /// <summary>Rain, which falls and drifts in its wind with the camera's movement.</summary>
    Rain,

    /// <summary>Spores, which rise anchored to the camera.</summary>
    Spores,

    /// <summary>Fog, anchored to the foreground, drifting right as it rises.</summary>
    Fog,
}
