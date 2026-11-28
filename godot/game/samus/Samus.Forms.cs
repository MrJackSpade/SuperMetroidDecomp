using Engine.Actors;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>How Samus's body takes a taller form: by feeling for room above and below, as the cartridge does.</summary>
public partial class Samus
{
    /// <summary>
    /// The least half height of an upright body. With no room above or below, a body shorter than this, the ball's,
    /// stays as it is where an upright one is put in the crouch.
    /// </summary>
    private const int SmallestUprightHalfHeight = 8;

    /// <summary>
    /// How much a body must grow for the blocks to be felt for in two goes, and the size of a block: such a growth is
    /// first tried to the middle of the block step it ends in, and stops there when that is blocked.
    /// </summary>
    private const int TwoGoGrowth = 8, BlockSize = 16;

    /// <summary>
    /// A taller form is fitted as the cartridge resolves a larger pose. The body she has feels for the growth above
    /// and below: first for something solid, then, apart, for the room's blocks. With nothing either way she grows
    /// about her middle. Stopped one way, she is moved the other by what the growth lacks, if there is room for
    /// that; if not she stays as she is. Stopped both ways she is put in the crouch, lifted so its feet are where
    /// hers were when she was shorter than it, unless she is shorter than an upright body, when she stays as she is.
    /// The tries leave their mark as her moves do: what part of a pixel she is at up and down is what the last try
    /// stopped by a block, or lying against something solid, left. A form no taller than her body is placed as any
    /// player's is.
    /// </summary>
    protected override FormFit FitForm(string form, bool aroundMiddle)
    {
        int now = Body.Rect.Height / 2;
        int growth = HalfHeightOf(form) - now;
        if (growth <= 0)
            return base.FitForm(form, aroundMiddle);

        PixelPosition before = Position;
        MoveOptions blocks = BodyMoves with { StoppedBySolids = false };
        MoveOptions solids = BodyMoves with { StoppedByTerrain = false };
        var grown = new PixelRect(Body.Rect.Left, -HalfHeightOf(form), Body.Rect.Width, HalfHeightOf(form) * 2);

        // Something solid first, with the body she has; one found on one side only is then felt for on the other with
        // the body she would have, by what is left of the growth: that way even when nothing of the growth is left.
        MoveResult solidAbove = Feel(-growth, solids, shapedAs: null, marksAlways: true);
        MoveResult solidBelow = Feel(growth, solids, shapedAs: null, marksAlways: true);
        int clearOfSolids = 0;
        if (solidAbove.Blocked != solidBelow.Blocked)
        {
            int left = growth - Math.Abs((solidAbove.Blocked ? solidAbove : solidBelow).Moved.Whole);
            MoveResult other = Feel(solidAbove.Blocked ? left : -left, solids, grown, marksAlways: true, stillUp: solidBelow.Blocked);
            if (other.Blocked)
                return Leaves(null, lift: 0, stoodThere: false);
            clearOfSolids = other.Moved.Whole;
        }

        MoveResult above = FeelBlocks(-growth), below = FeelBlocks(growth);
        if (above.Blocked && below.Blocked)
            return BlockedBothWays();

        int shift;
        if (below.Blocked)
        {
            // Up, away from the floor, by the growth less the room she has below: her feet stay where they were.
            shift = -(growth - Math.Max(0, below.Moved.Whole));
            if (FeelBlocks(shift).Blocked)
                return Leaves(null, lift: 0, stoodThere: false);
            if (solidAbove.Blocked)
                return BlockedBothWays();
        }
        else if (above.Blocked)
        {
            // Down, away from the ceiling, only as far as her taller body needs.
            shift = growth - Math.Max(0, -above.Moved.Whole);
            if (FeelBlocks(shift).Blocked)
                return Leaves(null, lift: 0, stoodThere: false);
            if (solidBelow.Blocked)
                return BlockedBothWays();
        }
        else
        {
            if (solidAbove.Blocked && solidBelow.Blocked)
                return BlockedBothWays();
            shift = clearOfSolids;
        }

        // Where any try was stopped the view takes her to have stood where the change leaves her, even unmoved.
        return Leaves(form, shift, above.Blocked || below.Blocked || solidAbove.Blocked || solidBelow.Blocked);

        // With no room either way an upright body is put in the crouch, and the ball stays the ball.
        FormFit BlockedBothWays()
        {
            if (now < SmallestUprightHalfHeight)
                return Leaves(null, lift: 0, stoodThere: false);
            int crouch = HalfHeightOf(Crouching);
            return now < crouch ? Leaves(Crouching, -(crouch - now), stoodThere: true) : Leaves(Crouching, lift: 0, stoodThere: false);
        }

        // What the fitting comes to: she is put back where she was, and the place given is that place with whatever
        // part of a pixel the tries left her, moved by the whole pixels of the lift.
        FormFit Leaves(string? taken, int lift, bool stoodThere)
        {
            PixelPosition place = Position + PixelOffset.FromWhole(0, lift);
            Position = before;
            return new FormFit(taken, place, stoodThere);
        }

        // The blocks, felt for as her moves up and down feel for them, with the body she has. A growth of half a
        // block or more is felt in two goes.
        MoveResult FeelBlocks(int distance)
        {
            if (Math.Abs(distance) >= TwoGoGrowth)
            {
                int middle = distance - (((distance % BlockSize) + BlockSize) % BlockSize) + (BlockSize / 2);
                MoveResult first = Feel(middle, blocks, shapedAs: null, marksAlways: false);
                if (first.Blocked)
                    return first;
            }

            return Feel(distance, blocks, shapedAs: null, marksAlways: false);
        }

        // One try of a move up or down that is not made. It leaves her the part of a pixel the move would have left:
        // after a try against something solid always, after one against the blocks when they stopped it.
        MoveResult Feel(int distance, MoveOptions options, PixelRect? shapedAs, bool marksAlways, bool stillUp = false)
        {
            MoveResult tried = TryBodyDown(Pixels.FromWhole(distance), options, out PixelPosition rest, shapedAs, stillUp);
            if (marksAlways || tried.Blocked)
                Position = Position with { Y = Pixels.FromParts(Position.Y.Whole, rest.Y.Fraction) };
            return tried;
        }
    }
}
