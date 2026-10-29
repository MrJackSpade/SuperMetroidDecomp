namespace SuperMetroid.Core.Hud;

/// <summary>The shade the HUD minimap shows a revealed map cell in (#1466).</summary>
public enum MinimapShade
{
    /// <summary>Revealed, by the area's map or a reveal mode, but not yet explored.</summary>
    Unexplored,

    /// <summary>Explored by Samus.</summary>
    Explored,

    /// <summary>The cell Samus is in, on the half of each sixteen-frame cycle the marker shows.</summary>
    SamusLocation,
}
