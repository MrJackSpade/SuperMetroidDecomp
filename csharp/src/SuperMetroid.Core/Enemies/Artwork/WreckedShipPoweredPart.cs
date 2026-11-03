namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The parts of the Wrecked Ship the power coming on after Phantoon's death recolours (#1512), each painted toward its
/// own powered colours: the shared gold and machinery, the three door colours, the foreground, the background and the
/// neutral lights. Phantoon's own body keeps its colours.
/// </summary>
public enum WreckedShipPoweredPart
{
    /// <summary>The shared gold, machinery, indicator and neutral colours.</summary>
    Shared,

    /// <summary>The green doors.</summary>
    GreenDoors,

    /// <summary>The magenta doors.</summary>
    MagentaDoors,

    /// <summary>The blue doors.</summary>
    BlueDoors,

    /// <summary>The ship's foreground.</summary>
    Foreground,

    /// <summary>The ship's background.</summary>
    Background,

    /// <summary>The neutral lights.</summary>
    Lights,
}
