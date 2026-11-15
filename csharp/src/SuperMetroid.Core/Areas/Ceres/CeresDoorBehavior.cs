using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The room's owner of the Ceres station's doors and walls (#1953, #1989): it places each one's own actor, each its own
/// definition and sheet (an ordinary door, the door of Ridley's room, the elevator room's blocked door, the elevator
/// room's wall or a wall of Ridley's getaway chamber), and lends them the animations they share, the station's escape
/// quake and the room's services.
/// </summary>
[RoomLifetime]
internal sealed class CeresDoorBehavior : IEnemySpecies, IEnemyEntityFactory, IGameplayShadingSource
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [
        EnemyDefinitionId.CeresDoor, EnemyDefinitionId.CeresRidleyRoomDoor, EnemyDefinitionId.CeresElevatorDoor,
        EnemyDefinitionId.CeresElevatorRoomWall, EnemyDefinitionId.CeresGetawayChamberWall,
    ];

    /// <summary>The door or wall actor <paramref name="place"/>'s definition names, facing or standing where its placement says.</summary>
    /// <exception cref="InvalidDataException">The definition is no Ceres door or wall, or the placement names no facing or side.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId actor = Ensure.NotNull(place).DefinitionId;
        if (actor == EnemyDefinitionId.CeresDoor)
        {
            return new CeresDoorEntity(this, place, common, place.SpawnSettings<CeresDoorSpawnSettings>().Facing switch
            {
                HorizontalDirection.Right => CeresDoorSegment.DoorFacingRight,
                HorizontalDirection.Left => CeresDoorSegment.DoorFacingLeft,
                var other => throw new InvalidDataException($"No Ceres door faces {other}."),
            });
        }
        if (actor == EnemyDefinitionId.CeresRidleyRoomDoor)
            return new CeresRidleyRoomDoorEntity(this, place, common);
        if (actor == EnemyDefinitionId.CeresElevatorDoor)
            return new CeresElevatorDoorEntity(this, place, common);
        if (actor == EnemyDefinitionId.CeresElevatorRoomWall)
            return new CeresElevatorRoomWallEntity(this, place, common);
        if (actor == EnemyDefinitionId.CeresGetawayChamberWall)
        {
            return new CeresGetawayChamberWallEntity(this, place, common, place.SpawnSettings<CeresGetawayChamberWallSpawnSettings>().Side switch
            {
                HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
                HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
                var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
            });
        }
        throw new InvalidDataException($"{actor} is no Ceres door or wall.");
    }

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

    /// <summary>Provides Ceres room artwork, boss status, the floor and the gameplay RNG.</summary>
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
    /// The step of the elevator room's six-colour glow the elevator door cycles, which the room's pictures show as
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
    /// Starts <paramref name="place"/>'s door or wall in <paramref name="animation"/>, in the colours of the escape's stage.
    /// </summary>
    internal void Start(RoomEnemyPlace place, CeresDoorSegment animation)
    {
        Ensure.NotNull(place);
        place.Frame = EnemyFrameId.CeresDoorOrWallInitial;
        place.AnimationClock.Restart();
        place.SetSpeciesState(new CeresDoorAnimationState
        {
            Playback = new AnimationPlayback<CeresDoorSegment>(CeresDoorAnimations.StartingWith(animation), AnimationPlace.StartOfAnimation),
        });

        // The game loads the door's colours into one of two colour sets, the inactive door's or the active door's, whose
        // completed fade target this runtime shows directly: the escaping colours once Ridley has escaped, otherwise the
        // normal ones. The door shows them as its shade (#1512); which set held them is not kept.
        place.Shading = Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped
            ? CeresDoorShades.InEscapingColours
            : CeresDoorShades.InNormalColours;
    }

    /// <summary>Plays a door's or wall's typed animation in place of the shared enemy animation.</summary>
    internal void AdvanceAnimation(RoomEnemyPlace place, SamusState? samus)
    {
        Ensure.NotNull(place);
        if (!place.AnimationClock.Expire())
            return;
        CeresDoorAnimationState state = place.RequireSpeciesState<CeresDoorAnimationState>();
        AnimationPlayback<CeresDoorSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, CeresDoorAnimations.Segments, ref playback,
            cue => ApplyCue(place, samus, cue) is { } target
                ? new AnimationContinuation<CeresDoorSegment>(CeresDoorAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of a door's or wall's animation, returning the segment a branch continues at, or null to run on.</summary>
    private CeresDoorSegment? ApplyCue(RoomEnemyPlace place, SamusState? samus, CeresDoorCue cue)
    {
        switch (cue.Kind)
        {
            case CeresDoorCueKind.GoToIfSamusDistant:
            {
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres door proximity cue requires the active Samus actor.");
                }

                // The distance test subtracts the two positions, takes the
                // wrapped difference, then its distance independently
                // on each axis. If either distance is at least 48 pixels, the door branches.
                int xDistance = CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole);
                int yDistance = CoordinateAxis.Distance(place.Position.Y.Whole, samus.Position.Y.Whole);
                return xDistance >= 48 || yDistance >= 48 ? cue.Target : null;
            }
            case CeresDoorCueKind.GoToIfAreaBossAlive:
                // The test reads whether the current area's boss is saved
                // as defeated. Ceres begins with that flag clear, so the door of Ridley's room
                // loops as a tangible actor during Ridley's fight. Ridley's escape publishes
                // the boss's defeat together with Ceres's escape status; the next pass must then run on
                // past the branch to the intangible setup.
                // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                // X=8 and clipped Samus at X=29 after the getaway cutscene.
                return Room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss) ? null : cue.Target;
            case CeresDoorCueKind.GoToIfRidleyNotEscaped:
                return Escape.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ? null : cue.Target;
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
}
