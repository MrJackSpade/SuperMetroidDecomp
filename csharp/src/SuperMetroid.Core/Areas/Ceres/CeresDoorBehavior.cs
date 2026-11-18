using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The room's owner of the Ceres station's doors and walls (#1953, #1989, #2426): it lends every door and wall the
/// animations they share, the station's escape quake and the room's services. Each is placed as its own actor, each its own
/// definition and sheet: an ordinary door, the door of Ridley's room, the elevator room's wall and a wall of Ridley's
/// getaway chamber by <see cref="CeresDoorFactory"/>, and the elevator room's blocked door, which a room with a turning
/// floor places, by <see cref="CeresElevatorDoorFactory"/>.
/// </summary>
[RoomLifetime]
internal sealed class CeresDoorBehavior : IEnemySpecies, IGameplayShadingSource
{

    /// <summary>Captures the room systems the Ceres doors and walls use.</summary>
    internal CeresDoorBehavior(CeresDoorBehaviorDependencies dependencies)
    {
        Ensure.NotNull(dependencies);
        Room = dependencies.Room;
        Audio = dependencies.Audio;
        Presentation = dependencies.Presentation;
        Projectiles = dependencies.Projectiles;
        Escape = dependencies.CeresEscape;
        RidleyScene = dependencies.RidleyScene;
        EscapeQuake = new CeresEscapeQuake(dependencies.Room, dependencies.Earthquake, dependencies.CeresEscape);
    }

    /// <summary>Provides Ceres room artwork, boss status and the gameplay RNG.</summary>
    internal EnemyRoomBinding Room { get; }

    /// <summary>Queues the doors' opening sound and the elevator door's explosions.</summary>
    internal EnemyAudioQueue Audio { get; }

    /// <summary>Shows the elevator door's own look over the doors' ordinary artwork.</summary>
    internal EnemyPresentation Presentation { get; }

    /// <summary>Spawns the elevator door's smoke and explosions.</summary>
    internal EnemyProjectileEngine Projectiles { get; }

    /// <summary>Supplies Ridley's escape status and receives the elevator door's destruction.</summary>
    internal ICeresEscapeStatus Escape { get; }

    /// <summary>Ceres Ridley's scene, which the door of his room attaches itself to.</summary>
    internal ICeresRidleyScene RidleyScene { get; }

    /// <summary>The quake every door and the elevator room's wall keeps up while the station self-destructs.</summary>
    internal CeresEscapeQuake EscapeQuake { get; }

    /// <summary>
    /// The step of the elevator room's glow the elevator door cycles, which the room's pictures show as
    /// <see cref="CeresDoorShades.ElevatorRoomGlow"/> (#1512); null before the door first cycles it.
    /// </summary>
    internal int? ElevatorRoomGlowStep { get; set; }

    /// <summary>Lays the elevator room's glow step, once the door has cycled one, over the room's pictures (#1512).</summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (ElevatorRoomGlowStep is int step)
            frame.Shade(GameplayPictures.Room, CeresDoorShades.ElevatorRoomGlow(step));
    }

    /// <summary>
    /// Starts <paramref name="place"/>'s door or wall in <paramref name="animation"/>, in the colours of the escape's stage:
    /// the animation's first step, on the next update, shows its own first picture (#2424).
    /// </summary>
    internal void Start(RoomEnemyPlace place, CeresDoorSegment animation)
    {
        Ensure.NotNull(place);
        place.AnimationClock.Restart();
        place.SetSpeciesState(new CeresDoorAnimationState
        {
            Playback = new AnimationPlayback<CeresDoorSegment>(CeresDoorAnimations.StartingWith(animation), AnimationPlace.StartOfAnimation),
        });

        // A door shows the escaping colours once Ridley has escaped, otherwise the normal ones, each as its fade ends
        // (#1512).
        place.Shading = Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped
            ? CeresDoorShades.InEscapingColours
            : CeresDoorShades.InNormalColours;
    }

    /// <summary>Plays a door's or wall's typed animation in place of the shared enemy animation.</summary>
    internal void AdvanceAnimation(RoomEnemyPlace place, SamusState samus)
    {
        Ensure.NotNull(place);
        if (!place.AnimationClock.Expire())
            return;
        CeresDoorAnimationState state = place.RequireSpeciesState<CeresDoorAnimationState>();
        AnimationPlayback<CeresDoorSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, CeresDoorAnimations.Segments, ref playback,
            cue => ApplyCue(place, samus, cue) is { } started
                ? new AnimationContinuation<CeresDoorSegment>(CeresDoorAnimations.StartingWith(started), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>The animation a right-facing door starts, standing closed, when it appears with Samus away (#2189).</summary>
    internal const CeresDoorSegment RightDoorClosedAnimation = CeresDoorSegment.ClosedFacingRight;

    /// <summary>The animation a left-facing door starts, standing closed, when it appears with Samus away (#2189).</summary>
    internal const CeresDoorSegment LeftDoorClosedAnimation = CeresDoorSegment.ClosedFacingLeft;

    /// <summary>The animation an open right-facing door starts, closing, once Samus is away (#2189).</summary>
    internal const CeresDoorSegment RightDoorClosingAnimation = CeresDoorSegment.CloseFacingRight;

    /// <summary>The animation an open left-facing door starts, closing, once Samus is away (#2189).</summary>
    internal const CeresDoorSegment LeftDoorClosingAnimation = CeresDoorSegment.CloseFacingLeft;

    /// <summary>The animation a closed right-facing door starts again, waiting closed, while Samus is away (#2189).</summary>
    internal const CeresDoorSegment RightDoorWaitingAnimation = CeresDoorSegment.ClosedFacingRightWait;

    /// <summary>The animation a closed left-facing door starts again, waiting closed, while Samus is away (#2189).</summary>
    internal const CeresDoorSegment LeftDoorWaitingAnimation = CeresDoorSegment.ClosedFacingLeftWait;

    /// <summary>The animation the door of Ridley's room starts again while the area boss is alive (#2189).</summary>
    internal const CeresDoorSegment RidleyRoomDoorWaitingAnimation = CeresDoorSegment.RidleyRoomDoorWait;

    /// <summary>The animation the elevator room's wall starts, as a left-facing door, until Ceres Ridley has escaped (#2189).</summary>
    internal const CeresDoorSegment ElevatorRoomWallDoorAnimation = CeresDoorSegment.DoorFacingLeft;

    /// <summary>
    /// Carries out one cue of a door's or wall's animation, returning the segment whose animation the owner starts on a waiting
    /// door's or wall's event, or null to run on.
    /// </summary>
    private CeresDoorSegment? ApplyCue(RoomEnemyPlace place, SamusState samus, CeresDoorCue cue)
    {
        switch (cue.Kind)
        {
            case CeresDoorCueKind.RightDoorAppears:
                return IsSamusAway(place, samus) ? RightDoorClosedAnimation : null;
            case CeresDoorCueKind.LeftDoorAppears:
                return IsSamusAway(place, samus) ? LeftDoorClosedAnimation : null;
            case CeresDoorCueKind.RightDoorStandsOpen:
                return IsSamusAway(place, samus) ? RightDoorClosingAnimation : null;
            case CeresDoorCueKind.LeftDoorStandsOpen:
                return IsSamusAway(place, samus) ? LeftDoorClosingAnimation : null;
            case CeresDoorCueKind.RightDoorStandsClosed:
                return IsSamusAway(place, samus) ? RightDoorWaitingAnimation : null;
            case CeresDoorCueKind.LeftDoorStandsClosed:
                return IsSamusAway(place, samus) ? LeftDoorWaitingAnimation : null;
            case CeresDoorCueKind.RidleyRoomDoorWaits:
                // The test reads whether the current area's boss is saved
                // as defeated. Ceres begins with that flag clear, so the door of Ridley's room
                // loops as a tangible actor during Ridley's fight. Ridley's escape publishes
                // the boss's defeat together with Ceres's escape status; the next pass then runs on
                // past the wait and makes the door intangible.
                return Room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss) ? null : RidleyRoomDoorWaitingAnimation;
            case CeresDoorCueKind.ElevatorRoomWallAppears:
                return Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ? null : ElevatorRoomWallDoorAnimation;
            case CeresDoorCueKind.MakeIntangible:
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.MakeTangible:
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.HandDrawingToRidley:
                place.RequireSpeciesState<CeresRidleyRoomDoorState>().HandsDrawingToRidley = true;
                return null;
            case CeresDoorCueKind.MakeInvisible:
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.MakeVisibleAndTakeDrawingBack:
                place.RequireSpeciesState<CeresRidleyRoomDoorState>().HandsDrawingToRidley = false;
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.MakeVisible:
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.PlayOpeningSound:
                Audio.QueueEnemySound(RoomSound.CeresDoorOpening, priority: SoundRequestPriority.Normal);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDoorCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Whether Samus is away from the door: distant from it along either axis.</summary>
    /// <remarks>Ceres's rooms are a few screens across, so Samus and a door are never more than a room apart.</remarks>
    private static bool IsSamusAway(RoomEnemyPlace place, SamusState samus)
    {
        int xDistance = Math.Abs(place.Position.X.Whole - samus.Position.X.Whole);
        int yDistance = Math.Abs(place.Position.Y.Whole - samus.Position.Y.Whole);
        return xDistance >= CeresDoorApproachDefinitions.DistantPixels || yDistance >= CeresDoorApproachDefinitions.DistantPixels;
    }
}
