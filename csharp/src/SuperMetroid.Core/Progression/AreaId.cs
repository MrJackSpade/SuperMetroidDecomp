namespace SuperMetroid.Core.Progression;

/// <summary>The seven areas of the game world: the six regions of Zebes and the Ceres research station.</summary>
/// <remarks>
/// An area names a region; it is not a number and not a flags field. <see cref="AreaIds.All"/> lists the areas in the
/// game's order; imported data and save files number the areas, and that numbering is their codecs' own.
/// </remarks>
public enum AreaId
{
    /// <summary>The planetary surface and adjoining caverns, where Samus lands.</summary>
    Crateria,

    /// <summary>The upper and lower Brinstar region.</summary>
    Brinstar,

    /// <summary>The upper and lower Norfair region.</summary>
    Norfair,

    /// <summary>The derelict spacecraft region.</summary>
    WreckedShip,

    /// <summary>The flooded Maridia region.</summary>
    Maridia,

    /// <summary>The final Tourian region.</summary>
    Tourian,

    /// <summary>The Ceres research station prologue.</summary>
    Ceres,
}
