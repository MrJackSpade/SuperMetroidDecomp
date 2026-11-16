using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's swoops across the room and its climb straight up.</summary>
[RoomLifetime]
internal sealed class DraygonSwoops
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;
    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    private readonly DraygonMovement _movement;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonSwoops(EnemyRoomBinding room, DraygonParts parts, DraygonTurrets turrets, DraygonMovement movement)
    {
        _room = room;
        _parts = parts;
        _turrets = turrets;
        _movement = movement;
    }

    /// <summary>Starts a swoop to the right: plans its path of heights and aims it at Samus.</summary>
    internal void SetupDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        BuildDraygonSwoopYPositions(state, samus);
        state.Phase = DraygonPhase.SwoopDescending;
        _parts.StartDraygonAnimation(state.Body, DraygonSegment.BodyFacingRightReset);
        state.FacingRight = true;
    }

    /// <summary>Captures Samus's height and computes the vertical waypoints for the next swoop.</summary>
    internal static void BuildDraygonSwoopYPositions(DraygonEnemyState state, SamusState samus)
    {
        // The climb ends only because its rising speed grows; every producer of the acceleration keeps it above zero.
        if (state.SwoopAcceleration.Y <= SignedEightEight.Zero)
            throw new InvalidOperationException("Draygon's swoop path climbs only under a swoop acceleration above zero.");
        int yPosition = 384;
        EightEightVector<RoomPixels> speed = default;
        var heights = ImmutableArray.CreateBuilder<int>();

        while (true)
        {
            speed += state.SwoopAcceleration;
            // The path ends once it rises past the reset height, at most 384 pixels up, so the rising speed stays a few
            // pixels an update.
            int candidate = CoordinateAxis.Wrap(yPosition - speed.Y.WholePixels);
            if (CoordinateAxis.Difference(candidate, state.LeftSideReset.Y) < 0)
                break;

            yPosition = candidate;
            heights.Add(candidate);
        }

        if (heights.Count == 0)
            throw new InvalidDataException("Draygon generated an empty swoop path.");
        int frameCount = heights.Count;
        state.SwoopPath = new DraygonSwoopPath(heights.ToImmutable(), state.Body.Position.Y.Whole);

        // The swoop covers the whole-pixel distance to Samus in as many updates as the path has entries.
        int distance = CoordinateAxis.Distance(
            state.LeftSideReset.X, samus.Position.X.Whole);
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.SwoopEntry = frameCount;
    }

    /// <summary>Runs the descending half of the swoop, right and left.</summary>
    internal void DescendDraygonSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter,
        DraygonBehavior.DraygonSwoopSide side)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemyPlace body = state.Body;
        int entry = state.SwoopEntry;
        if (entry == DraygonBehavior.DraygonArmsReachUpdatesFromSwoopBottom)
            _parts.StartDraygonAnimation(state.Arms, side.ArmsNearApex);

        body.Position = body.Position.WithWholeY(PathOf(state).HeightAt(entry));
        state.SwoopEntry = entry - 1;
        if (state.SwoopEntry == 0)
        {
            state.Phase = DraygonPhase.SwoopApex;
            return;
        }
        DraygonMovement.MoveDraygonHorizontally(state, side.Direction == HorizontalDirection.Right);
    }

    /// <summary>Runs the velocity recalculation at the bottom of the swoop.</summary>
    internal void CrossDraygonRightSwoopApex(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        int frameCount = PathOf(state).EntryCount;
        int distance = CoordinateAxis.Wrap(672 - body.Position.X.Whole);
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Phase = DraygonPhase.SwoopAscending;
    }

    /// <summary>
    /// Runs the ascending half and random next-attack choice, right and left. Only the leftward
    /// ascent emits breath bubbles. At the top the rightward swoop chooses the left swoop or goop; the leftward one
    /// returns to the left reset point first and chooses the right swoop, at the reset height, or goop.
    /// </summary>
    internal void AscendDraygonSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter,
        DraygonBehavior.DraygonSwoopSide side)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (side.Direction != HorizontalDirection.Right)
            _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemyPlace body = state.Body;
        int entry = state.SwoopEntry;
        if (entry == DraygonBehavior.DraygonArmsReachUpdatesFromSwoopBottom)
            _parts.StartDraygonAnimation(state.Arms, side.ArmsIdle);

        DraygonSwoopPath path = PathOf(state);
        body.Position = body.Position.WithWholeY(path.HeightAt(entry));
        state.SwoopEntry = entry + 1;
        if (state.SwoopEntry != path.EntryCount)
        {
            DraygonMovement.MoveDraygonHorizontally(state, side.Direction == HorizontalDirection.Right);
            return;
        }

        if (side.Direction == HorizontalDirection.Right)
        {
            state.SwoopEntry = 0;
            int random = _room.Random.Read();
            state.Phase = DraygonRandomDefinitions.SwoopRatherThanGoop.IsSetIn(random)
                ? DraygonPhase.SwoopSetup
                : DraygonPhase.GoopSetup;
            state.PassDirection = HorizontalDirection.Left;
            return;
        }

        int choice = _room.Random.Read();
        body.Position = body.Position.WithWholeX(state.LeftSideReset.X);
        if (DraygonRandomDefinitions.SwoopRatherThanGoop.IsSetIn(choice))
        {
            state.SwoopEntry = 0;
            body.Position = body.Position.WithWholeY(state.LeftSideReset.Y);
            state.Phase = DraygonPhase.SwoopSetup;
        }
        else
        {
            state.Phase = DraygonPhase.GoopSetup;
        }
        state.PassDirection = HorizontalDirection.Right;
    }

    /// <summary>Runs the mirrored left-swoop setup.</summary>
    internal void SetupDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        int frameCount = PathOf(state).EntryCount;

        int distance = CoordinateAxis.Distance(
            state.RightSideReset.X, samus.Position.X.Whole);
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Phase = DraygonPhase.SwoopDescending;
        _parts.StartDraygonAnimation(state.Body, DraygonSegment.BodyFacingLeftReset);
        state.FacingRight = false;
        state.SwoopEntry = frameCount;
    }

    /// <summary>Aims the leftward climb at the left-side reset point, the wrapped distance back from Draygon's X.</summary>
    internal void CrossDraygonLeftSwoopApex(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        int frameCount = PathOf(state).EntryCount;
        int distance = CoordinateAxis.Wrap(body.Position.X.Whole - state.LeftSideReset.X);
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Phase = DraygonPhase.SwoopAscending;
    }

    /// <summary>The path of the swoop Draygon is flying, which its setup planned.</summary>
    /// <exception cref="InvalidOperationException">Draygon has not planned a swoop.</exception>
    internal static DraygonSwoopPath PathOf(DraygonEnemyState state) =>
        state.SwoopPath ?? throw new InvalidOperationException("Draygon flies a swoop he has not planned.");

    /// <summary>Sets the swoop's horizontal speed: the speed that covers <paramref name="distance"/> in one update per path entry.</summary>
    /// <param name="state">Draygon's state, which receives the velocity.</param>
    /// <param name="distance">The whole pixels the swoop covers.</param>
    /// <param name="frameCount">The path's entries.</param>
    private static void SetDraygonSwoopVelocity(DraygonEnemyState state, int distance, int frameCount) =>
        state.SwoopVelocity = UnsignedSixteenSixteen.Covering(distance, frameCount);

    /// <summary>Runs the four-pixel upward retreat.</summary>
    internal void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 4);
        if (CoordinateAxis.AsSigned(body.Position.Y.Whole) >= 0)
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = DraygonPhase.SwoopSetup;
        state.PassDirection = HorizontalDirection.Right;
        state.SwoopEntry = 0;
        body.Position = body.Position.WithWhole(state.LeftSideReset);
    }
}
