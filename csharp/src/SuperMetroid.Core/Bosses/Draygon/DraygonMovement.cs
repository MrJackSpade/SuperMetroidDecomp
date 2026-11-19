using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
[RoomLifetime]
internal sealed class DraygonMovement
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonMovement(RoomEffectActors effectActors)
    {
        _effectActors = effectActors;
    }

    /// <summary>Moves Draygon toward <paramref name="direction"/> by the current swoop speed without losing its fraction.</summary>
    internal static void MoveDraygonHorizontally(DraygonEnemyState state, HorizontalDirection direction)
    {
        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithX(direction == HorizontalDirection.Right
            ? body.Position.X.MovedForward(state.SwoopVelocity)
            : body.Position.X.MovedBack(state.SwoopVelocity));
    }

    /// <summary>Emits a breath bubble every 128 body frames.</summary>
    internal void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if (!DraygonCadenceDefinitions.BreathBubbleEvery128thUpdate.IsDue(state.Body.FrameCounter))
            return;
        _effectActors.SpawnRoomEffectActor(
            state.Body.Position.Whole + DraygonArenaDefinitions.BreathBubbleOffset,
            RoomEffectActorKind.DraygonBreathBubbles,
            graphics: default);
    }

    /// <summary>
    /// Moves <paramref name="body"/> along <paramref name="angle"/> at <paramref name="speed"/> whole pixels an update
    /// (<see cref="EnemyMath.VelocityAlong"/>), keeping each axis's fraction.
    /// </summary>
    internal static void MoveDraygonAtAngle(RoomEnemyPlace body, TrigonometryAngle angle, int speed) =>
        body.Position = body.Position.Advance(EnemyMath.VelocityAlong(angle, speed));
}
