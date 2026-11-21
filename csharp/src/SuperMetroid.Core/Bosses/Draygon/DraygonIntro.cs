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

    /// <summary>Waits out the opening (<see cref="DraygonArenaDefinitions.IntroUpdates"/>).</summary>
    internal void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        if (state.IntroStage.IsRunning)
        {
            if (state.IntroStage.IsUncounted)
            {
                // Each Evir the dance spawns carries its own start delay; the dance moves exactly these.
                foreach (int startDelay in DraygonIntroDanceDefinitions.DancerStartDelays)
                {
                    RoomEffectActor sprite = _effectActors.SpawnRoomEffectActor(
                        DraygonArenaDefinitions.IntroEvirEntry,
                        RoomEffectActorKind.EvirFacingLeft,
                        SpriteGraphics.SharedIn(DraygonColours.Evirs));
                    state.IntroDancers.Add(new DraygonIntroDancer(sprite, startDelay));
                }
            }
            state.IntroStage.Count();
            return;
        }

        state.Step = DraygonPhaseSteps.IntroDance();
        state.IntroStage = UpdateCountdown.Lasting(DraygonIntroDanceDefinitions.DurationFrames);
        state.LeftSideReset = state.Body.Position.Whole;
        // The left-side place is where the room places Draygon, left of and above the arena, so the right-side place
        // counts on from a place before the room.
        state.RightSideResetX = OffRoomPlaces.Place(state.LeftSideReset.X + DraygonArenaDefinitions.RightSideResetDistance);
        state.Body.Position = state.Body.Position.WithWhole(state.LeftSideReset);
        state.SwoopAcceleration = DraygonSwoopDefinitions.InitialAcceleration;
    }

    /// <summary>Runs the Evir dance (<see cref="DraygonIntroDanceDefinitions.DurationFrames"/>).</summary>
    internal void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        if (state.IntroStage.IsOver)
        {
            state.Step = DraygonPhaseSteps.RightSwoopSetup();
            state.PassDirection = HorizontalDirection.Right;
            return;
        }

        // The dance moves the Evirs it spawned, in the order they spawned, each once it has waited out its start delay.
        foreach (DraygonIntroDancer dancer in state.IntroDancers)
        {
            if (state.IntroStage.Age < dancer.StartDelay || !dancer.Sprite.IsActive)
                continue;

            dancer.Step();
        }

        state.IntroStage.Count();
    }
}
