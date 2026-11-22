using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Rinka species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed partial class RinkaBehavior : IEnemySpecies, IEnemySpawnFactory<RinkaSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Rinka];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RinkaSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new RinkaEntity(this, occupant, place, common, settings));

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
    /// Special Rinkas awaiting the door loader's camera for spawn selection, each with the spawn point it shares (#2471), in
    /// the order the room's enemy setup reaches them.
    /// </summary>
    private readonly List<DeferredRinkaSpawn> _deferredRinkaSpawns = new();

    /// <summary>A special Rinka whose spawn choice waits for the door loader's camera.</summary>
    /// <param name="Place">The Rinka's place.</param>
    /// <param name="Shared">The spawn point it shares with Mother Brain's other Rinkas.</param>
    private readonly partial record struct DeferredRinkaSpawn(RoomEnemyPlace Place, RinkaSharedSpawn Shared);

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
    /// <returns>The state the enemy holds from its setup.</returns>
    internal RinkaEnemyState InitializeRinka(RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place)
    {
        var state = new RinkaEnemyState(RinkaAction.WatchForLeavingViewport, LifeAnimation(rinkaSpawnSettings), rinkaSpawnSettings);

        // Every Rinka starts animating and untouchable. Mother Brain's Rinkas also run off-screen and never respawn;
        // an ordinary room Rinka respawns: its death explosion brings it back in its place in the room's listing.
        place.Presence = place.Presence.Without(EnemyPresence.RespawnsIfKilled);
        place.Processing = place.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        if (state.SharedSpawn is { } shared)
        {
            // The spawn selection tests its spawn points against the foreground. A door's loader runs this
            // while the door transition is still scrolling, so that choice waits for the camera
            // of the update in which the loader reaches this place.
            if (_frameContext.DeferLoaderTimeCameraReads)
                _deferredRinkaSpawns.Add(new DeferredRinkaSpawn(place, shared));
            else
                ReserveSpecialRinkaSpawn(place, shared);
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
        InitializeRinkaLife(rinkaSpawnSettings, place, state);
        return state;
    }

    /// <summary>Re-initializes a Rinka, covering both of its later setup paths.</summary>
    private void ReinitializeRinka(RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place, RinkaEnemyState state)
    {
        if (state.SharedSpawn is { } sharing)
            ReserveSpecialRinkaSpawn(place, sharing);

        // A special Rinka returns to its spawn point, the one it last reserved; an ordinary Rinka to its own placement.
        place.Position = state.SharedSpawn is { } shared
            ? place.Position.WithWhole(RinkaSpawnReservations.Centre(shared.SpawnPoint))
            : occupant.Spawn.Placing.Place(place.Position);
        InitializeRinkaLife(rinkaSpawnSettings, place, state);
    }

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place, RinkaEnemyState state)
    {
        state.Action = RinkaAction.WatchForLeavingViewport;
        state.AimDelay.LastFor(RinkaLifeDefinitions.AimDelayUpdates);
        state.Velocity = default;

        if (IsSpecialRinka(rinkaSpawnSettings) && _rinkaTerminationFlag)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }
        state.Playback = RinkaEnemyState.Starting(LifeAnimation(rinkaSpawnSettings));
        place.AnimationClock.Restart();
    }

    /// <summary>The animation each of <paramref name="place"/>'s lives starts: a special Rinka's or an ordinary one's.</summary>
    private static RinkaSegment LifeAnimation(RinkaSpawnSettings rinkaSpawnSettings) =>
        IsSpecialRinka(rinkaSpawnSettings) ? RinkaSegment.KeptRunningOffScreen : RinkaSegment.Ordinary;

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
            ReserveSpecialRinkaSpawn(rinka.Place, rinka.Shared);
        }
    }

    /// <summary>What <see cref="RunRinkaMain"/> does for each <see cref="RinkaAction"/> member, each member's own arm.</summary>
    private IdentityTable<RinkaAction, Action<RoomEnemyOccupant, RinkaSpawnSettings, RoomEnemyPlace, RinkaEnemyState, SamusState>> RunRinkaMainForRinkaAction => field ??= new(new Dictionary<RinkaAction, Action<RoomEnemyOccupant, RinkaSpawnSettings, RoomEnemyPlace, RinkaEnemyState, SamusState>>
    {
        [RinkaAction.WatchForLeavingViewport] = (occupant, settings, place, state, samus) =>
        {
            if (RinkaIsOutsideFlightWindow(place))
                RecycleRinkaAfterLeavingViewport(occupant, settings, place, state);
        },
        [RinkaAction.AimDelay] = (_, _, place, state, samus) =>
        {
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
        },
        [RinkaAction.Flying] = (occupant, settings, place, state, samus) =>
        {
            place.Position = place.Position.Advance(state.Velocity.ToDisplacement());
            if (RinkaIsOutsideFlightWindow(place))
                RecycleRinkaAfterLeavingViewport(occupant, settings, place, state);
        },
        [RinkaAction.DeathRespawnDelay] = (occupant, settings, place, state, samus) =>
        {
            if (state.RespawnDelay.StepKeepsRunning())
                return;
            place.Health = RinkaLifeDefinitions.RespawnHealth;
            ReinitializeRinka(occupant, settings, place, state);
        },
    });

    /// <summary>
    /// Runs a Rinka's main AI for one update through all four of its actions. Every special Rinka has chosen its spawn
    /// point by then: the door's loading checks, as its scroll finishes, that it reached every place whose setup it
    /// deferred, and no enemy runs before.
    /// </summary>
    private void RunRinkaMain(
        RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place,
        RinkaEnemyState state,
        SamusState samus)
    {
        if (_deferredRinkaSpawns.Count != 0)
            throw new InvalidOperationException(
                "A Rinka ran before the door loader chose every special Rinka's spawn point.");
        if (IsSpecialRinka(rinkaSpawnSettings) && _rinkaTerminationFlag)
        {
            ReleaseSpecialRinkaSpawn(state);
            RunRinkaDeathAnimation(occupant);
            return;
        }

        RunRinkaMainForRinkaAction[state.Action](occupant, rinkaSpawnSettings, place, state, samus);
    }

    /// <summary>
    /// Finishes a Rinka's reaction to a touch, a shot or a power bomb after the common damage: a killed Rinka either
    /// dies, or, one of Mother Brain's, hides and waits to respawn.
    /// </summary>
    internal void ResolveRinkaCombatAfterCommon(RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place, RinkaEnemyState state)
    {
        if (place.Health != 0)
            return;
        ReleaseSpecialRinkaSpawn(state);
        if (!IsSpecialRinka(rinkaSpawnSettings))
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
    internal bool ThawOrDieWhileFrozen(RoomEnemyOccupant occupant, RinkaEnemyState rinkaEnemyState)
    {
        RoomEnemyPlace place = occupant.Place;
        if (RinkaIsOutsideFlightWindow(place))
            place.Freeze.End();
        if (!_rinkaTerminationFlag)
            return false;

        ReleaseSpecialRinkaSpawn(rinkaEnemyState);
        RunRinkaDeathAnimation(occupant);
        return true;
    }

    /// <summary>What <see cref="ApplyRinkaCue"/> does for each <see cref="RinkaCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<RinkaCueKind, Action<RoomEnemyPlace, RinkaEnemyState>> ApplyRinkaCueForRinkaCueKind => field ??= new(new Dictionary<RinkaCueKind, Action<RoomEnemyPlace, RinkaEnemyState>>
    {
        [RinkaCueKind.Hide] = static (place, state) =>
        {
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Hidden);
        },
        [RinkaCueKind.HideAndProcessOffScreen] = static (place, state) =>
        {
            place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Hidden);
        },
        [RinkaCueKind.Fire] = static (place, state) =>
        {
            place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.Without(EnemyPresence.Hidden);
            state.Action = RinkaAction.AimDelay;
        },
    });

    /// <summary>Carries out one event of the Rinkas' animations: hiding, hiding with off-screen processing, and firing.</summary>
    private static void ApplyRinkaCue(RoomEnemyPlace place, RinkaEnemyState state, RinkaCue cue)
    {
        ApplyRinkaCueForRinkaCueKind[cue.Kind](place, state);
    }

    /// <summary>Releases a special spawn resource and restarts its life unless the room has terminated it.</summary>
    private void RecycleRinkaAfterLeavingViewport(
        RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place,
        RinkaEnemyState state)
    {
        if (IsSpecialRinka(rinkaSpawnSettings))
        {
            ReleaseSpecialRinkaSpawn(state);
            if (_rinkaTerminationFlag)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                return;
            }
        }

        ReinitializeRinka(occupant, rinkaSpawnSettings, place, state);
    }

    /// <summary>
    /// Reserves a special Rinka's spawn point against the current camera, moving the Rinka to the point it reserves, which
    /// becomes its spawn point when it cannot keep its own.
    /// </summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemyPlace place, RinkaSharedSpawn shared)
    {
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
    private static bool IsSpecialRinka(RinkaSpawnSettings rinkaSpawnSettings) => rinkaSpawnSettings is RinkaSpawnSettings.SharingPoints;

    /// <summary>Rinka's reaction to a bomb: the common damage without the generic death, then, when the hit kills it, Rinka's own death.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal static EnemyBombReaction RinkaBombReaction(RinkaEntity rinka) => new()
    {
        RunsGenericDeath = false,
        // The bomb's common damage shows no hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = Ensure.NotNull(rinka).ResolveCombatAfterCommon,
    };

    /// <summary>Runs Rinka's main AI.</summary>
    internal void RunRinkaMainAi(RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, RoomEnemyPlace place, RinkaEnemyState rinkaEnemyState, IEnemyAiFrame frame)
    {
        RunRinkaMain(occupant, rinkaSpawnSettings, place, rinkaEnemyState, frame.Samus);
    }

    /// <summary>Advances Rinka's typed animation.</summary>
    internal static void AdvanceRinkaTypedAnimation(RoomEnemyPlace place, RinkaEnemyState rinkaEnemyState) =>
        EnemyAnimationPlayback.Advance(place, rinkaEnemyState, RinkaAnimations.Segments,
            static (RinkaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRinkaCue(place, state, cue);
                return null;
            });

    /// <summary>Skips invisible actors and applies the private death lifecycle after power-bomb damage.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal static EnemyPowerBombReaction RinkaPowerBombReaction(RinkaEntity rinka) => new()
    {
        SkipsEnemy = IsHidden,
        Lethal = EnemyPowerBombLethal.SpeciesOwned,
        AfterCommon = Ensure.NotNull(rinka).ResolveCombatAfterCommon,
    };

    /// <summary>Whether a Rinka is hidden, which a Power Bomb skips.</summary>
    private static bool IsHidden(RoomEnemyPlace enemy) => enemy.Presence.HasAny(EnemyPresence.Hidden);

    /// <summary>Rinka's reaction to a shot: its own death follows the damage, but not a shot that freezes it at once.</summary>
    /// <param name="rinka">The Rinka the reaction is built for.</param>
    internal static EnemyShotReaction RinkaShotReaction(RinkaEntity rinka) => new()
    {
        // Rinka's own death follows damage only; a shot that freezes it at once skips it.
        Lethal = EnemyShotLethal.SpeciesOwned,
        // Rinka's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = Ensure.NotNull(rinka).ResolveCombatAfterCommon,
    };

    /// <summary>Applies normal contact damage before the Rinka-specific lethal-health lifecycle.</summary>
    internal void ReactToRinkaTouch(RoomEnemyOccupant occupant, RinkaSpawnSettings rinkaSpawnSettings, EnemyTouchContext context, RinkaEnemyState rinkaEnemyState)
    {
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        ResolveRinkaCombatAfterCommon(occupant, rinkaSpawnSettings, context.Place, rinkaEnemyState);
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
