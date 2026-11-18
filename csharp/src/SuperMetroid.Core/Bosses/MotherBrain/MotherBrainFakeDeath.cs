using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's false death: the fade to gray, the tubes collapsing and falling, the room rows and the fade back.</summary>
[RoomLifetime]
internal sealed class MotherBrainFakeDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Spawns the enemies the fake death releases into the room.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>Mother Brain's projectiles, whose ceiling tubes the collapse drops.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFakeDeath(MotherBrainFakeDeathDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;

        _access = dependencies.Access;
        _projectileAttacks = dependencies.ProjectileAttacks;
    }

    /// <summary>
    /// Collision radii and landing floor selected by piece role. The two
    /// outer and two inner pieces share their values; the main tube has its own floor.
    /// </summary>
    internal static MotherBrainFallingTubeDefinition MotherBrainFallingTubeBody(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubePopulationDefinitions.Bodies[piece];

    /// <summary>Steps Mother Brain's fake-death gray fade and advances when its colour cycle completes.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        if (state.PhaseTimer.StepRunsOut())
        {
            state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.FakeDeathGreyStepUpdates);
            if (MotherBrainFadeDefinitions.FakeDeathToGrey.ImageAt(state.FakeDeathGreying.TakeStep()) is { } image)
            {
                // The fake-death fade greys her brain's three colours only; the room's other colours stay as they are.
                state.Colours.ShowFakeDeathToGrey(image);
            }
            else
            {
                state.DescentPhase = MotherBrainDescentPhase.CollapseTubes;
            }
        }

        // Every update of the grey fade also runs the tube collapse and the explosion, so tube sequencing and dust
        // begin during the grey fade rather than waiting for the eighth fade step.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Stops her room's flash and shows its steady grey.</summary>
    internal static void StopMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        state.RoomFlashImage = null;
        state.RoomFlashTimer = 0;
        state.Colours.ShowRoomGrey();
    }

    /// <summary>Emits timed explosions while the first-phase body appears to die.</summary>
    internal void RunMotherBrainFakeDeathExplosion(MotherBrainEnemyState state)
    {
        if (state.FakeDeathExplosionTimer.StepKeepsRunning())
            return;

        state.FakeDeathExplosionTimer.LastFor(MotherBrainWaitDefinitions.FakeDeathExplosionIntervalUpdates);
        Position16<RoomPixels> explosion = state.NextFakeDeathExplosion.Entry;
        state.NextFakeDeathExplosion = state.NextFakeDeathExplosion.NextInCycle;

        // The tube dust reads the current random number and never draws a new one. Drawing a new
        // number here would desynchronize every later Rinka and phase-two attack decision.
        DustExplosionAnimation animation = MotherBrainRandomDefinitions.FakeDeathDustSmokes.IsTakenBy(_room.Random.Read())
            ? DustExplosionAnimation.Smoke
            : DustExplosionAnimation.SmallExplosion;
        _projectiles.SpawnDustOrExplosion(explosion, animation);
        _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>
    /// Drops the tubes about her tank in turn (<see cref="MotherBrainTubeCollapseDefinitions.Drops"/>): each tube falls,
    /// and on the next update the room is cleared where it stood and the wait before the next tube starts.
    /// </summary>
    /// <remarks>
    /// The first tube falls once enough enemy projectiles are free; every later tube waits out the wait its predecessor's
    /// clear started.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Her fake death has not started the collapse.</exception>
    internal void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        MotherBrainTubeCollapseProgress progress = state.TubeCollapse ??
            throw new InvalidOperationException("Mother Brain's tube collapse ran before her fake death started it.");
        if (progress.Tube is not { } tube)
            return;

        MotherBrainTubeDrop drop = tube.Entry;
        if (progress.Stage == MotherBrainTubeDropStage.Standing)
        {
            bool falls = tube.Place == 0
                ? _projectiles.EnemyProjectiles.Count(projectile => !projectile.IsActive) >= MotherBrainTubeCollapseDefinitions.FreeProjectilesNeeded
                : state.TubeCollapseTimer.StepRunsOut();
            if (!falls)
                return;
            switch (drop)
            {
                case MotherBrainBottomTubeDrop bottom:
                    SpawnMotherBrainFallingTube(state, bottom.Piece);
                    break;
                case MotherBrainTopTubeDrop top:
                    SpawnMotherBrainTopTube(top.Kind, top.Segment, top.Start);
                    break;
                default:
                    throw new InvalidOperationException($"Mother Brain's tube collapse cannot drop {drop}.");
            }
            state.TubeCollapse = new(tube, MotherBrainTubeDropStage.Fallen);
            return;
        }

        _access.RequestMotherBrainObject(drop.Clears.Block, drop.Clears.RoomObject);
        state.TubeCollapseTimer.LastFor(drop.WaitAfterUpdates);
        state.TubeCollapse = new(tube.Following, MotherBrainTubeDropStage.Standing);
    }

    /// <summary>Spawns the top tube projectile of <paramref name="kind"/>, falling as <paramref name="tube"/> shows, for the scripted collapse.</summary>
    internal void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        MotherBrainTopTubeSegment tube,
        Position16<RoomPixels> position)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, _projectileAttacks.Projectile,
            graphics: SpriteGraphics.SharedIn(MotherBrainColours.GlassAndTubes));
        projectile.StartAnimation(new MotherBrainTopTubeAnimation(tube));
        projectile.Position = projectile.Position.WithWhole(position);
        projectile.Velocity = EightEightVector<RoomPixels>.Zero;
        projectile.State = new MotherBrainTopTubeState();
    }

    /// <summary>Creates the falling tube enemy of <paramref name="piece"/>.</summary>
    internal void SpawnMotherBrainFallingTube(MotherBrainEnemyState state, MotherBrainTubePiece piece)
    {
        // The bound source supplies the fixed cutscene placement; rooms resolve it from the loaded definitions.
        RoomEnemyPlacement population = _room.Definitions.ReadFallingTubePopulation(piece);

        // A spawn quietly does nothing when all 32 enemy places are occupied.
        // Each tube holds the encounter that spawned it from its setup (#2218).
        _ = _lifecycle.SpawnEnemyJoining(population, state);
    }

    /// <summary>Initializes a falling tube.</summary>
    internal static void InitializeMotherBrainFallingTube(RoomEnemyPlace tube)
    {
        MotherBrainTubeSpawnSettings settings = tube.Spawn.Population.SpawnSettingsOf<MotherBrainTubeSpawnSettings>();
        MotherBrainTubePiece piece = settings.Piece;
        MotherBrainFallingTubeDefinition tubeBody = MotherBrainFallingTubeBody(piece);
        tube.Radius = tubeBody.Radius;
        bool main = piece == MotherBrainTubePiece.Main;
        var state = new MotherBrainFallingTubeState(piece, main ? MotherBrainTubeAction.WaitingToFall : MotherBrainTubeAction.NonMainTube)
        {
            FloorY = tubeBody.Floor,
        };
        // Only the main tube waits before it falls; the side tubes fall from their first update.
        if (main)
            state.FallDelay.LastFor(MotherBrainFallingTubePopulationDefinitions.MainFallWaitUpdates);
        tube.SetSpeciesState(state);
    }

    /// <summary>Runs the actions of the falling tubes.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        switch (tubeState.Action)
        {
            case MotherBrainTubeAction.NonMainTube:
                FallMotherBrainSideTube(tube);
                return;
            case MotherBrainTubeAction.WaitingToFall:
                // The main tube falls as its fall delay runs out.
                if (tubeState.FallDelay.StepKeepsRunning())
                    return;
                tubeState.Action = MotherBrainTubeAction.Falling;
                FallMotherBrainMainTube(tube);
                return;
            case MotherBrainTubeAction.Falling:
                FallMotherBrainMainTube(tube);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube has no behaviour for action {tubeState.Action}.");
        }
    }

    /// <summary>Drops a side tube until it reaches the floor and explodes, trailing smoke as it falls.</summary>
    private void FallMotherBrainSideTube(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = AccelerateMotherBrainTube(tube);
        if (tube.Position.Y.Whole >= tubeState.FloorY)
            ExplodeMotherBrainTube(tube);
        else
            SpawnMotherBrainTubeSmoke(tube);
    }

    /// <summary>Accelerates a falling tube by its gravity and moves it down by its fall speed.</summary>
    private static MotherBrainFallingTubeState AccelerateMotherBrainTube(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        // A tube falls to its floor and explodes there, so its speed stays a few pixels an update.
        tubeState.FallSpeed += MotherBrainTubeCollapseDefinitions.TubeGravity;
        tube.Position = tube.Position.AdvanceY(tubeState.FallSpeed.ToSixteenSixteenDelta());
        return tubeState;
    }

    /// <summary>Drops the main tube, carrying Mother Brain's head with it and landing her body on the floor.</summary>
    private void FallMotherBrainMainTube(RoomEnemyPlace tube)
    {
        AccelerateMotherBrainTube(tube);

        // Hiding the tube and moving the brain are independent branches.
        // The brain follows the tube throughout its descent, including before hiding.
        if (tube.Position.Y.Whole >= MotherBrainTubeCollapseDefinitions.MainTubeHiddenFromY)
            tube.Presence = tube.Presence.With(EnemyPresence.Hidden);
        // The tube holds the encounter that spawned it (#2218).
        MotherBrainEnemyState state = tube.RequireSpeciesState<MotherBrainEnemyState>();
        state.Head.Position = state.Head.Position.WithWholeY(tube.Position.Y.Whole - MotherBrainTubeCollapseDefinitions.HeadAboveMainTube);
        if (state.Head.Position.Y.Whole < MotherBrainFightDefinitions.BrainFloorY)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomFlash(state);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundBackgroundAndEnemies), MotherBrainTubeCollapseDefinitions.LandingShakeUpdates);
        // The fake death removes the room's liquid effects, and any X-ray cone, for the rest of the encounter.
        _frameResults.RemoveLiquidEffects = true;
        state.Head.Position = state.Head.Position.WithWholeY(MotherBrainFightDefinitions.BrainFloorY);
        state.Body.Position = state.Body.Position.WithWhole(MotherBrainFightDefinitions.StandingBodyPosition);

        // The main tube's landing sets the neck's starting angles, not phase-two setup, several updates before the ascent
        // first shows the neck.
        state.LowerNeckAngle = MotherBrainNeckJointDefinitions.InitialLowerAngle;
        state.UpperNeckAngle = MotherBrainNeckJointDefinitions.InitialUpperAngle;
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Steady;
        state.AscentPhase = MotherBrainAscentPhase.DrawMural;
        state.NextMuralRow = MotherBrainMuralRow.First;
        ExplodeMotherBrainTube(tube);
    }

    /// <summary>Emits smoke at the falling tube's current position.</summary>
    private void SpawnMotherBrainTubeSmoke(RoomEnemyPlace tube)
    {
        // The smoke cadence counts down and emits as it runs out.
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        if (tubeState.SmokeDelay.StepKeepsRunning())
            return;

        tubeState.SmokeDelay.LastFor(MotherBrainTubeCollapseDefinitions.SmokeIntervalUpdates);
        tubeState.SmokePuff = tubeState.SmokePuff.NextInCycle;
        _projectiles.SpawnDustOrExplosion(
            new(tube.Position.X.Whole + tubeState.SmokePuff.Entry, MotherBrainTubeCollapseDefinitions.SmokeY),
            DustExplosionAnimation.SmallDustCloud);
    }

    /// <summary>Finishes a falling tube with its explosion and block update.</summary>
    private void ExplodeMotherBrainTube(RoomEnemyPlace tube)
    {
        Position16<RoomPixels> position = tube.Position.Whole;
        tube.Presence = tube.Presence.With(EnemyPresence.Deleted);
        _projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.SmallExplosion);
        _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>
    /// Draws the next rows of the mural the tube collapse revealed, two an update from the top (#2211), and hands off to
    /// the phase-two graphics setup once every row is drawn.
    /// </summary>
    internal void DrawNextMuralRows(MotherBrainEnemyState state)
    {
        ListStep<MotherBrainMuralRow>? row = state.NextMuralRow;
        for (int drawn = 0; drawn < MotherBrainMuralDefinitions.RowsEachUpdate && row is { } shown; drawn++)
        {
            _access.RequestMuralRowReveal(shown.Entry);
            row = shown.Following;
        }
        state.NextMuralRow = row;
        if (row is null)
            state.AscentPhase = MotherBrainAscentPhase.SetupPhase2Graphics;
    }

    /// <summary>Restores the combat colours after the gray fake-death phase.</summary>
    internal static void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.FromGreyStepUpdates);
        if (MotherBrainFadeDefinitions.FakeDeathFromGrey.ImageAt(state.FromFakeDeathGrey.TakeStep()) is { } image)
        {
            // The restoration from grey recolours only three of the brain's colours.
            state.Colours.ShowFakeDeathFromGrey(image);
            return;
        }

        state.Form = MotherBrainForm.SecondPhase;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.StretchingShakeHead;
        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.MenacingShakeUpdates);
    }
}
