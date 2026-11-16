using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres steam species owner: its per-room state, behaviour and the interactions it registers.</summary>
/// <remarks>
/// The steam cannot be destroyed (#2019): its definition takes no damage from any weapon or contact
/// (<see cref="EnemyVulnerabilityDefinitions.Invulnerable"/>) and withholds its plume from shots, so its health never
/// leaves the definition's.
/// </remarks>
[RoomLifetime]
internal sealed class CeresSteamBehavior : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CeresSteam];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new CeresSteamEntity(this, place, common);

    /// <summary>Room services that supply the live RNG used to stagger vent activation.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Common contact resolution used after steam's extended hitbox selects its touch reaction.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Stores room and collision services for steam initialization and contact handling.</summary>
    internal CeresSteamBehavior(EnemyRoomBinding room, EnemyCollision collision)
    {
        _room = room;
        _collision = collision;
    }

    /// <summary>Plays a vent's typed animation in place of the shared enemy animation.</summary>
    internal static void AdvanceCeresSteamAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        CeresSteamEnemyState state = place.RequireSpeciesState<CeresSteamEnemyState>();
        AnimationPlayback<CeresSteamSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, CeresSteamAnimations.Segments, ref playback,
            cue => ApplyCeresSteamCue(place, cue) is { } target
                ? new AnimationContinuation<CeresSteamSegment>(CeresSteamAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of a vent's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static CeresSteamSegment? ApplyCeresSteamCue(RoomEnemyPlace place, CeresSteamCue cue)
    {
        switch (cue)
        {
            case CeresSteamCue.Signal { Kind: CeresSteamCueKind.Vanish }:
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                return null;
            case CeresSteamCue.CountDownToActivation countDown:
            {
                // The vent's activation delay, which initialization staggers.
                CeresSteamEnemyState state = place.RequireSpeciesState<CeresSteamEnemyState>();
                CountdownStep step = state.ActivationDelay.Decrement();
                state.ActivationDelay = step.Remaining;
                if (!step.IsZero)
                    return countDown.Dormant;
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return countDown.Puffing;
            }
            case CeresSteamCue.Signal { Kind: CeresSteamCueKind.Appear }:
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return null;
            default:
                throw new InvalidOperationException($"Undefined Ceres steam cue {cue}.");
        }
    }

    /// <summary>Initializes the Ceres steam.</summary>
    internal void InitializeCeresSteam(RoomEnemyPlace place)
    {
        CeresSteamInitialization initialization = CeresSteamInitializations.Initialization(
            place.Spawn.Population.SpawnSettingsOf<CeresSteamSpawnSettings>().Variant);

        // The vent animates, which shows and hides its plume, and collides by its frames' plume rectangles, which no
        // radius can describe.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.CollidesByFrameHitboxes = true;
        place.AnimationClock.Restart();
        // The initializer selects the enemy projectiles' colours, which the import colours the steam's sheets in (#1512).

        // Each vent waits a random 1 to 32 updates before its first plume, counted down by its animation's wait, so the
        // vents puff out of step.
        var state = new CeresSteamEnemyState(initialization.Action)
        {
            ActivationDelay = WrappingCountdown.Start(_room.Random.Next(CeresSteamDefinitions.ActivationDelay) + 1),
            Playback = new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation),
        };
        place.SetSpeciesState(state);

    }

    /// <summary>Runs the Ceres steam's main AI.</summary>
    private static void RunCeresSteamMain(
        RoomEnemyPlace place,
        TurningFloorTransform? floorTransform)
    {
        CeresSteamEnemyState state = place.RequireSpeciesState<CeresSteamEnemyState>();
        switch (state.Action)
        {
            // The up, left, down and right vents hold their place. Their base world position is also their draw
            // position, so no drawing offset is changed.
            case CeresSteamAction.HoldsItsPlace:
                return;

            // The rotating-elevator vents sit inside the rotating elevator shaft.
            // The rotating-shaft behaviour calls the same transform used for Samus, then stores only
            // transformed-minus-base deltas as the enemy's drawing offsets.
            // Collision continues to use the untransformed world point; drawing consumes
            // these offsets later, when the enemy is drawn.
            case CeresSteamAction.ApplyRotatingElevatorOffset:
                if (floorTransform is not TurningFloorTransform transform)
                {
                    throw new InvalidOperationException(
                        "Ceres steam elevator variant requires the turning floor's transform.");
                }

                Position16<RoomPixels> transformed = transform.Transform(place.Position.Whole);
                place.DrawOffset = transformed - place.Position.Whole;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres steam has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the Ceres steam's main AI.</summary>
    internal static void RunCeresSteamMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunCeresSteamMain(place, frame.TurningFloor);
    }

    /// <summary>Handles contact only for the steam's authored extended hitbox reaction.</summary>
    internal void ReactToCeresSteamTouch(EnemyTouchContext context)
    {
        // Every visible steam frame stores the steam's touch reaction in its authored hitbox.
        // The last two dispersal frames have no plume and cannot reach
        // this branch. Insisting on both facts keeps collision tied to the authored geometry.
        if (!context.UsesFrameHitboxes || context.HitboxTouch != CeresSteamReactions.Touch)
        {
            throw new InvalidDataException(
                $"Ceres steam requires extended touch AI {CeresSteamReactions.Touch}; " +
                $"extended={context.UsesFrameHitboxes}, selected={context.HitboxTouch?.ToString() ?? "(unnamed)"}, " +
                $"frame={context.Place.Frame}.");
        }

        // Definition damage is zero: contact still installs the common invincibility and knockback, while the steam's
        // definition takes no damage from any weapon or contact, so the Screw Attack or the Speed Booster cannot destroy it.
        _collision.ResolveNormalEnemyTouch(context);
    }

    /// <summary>The reaction of the plume rectangle the frame shown has, when the probe overlaps it.</summary>
    /// <exception cref="InvalidDataException">The frame shown is not one of the steam's frames.</exception>
    internal static bool? FindHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // Only the frame's plume rectangle damages Samus, whatever its installed picture shows; a hidden plume has none.
        if (enemy.Frame is not { } frame || !CeresSteamCollisionDefinitions.Plumes.TryGetValue(frame, out EnemyHitboxRectangle? plume))
            throw new InvalidDataException($"Ceres steam frame {enemy.Frame} has no plume collision.");
        if (plume is not { } rectangle)
            return false;

        // The probe compares the edges by their signed distances, so an edge past the room's edge needs no wrapping.
        Position16<RoomPixels> vent = enemy.Position.Whole;
        if (!probe.Overlaps(vent.X + rectangle.Left, vent.Y + rectangle.Top, vent.X + rectangle.Right, vent.Y + rectangle.Bottom))
            return false;
        reaction = probe.SelectsShot
            ? CeresSteamCollisionDefinitions.ShotReaction
            : CeresSteamCollisionDefinitions.TouchReaction;
        return true;
    }
}
