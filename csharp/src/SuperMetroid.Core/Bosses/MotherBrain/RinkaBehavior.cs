using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Rinka species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RinkaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Rinka];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new RinkaEntity(this, place, common);

    /// <summary>Applies normal Samus contact damage before the Rinka-specific death tail.</summary>
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
    /// Special Rinkas awaiting the door loader's camera for spawn selection, in the
    /// order the room's enemy setup reaches them.
    /// </summary>
    private readonly List<RoomEnemyPlace> _deferredRinkaSpawnPlaces = new();

    /// <summary>
    /// Captures camera coordinates used by the Rinka's flight and spawn checks. StepFrame refreshes these before
    /// any Rinka AI; Load supplies the already-positioned destination camera for initialization.
    /// </summary>
    internal void SetRinkaCamera(Position16<RoomPixels> camera)
    {
        _rinkaCamera = camera;
    }

    /// <summary>Sets a Rinka up when the room spawns it.</summary>
    internal void InitializeRinka(RoomEnemyPlace place)
    {
        var state = new RinkaEnemyState(RinkaAction.WatchForLeavingViewport, place.SpawnSettings<RinkaSpawnSettings>().SharedSpawnPoint);
        place.SetSpeciesState(state);

        // Both branches first clear the four lifecycle/collision bits. The special
        // Mother Brain variant is always processed off-screen but never uses generic death
        // respawn; ordinary room Rinkas retain the respawn property so their death explosion resurrects
        // the enemy in its place in the room's listing from its population snapshot.
        place.Presence = place.Presence.Without(EnemyPresence.RespawnsIfKilled);
        place.Processing = place.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        if (IsSpecialRinka(place))
        {
            // The spawn selection tests its spawn points against the foreground. A door's loader runs this
            // while the door transition is still scrolling, so that choice waits for the camera
            // of the update in which the loader reaches this place.
            if (_frameContext.DeferLoaderTimeCameraReads)
                _deferredRinkaSpawnPlaces.Add(place);
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
        InitializeRinkaLife(place, state);
    }

    /// <summary>Re-initializes a Rinka, covering both of its later setup paths.</summary>
    private void ReinitializeRinka(RoomEnemyPlace place, RinkaEnemyState state)
    {
        if (IsSpecialRinka(place))
            ReserveSpecialRinkaSpawn(place, state);

        // A special Rinka returns to its spawn point, the one it last reserved; an ordinary Rinka to its own placement.
        place.Position = place.Position.WithWhole(state.SpawnPoint is { } point
            ? RinkaSpawnReservations.Centre(point)
            : place.Spawn.Population.Position);
        InitializeRinkaLife(place, state);
    }

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RoomEnemyPlace place, RinkaEnemyState state)
    {
        state.Action = RinkaAction.WatchForLeavingViewport;
        state.DelayTimer = RinkaLifeDefinitions.AimDelay;
        state.Velocity = default;

        if (IsSpecialRinka(place))
        {
            if (_rinkaTerminationFlag)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                return;
            }
            state.Playback = new(RinkaAnimations.StartingWith(RinkaSegment.Special), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(RinkaAnimations.StartingWith(RinkaSegment.Ordinary), AnimationPlace.StartOfAnimation);
        }
        place.AnimationClock.Restart();
    }

    /// <summary>True while a door load still owes a special Rinka its spawn choice.</summary>
    public bool HasDeferredLoaderTimeCameraReads => _deferredRinkaSpawnPlaces.Count != 0;

    /// <summary>
    /// Runs the camera-dependent part of each deferred initialization whose place the door
    /// loader has reached, against that update's camera position.
    /// </summary>
    public void CompleteLoaderTimeCameraReads(
        Func<int, bool> loaderInitializedPlace,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(loaderInitializedPlace);
        while (_deferredRinkaSpawnPlaces.Count != 0 && loaderInitializedPlace(_deferredRinkaSpawnPlaces[0].Ordinal))
        {
            RoomEnemyPlace rinka = _deferredRinkaSpawnPlaces[0];
            _deferredRinkaSpawnPlaces.RemoveAt(0);
            RinkaEnemyState state = rinka.SpeciesState<RinkaEnemyState>() ?? throw new InvalidOperationException(
                $"Deferred Rinka spawn place {rinka.Ordinal} no longer holds a Rinka.");
            SetRinkaCamera(camera);
            ReserveSpecialRinkaSpawn(rinka, state);
        }
    }

    /// <summary>Runs a Rinka's main AI for one update through all four of its actions.</summary>
    private void RunRinkaMain(
        RoomEnemyPlace place,
        RinkaEnemyState state,
        SamusState samus)
    {
        if (_deferredRinkaSpawnPlaces.Count != 0)
            throw new InvalidOperationException(
                "A Rinka ran before the door loader chose every special Rinka's spawn point.");
        if (IsSpecialRinka(place) && _rinkaTerminationFlag)
        {
            ReleaseSpecialRinkaSpawn(state);
            RunRinkaDeathAnimation(place);
            return;
        }

        switch (state.Action)
        {
            case RinkaAction.WatchForLeavingViewport:
                if (RinkaIsOutsideFlightWindow(place))
                    RecycleRinkaAfterLeavingViewport(place, state);
                return;

            case RinkaAction.AimDelay:
                if (--state.DelayTimer >= 0)
                    return;
                state.Action = RinkaAction.Flying;

                // Special and ordinary branches have different source masks in code,
                // but both observable results make the Rinka tangible and set its processing property here.
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);

                TrigonometryAngle velocityAngle = EnemyMath.CalculateBearing(
                    new(CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole))).ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);
                state.Velocity = new(
                    EnemyMath.SineSpeed(RinkaLifeDefinitions.Speed, velocityAngle),
                    EnemyMath.SineSpeed(
                        RinkaLifeDefinitions.Speed,
                        velocityAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
                return;

            case RinkaAction.Flying:
                place.Position = place.Position.Advance(state.Velocity.ToSixteenSixteen());
                if (RinkaIsOutsideFlightWindow(place))
                    RecycleRinkaAfterLeavingViewport(place, state);
                return;

            case RinkaAction.DeathRespawnDelay:
                if (--state.DelayTimer >= 0)
                    return;
                place.Health = RinkaLifeDefinitions.RespawnHealth;
                ReinitializeRinka(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Rinka has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Runs the family-specific tail run after common touch, shot, or power-bomb damage.
    /// Common damage deliberately leaves lethal Rinkas alive long enough for this method to
    /// select their two distinct death lifecycles.
    /// </summary>
    private void ResolveRinkaCombatAfterCommon(RoomEnemyPlace place)
    {
        if (place.Health != 0)
            return;

        RinkaEnemyState state = place.RequireSpeciesState<RinkaEnemyState>();
        ReleaseSpecialRinkaSpawn(state);
        if (!IsSpecialRinka(place))
        {
            RunRinkaDeathAnimation(place);
            return;
        }

        // Special Mother Brain Rinkas do not clear their enemy place. They hide, emit the
        // room-graphics small explosion, wait for
        // the 1, 0, -1 countdown, restore ten health, and select a spawn.
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        SpawnRinkaDustExplosion(place.Position.Whole);
        state.Action = RinkaAction.DeathRespawnDelay;
        state.DelayTimer = 1;
    }

    /// <summary>Runs Rinka's private frozen-AI prelude/tail around common frozen handling.</summary>
    internal bool RunRinkaFrozenTail(RoomEnemyPlace place)
    {
        if (RinkaIsOutsideFlightWindow(place))
            place.FrozenTimer = 0;
        if (!_rinkaTerminationFlag)
            return false;

        ReleaseSpecialRinkaSpawn(place.RequireSpeciesState<RinkaEnemyState>());
        RunRinkaDeathAnimation(place);
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
        RoomEnemyPlace place,
        RinkaEnemyState state)
    {
        if (IsSpecialRinka(place))
        {
            ReleaseSpecialRinkaSpawn(state);
            if (_rinkaTerminationFlag)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                return;
            }
        }

        ReinitializeRinka(place, state);
    }

    /// <summary>
    /// Reserves a special Rinka's spawn point against the current camera, moving the Rinka to the point it reserves, which
    /// becomes its spawn point when it cannot keep its own.
    /// </summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemyPlace place, RinkaEnemyState state)
    {
        RinkaSpawnPoint own = state.SpawnPoint ?? throw new InvalidOperationException(
            $"Special Rinka {place.Ordinal} has no spawn point to reserve.");
        // Every point being held is a real failure path: the Rinka keeps its place and its spawn point.
        if (_spawnReservations.Reserve(own, _rinkaCamera) is not { } reserved)
            return;

        state.SpawnPoint = reserved;
        state.HeldSpawnPoint = reserved;
        place.Position = place.Position.WithWhole(RinkaSpawnReservations.Centre(reserved));
    }

    /// <summary>Releases the spawn point a special Rinka holds, if it holds one.</summary>
    private void ReleaseSpecialRinkaSpawn(RinkaEnemyState state)
    {
        if (state.HeldSpawnPoint is not { } held)
            return;

        _spawnReservations.Release(held);
        state.HeldSpawnPoint = null;
    }

    /// <summary>The Rinka's asymmetric (-16..271, -16..239) flight rectangle.</summary>
    private bool RinkaIsOutsideFlightWindow(RoomEnemyPlace place)
    {
        if (CoordinateAxis.AsSigned(place.Position.Y.Whole) < 0)
            return true;
        int relativeY = CoordinateAxis.Difference(place.Position.Y.Whole + 16, _rinkaCamera.Y);
        if (relativeY is < 0 or >= 256)
            return true;
        if (CoordinateAxis.AsSigned(place.Position.X.Whole) < 0)
            return true;
        int relativeX = CoordinateAxis.Difference(place.Position.X.Whole + 16, _rinkaCamera.X);
        return relativeX is < 0 or >= 288;
    }

    /// <summary>Distinguishes Mother Brain's parameterized spawn-sharing actors from ordinary room Rinkas.</summary>
    private static bool IsSpecialRinka(RoomEnemyPlace place) => place.SpawnSettings<RinkaSpawnSettings>().SharedSpawnPoint is not null;

    /// <summary>Runs the private lethal-health tail after common bomb damage without generic death.</summary>
    internal EnemyBombReaction RinkaBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // Rinka's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
    };

    /// <summary>Runs Rinka's main AI.</summary>
    internal void RunRinkaMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunRinkaMain(place, place.RequireSpeciesState<RinkaEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Rinka's typed animation.</summary>
    internal static void AdvanceRinkaTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, RinkaAnimations.Segments,
            static (RinkaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRinkaCue(place, state, cue);
                return null;
            });

    /// <summary>Skips invisible actors and applies the private death lifecycle after power-bomb damage.</summary>
    internal EnemyPowerBombReaction RinkaPowerBombReaction => field ??= new()
    {
        SkipsEnemy = enemy => enemy.Presence.HasAny(EnemyPresence.Hidden),
        ExemptFromGenericDeath = true,
        AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
    };

    /// <summary>Applies the private death tail after projectile damage but not immediate freezing.</summary>
    internal EnemyShotReaction RinkaShotReaction => field ??= new()
    {
        // Rinka's tail follows the damage path only; the immediate-freeze path skips it.
        ExemptFromGenericDeath = true,
        // Rinka's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (!c.Froze)
                ResolveRinkaCombatAfterCommon(c.Enemy);
        },
    };

    /// <summary>Applies normal contact damage before the Rinka-specific lethal-health lifecycle.</summary>
    internal void ReactToRinkaTouch(EnemyTouchContext context)
    {
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        ResolveRinkaCombatAfterCommon(context.Place);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        SetRinkaCamera(frame.Camera);
    }
    /// <summary>The room-graphics dust animation special Rinka deaths show.</summary>
    private const MiscDustAnimation RinkaDustAnimationIndex = MiscDustAnimation.SmallExplosion;
    /// <summary>Allocates the room-graphics small explosion.</summary>
    internal void SpawnRinkaDustExplosion(Position16<RoomPixels> position)
        => _projectiles.SpawnDustOrExplosion(position, RinkaDustAnimationIndex);

    /// <summary>
    /// Removes a dying Rinka in its small death explosion: an ordinary Rinka respawns from its population entry when the
    /// explosion ends, and a terminated special Rinka, which does not respawn, is gone.
    /// </summary>
    internal void RunRinkaDeathAnimation(RoomEnemyPlace place)
    {
        _deathsAndDrops.RecordKill(place);
        _deathsAndDrops.RemoveKilledEnemy(place, EnemyDeathExplosion.Small, respawns: place.Presence.HasAny(EnemyPresence.RespawnsIfKilled));
    }
}
