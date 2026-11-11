using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
[RoomLifetime]
internal sealed class DraygonMovement
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonMovement(RoomSpriteObjects spriteObjects)
    {
        _spriteObjects = spriteObjects;
    }

    /// <summary>Moves Draygon right or left by the current swoop speed without losing its fraction.</summary>
    internal static void MoveDraygonHorizontally(DraygonEnemyState state, bool rightward)
    {
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithX(rightward
            ? body.Position.X.MovedForward(state.SwoopVelocity)
            : body.Position.X.MovedBack(state.SwoopVelocity));
    }

    /// <summary>Emits a breath bubble every 128 body frames.</summary>
    internal void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if (!DraygonCadenceDefinitions.BreathBubbleEvery128thUpdate.IsDue(state.Body.FrameCounter))
            return;
        _spriteObjects.SpawnRoomSpriteObject(
            new(CoordinateAxis.Wrap(state.Body.Position.X.Whole - 16), CoordinateAxis.Wrap(state.Body.Position.Y.Whole - 16)),
            RoomSpriteObjectKind.DraygonBreathBubble,
            graphics: default);
    }

    /// <summary>
    /// The distance one axis of an angled move covers: the unsigned sine sample <paramref name="phase"/> ahead of
    /// <paramref name="angle"/> times <paramref name="speed"/> whole pixels, a quarter turn ahead for the horizontal axis
    /// and a half turn for the vertical, whose direction the caller takes from the angle.
    /// </summary>
    /// <param name="angle">The movement angle.</param>
    /// <param name="speed">The speed in whole pixels an update, never negative.</param>
    /// <param name="phase">The turn the sample is read ahead of the angle.</param>
    internal static UnsignedSixteenSixteen AngleMagnitude(TrigonometryAngle angle, int speed, RotationAngle phase) =>
        EnemyMath.UnsignedSineSpeed(angle.TurnedBy(phase.ToTrigonometryAngle()), speed);

    /// <summary>Moves one axis of the body back or forward by a sine-derived distance.</summary>
    internal static FixedCoordinate AddDraygonAngleMagnitude(
        FixedCoordinate position,
        UnsignedSixteenSixteen magnitude,
        bool subtract) =>
        subtract ? position.MovedBack(magnitude) : position.MovedForward(magnitude);
}
