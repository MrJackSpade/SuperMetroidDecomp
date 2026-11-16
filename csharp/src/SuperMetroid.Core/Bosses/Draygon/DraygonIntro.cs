using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's entrance: the opening delay and its dance.</summary>
[RoomLifetime]
internal sealed class DraygonIntro
{
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonIntro(RoomEffectActors effectActors, DraygonTurrets turrets)
    {
        _effectActors = effectActors;
        _turrets = turrets;
    }

    /// <summary>Runs the opening 256-frame delay.</summary>
    internal void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (state.IntroFrames < 256)
        {
            if (state.IntroFrames == 0)
            {
                // Each Evir the dance spawns carries its own start delay, by the order it spawned in; the dance moves
                // exactly these.
                for (int dancer = 0; dancer < DraygonIntroDanceDefinitions.EvirCount; dancer++)
                {
                    RoomEffectActor sprite = _effectActors.SpawnRoomEffectActor(
                        new(16, 384),
                        RoomEffectActorKind.DraygonIntroEvir,
                        SpriteGraphics.SharedIn(DraygonColours.Evirs));
                    state.IntroDancers.Add(new DraygonIntroDancer(sprite, DraygonIntroDance.StartDelay(dancer)));
                }
            }
            state.IntroFrames++;
            return;
        }

        state.Phase = DraygonPhase.IntroDance;
        state.IntroFrames = 0;
        state.LeftSideReset = state.Body.Position.Whole;
        state.RightSideReset = state.LeftSideReset + DraygonBehavior.DraygonRightSideResetDistance;
        state.Body.Position = state.Body.Position.WithWhole(state.LeftSideReset);
        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
    }

    /// <summary>Runs the 1,232-frame Evir dance.</summary>
    internal void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (state.IntroFrames >= DraygonIntroDanceDefinitions.DurationFrames)
        {
            state.Phase = DraygonPhase.SwoopSetup;
            state.PassDirection = HorizontalDirection.Right;
            state.IntroFrames = 0;
            return;
        }

        // The dance moves the Evirs it spawned, in the order they spawned, each once it has waited out its start delay.
        foreach (DraygonIntroDancer dancer in state.IntroDancers)
        {
            RoomEffectActor sprite = dancer.Sprite;
            int step = state.IntroDanceStep - dancer.StartDelay;
            if (step < 0 || !sprite.IsActive)
                continue;

            DraygonIntroMovement movement = DraygonIntroDance.Movement(step);

            if (movement.DeletesSprite)
            {
                sprite.End();
                continue;
            }

            sprite.Position += movement.Delta;
        }

        state.IntroDanceStep++;
        state.IntroFrames++;
    }
}
