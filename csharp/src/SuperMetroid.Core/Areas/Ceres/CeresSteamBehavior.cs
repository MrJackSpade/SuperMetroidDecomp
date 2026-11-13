using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres steam species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CeresSteamBehavior : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Steam];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CeresSteamEntity(this, slot, common);

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

    /// <summary>Plays a vent's typed animation where the shared enemy animation would run its list.</summary>
    internal static void AdvanceCeresSteamAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        CeresSteamEnemyState state = slot.RequireSpeciesState<CeresSteamEnemyState>();
        AnimationPlayback<CeresSteamSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, CeresSteamAnimations.Segments, ref playback,
            cue => ApplyCeresSteamCue(slot, cue) is { } target
                ? new AnimationContinuation<CeresSteamSegment>(CeresSteamAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of a vent's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static CeresSteamSegment? ApplyCeresSteamCue(RoomEnemySlot slot, CeresSteamCue cue)
    {
        switch (cue)
        {
            case CeresSteamCue.Command { Kind: CeresSteamCueKind.Vanish }:
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                return null;
            case CeresSteamCue.CountDownToActivation countDown:
            {
                // The vent's activation delay, which initialization staggers.
                CeresSteamEnemyState state = slot.RequireSpeciesState<CeresSteamEnemyState>();
                CountdownStep step = state.ActivationDelay.Decrement();
                state.ActivationDelay = step.Remaining;
                if (!step.IsZero)
                    return countDown.Target;
                slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                return countDown.Alternate;
            }
            case CeresSteamCue.Command { Kind: CeresSteamCueKind.Appear }:
                slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
                slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
                return null;
            default:
                throw new InvalidOperationException($"Undefined Ceres steam cue {cue}.");
        }
    }
    /// <summary>Health restored by steam's main and touch behaviours so the vent cannot be destroyed.</summary>
    private const int CeresSteamIndestructibleHealth = 32767;

    /// <summary>Initializes the Ceres steam.</summary>
    internal void InitializeCeresSteam(RoomEnemySlot slot)
    {
        CeresSteamInitialization initialization = CeresSteamInitializations.Initialization(
            slot.Spawn.Population.SpawnSettingsOf<CeresSteamSpawnSettings>().Variant);

        // Both property changes are literal ORs performed by the initializer: normal animation steps animate visibility,
        // while extended frames supply per-frame art and collision boxes whose geometry cannot be represented by definition radii.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.CollidesByFrameHitboxes = true;
        slot.AnimationClock.Restart();
        // The initializer selects the enemy projectiles' colours, which the import colours the steam's sheets in (#1512).

        // The initializer masks the live RNG to 0..31 and adds one. The delay is then
        // consumed by the vent's wait cue, giving each vent an authored stagger.
        var state = new CeresSteamEnemyState
        {
            ActivationDelay = WrappingCountdown.Start(_room.Random.Next(CeresSteamDefinitions.ActivationDelay) + 1),
            Playback = new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation),
        };
        slot.SetSpeciesState(state);

        state.Function = initialization.Function;
    }

    /// <summary>Runs the Ceres steam's main AI.</summary>
    private static void RunCeresSteamMain(
        RoomEnemySlot slot,
        TurningFloorTransform? floorTransform)
    {
        // Both main AI and touch AI restore this value. Steam is scenery/hazard state and
        // cannot be destroyed by a projectile that happened to overlap an active plume.
        slot.Health = CeresSteamIndestructibleHealth;

        CeresSteamEnemyState state = slot.RequireSpeciesState<CeresSteamEnemyState>();
        switch (state.Function)
        {
            // Parameters zero through three use a behaviour that does nothing. Their base
            // world position is also their draw position, so no drawing offset is changed.
            case CeresSteamFunction.None:
                return;

            // Parameters four and five are the vents inside the rotating elevator shaft.
            // The rotating-shaft behaviour calls the same transform used for Samus, then stores only
            // transformed-minus-base deltas as the enemy's drawing offsets.
            // Collision continues to use the untransformed world point; drawing consumes
            // these offsets later, when the enemy is drawn.
            case CeresSteamFunction.ApplyRotatingElevatorOffset:
                if (floorTransform is not TurningFloorTransform transform)
                {
                    throw new InvalidOperationException(
                        "Ceres steam elevator variant requires the turning floor's transform.");
                }

                Position16<RoomPixels> transformed = transform.Transform(slot.Position.Whole);
                slot.DrawOffset = transformed - slot.Position.Whole;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres steam function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Runs the Ceres steam's main AI.</summary>
    internal static void RunCeresSteamMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunCeresSteamMain(slot, frame.TurningFloor);
    }

    /// <summary>Registers contact handling only for steam's authored extended hitbox reaction.</summary>
    internal void AddCeresSteamTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Steam &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.CeresSteamTouch,
            context =>
            {
                // Every visible steam frame stores the steam's touch reaction in its authored hitbox.
                // The last two dispersal frames use an empty hitbox list and cannot reach
                // this branch. Insisting on both facts keeps collision tied to the authored geometry.
                if (!context.UsesFrameHitboxes || context.HitboxTouchAi != EnemyReaction.CeresSteamTouch)
                {
                    throw new InvalidDataException(
                        $"Ceres steam requires extended touch AI {EnemyReaction.CeresSteamTouch}; " +
                        $"extended={context.UsesFrameHitboxes}, selected={context.HitboxTouchAi?.ToString() ?? "(unnamed)"}, " +
                        $"frame={context.Slot.Frame}.");
                }

                // The steam's touch reaction restores health immediately before entering common touch AI.
                // Definition damage is deliberately zero: contact still installs the common
                // invincibility/knockback state, while Screw/Speed cannot destroy the vent.
                context.Slot.Health = CeresSteamIndestructibleHealth;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
            });
    }

    /// <summary>Routes steam shots to ordinary common shot AI without a species-specific reaction.</summary>
    internal static void AddCeresSteamShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Steam &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.Ignore));
    }

    /// <summary>The reaction of the first of the occupant's hitboxes the probe overlaps on the frame it shows.</summary>
    /// <exception cref="InvalidDataException">The frame shown has no compiled steam collision.</exception>
    internal static bool? FindHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // A steam frame's drawing details are not another component: only its
        // listed components damage Samus. Both damaging rectangles and
        // reactions remain fixed even if its installed sprite pieces are edited.
        foreach (CeresSteamCollisionComponent component in
                 CeresSteamCollisionDefinitions.Frames.Of(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyHitboxRectangle hitbox in
                     component.Hitboxes)
            {
                int left = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left);
                int top = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top);
                int right = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right);
                int bottom = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom);
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                reaction = probe.SelectsShot
                    ? CeresSteamCollisionDefinitions.ShotReaction
                    : CeresSteamCollisionDefinitions.TouchReaction;
                return true;
            }
        }
        return false;
    
    }
}
