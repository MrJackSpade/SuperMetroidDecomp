using SuperMetroid.Core.Assets;
﻿using SuperMetroid.Core.Programs;
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
    /// <summary>Flight speed in 8.8 pixels per update (1 + 32/256 pixels), resolved along its aim angle into flight velocity.</summary>
    private static readonly SignedEightEight RinkaSpeed = SignedEightEight.FromParts(1, 32);
    /// <summary>Number of Mother Brain Rinka actors sharing the eleven spawn resources.</summary>
    private const int RinkaMaximumSpecialActors = 3;

    /// <summary>
    /// The eleven authored Japan/USA spawn points. Mother Brain's
    /// three special actors compete for these resources and may rewrite their spawn snapshots
    /// when their authored point is outside the current screen. The even reservation token is
    /// derived from each point's position in this table.
    /// </summary>
    private static readonly Position16<RoomPixels>[] RinkaSpawnPoints =
    [
        new(999, 38),
        new(999, 166),
        new(823, 54),
        new(823, 166),
        new(631, 28),
        new(631, 182),
        new(439, 54),
        new(439, 166),
        new(247, 28),
        new(247, 182),
        new(128, 168),
    ];

    /// <summary>Pairs an authored spawn point with its one-based even reservation token.</summary>
    internal static RinkaSpawnResource RinkaSpawnResourceAt(int index) =>
        new(RinkaSpawnPoints[index], 2 * (index + 1));

    /// <summary>Marks which of the eleven authored spawn points are currently reserved.</summary>
    /// <remarks>Eleven literal table entries.</remarks>
    private readonly bool[] _rinkaOccupiedSpawnResources =
        new bool[RinkaMaximumSpecialActors + 8];
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
        // the physical slot from its population snapshot.
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

        // The spawn record is separate and survives clearing the common
        // actor. Special spawn selection deliberately mutates that snapshot; ordinary
        // Rinkas continue to return to their original population coordinate.
        slot.Position = slot.Position.WithWhole(new(slot.Spawn.Population.Position.X, slot.Spawn.Population.Position.Y));
        InitializeRinkaLife(slot, state);
    }

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RoomEnemySlot slot, RinkaEnemyState state)
    {
        state.Function = RinkaEnemyFunction.WatchForLeavingViewport;
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

    /// <summary>Runs a Rinka's per-frame behaviour through all four of its functions.</summary>
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

        switch (state.Function)
        {
            case RinkaEnemyFunction.WatchForLeavingViewport:
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaEnemyFunction.AimDelay:
                if (--state.DelayTimer >= 0)
                    return;
                if (samus is null)
                    throw new InvalidOperationException("Rinka homing requires the active Samus actor.");

                state.Function = RinkaEnemyFunction.Flying;

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

            case RinkaEnemyFunction.Flying:
                slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaEnemyFunction.DeathRespawnDelay:
                if (--state.DelayTimer >= 0)
                    return;
                slot.Health = RinkaHealth;
                ReinitializeRinka(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Rinka function {state.Function?.ToString() ?? "none"} is not translated.");
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
        // room-graphics dust/explosion with parameter three, wait for
        // the 1, 0, -1 countdown, restore ten health, and select a spawn.
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        SpawnRinkaDustExplosion(slot.Position.Whole);
        state.Function = RinkaEnemyFunction.DeathRespawnDelay;
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
                state.Function = RinkaEnemyFunction.AimDelay;
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

    /// <summary>Runs the special Rinka's fixed-resource spawn selection without memory alias tricks.</summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemySlot slot, RinkaEnemyState state)
    {
        Position16<RoomPixels> spawn = slot.Spawn.Population.Position;
        int mappedIndex = FindRinkaSpawnResource(spawn);
        bool mustRelocate = RinkaSpawnIsOutsideCamera(spawn) ||
            _rinkaOccupiedSpawnResources[mappedIndex];

        if (mustRelocate)
        {
            // The first pass prefers a free point on the current camera. If all eleven are
            // off-camera or occupied, code performs a second pass that ignores the
            // camera but still refuses to steal an occupied resource.
            int selectedIndex = -1;
            for (int index = 0; index < RinkaSpawnPoints.Length; index++)
            {
                RinkaSpawnResource candidate = RinkaSpawnResourceAt(index);
                if (!RinkaSpawnIsOutsideCamera(candidate.Position) &&
                    !_rinkaOccupiedSpawnResources[index])
                {
                    selectedIndex = index;
                    break;
                }
            }
            if (selectedIndex < 0)
            {
                for (int index = 0; index < RinkaSpawnPoints.Length; index++)
                {
                    if (!_rinkaOccupiedSpawnResources[index])
                    {
                        selectedIndex = index;
                        break;
                    }
                }
            }

            // All eleven resources being occupied is a real failure path: the reservation
            // returns without inventing a twelfth location or overwriting another owner.
            if (selectedIndex < 0)
                return;

            mappedIndex = selectedIndex;
            RinkaSpawnResource selected = RinkaSpawnResourceAt(selectedIndex);
            RoomEnemyPopulationRecord rewrittenPopulation = slot.Spawn.Population with
            {
                Position = selected.Position,
            };
            slot.Spawn = slot.Spawn with { Population = rewrittenPopulation };
            slot.Position = slot.Position.WithWhole(selected.Position);
        }

        _rinkaOccupiedSpawnResources[mappedIndex] = true;
        state.SpawnResourceToken = RinkaSpawnResourceAt(mappedIndex).Token;
    }

    /// <summary>Finds the authored point for a population position, using resource zero for an unknown point.</summary>
    private static int FindRinkaSpawnResource(Position16<RoomPixels> position)
    {
        for (int index = 0; index < RinkaSpawnPoints.Length; index++)
        {
            RinkaSpawnResource resource = RinkaSpawnResourceAt(index);
            if (resource.Position == position)
                return index;
        }

        // The location lookup falls back to the first table resource when a population coordinate does
        // not match. Preserve that odd behavior instead of rejecting a synthetic fixture.
        return 0;
    }

    /// <summary>Clears the occupied spawn entry identified by a special Rinka's even resource token.</summary>
    private void ReleaseSpecialRinkaSpawn(RinkaEnemyState state)
    {
        if (state.SpawnResourceToken == 0)
            return;

        for (int index = 0; index < RinkaSpawnPoints.Length; index++)
        {
            if (RinkaSpawnResourceAt(index).Token == state.SpawnResourceToken)
            {
                _rinkaOccupiedSpawnResources[index] = false;
                break;
            }
        }
        state.SpawnResourceToken = 0;
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

    /// <summary>The strict camera-origin test used only for special spawns.</summary>
    private bool RinkaSpawnIsOutsideCamera(Position16<RoomPixels> position)
    {
        if (CoordinateAxis.AsSigned(position.Y) < 0 ||
            CoordinateAxis.Difference(position.Y, _rinkaCamera.Y) is < 0 or >= 224)
        {
            return true;
        }
        return CoordinateAxis.Difference(position.X, _rinkaCamera.X) is < 0 or >= 256;
    }

    /// <summary>Distinguishes Mother Brain's parameterized spawn-sharing actors from ordinary room Rinkas.</summary>
    private static bool IsSpecialRinka(RoomEnemySlot slot) => slot.Spawn.Population.SpawnSettingsOf<RinkaSpawnSettings>().Placement == RinkaPlacement.FreeSpawnPoint;

    /// <summary>Runs the private lethal-health tail after common bomb damage without generic death.</summary>
    internal void AddRinkaBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Rinka &&
                                                       c.SelectedShotAi == EnemyReaction.RinkaShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
        });
    }

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
    internal void AddRinkaPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Rinka &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.RinkaPowerBomb)
        {
            SkipsEnemy = enemy => enemy.Presence.HasAny(EnemyPresence.Hidden),
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, _) => ResolveRinkaCombatAfterCommon(enemy),
        });
    }

    /// <summary>Applies the private death tail after projectile damage but not immediate freezing.</summary>
    internal void AddRinkaShotReactions(EnemySharedReactions table)
    {
        // Rinka's tail follows the damage path only; the immediate-freeze path skips it.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Rinka &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.RinkaShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = c =>
            {
                if (!c.Froze)
                    ResolveRinkaCombatAfterCommon(c.Enemy);
            },
        });
    }

    /// <summary>Applies normal contact damage before the Rinka-specific lethal-health lifecycle.</summary>
    internal void AddRinkaTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Rinka &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.RinkaTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveRinkaCombatAfterCommon(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        SetRinkaCamera(frame.Camera);
    }
    /// <summary>Room-graphics dust definition parameter used by special Rinka deaths.</summary>
    private const MiscDustAnimation RinkaDustAnimationIndex = MiscDustAnimation.SmallExplosion;
    /// <summary>Allocates the room-graphics dust explosion with parameter three.</summary>
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
            MotherBrainProjectileStarts.Initialize(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphics: default);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.DropDefinitionId = slot.DefinitionId;
            projectile.KilledEnemy = KilledEnemy.Of(slot, respawns);
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallExplosion));
        }

        slot.Clear();

        // Clearing the enemy record leaves its spawn data in place, which the respawn cue rebuilds
        // the Rinka from; a special Rinka that does not respawn keeps it as every cleared record does.
        slot.Spawn = survivingSpawnSnapshot;
        if (respawns)
        {
            _deathsAndDrops.InstallRespawnPlaceholder(slot);
        }
    }

    /// <summary>Runs the strict 256x256 camera cull for dust/explosion actors.</summary>
    internal static void CullMiscDustOutsideCamera(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        // The cull compares the position against camera + 256 and culls when not below it, so
        // a projectile exactly 256 past the camera is already off-screen.
        if (CoordinateAxis.Difference(projectile.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.Difference(projectile.Position.X.Whole, (camera.X + 256)) >= 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, (camera.Y + 256)) >= 0)
        {
            projectile.Delete();
        }
    }

    /// <summary>Attaches the camera cull to miscellaneous dust and explosion projectiles.</summary>
    internal static void AddMiscDustFrameBehavior(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            CullMiscDustOutsideCamera(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.DustCameraCull);
    }

    /// <summary>Runs the Rinka viewport and termination check before common frozen AI.</summary>
    internal void AddRinkaFrozenPreemption(EnemySharedReactions table) =>
        table.AddFrozenPreemption(slot => slot.DefinitionId == EnemyDefinitionId.Rinka, RunRinkaFrozenTail);
}
