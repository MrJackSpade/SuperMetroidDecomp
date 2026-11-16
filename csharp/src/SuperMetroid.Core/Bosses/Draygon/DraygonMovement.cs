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
    /// The distance one axis of an angled move covers: the unsigned sine sample <paramref name="phase"/> ahead of
    /// <paramref name="angle"/> times <paramref name="speed"/> whole pixels, a quarter turn ahead for the horizontal axis
    /// and a half turn for the vertical, whose direction Draygon's movement takes from the angle.
    /// </summary>
    /// <param name="angle">The movement angle.</param>
    /// <param name="speed">The speed in whole pixels an update, never negative.</param>
    /// <param name="phase">The turn the sample is read ahead of the angle.</param>
    internal static UnsignedSixteenSixteen AngleMagnitude(TrigonometryAngle angle, int speed, RotationAngle phase) =>
        EnemyMath.UnsignedSineSpeed(angle.TurnedBy(phase.ToTrigonometryAngle()), speed);

    /// <summary>
    /// Moves <paramref name="body"/> along <paramref name="angle"/> at <paramref name="speed"/> whole pixels an update,
    /// across first and then down: each axis moves back where the angle's cosine or sine is negative, and forward
    /// otherwise, keeping its fraction.
    /// </summary>
    internal static void MoveDraygonAtAngle(RoomEnemyPlace body, TrigonometryAngle angle, int speed)
    {
        UnsignedSixteenSixteen across = AngleMagnitude(angle, speed, RotationAngle.QuarterTurn);
        UnsignedSixteenSixteen down = AngleMagnitude(angle, speed, RotationAngle.HalfTurn);
        body.Position = body.Position.WithX(EnemyMath.CosineIsNegative(angle)
            ? body.Position.X.MovedBack(across)
            : body.Position.X.MovedForward(across));
        body.Position = body.Position.WithY(EnemyMath.SineIsNegative(angle)
            ? body.Position.Y.MovedBack(down)
            : body.Position.Y.MovedForward(down));
    }
}
