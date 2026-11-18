using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Ceres's steam vents: each puffs its plume after its own random wait, and the plume touches Samus by the rectangle its
/// frame shows.
/// </summary>
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
    /// <summary>
    /// The motion of the vents a room places in its turning elevator shaft: riding the shaft's turn, in the one room whose
    /// scope has that turn; none in every other room, which places no such vent.
    /// </summary>
    private readonly CeresSteamMotion.RidesShaftTurn? _shaftVents;

    /// <summary>Stores room and collision services for steam initialization and contact handling.</summary>
    /// <param name="room">The room's services, whose random numbers stagger the vents.</param>
    /// <param name="collision">The common contact resolution.</param>
    /// <param name="shaftTurn">The turn of the elevator shaft the room is, or null for a room that is no turning shaft.</param>
    internal CeresSteamBehavior(EnemyRoomBinding room, EnemyCollision collision, CeresElevatorShaftRoomMainState? shaftTurn)
    {
        _room = room;
        _collision = collision;
        _shaftVents = shaftTurn is { } shaft ? new CeresSteamMotion.RidesShaftTurn(shaft) : null;
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
                if (--state.UpdatesToActivation != 0)
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

        // Each vent waits a random 1 to 32 updates before its first plume, counted down by its animation's wait, so the
        // vents puff out of step.
        var state = new CeresSteamEnemyState(MotionOf(initialization.Action))
        {
            UpdatesToActivation = _room.Random.Next(CeresSteamDefinitions.ActivationDelay) + 1,
            Playback = new AnimationPlayback<CeresSteamSegment>(CeresSteamAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation),
        };
        place.SetSpeciesState(state);

    }

    /// <summary>
    /// The motion <paramref name="action"/> gives a vent: the up, left, down and right vents hold their place, and the
    /// rotating-elevator vents ride the turn of the shaft they stand in.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A rotating-elevator vent is placed in a room that is no turning elevator shaft, or the action is undefined.
    /// </exception>
    private CeresSteamMotion MotionOf(CeresSteamAction action) => action switch
    {
        CeresSteamAction.HoldsItsPlace => CeresSteamMotion.HoldsItsPlace.Instance,
        CeresSteamAction.ApplyRotatingElevatorOffset => _shaftVents
            ?? throw new InvalidDataException("A rotating-elevator Ceres steam vent is placed in a room that is no turning elevator shaft."),
        _ => throw new InvalidDataException($"Ceres steam has no behaviour for action {action}."),
    };

    /// <summary>Runs the Ceres steam's main AI: the vent's drawing follows its motion.</summary>
    internal static void RunCeresSteamMainAi(RoomEnemyPlace place) =>
        place.RequireSpeciesState<CeresSteamEnemyState>().Motion.Move(place);

    /// <summary>Handles contact only for the steam's authored extended hitbox reaction.</summary>
    internal void ReactToCeresSteamTouch(EnemyTouchContext context)
    {
        // Every visible steam frame stores the steam's touch reaction in its authored hitbox.
        // The last two dispersal frames have no plume and touch nothing. Steam touches Samus only where a frame's
        // authored hitbox carries the touch reaction.
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
