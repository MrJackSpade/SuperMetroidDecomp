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
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;
    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    private readonly DraygonMovement _movement;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonSwoops(EnemyRoomBinding room, DraygonTurrets turrets, DraygonMovement movement)
    {
        _room = room;
        _turrets = turrets;
        _movement = movement;
    }

    /// <summary>Starts a swoop to the right: plans its path of heights and aims it at Samus.</summary>
    internal void SetupDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        BuildDraygonSwoopYPositions(state, samus);
        state.Phase = DraygonPhase.SwoopDescending;
        state.BodyPart.StartAnimation(DraygonSegment.BodyFacingRightReset);
        state.Facing = HorizontalDirection.Right;
    }

    /// <summary>Captures Samus's height and computes the vertical waypoints for the next swoop.</summary>
    internal static void BuildDraygonSwoopYPositions(DraygonEnemyState state, SamusState samus)
    {
        // The climb ends only because its rising speed grows; every producer of the acceleration keeps it above zero.
        if (state.SwoopAcceleration <= SignedEightEight.Zero)
            throw new InvalidOperationException("Draygon's swoop path climbs only under a swoop acceleration above zero.");
        int yPosition = DraygonArenaDefinitions.SwimLineY;
        SignedEightEight speed = SignedEightEight.Zero;
        var heights = ImmutableArray.CreateBuilder<int>();

        while (true)
        {
            speed += state.SwoopAcceleration;
            // The path ends once it rises past the reset height, no higher than the swim line lies, so the rising speed stays a few
            // pixels an update.
            int candidate = OffRoomPlaces.Place(yPosition - speed.WholePixels);
            if (DistanceAcrossRoomEdge.LiesBefore(candidate, state.LeftSideReset.Y))
                break;

            yPosition = candidate;
            heights.Add(candidate);
        }

        if (heights.Count == 0)
            throw new InvalidDataException("Draygon generated an empty swoop path.");
        int frameCount = heights.Count;
        state.SwoopPath = new DraygonSwoopPath(heights.ToImmutable(), state.Body.Position.Y.Whole);

        // The swoop covers the whole-pixel distance to Samus in as many updates as the path has entries.
        int distance = DistanceAcrossRoomEdge.Between(
            state.LeftSideReset.X, samus.Position.X.Whole);
        SetDraygonSwoopVelocity(state, distance, frameCount, HorizontalDirection.Right);
        state.SwoopHalf = UpdateCountdown.Lasting(frameCount);
    }

    /// <summary>Runs the descending half of the swoop, right and left.</summary>
    internal void DescendDraygonSwoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter,
        DraygonBehavior.DraygonSwoopSide side)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemyPlace body = state.Body;
        int entry = state.SwoopHalf.UpdatesLeft;
        if (entry == DraygonSwoopDefinitions.ArmsReachUpdatesFromBottom)
            state.ArmsPart.StartAnimation(side.ArmsNearApex);

        body.Position = body.Position.WithWholeY(PathOf(state).HeightAt(entry));
        if (state.SwoopHalf.Count())
        {
            state.Phase = DraygonPhase.SwoopApex;
            return;
        }
        DraygonMovement.MoveDraygonHorizontally(state);
    }

    /// <summary>Runs the velocity recalculation at the bottom of the swoop.</summary>
    internal void CrossDraygonRightSwoopApex(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        int frameCount = PathOf(state).EntryCount;
        // The rightward swoop's bottom lies over Samus, inside the arena, so the distance on to the right-side place is a plain one.
        int distance = DraygonArenaDefinitions.RightSideResetDistance - body.Position.X.Whole;
        SetDraygonSwoopVelocity(state, distance, frameCount, HorizontalDirection.Right);
        state.SwoopHalf = UpdateCountdown.Lasting(frameCount);
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
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        if (side.Direction != HorizontalDirection.Right)
            _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemyPlace body = state.Body;
        int entry = state.SwoopHalf.Age;
        if (entry == DraygonSwoopDefinitions.ArmsReachUpdatesFromBottom)
            state.ArmsPart.StartAnimation(side.ArmsIdle);

        body.Position = body.Position.WithWholeY(PathOf(state).HeightAt(entry));
        if (!state.SwoopHalf.Count())
        {
            DraygonMovement.MoveDraygonHorizontally(state);
            return;
        }

        if (side.Direction == HorizontalDirection.Right)
        {
            int random = _room.Random.Read();
            state.Phase = DraygonRandomDefinitions.SwoopsRatherThanSpitsGoop.IsTakenBy(random)
                ? DraygonPhase.SwoopSetup
                : DraygonPhase.GoopSetup;
            state.PassDirection = HorizontalDirection.Left;
            return;
        }

        int choice = _room.Random.Read();
        body.Position = body.Position.WithWholeX(state.LeftSideReset.X);
        if (DraygonRandomDefinitions.SwoopsRatherThanSpitsGoop.IsTakenBy(choice))
        {
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
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        int frameCount = PathOf(state).EntryCount;

        int distance = DistanceAcrossRoomEdge.Between(
            state.RightSideResetX, samus.Position.X.Whole);
        SetDraygonSwoopVelocity(state, distance, frameCount, HorizontalDirection.Left);
        state.Phase = DraygonPhase.SwoopDescending;
        state.BodyPart.StartAnimation(DraygonSegment.BodyFacingLeftReset);
        state.Facing = HorizontalDirection.Left;
        state.SwoopHalf = UpdateCountdown.Lasting(frameCount);
    }

    /// <summary>Aims the leftward climb at the left-side reset point, measured back from Draygon's X to the place left of the arena (<see cref="DistanceAcrossRoomEdge.Apart"/>).</summary>
    internal void CrossDraygonLeftSwoopApex(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        int frameCount = PathOf(state).EntryCount;
        int distance = DistanceAcrossRoomEdge.Apart(body.Position.X.Whole, state.LeftSideReset.X);
        SetDraygonSwoopVelocity(state, distance, frameCount, HorizontalDirection.Left);
        state.SwoopHalf = UpdateCountdown.Lasting(frameCount);
        state.Phase = DraygonPhase.SwoopAscending;
    }

    /// <summary>The path of the swoop Draygon is flying, which its setup planned.</summary>
    /// <exception cref="InvalidOperationException">Draygon has not planned a swoop.</exception>
    internal static DraygonSwoopPath PathOf(DraygonEnemyState state) =>
        state.SwoopPath ?? throw new InvalidOperationException("Draygon flies a swoop he has not planned.");

    /// <summary>
    /// Sets the swoop's horizontal velocity: toward <paramref name="direction"/> at the speed that covers
    /// <paramref name="distance"/> in one update per path entry.
    /// </summary>
    /// <param name="state">Draygon's state, which receives the velocity.</param>
    /// <param name="distance">The whole pixels the swoop covers.</param>
    /// <param name="frameCount">The path's entries.</param>
    /// <param name="direction">The side the swoop flies toward.</param>
    private static void SetDraygonSwoopVelocity(DraygonEnemyState state, int distance, int frameCount, HorizontalDirection direction) =>
        state.SwoopVelocity = SignedSixteenSixteen.Covering(distance, frameCount).Toward(direction);

    /// <summary>Runs the upward retreat at <see cref="DraygonArenaDefinitions.SwoopClimbOutSpeed"/>.</summary>
    internal void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        RoomEnemyPlace body = state.Body;
        body.Position = OffRoomPlaces.WithWholeY(body.Position, body.Position.Y.Whole - DraygonArenaDefinitions.SwoopClimbOutSpeed);
        if (!OffRoomPlaces.LiesBeforeRoom(body.Position.Y.Whole))
            return;

        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = DraygonPhase.SwoopSetup;
        state.PassDirection = HorizontalDirection.Right;
        body.Position = body.Position.WithWhole(state.LeftSideReset);
    }
}
