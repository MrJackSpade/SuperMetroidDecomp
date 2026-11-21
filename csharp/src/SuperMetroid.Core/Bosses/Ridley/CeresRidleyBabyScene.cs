using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Baby Metroid Ceres Ridley carries: its fall, its animation and its picture.</summary>
[RoomLifetime]
internal sealed class CeresRidleyBabyScene
{
    /// <summary>Provides room randomness.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues the Baby's cries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Publishes the Baby's frames.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The room's view of Ridley: Ceres Ridley as he attached himself.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyBabyScene(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyPresentation presentation, RidleyEncounter ridleyEncounter)
    {
        _room = room;
        _audio = audio;
        _presentation = presentation;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>Keeps the Baby at Ridley's arm anchor, falls independently, then follows the grasp point on retrieval.</summary>
    /// <param name="place">Ridley's body, which carries the Baby.</param>
    /// <param name="baby">The Baby.</param>
    internal static void TickCeresBaby(RoomEnemyPlace place, CeresBaby baby) => baby.Action.Move(place, baby);

    /// <summary>
    /// Runs the Baby's animation at the enemy pass's end while Ridley's movement and animation are off (#2573). Until his
    /// reveal starts his animation, the Baby's animation, whose cries read the room's random number and queue a sound,
    /// runs as his enemy pass ends, before Samus's and the actors' drawing steps.
    /// </summary>
    public void StepCeresBabyWithEnemyPass()
    {
        if (_ridleyEncounter.CeresRidley is { } ceres && !ceres.Ridley.MovementAnimationEnabled)
            StepCeresBaby(ceres.Ridley);
    }

    /// <summary>
    /// Runs the Baby's animation after the actors' drawing steps once Ceres Ridley's reveal has started his animation
    /// (#2573), so its cries queue after the sounds Samus's drawing steps queue.
    /// </summary>
    public void StepCeresBabyAfterActors()
    {
        if (_ridleyEncounter.CeresRidley is { } ceres && ceres.Ridley.MovementAnimationEnabled)
            StepCeresBaby(ceres.Ridley);
    }

    /// <summary>Runs the Baby's animation one update while Ridley has not escaped with it.</summary>
    private void StepCeresBaby(CeresRidleyState ridley)
    {
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            AdvanceCeresBabyAnimation(ridley);
    }

    /// <summary>
    /// Shows the Baby's current frame where it is, at its picture's own depth among the actors and in the layering its phase
    /// gives it (<see cref="LayeringOf"/>, #2573), while Ridley has not escaped with it and once its animation has shown a
    /// pose. The Baby shows its own look of the hurt flash.
    /// </summary>
    public void PublishCeresBaby(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        if (_ridleyEncounter.CeresRidley is not { } ceres || _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped
            || CeresBabyFrame(ceres.Ridley.Baby.ShownPose) is not { } babyFrame)
            return;
        _presentation.PublishEncounterFrame(sprites, new SpriteSource(SpriteSourceKind.CeresBaby), babyFrame,
            ceres.Ridley.Baby.Position.Whole.RelativeToCamera(camera), EnemyShades.HurtFlashLook, layering: LayeringOf(ceres.Ridley));
    }

    /// <summary>
    /// How the Baby shows among the other actors (#2573): in front of them all, Ridley's dark shape among them, while
    /// Ridley's movement and animation are off before his reveal, and in its place among them once his reveal has started
    /// his animation.
    /// </summary>
    private static ActorLayering LayeringOf(CeresRidleyState ridley) => ridley.Motion switch
    {
        RidleyMotion.Running => ActorLayering.InDrawOrder,
        RidleyMotion.Still => ActorLayering.InFrontOfActors,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyMotion)} {ridley.Motion}."),
    };

    /// <summary>
    /// Runs the Baby's typed cutscene animation one update on the Baby's own clock (#2228): the pose it shows holds for its
    /// duration, counted off one each update, and the next pose is then shown.
    /// </summary>
    /// <param name="ridley">Ceres Ridley's encounter, which holds the Baby and the colours its cues show.</param>
    internal void AdvanceCeresBabyAnimation(CeresRidleyState ridley)
    {
        CeresBaby baby = ridley.Baby;
        if (!baby.Clock.Expire())
            return;

        Animation<CeresBabySegment> animation = baby.Playback.Animation;
        AnimationPlace place = baby.Playback.Place;
        AnimationShown<CeresBabySegment, CeresBabyPose> outcome = CeresBabyAnimations.Segments.PlayToNextFrame(ref animation, ref place,
            cue => ApplyCeresBabyCue(ridley, cue) is { } target
                ? new AnimationContinuation<CeresBabySegment>(CeresBabyAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        baby.Playback = new AnimationPlayback<CeresBabySegment>(animation, place);
        if (outcome is not AnimationShown<CeresBabySegment, CeresBabyPose>.Frame shown)
            throw new InvalidOperationException($"The Ceres Baby's animation ended an update with {outcome}.");
        baby.Clock.Show(shown.Duration);
        baby.ShownPose = shown.Shown;
    }

    /// <summary>The frame the Baby shows for <paramref name="pose"/>, or none before the Baby's first pose.</summary>
    private static EnemyFrameId? CeresBabyFrame(CeresBabyPose? pose) =>
        pose is { } shown ? CeresBabyFrameDefinitions.ShownFrames.Of(shown) : null;

    /// <summary>The animation the Baby starts while it moves, where it reacts to its motion (#2167).</summary>
    internal const CeresBabySegment MovingAnimation = CeresBabySegment.ExpressiveLoop;

    /// <summary>The animation the Baby starts when it rests silently instead of crying (#2167).</summary>
    internal const CeresBabySegment SilentAnimation = CeresBabySegment.IdleSquish;

    /// <summary>Carries out one cue of the Baby's animation, returning the segment whose animation its owner starts, or null to play on.</summary>
    private CeresBabySegment? ApplyCeresBabyCue(CeresRidleyState ridley, CeresBabyCue cue)
    {
        switch (cue)
        {
            case CeresBabyCue.Cries:
                // While the Baby is falling it always cries and plays on. Otherwise the current random number, read without
                // drawing a new one, makes it jump silently one time in two.
                if (ridley.Baby.VerticalVelocity.IsZero && RidleyRandomDefinitions.CeresBabySilentJump.IsTakenBy(_room.Random.Read()))
                    return SilentAnimation;
                _audio.QueueEnemySound(RoomSound.CeresBabyMetroidCry, priority: SoundRequestPriority.Normal);
                return null;
            case CeresBabyCue.UpdateColors updateColors:
                ridley.Colours.ShowBaby(updateColors.Shade);
                return null;
            case CeresBabyCue.ReactsToMotion:
                return !ridley.Baby.VerticalVelocity.IsZero ? MovingAnimation : null;
            default:
                throw new InvalidOperationException($"Undefined Ceres Baby cue {cue}.");
        }
    }
}
