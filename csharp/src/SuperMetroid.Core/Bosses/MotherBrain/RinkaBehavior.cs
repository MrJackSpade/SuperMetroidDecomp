using SuperMetroid.Core.Assets;
﻿using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new RinkaEntity(this, slot, common);

    /// <summary>Applies normal Samus contact damage before the Rinka-specific death tail.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Indicates when door loading must defer camera-sensitive spawn selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Allocates Rinka dust and death explosion projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Installs the ordinary Rinka's respawn placeholder after death.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects Rinka AI to collision, frame context, and death effects.</summary>
    internal RinkaBehavior(EnemyCollision collision, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyDeathsAndDrops deathsAndDrops)
    {
        _collision = collision;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Countdown before a newly fired Rinka aims at Samus.</summary>
    private const int RinkaInitialDelay = 26;
    /// <summary>Health restored to a special Rinka after its death delay.</summary>
    private const int RinkaHealth = 10;
    /// <summary>Flight speed in 1/256 pixels per update (1 + 32/256 pixels), resolved along its aim angle into flight velocity.</summary>
    private static readonly SignedEightEight RinkaSpeed = SignedEightEight.FromParts(1, 32);
    /// <summary>
    /// The spawn points Mother Brain's three special Rinkas compete for; a Rinka whose own point is off screen or held
    /// moves to another and rewrites its spawn snapshot.
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
    private readonly List<RoomEnemySlot> _deferredRinkaSpawnSlots = new();

    /// <summary>
    /// Captures camera coordinates used by the Rinka's flight and spawn checks. StepFrame refreshes these before
    /// any Rinka AI; Load supplies the already-positioned destination camera for initialization.
    /// </summary>
    internal void SetRinkaCamera(Position16<RoomPixels> camera)
    {
        _rinkaCamera = camera;
    }

    /// <summary>Sets a Rinka up when the room spawns it.</summary>
    internal void InitializeRinka(RoomEnemySlot slot)
    {
        var state = new RinkaEnemyState();
        slot.SetSpeciesState(state);

        // Both branches first clear the four lifecycle/collision bits. The special
        // Mother Brain variant is always processed off-screen but never uses generic death
        // respawn; ordinary room Rinkas retain the respawn property so their death explosion resurrects
        // the enemy in its place in the room's listing from its population snapshot.
        slot.Presence = slot.Presence.Without(EnemyPresence.RespawnsIfKilled);
        slot.Processing = slot.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
        if (IsSpecialRinka(slot))
        {
            // The spawn selection tests its spawn points against the foreground. A door's loader runs this
            // while the door transition is still scrolling, so that choice waits for the camera
            // of the update in which the loader reaches this slot.
            if (_frameContext.DeferLoaderTimeCameraReads)
                _deferredRinkaSpawnSlots.Add(slot);
            else
                ReserveSpecialRinkaSpawn(slot, state);
            slot.Processing = slot.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
            slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        }
        else
        {
            slot.Presence = slot.Presence.With(EnemyPresence.RespawnsIfKilled);
            slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
            slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        }

        // Both ordinary and Mother Brain variants draw in the Rinka's own colours, which every room placing
        // Rinkas draws them in: their variant's standard colours (#1512, #1599).
        slot.Shading = PictureShading.Standard;
        InitializeRinkaLife(slot, state);
    }

    /// <summary>Re-initializes a Rinka, covering both of its later setup paths.</summary>
    private void ReinitializeRinka(RoomEnemySlot slot, RinkaEnemyState state)
    {
        if (IsSpecialRinka(slot))
            ReserveSpecialRinkaSpawn(slot, state);

        // The population entry is separate and survives clearing the common
        // actor. Special spawn selection deliberately mutates that snapshot; ordinary
        // Rinkas continue to return to their original population coordinate.
        slot.Position = slot.Position.WithWhole(new(slot.Spawn.Population.Position.X, slot.Spawn.Population.Position.Y));
        InitializeRinkaLife(slot, state);
    }

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RoomEnemySlot slot, RinkaEnemyState state)
    {
        state.Action = RinkaAction.WatchForLeavingViewport;
        state.DelayTimer = RinkaInitialDelay;
        state.Velocity = default;

        if (IsSpecialRinka(slot))
        {
            if (_rinkaTerminationFlag)
            {
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                return;
            }
            state.Playback = new(RinkaAnimations.StartingWith(RinkaSegment.Special), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(RinkaAnimations.StartingWith(RinkaSegment.Ordinary), AnimationPlace.StartOfAnimation);
        }
        slot.AnimationClock.Restart();
    }

    /// <summary>True while a door load still owes a special Rinka its spawn choice.</summary>
    public bool HasDeferredLoaderTimeCameraReads => _deferredRinkaSpawnSlots.Count != 0;

    /// <summary>
    /// Runs the camera-dependent part of each deferred initialization whose slot the door
    /// loader has reached, against that update's camera position.
    /// </summary>
    public void CompleteLoaderTimeCameraReads(
        Func<int, bool> loaderInitializedSlot,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(loaderInitializedSlot);
        while (_deferredRinkaSpawnSlots.Count != 0 && loaderInitializedSlot(_deferredRinkaSpawnSlots[0].SlotIndex))
        {
            RoomEnemySlot rinka = _deferredRinkaSpawnSlots[0];
            _deferredRinkaSpawnSlots.RemoveAt(0);
            RinkaEnemyState state = rinka.SpeciesState<RinkaEnemyState>() ?? throw new InvalidOperationException(
                $"Deferred Rinka spawn slot {rinka.SlotIndex} no longer holds a Rinka.");
            SetRinkaCamera(camera);
            ReserveSpecialRinkaSpawn(rinka, state);
        }
    }

    /// <summary>Runs a Rinka's per-frame behaviour through all four of its actions.</summary>
    private void RunRinkaMain(
        RoomEnemySlot slot,
        RinkaEnemyState state,
        SamusState? samus)
    {
        if (_deferredRinkaSpawnSlots.Count != 0)
            throw new InvalidOperationException(
                "A Rinka ran before the door loader chose every special Rinka's spawn point.");
        if (IsSpecialRinka(slot) && _rinkaTerminationFlag)
        {
            ReleaseSpecialRinkaSpawn(state);
            RunRinkaDeathAnimation(slot);
            return;
        }

        switch (state.Action)
        {
            case RinkaAction.WatchForLeavingViewport:
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaAction.AimDelay:
                if (--state.DelayTimer >= 0)
                    return;
                if (samus is null)
                    throw new InvalidOperationException("Rinka homing requires the active Samus actor.");

                state.Action = RinkaAction.Flying;

                // Special and ordinary branches have different source masks in code,
                // but both observable results make the Rinka tangible and set its processing property here.
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);

                TrigonometryAngle velocityAngle = EnemyMath.CalculateBearing(
                    new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToTableAngle().TurnedBy(TrigonometryAngle.QuarterTurn);
                state.Velocity = new(
                    EnemyMath.MultiplySineSample(RinkaSpeed, velocityAngle),
                    EnemyMath.MultiplySineSample(
                        RinkaSpeed,
                        velocityAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
                return;

            case RinkaAction.Flying:
                slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaAction.DeathRespawnDelay:
                if (--state.DelayTimer >= 0)
                    return;
                slot.Health = RinkaHealth;
                ReinitializeRinka(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Rinka has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>
    /// Runs the family-specific tail run after common touch, shot, or power-bomb damage.
    /// Common damage deliberately leaves lethal Rinkas alive long enough for this method to
    /// select their two distinct death lifecycles.
    /// </summary>
    private void ResolveRinkaCombatAfterCommon(RoomEnemySlot slot)
    {
        if (slot.Health != 0)
            return;

        RinkaEnemyState state = slot.RequireSpeciesState<RinkaEnemyState>();
        ReleaseSpecialRinkaSpawn(state);
        if (!IsSpecialRinka(slot))
        {
            RunRinkaDeathAnimation(slot);
            return;
        }

        // Special Mother Brain Rinkas do not clear their enemy slot. They hide, emit the
        // room-graphics small explosion, wait for
        // the 1, 0, -1 countdown, restore ten health, and select a spawn.
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        SpawnRinkaDustExplosion(slot.Position.Whole);
        state.Action = RinkaAction.DeathRespawnDelay;
        state.DelayTimer = 1;
    }

    /// <summary>Runs Rinka's private frozen-AI prelude/tail around common frozen handling.</summary>
    internal bool RunRinkaFrozenTail(RoomEnemySlot slot)
    {
        if (RinkaIsOutsideFlightWindow(slot))
            slot.FrozenTimer = 0;
        if (!_rinkaTerminationFlag)
            return false;

        ReleaseSpecialRinkaSpawn(slot.RequireSpeciesState<RinkaEnemyState>());
        RunRinkaDeathAnimation(slot);
        return true;
    }

    /// <summary>Carries out one event of the Rinkas' animations: hiding, hiding with off-screen processing, and firing.</summary>
    private static void ApplyRinkaCue(RoomEnemySlot slot, RinkaEnemyState state, RinkaCue cue)
    {
        switch (cue.Kind)
        {
            case RinkaCueKind.Hide:
                slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                break;
            case RinkaCueKind.HideAndProcessOffScreen:
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
                slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                break;
            case RinkaCueKind.Fire:
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
                state.Action = RinkaAction.AimDelay;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RinkaCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Releases a special spawn resource and restarts its life unless the room has terminated it.</summary>
    private void RecycleRinkaAfterLeavingViewport(
        RoomEnemySlot slot,
        RinkaEnemyState state)
    {
        if (IsSpecialRinka(slot))
        {
            ReleaseSpecialRinkaSpawn(state);
            if (_rinkaTerminationFlag)
            {
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                return;
            }
        }

        ReinitializeRinka(slot, state);
    }

    /// <summary>
    /// Reserves a special Rinka's spawn point against the current camera, moving the Rinka and its spawn snapshot to the
    /// point when it cannot keep its own.
    /// </summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemySlot slot, RinkaEnemyState state)
    {
        // Every point being held is a real failure path: the Rinka keeps its place and whatever point it held.
        if (_spawnReservations.Reserve(slot.Spawn.Population.Position, _rinkaCamera) is not { } reservation)
            return;

        if (reservation.MovesTo is { } centre)
        {
            RoomEnemyPlacement rewrittenPopulation = slot.Spawn.Population with
            {
                Position = centre,
            };
            slot.Spawn = slot.Spawn with { Population = rewrittenPopulation };
            slot.Position = slot.Position.WithWhole(centre);
        }
        state.HeldSpawnPoint = reservation.Reserved;
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
    private bool RinkaIsOutsideFlightWindow(RoomEnemySlot slot)
    {
        if (CoordinateAxis.AsSigned(slot.Position.Y.Whole) < 0)
            return true;
        int relativeY = CoordinateAxis.Difference(slot.Position.Y.Whole + 16, _rinkaCamera.Y);
        if (relativeY is < 0 or >= 256)
            return true;
        if (CoordinateAxis.AsSigned(slot.Position.X.Whole) < 0)
            return true;
        int relativeX = CoordinateAxis.Difference(slot.Position.X.Whole + 16, _rinkaCamera.X);
        return relativeX is < 0 or >= 288;
    }

    /// <summary>Distinguishes Mother Brain's parameterized spawn-sharing actors from ordinary room Rinkas.</summary>
    private static bool IsSpecialRinka(RoomEnemySlot slot) => slot.Spawn.Population.SpawnSettingsOf<RinkaSpawnSettings>().Placement == RinkaPlacement.FreeSpawnPoint;

    /// <summary>Runs the private lethal-health tail after common bomb damage without generic death.</summary>
    internal EnemyBombReaction RinkaBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // Rinka's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
    };

    /// <summary>Runs Rinka's main AI.</summary>
    internal void RunRinkaMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRinkaMain(slot, slot.RequireSpeciesState<RinkaEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Rinka's typed animation.</summary>
    internal static void AdvanceRinkaTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, RinkaAnimations.Segments,
            static (RinkaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyRinkaCue(slot, state, cue);
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
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
        ResolveRinkaCombatAfterCommon(context.Slot);
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
        => _projectiles.SpawnRoomGraphicsDustExplosion(position, RinkaDustAnimationIndex);

    /// <summary>Runs a Rinka's death animation.</summary>
    internal void RunRinkaDeathAnimation(RoomEnemySlot slot)
    {
        _deathsAndDrops.RecordKill(slot);
        bool respawns = slot.Presence.HasAny(EnemyPresence.RespawnsIfKilled);
        RoomEnemySpawnSnapshot survivingSpawnSnapshot = slot.Spawn;

        // The death explosion draws from the room's shared sheet in its standard colours. When every place is held,
        // the spawn is lost, and with it both the effect and the only future respawn cue.
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                _deathsAndDrops.DeathProjectiles,
                graphics: default);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.DropDefinitionId = slot.DefinitionId;
            projectile.KilledEnemy = KilledEnemy.Of(slot, respawns);
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallExplosion));
        }

        slot.Clear();

        // Clearing the enemy leaves its spawn data in place, which the respawn cue rebuilds
        // the Rinka from; a special Rinka that does not respawn keeps it as every cleared enemy does.
        slot.Spawn = survivingSpawnSnapshot;
        if (respawns)
        {
            slot.AwaitRespawn();
        }
    }
}
