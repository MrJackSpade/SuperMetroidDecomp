using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
[RoomLifetime]
internal sealed partial class DraygonMovement
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonMovement(RoomEffectActors effectActors)
    {
        _effectActors = effectActors;
    }

    /// <summary>Moves Draygon's X by the pass's velocity without losing its fraction.</summary>
    internal static void MoveDraygonHorizontally(DraygonEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithX(body.Position.X.MovedBy(state.SwoopVelocity));
    }

    /// <summary>Emits a breath bubble at its body-frame cadence.</summary>
    internal void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if (!DraygonCadenceDefinitions.BreathBubble.IsDue(state.Body.FrameCounter))
            return;
        _effectActors.SpawnRoomEffectActor(
            state.Body.Position.Whole + DraygonArenaDefinitions.BreathBubbleOffset,
            RoomEffectActorKind.DraygonBreathBubbles,
            graphics: SpriteGraphics.SharedStandard);
    }

    /// <summary>
    /// Moves <paramref name="body"/> along <paramref name="angle"/> at <paramref name="speed"/> whole pixels an update
    /// (<see cref="EnemyMath.VelocityAlong"/>), keeping each axis's fraction.
    /// </summary>
    internal static void MoveDraygonAtAngle(RoomEnemyPlace body, TrigonometryAngle angle, int speed) =>
        body.Position = body.Position.Advance(EnemyMath.VelocityAlong(angle, speed));
}
