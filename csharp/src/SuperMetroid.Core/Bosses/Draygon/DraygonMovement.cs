using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
[RoomLifetime]
internal sealed class DraygonMovement
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonMovement(RoomSpriteObjectPool spriteObjects)
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

    /// <summary>Emits a breath bubble every 128 body frames when a sprite slot is available.</summary>
    internal void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if ((state.Body.FrameCounter & 127) != 0)
            return;
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(CoordinateAxis.Wrap(state.Body.Position.X.Whole - 16), CoordinateAxis.Wrap(state.Body.Position.Y.Whole - 16)),
                RoomSpriteObjectKind.DraygonBreathBubble,
                graphics: default) is not null)
        {
            state.BreathBubblesSpawned++;
        }
    }

    /// <summary>
    /// The distance one axis of an angled move covers: the unsigned sine sample <paramref name="phase"/> ahead of
    /// <paramref name="angle"/> times <paramref name="speed"/> whole pixels, a quarter turn ahead for the horizontal axis
    /// and a half turn for the vertical, whose direction the caller takes from the angle.
    /// </summary>
    /// <param name="angle">The movement angle.</param>
    /// <param name="speed">The speed, 0 through 65535 pixels an update.</param>
    /// <param name="phase">The turn the sample is read ahead of the angle.</param>
    internal static UnsignedSixteenSixteen AngleMagnitude(TrigonometryAngle angle, int speed, RotationAngle phase) =>
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            angle.TurnedBy(phase.ToTrigonometryAngle()),
            Ensure.BetweenInclusive(0, ushort.MaxValue, speed),
            angleOffset: 0)));

    /// <summary>Moves one axis of the body back or forward by a sine-derived distance.</summary>
    internal static FixedCoordinate AddDraygonAngleMagnitude(
        FixedCoordinate position,
        UnsignedSixteenSixteen magnitude,
        bool subtract) =>
        subtract ? position.MovedBack(magnitude) : position.MovedForward(magnitude);
}
