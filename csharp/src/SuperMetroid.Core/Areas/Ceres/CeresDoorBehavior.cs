using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres.Artwork;

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
        _earthquake = dependencies.Earthquake;
        EscapeQuake = new CeresEscapeQuake(dependencies.Room, dependencies.Earthquake, dependencies.CeresEscape);
    }

    /// <summary>The room's quake, which shakes the overlay of Ridley's room's door sideways.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>The door of Ceres Ridley's room once it has initialized; null in any other room.</summary>
    private CeresRidleyRoomDoor? _ridleyRoomDoor;

    /// <summary>The door of Ceres Ridley's room: its place and the animation that sets how it shows.</summary>
    /// <param name="Place">The place the door occupies.</param>
    /// <param name="Animation">The door's animation, whose cues set its look.</param>
    private sealed record CeresRidleyRoomDoor(RoomEnemyPlace Place, CeresDoorAnimationState Animation);

    /// <summary>
    /// Starts <paramref name="place"/>'s door as the door of Ceres Ridley's room: the room's Ceres doors show its overlay
    /// while its look is the overlay (#2573).
    /// </summary>
    internal CeresDoorAnimationState StartRidleyRoomDoor(RoomEnemyPlace place)
    {
        CeresDoorAnimationState animation = Start(place, CeresDoorSegment.RidleyRoomDoor);
        _ridleyRoomDoor = new CeresRidleyRoomDoor(place, animation);
        return animation;
    }

    /// <summary>Starts <paramref name="place"/>'s door as the elevator room's blocked door, which alone shows the door's own look (#1406, #1538).</summary>
    internal CeresDoorAnimationState StartElevatorDoor(RoomEnemyPlace place)
    {
        CeresDoorAnimationState animation = Start(place, CeresDoorSegment.ElevatorDoor);
        Presentation.ShowLook(CeresElevatorDoorLookDefinitions.OverlayLook);
        return animation;
    }

    /// <summary>
    /// Shows the door of Ridley's room as its fixed overlay picture while its look is the overlay (#2573): hidden as an
    /// actor, the door shows the overlay at its place, shaken sideways by the room's quake, in the overlay's own standard
    /// colours, which the door actor's looks never reach (#1512).
    /// </summary>
    internal void PublishRidleyRoomDoorOverlay(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        if (_ridleyRoomDoor is not { } door || door.Animation.Look != CeresRidleyRoomDoorLook.Overlay)
            return;
        int quakeXOffset = CeresDoorQuake.XOffset(_earthquake.RemainingFrames);
        Presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresDoorOverlay), CeresDoorFrameDefinitions.RidleyRoomDoorOverlay,
            (door.Place.Position.Whole + new Offset16<RoomPixels>(quakeXOffset, 0)).RelativeToCamera(camera), PictureShading.Standard);
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
    internal CeresDoorAnimationState Start(RoomEnemyPlace place, CeresDoorSegment animation)
    {
        Ensure.NotNull(place);
        place.AnimationClock.Restart();
        var state = new CeresDoorAnimationState
        {
            Playback = new AnimationPlayback<CeresDoorSegment>(CeresDoorAnimations.StartingWith(animation), AnimationPlace.StartOfAnimation),
        };

        // A door shows the escaping colours once Ridley has escaped, otherwise the normal ones, each as its fade ends
        // (#1512).
        place.Shading = Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped
            ? CeresDoorShades.InEscapingColours
            : CeresDoorShades.InNormalColours;
        return state;
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
                // The door of Ridley's room waits while the area's boss is undefeated, as he is when Ceres begins, so it
                // loops as a tangible actor during Ridley's fight. Ridley's escape records the boss's defeat together with
                // Ceres's escape status; the next pass then runs on past the wait and makes the door intangible.
                return Room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss) ? null : RidleyRoomDoorWaitingAnimation;
            case CeresDoorCueKind.ElevatorRoomWallAppears:
                return Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ? null : ElevatorRoomWallDoorAnimation;
            case CeresDoorCueKind.MakeIntangible:
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.MakeTangible:
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return null;
            case CeresDoorCueKind.ShowOverlay:
                place.RequireSpeciesState<CeresDoorAnimationState>().Look = CeresRidleyRoomDoorLook.Overlay;
                return null;
            case CeresDoorCueKind.MakeInvisible:
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                return null;
            case CeresDoorCueKind.MakeVisibleAndShowActor:
                place.RequireSpeciesState<CeresDoorAnimationState>().Look = CeresRidleyRoomDoorLook.Actor;
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
