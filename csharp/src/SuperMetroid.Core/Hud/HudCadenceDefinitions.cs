namespace SuperMetroid.Core.Hud;

/// <summary>The counter phase on which the minimap blinks Samus's square (#1606).</summary>
internal static class HudCadenceDefinitions
{
    /// <summary>
    /// The phase of the frame counter on which Samus's square on the minimap keeps its explored shade: set for eight
    /// updates of every sixteen; for the other eight it shows the Samus-location shade, so her square blinks.
    /// </summary>
    internal static readonly UpdateCounterField SamusSquareShowsExploredShade = new(3, 1);
}
