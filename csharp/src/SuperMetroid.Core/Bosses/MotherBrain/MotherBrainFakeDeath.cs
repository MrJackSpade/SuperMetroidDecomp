using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's false death: the fade to gray, the tubes collapsing and falling, the room rows and the fade back.</summary>
[RoomLifetime]
internal sealed class MotherBrainFakeDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;
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
        _passEvents = dependencies.PassEvents;
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
    private static MotherBrainFallingTubeDefinition MotherBrainFallingTubeBody(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubePopulationDefinitions.Bodies[piece];

    /// <summary>Steps Mother Brain's fake-death grey fade and moves on once the fade is over.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        if (state.FakeDeathGreying.Step() is { } image)
        {
            // The fake-death fade greys her brain's three colours only; the room's other colours stay as they are.
            state.Colours.ShowFakeDeathToGrey(image);
        }
        else if (state.FakeDeathGreying.IsOver)
        {
            state.DescentPhase = MotherBrainDescentPhase.CollapseTubes;
        }

        // Every update of the grey fade also runs the tube collapse and the explosion, so tube sequencing and dust
        // begin during the grey fade rather than waiting for its last fade step.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Stops her room's flash and shows its steady grey.</summary>
    private static void StopMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        state.RoomFlash = null;
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
            bool falls = tube == MotherBrainTubeCollapseProgress.FirstTube
                ? _projectiles.FreePlaceCount >= MotherBrainTubeCollapseDefinitions.FreeProjectilesNeeded
                : state.TubeCollapseTimer.StepRunsOut();
            if (!falls)
                return;
            drop.React(TubeDrops, new TubeDropping(this, state));
            state.TubeCollapse = new(tube, MotherBrainTubeDropStage.Fallen);
            return;
        }

        _access.RequestMotherBrainObject(drop.Clears.Block, drop.Clears.RoomObject);
        if (drop is MotherBrainFollowedTubeDrop followed)
            state.TubeCollapseTimer.LastFor(followed.WaitAfterUpdates);
        state.TubeCollapse = new(tube.Following, MotherBrainTubeDropStage.Standing);
    }

    /// <summary>What the collapse drops for each tube (#2553): a bottom or the main tube falls, a top tube spawns as a projectile.</summary>
    private static MotherBrainTubeDropReactions<TubeDropping> TubeDrops { get; } = new(
        Bottom: static (bottom, dropping) => dropping.Owner.SpawnMotherBrainFallingTube(dropping.State, bottom.Piece),
        Top: static (top, dropping) => dropping.Owner.SpawnMotherBrainTopTube(top.Kind, top.Segment, top.Start),
        Main: static (_, dropping) => dropping.Owner.SpawnMotherBrainFallingTube(dropping.State, MotherBrainTubePiece.Main));

    /// <summary>A tube the collapse drops (#2553): the fake death dropping it and Mother Brain's state.</summary>
    /// <param name="Owner">The fake death dropping the tube.</param>
    /// <param name="State">Mother Brain's state.</param>
    private readonly record struct TubeDropping(MotherBrainFakeDeath Owner, MotherBrainEnemyState State);

    /// <summary>Spawns the top tube projectile of <paramref name="kind"/>, falling as <paramref name="tube"/> shows, for the scripted collapse.</summary>
    private void SpawnMotherBrainTopTube(
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
        projectile.Velocity = PixelVelocity<RoomPixels>.Zero;
        _projectileAttacks.Projectile.TopTubes.Start(projectile, new MotherBrainTopTubeState());
    }

    /// <summary>Creates the falling tube enemy of <paramref name="piece"/>.</summary>
    private void SpawnMotherBrainFallingTube(MotherBrainEnemyState state, MotherBrainTubePiece piece)
    {
        // The bound source supplies the fixed cutscene placement; rooms resolve it from the loaded definitions.
        RoomEnemyPlacement population = _room.Definitions.ReadFallingTubePopulation(piece);

        // A spawn quietly does nothing when every enemy place is occupied.
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

    /// <summary>What <c>RunMotherBrainFallingTubeMain</c> does for each <see cref="MotherBrainTubeAction"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainTubeAction, Action<RoomEnemyPlace, MotherBrainFallingTubeState>> RunMotherBrainFallingTubeMainForMotherBrainTubeAction => field ??= new(new Dictionary<MotherBrainTubeAction, Action<RoomEnemyPlace, MotherBrainFallingTubeState>>
    {
        [MotherBrainTubeAction.NonMainTube] = (tube, tubeState) => FallMotherBrainSideTube(tube),
        [MotherBrainTubeAction.WaitingToFall] = (tube, tubeState) =>
        {
            // The main tube falls as its fall delay runs out.
            if (tubeState.FallDelay.StepKeepsRunning())
                return;
            tubeState.Action = MotherBrainTubeAction.Falling;
            FallMotherBrainMainTube(tube);
        },
        [MotherBrainTubeAction.Falling] = (tube, tubeState) => FallMotherBrainMainTube(tube),
    });

    /// <summary>Runs the actions of the falling tubes.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        RunMotherBrainFallingTubeMainForMotherBrainTubeAction[tubeState.Action](tube, tubeState);
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
        _earthquake.Start(MotherBrainTubeCollapseDefinitions.LandingShake, MotherBrainTubeCollapseDefinitions.LandingShakeUpdates);
        // The fake death removes the room's liquid effects, and any X-ray cone, for the rest of the encounter.
        _passEvents.Publish(new LiquidSurfaceStilled());
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
    /// Draws the next rows of the mural the tube collapse revealed, <see cref="MotherBrainMuralDefinitions.RowsEachUpdate"/> an update from the top (#2211), and hands off to
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
        if (state.FromFakeDeathGrey.Step() is { } image)
        {
            // The restoration from grey recolours only three of the brain's colours.
            state.Colours.ShowFakeDeathFromGrey(image);
            return;
        }
        if (!state.FromFakeDeathGrey.IsOver)
            return;

        state.Form = MotherBrainForm.SecondPhase;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.StretchingShakeHead;
        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.MenacingShakeUpdates);
    }
}
