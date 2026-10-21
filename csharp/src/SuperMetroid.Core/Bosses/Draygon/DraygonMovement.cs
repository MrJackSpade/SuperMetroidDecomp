using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
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

    /// <summary>Moves Draygon right or left by the current 16.16 velocity without losing its fraction.</summary>
    internal static void MoveDraygonHorizontally(DraygonEnemyState state, bool rightward)
    {
        RoomEnemySlot body = state.Body;
        uint position = body.Position.X.RawValue;
        uint velocity = state.SwoopVelocity;
        position = rightward ? unchecked(position + velocity) : unchecked(position - velocity);
        body.Position = body.Position.WithX(SnesFixedPosition.FromRaw(unchecked(position)));
    }

    /// <summary>Emits a breath bubble every 128 body frames when a sprite slot is available.</summary>
    internal void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if ((state.Body.FrameCounter & 127) != 0)
            return;
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(state.Body.Position.X.Whole - 16)), unchecked((ushort)(state.Body.Position.Y.Whole - 16))),
                RoomSpriteObjectKind.DraygonBreathBubble,
                graphicsIndex: 0) is not null)
        {
            state.BreathBubblesSpawned++;
        }
    }

    /// <summary>Adds or subtracts a sine-derived magnitude with native 16.16 wraparound.</summary>
    internal static SnesFixedPosition AddDraygonAngleMagnitude(
        SnesFixedPosition position,
        uint magnitude,
        bool subtract)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = subtract
            ? unchecked(fixedPosition - magnitude)
            : unchecked(fixedPosition + magnitude);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }
}
