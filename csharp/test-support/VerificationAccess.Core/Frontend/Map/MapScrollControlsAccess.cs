using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Verification access to <see cref="MapScrollControls"/> members production does not use.</summary>
internal static class MapScrollControlsAccess
{
    /// <summary>Lists the configured scroll buttons for every nonzero map direction.</summary>
    extension(MapScrollControls)
    {
        /// <summary>Controller buttons corresponding to all defined map scroll directions.</summary>
        internal static SnesButton[] Buttons =>
            [.. Enumerable.Range(1, MapScrollControls.DirectionCount).Select(direction => MapScrollControls.ButtonFor((MapScrollDirection)direction))];
    }
}
