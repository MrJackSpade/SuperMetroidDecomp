using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's entrance: the opening delay and its dance.</summary>
[RoomLifetime]
internal sealed class DraygonIntro
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonIntro(RoomSpriteObjectPool spriteObjects, DraygonTurrets turrets)
    {
        _spriteObjects = spriteObjects;
        _turrets = turrets;
    }

    /// <summary>Ports the opening 256-frame delay.</summary>
    internal void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (state.IntroFrames < 256)
        {
            if (state.IntroFrames == 0)
            {
                // Each spawn takes the next slot in processing order, so these become the first four slots in
                // processing order, the exact four the dance later moves.
                for (int actor = 0; actor < DraygonIntroDanceDefinitions.EvirCount; actor++)
                {
                    if (_spriteObjects.SpawnRoomSpriteObject(
                            new(16, 384),
                            RoomSpriteObjectKind.DraygonIntroEvir,
                            SpriteGraphics.SharedIn(state.Colours.Sprites)) is not null)
                    {
                        state.IntroEvirsSpawned++;
                    }
                }
            }
            state.IntroFrames++;
            return;
        }

        state.Function = DraygonAiFunction.IntroDance;
        state.IntroFrames = 0;
        state.LeftSideReset = state.Body.Position.Whole;
        state.RightSideReset = state.LeftSideReset + DraygonBehavior.DraygonRightSideResetDistance;
        state.Body.Position = state.Body.Position.WithWhole(state.LeftSideReset);
        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
    }

    /// <summary>Ports the 1,232-frame Evir dance.</summary>
    internal void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (state.IntroFrames >= DraygonIntroDanceDefinitions.DurationFrames)
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.IntroFrames = 0;
            return;
        }

        // The dance moves only the first four slots in processing order, each once it has waited out its start delay.
        foreach (RoomSpriteObjectSlot sprite in _spriteObjects.InProcessingOrder.Take(DraygonIntroDanceDefinitions.EvirCount))
        {
            int step = state.IntroDanceStep - DraygonIntroDance.StartDelayForSlot(sprite.SlotIndex);
            if (step < 0 || !sprite.IsActive)
                continue;

            DraygonIntroMovement movement = DraygonIntroDance.Movement(step);

            if (movement.DeletesSprite)
            {
                sprite.Clear();
                continue;
            }

            sprite.Position += movement.Delta;
        }

        state.IntroDanceStep++;
        state.IntroFrames++;
    }
}
