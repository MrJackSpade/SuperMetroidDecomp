using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of Samus's super missiles. Its settings fly it and try its middle against the terrain where each move ends.
/// What is its own is a second point. A super missile gains speed on every update, until one move carries its middle
/// over a whole block; so once it moves <see cref="FastFrom"/> whole pixels or more along an axis, it also tries,
/// after its middle, the point <see cref="Lead"/> pixels on from where that move began (the cartridge's unseen link,
/// SuperMissileDefinitions.FastLinkSpeed and FastLinkShortfall). A block there is struck as its middle strikes one,
/// on the move that ended the missile too. Where the block stops a missile and the missile itself flies on, it bursts
/// there, and from then on has no second point.
/// </summary>
/// <remarks>
/// The point is a place of its own, as the link is: each move sets it along the way moved and leaves it where it was
/// the other way, so that of a missile flying aslant it trails by a move the other way. Slower than
/// <see cref="FastFrom"/> along an axis, it only keeps to the missile's middle along that axis and tries nothing.
/// </remarks>
public partial class SuperMissile : Projectile
{
    /// <summary>The whole pixels of speed along an axis from which the second point is tried.</summary>
    private const int FastFrom = 11;

    /// <summary>How many pixels on from where a move began the second point is tried.</summary>
    private const int Lead = 10;

    /// <summary>Where the second point stands, on whole pixels; null until the missile's first move places it.</summary>
    private (int X, int Y)? _point;

    /// <summary>Whether the second point is gone: it burst, or the missile ended.</summary>
    private bool _pointGone;

    /// <inheritdoc/>
    protected override void OnMovedToMeetTerrain(Room room, bool across, Pixels distance)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (_pointGone)
            return;

        // The point starts where the missile was launched; the way moved is then set below.
        (int X, int Y) point = _point ?? (Position.X.Whole, Position.Y.Whole);
        int speed = distance.Abs().Whole;
        int own = across ? Position.X.Whole : Position.Y.Whole;
        if (speed < FastFrom)
        {
            // Slow, the point keeps to the missile's middle along the way moved, and goes with a missile that ended.
            if (HasEnded)
                _pointGone = true;
            else
                _point = across ? (own, point.Y) : (point.X, own);
            return;
        }

        // Fast, it stands as far behind the missile as the missile's speed is over the lead: the lead on from where
        // the move began, a missile that moves by its speed. It is tried there even on the move that ended the missile.
        int behind = own - (distance.IsNegative ? -1 : 1) * (speed - Lead);
        point = across ? (behind, point.Y) : (point.X, behind);
        _point = point;
        PixelPosition at = PixelPosition.FromWhole(point.X, point.Y);
        bool stopped = StrikeTerrainAt(room, at);
        if (HasEnded)
        {
            _pointGone = true;
            return;
        }

        if (!stopped)
            return;

        // Stopped where the missile was not, the point bursts as a missile does and the missile flies on without it.
        BurstAgainstTerrainAt(room, at);
        (Launcher as Samus)?.OnShotEnded(this);
        _pointGone = true;
    }

    /// <inheritdoc/>
    /// <remarks>A super missile turned another way has its second point anew, from where it was turned.</remarks>
    protected override void OnTurned(Player player, int across, int down)
    {
        base.OnTurned(player, across, down);
        (_point, _pointGone) = (null, false);
    }
}
