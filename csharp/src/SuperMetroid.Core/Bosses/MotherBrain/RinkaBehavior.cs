using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Rinka species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class RinkaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Rinka];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new RinkaEntity(this, occupant, place, common));

    /// <summary>Applies normal Samus contact damage before the Rinka's own death.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Indicates when door loading must defer camera-sensitive spawn selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Allocates Rinka dust and death explosion projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Removes a dying Rinka in its death explosion, which an ordinary Rinka respawns from.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects Rinka AI to collision, frame context, and death effects.</summary>
    internal RinkaBehavior(EnemyCollision collision, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyDeathsAndDrops deathsAndDrops)
    {
        _collision = collision;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// The spawn points Mother Brain's three special Rinkas compete for; a Rinka whose own point is off screen or held
    /// moves to another, which becomes its spawn point.
    /// </summary>
    private readonly RinkaSpawnReservations _spawnReservations = new();
    /// <summary>Causes special Rinkas to be removed instead of launched or respawned.</summary>
    private bool _rinkaTerminationFlag;

    /// <summary>Mother Brain's request that special Rinkas be removed instead of launched or respawned.</summary>
    internal void RequestTermination() => _rinkaTerminationFlag = true;
    /// <summary>Camera coordinates used by the spawn and flight windows.</summary>
    private Position16<RoomPixels> _rinkaCamera;
    /// <summary>
    /// Special Rinkas awaiting the door loader's camera for spawn selection, each with the state its setup gave it (#2471),
    /// in the order the room's enemy setup reaches them.
    /// </summary>
    private readonly List<DeferredRinkaSpawn> _deferredRinkaSpawns = new();

    /// <summary>A special Rinka whose spawn choice waits for the door loader's camera.</summary>
    /// <param name="Place">The Rinka's place.</param>
    /// <param name="State">The state its setup gave it.</param>
    private readonly partial record struct DeferredRinkaSpawn(RoomEnemyPlace Place, RinkaEnemyState State);

    /// <summary>
    /// Captures camera coordinates used by the Rinka's flight and spawn checks. <see cref="BeginEnemyFrame"/> refreshes
    /// them before any Rinka AI, the room load supplies the already-positioned destination camera for initialization,
    /// and <see cref="CompleteLoaderTimeCameraReads"/> supplies the camera of the update the door loader reaches a place on.
    /// </summary>
    internal void SetRinkaCamera(Position16<RoomPixels> camera)
    {
        _rinkaCamera = camera;
    }

    /// <summary>Sets a Rinka up when the room spawns it.</summary>
    internal void InitializeRinka(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new RinkaEnemyState(RinkaAction.WatchForLeavingViewport, LifeAnimation(occupant), occupant.SpawnSettings<RinkaSpawnSettings>());
        place.SetSpeciesState(state);

        // Every Rinka starts animating and untouchable. Mother Brain's Rinkas also run off-screen and never respawn;
        // an ordinary room Rinka respawns: its death explosion brings it back in its place in the room's listing.
        place.Presence = place.Presence.Without(EnemyPresence.RespawnsIfKilled);
        place.Processing = place.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        if (IsSpecialRinka(occupant))
        {
            // The spawn selection tests its spawn points against the foreground. A door's loader runs this
            // while the door transition is still scrolling, so that choice waits for the camera
            // of the update in which the loader reaches this place.
            if (_frameContext.DeferLoaderTimeCameraReads)
                _deferredRinkaSpawns.Add(new DeferredRinkaSpawn(place, state));
            else
                ReserveSpecialRinkaSpawn(place, state);
            place.Processing = place.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        }
        else
        {
            place.Presence = place.Presence.With(EnemyPresence.RespawnsIfKilled);
            place.Processing = place.Processing.With(EnemyProcessing.Animates);
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        }

        // Both ordinary and Mother Brain variants draw in the Rinka's own colours, which every room placing
        // Rinkas draws them in: their variant's standard colours (#1512, #1599).
        place.Shading = PictureShading.Standard;
        InitializeRinkaLife(occupant, place, state);
    }

    /// <summary>Re-initializes a Rinka, covering both of its later setup paths.</summary>
    private void ReinitializeRinka(RoomEnemyOccupant occupant, RoomEnemyPlace place, RinkaEnemyState state)
    {
        if (IsSpecialRinka(occupant))
            ReserveSpecialRinkaSpawn(place, state);

        // A special Rinka returns to its spawn point, the one it last reserved; an ordinary Rinka to its own placement.
        place.Position = state.SharedSpawn is { } shared
            ? place.Position.WithWhole(RinkaSpawnReservations.Centre(shared.SpawnPoint))
            : occupant.Spawn.Placing.Place(place.Position);
        InitializeRinkaLife(occupant, place, state);
    }

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RoomEnemyOccupant occupant, RoomEnemyPlace place, RinkaEnemyState state)
    {
        state.Action = RinkaAction.WatchForLeavingViewport;
        state.AimDelay.LastFor(RinkaLifeDefinitions.AimDelayUpdates);
        state.Velocity = default;

        if (IsSpecialRinka(occupant) && _rinkaTerminationFlag)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }
        state.Playback = RinkaEnemyState.Starting(LifeAnimation(occupant));
        place.AnimationClock.Restart();
    }

    /// <summary>The animation each of <paramref name="place"/>'s lives starts: a special Rinka's or an ordinary one's.</summary>
    private static RinkaSegment LifeAnimation(RoomEnemyOccupant occupant) =>
        IsSpecialRinka(occupant) ? RinkaSegment.KeptRunningOffScreen : RinkaSegment.Ordinary;

    /// <summary>True while a door load still owes a special Rinka its spawn choice.</summary>
    public bool HasDeferredLoaderTimeCameraReads => _deferredRinkaSpawns.Count != 0;

    /// <summary>
    /// Runs the camera-dependent part of each deferred initialization whose place the door
    /// loader has reached, against that update's camera position.
    /// </summary>
    public void CompleteLoaderTimeCameraReads(
        Func<RoomEnemyPlace, bool> loaderInitializedPlace,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(loaderInitializedPlace);
        while (_deferredRinkaSpawns.Count != 0 && loaderInitializedPlace(_deferredRinkaSpawns[0].Place))
        {
            DeferredRinkaSpawn rinka = _deferredRinkaSpawns[0];
            _deferredRinkaSpawns.RemoveAt(0);
            SetRinkaCamera(camera);
            ReserveSpecialRinkaSpawn(rinka.Place, rinka.State);
        }
    }

    /// <summary>Runs a Rinka's main AI for one update through all four of its actions.</summary>
    private void RunRinkaMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        RinkaEnemyState state,
        SamusState samus)
    {
        if (_deferredRinkaSpawns.Count != 0)
            throw new InvalidOperationException(
                "A Rinka ran before the door loader chose every special Rinka's spawn point.");
        if (IsSpecialRinka(occupant) && _rinkaTerminationFlag)
        {
            ReleaseSpecialRinkaSpawn(state);
            RunRinkaDeathAnimation(occupant);
            return;
        }

        switch (state.Action)
        {
            case RinkaAction.WatchForLeavingViewport:
                if (RinkaIsOutsideFlightWindow(place))
                    RecycleRinkaAfterLeavingViewport(occupant, place, state);
                return;

            case RinkaAction.AimDelay:
                if (state.AimDelay.StepKeepsRunning())
                    return;
                state.Action = RinkaAction.Flying;

                // Every Rinka becomes touchable as it flies, and runs off-screen.
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);

                TrigonometryAngle velocityAngle = EnemyMath.CalculateAimAngle(
                    new(DistanceAcrossRoomEdge.Apart(samus.Position.X.Whole, place.Position.X.Whole), DistanceAcrossRoomEdge.Apart(samus.Position.Y.Whole, place.Position.Y.Whole))).TurnedBy(TrigonometryAngle.QuarterTurn);
                state.Velocity = new(
                    EnemyMath.SineSpeed(RinkaLifeDefinitions.Speed, velocityAngle),
                    EnemyMath.SineSpeed(
                        RinkaLifeDefinitions.Speed,
                        velocityAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
                return;

            case RinkaAction.Flying:
                place.Position = place.Position.Advance(state.Velocity.ToDisplacement());
                if (RinkaIsOutsideFlightWindow(place))
                    RecycleRinkaAfterLeavingViewport(occupant, place, state);
                return;

            case RinkaAction.DeathRespawnDelay:
                if (state.RespawnDelay.StepKeepsRunning())
                    return;
                place.Health = RinkaLifeDefinitions.RespawnHealth;
                ReinitializeRinka(occupant, place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Rinka has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Finishes a Rinka's reaction to a touch, a shot or a power bomb after the common damage: a killed Rinka either
    /// dies, or, one of Mother Brain's, hides and waits to respawn.
    /// </summary>
    private void ResolveRinkaCombatAfterCommon(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        if (place.Health != 0)
            return;

        RinkaEnemyState state = place.RequireSpeciesState<RinkaEnemyState>();
        ReleaseSpecialRinkaSpawn(state);
        if (!IsSpecialRinka(occupant))
        {
            RunRinkaDeathAnimation(occupant);
            return;
        }

        // Special Mother Brain Rinkas do not leave their enemy place. They hide, raise their small death explosion,
        // wait out their respawn delay, restore their respawn health, and select a spawn.
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        SpawnRinkaDustExplosion(place.Position.Whole);
        state.Action = RinkaAction.DeathRespawnDelay;
        state.RespawnDelay.LastFor(RinkaLifeDefinitions.RespawnDelayUpdates);
    }

    /// <summary>
    /// Runs a frozen Rinka's own step before the common thaw: outside its flight window it thaws at once, and once the
    /// Rinkas are ending it dies; true when it died.
    /// </summary>
    internal bool ThawOrDieWhileFrozen(RoomEnemyOccupant occupant)
    {
        RoomEnemyPlace place = occupant.Place;
        if (RinkaIsOutsideFlightWindow(place))
            place.Freeze.End();
        if (!_rinkaTerminationFlag)
            return false;

        ReleaseSpecialRinkaSpawn(place.RequireSpeciesState<RinkaEnemyState>());
        RunRinkaDeathAnimation(occupant);
        return true;
    }

    /// <summary>Carries out one event of the Rinkas' animations: hiding, hiding with off-screen processing, and firing.</summary>
    private static void ApplyRinkaCue(RoomEnemyPlace place, RinkaEnemyState state, RinkaCue cue)
    {
        switch (cue.Kind)
        {
            case RinkaCueKind.Hide:
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                break;
            case RinkaCueKind.HideAndProcessOffScreen:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                break;
            case RinkaCueKind.Fire:
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                state.Action = RinkaAction.AimDelay;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RinkaCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Releases a special spawn resource and restarts its life unless the room has terminated it.</summary>
    private void RecycleRinkaAfterLeavingViewport(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        RinkaEnemyState state)
    {
        if (IsSpecialRinka(occupant))
        {
            ReleaseSpecialRinkaSpawn(state);
            if (_rinkaTerminationFlag)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                return;
            }
        }

        ReinitializeRinka(occupant, place, state);
    }

    /// <summary>
    /// Reserves a special Rinka's spawn point against the current camera, moving the Rinka to the point it reserves, which
    /// becomes its spawn point when it cannot keep its own.
    /// </summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemyPlace place, RinkaEnemyState state)
    {
        RinkaSharedSpawn shared = state.SharedSpawn ?? throw new InvalidOperationException(
            $"Special Rinka {place.Ordinal} has no spawn point to reserve.");
        // Every point being held is a real failure path: the Rinka keeps its place and its spawn point.
        if (_spawnReservations.Reserve(shared.SpawnPoint, _rinkaCamera) is not { } reserved)
            return;

        shared.SpawnPoint = reserved;
        shared.HeldSpawnPoint = reserved;
        place.Position = place.Position.WithWhole(RinkaSpawnReservations.Centre(reserved));
    }

    /// <summary>Releases the spawn point a special Rinka holds, if it holds one.</summary>
    private void ReleaseSpecialRinkaSpawn(RinkaEnemyState state)
    {
        if (state.SharedSpawn is not { HeldSpawnPoint: { } held } shared)
            return;

        _spawnReservations.Release(held);
        shared.HeldSpawnPoint = null;
    }

    /// <summary>The Rinka's flight rectangle: the screen widened by its flight margin on every side. A Rinka flown above or left of the room lies outside it.</summary>
    private bool RinkaIsOutsideFlightWindow(RoomEnemyPlace place)
    {
        if (OffRoomPlaces.LiesBeforeRoom(place.Position.Y.Whole))
            return true;
        int relativeY = CameraPastRoomEdge.FromCamera(place.Position.Y.Whole + RinkaLifeDefinitions.FlightMargin, _rinkaCamera.Y);
        if (relativeY is < 0 or >= RinkaLifeDefinitions.FlightWindowHeight)
            return true;
        if (OffRoomPlaces.LiesBeforeRoom(place.Position.X.Whole))
            return true;
        int relativeX = CameraPastRoomEdge.FromCamera(place.Position.X.Whole + RinkaLifeDefinitions.FlightMargin, _rinkaCamera.X);
        return relativeX is < 0 or >= RinkaLifeDefinitions.FlightWindowWidth;
    }

    /// <summary>Distinguishes Mother Brain's parameterized spawn-sharing actors from ordinary room Rinkas.</summary>
    private static bool IsSpecialRinka(RoomEnemyOccupant occupant) => occupant.SpawnSettings<RinkaSpawnSettings>() is RinkaSpawnSettings.SharingPoints;

    /// <summary>Rinka's reaction to a bomb: the common damage without the generic death, then, when the hit kills it, Rinka's own death.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal EnemyBombReaction RinkaBombReaction(RoomEnemyOccupant rinka) => new()
    {
        RunsGenericDeath = false,
        // The bomb's common damage shows no hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveRinkaCombatAfterCommon(rinka, c.Enemy),
    };

    /// <summary>Runs Rinka's main AI.</summary>
    internal void RunRinkaMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunRinkaMain(occupant, place, place.RequireSpeciesState<RinkaEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Rinka's typed animation.</summary>
    internal static void AdvanceRinkaTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, RinkaAnimations.Segments,
            static (RinkaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRinkaCue(place, state, cue);
                return null;
            });

    /// <summary>Skips invisible actors and applies the private death lifecycle after power-bomb damage.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal EnemyPowerBombReaction RinkaPowerBombReaction(RoomEnemyOccupant rinka) => new()
    {
        SkipsEnemy = enemy => enemy.Presence.HasAny(EnemyPresence.Hidden),
        Lethal = EnemyPowerBombLethal.SpeciesOwned,
        AfterCommon = c => ResolveRinkaCombatAfterCommon(rinka, c.Enemy),
    };

    /// <summary>Rinka's reaction to a shot: its own death follows the damage, but not a shot that freezes it at once.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal EnemyShotReaction RinkaShotReaction(RoomEnemyOccupant rinka) => new()
    {
        // Rinka's own death follows damage only; a shot that freezes it at once skips it.
        Lethal = EnemyShotLethal.SpeciesOwned,
        // Rinka's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (!c.Froze)
                ResolveRinkaCombatAfterCommon(rinka, c.Enemy);
        },
    };

    /// <summary>Applies normal contact damage before the Rinka-specific lethal-health lifecycle.</summary>
    internal void ReactToRinkaTouch(RoomEnemyOccupant occupant, EnemyTouchContext context)
    {
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        ResolveRinkaCombatAfterCommon(occupant, context.Place);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        SetRinkaCamera(frame.Camera);
    }
    /// <summary>The dust a Rinka's special death raises.</summary>
    private const DustExplosionAnimation DeathDust = DustExplosionAnimation.SmallExplosion;
    /// <summary>Raises a Rinka's small death explosion at <paramref name="position"/>.</summary>
    internal void SpawnRinkaDustExplosion(Position16<RoomPixels> position)
        => _projectiles.SpawnDustOrExplosion(position, DeathDust);

    /// <summary>
    /// Removes a dying Rinka in its small death explosion: an ordinary Rinka respawns from its population entry when the
    /// explosion ends, and a terminated special Rinka, which does not respawn, is gone.
    /// </summary>
    internal void RunRinkaDeathAnimation(RoomEnemyOccupant rinka)
    {
        RoomEnemyPlace place = rinka.Place;
        _deathsAndDrops.RecordKill(place);
        _deathsAndDrops.RemoveKilledEnemy(rinka, EnemyDeathExplosion.Small, respawns: place.Presence.HasAny(EnemyPresence.RespawnsIfKilled));
    }
}
