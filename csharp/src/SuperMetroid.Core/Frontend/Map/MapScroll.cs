using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The area map's scroll on a map screen (#1975): where the screen frames the map when it opens, which arrows may scroll
/// it, and the eight-update step a held arrow runs. File select's room map and the pause screen's map page run this one
/// framing; how their screens differ is data (<see cref="MapFraming"/>).
/// </summary>
/// <remarks>
/// The scroll is the screen's top-left corner as a signed offset from the map's corner, which the screen subtracts from
/// what it draws: negative where the screen lies left of or above the map. The limits, the edges and the scroll stay
/// within a few hundred pixels of the 512-by-256 map (the framing starts there and a step is taken only while an arrow
/// allows it), so the offset never wraps and the arrows' tests compare plain differences.
/// </remarks>
public sealed class MapScroll
{
    /// <summary>Updates taken so far of the accepted eight-update step.</summary>
    private int tick;

    /// <summary>The scroll at <paramref name="scroll"/> on a screen framed by <paramref name="framing"/>.</summary>
    /// <param name="framing">How the screen frames the map.</param>
    /// <param name="limits">How far the map's revealed cells reach.</param>
    /// <param name="scroll">The screen's top-left corner as an offset from the map's corner.</param>
    public MapScroll(MapFraming framing, MapScrollLimits limits, Offset16<MapPixels> scroll)
    {
        Framing = Ensure.NotNull(framing);
        Limits = limits;
        Scroll = scroll;
    }

    /// <summary>How the screen frames the map.</summary>
    public MapFraming Framing { get; }

    /// <summary>How far the map's revealed cells reach.</summary>
    public MapScrollLimits Limits { get; }

    /// <summary>The screen's top-left corner as an offset from the map's corner, negative left of or above it.</summary>
    public Offset16<MapPixels> Scroll { get; set; }

    /// <summary>The direction of the accepted eight-update scroll step, or <see cref="MapScrollDirection.None"/>.</summary>
    public MapScrollDirection Direction { get; private set; }

    /// <summary>
    /// The scroll a screen opens on: the revealed reach centred on the screen, the centring offset rounded down to whole
    /// map rows, then moved so the player's marker lies inside the screen's marker bounds (<see cref="MapFramingDefinitions"/>).
    /// </summary>
    /// <param name="framing">How the screen frames the map.</param>
    /// <param name="limits">How far the map's revealed cells reach.</param>
    /// <param name="player">The player marker's map position.</param>
    public static MapScroll Centred(MapFraming framing, MapScrollLimits limits, Position16<MapPixels> player)
    {
        Ensure.NotNull(framing);
        int horizontal = limits.Left + (limits.Right - limits.Left) / 2 - MapFramingDefinitions.CentreColumn;
        int markerColumn = player.X - horizontal;
        if (markerColumn > MapFramingDefinitions.MarkerRightmostColumn)
            horizontal += markerColumn - MapFramingDefinitions.MarkerRightmostColumn;
        else if (markerColumn <= MapFramingDefinitions.MarkerLeftmostColumn)
            horizontal -= MapFramingDefinitions.MarkerLeftmostColumn - markerColumn;
        int middle = limits.Top + (limits.Bottom - limits.Top) / 2 + MapFramingDefinitions.RowMiddleAboveCentredLine;
        int offset = MapScreenConversions.RoundDownToMapTile(framing.CentreLine - middle);
        int vertical = -offset;
        int markerLine = player.Y + offset;
        if (markerLine <= MapFramingDefinitions.MarkerTopLine)
        {
            vertical -= MapFramingDefinitions.MarkerTopLine - markerLine;
            vertical = Math.Max(vertical, -MapFramingDefinitions.LowestMapTop);
        }
        return new(framing, limits, new Offset16<MapPixels>(horizontal, vertical));
    }

    /// <summary>Whether another step in <paramref name="direction"/> fits the screen's edges, and its arrow shows.</summary>
    /// <param name="direction">The direction tested.</param>
    public bool CanScroll(MapScrollDirection direction)
    {
        return direction switch
        {
            MapScrollDirection.Left => Limits.Left - Scroll.X < Framing.LeftEdge,
            MapScrollDirection.Right => Limits.Right - Scroll.X >= Framing.RightEdge,
            MapScrollDirection.Up => Limits.Top - Scroll.Y < Framing.TopEdge,
            MapScrollDirection.Down => Limits.Bottom - Scroll.Y >= Framing.BottomEdge,
            MapScrollDirection.None => false,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined map scroll direction."),
        };
    }

    /// <summary>
    /// Runs one update of held arrows: with no step under way the first held direction that may scroll, in
    /// <see cref="MapScrollControls.Directions"/> order, starts one. A step moves the map on its fourth update and finishes
    /// on its eighth even when its button has been released. Returns true on the update a step finishes, when the scroll
    /// sound plays.
    /// </summary>
    /// <param name="heldInput">The controller buttons held.</param>
    public bool Step(ControllerButton heldInput)
    {
        if (Direction == MapScrollDirection.None)
            Direction = MapScrollControls.Directions.FirstOrDefault(direction =>
                CanScroll(direction) && (heldInput & MapScrollControls.ButtonFor(direction)) != ControllerButton.None,
                MapScrollDirection.None);
        // The game cancels a step under way only at the lower limit.
        if (Direction == MapScrollDirection.Down && !CanScroll(Direction))
        {
            Direction = MapScrollDirection.None;
            tick = 0;
        }
        if (Direction == MapScrollDirection.None) return false;
        if (++tick == MapScrollDefinitions.PulseTick)
            Scroll += MapScrollControls.StepOffset(Direction);
        if (tick != MapScrollDefinitions.StepTicks) return false;
        tick = 0;
        Direction = MapScrollDirection.None;
        return true;
    }
}
