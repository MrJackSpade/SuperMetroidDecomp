using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy animation player (#1624): advances each animating enemy's typed animation through its entity, and shows
/// the body frames bosses show on their bodies.
/// </summary>
[RoomLifetime]
internal sealed class EnemyAnimationPlayer
{
    /// <summary>Holds the body the room's boss shows its parts' frames on.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Keeps the presentation the cues it carries out change.</summary>
    internal EnemyAnimationPlayer(EnemyPresentation presentation) => _presentation = presentation;

    /// <summary>
    /// Shows the body frame the already resolved visual frame shows on its boss's body (#1466, #1515, #1540), on the update its
    /// cue selects a new frame, and on any later update the body still shows another of the part's frames (#2573): from
    /// then on it is the frame the place's part of the boss shows, so the body always shows the part's current picture.
    /// The common enemy drawing draws its sprite half, which may be empty for a pose shown only on the body. Selecting a
    /// pose with no body frame leaves the part's frame as it was.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed body sheet lacks the frame.</exception>
    /// <exception cref="InvalidOperationException">
    /// The frame is shown before its boss attached the body it shows on, or by a part its boss does not draw there.
    /// </exception>
    internal static void DrawBossBodyFrame(RoomEnemyPlace place, EnemyFrameId selected)
    {
        if (place.Entity is not IBossBodyFrames boss || boss.FindBodyFrame(selected) is not { } frame)
            return;
        // Each part shows its frames on its own boss's body; Mother Brain's shows only once her second phase begins.
        BossBodyParts body = boss.BodyParts;
        if (!place.AnimationClock.ShowsNewFrame && body.ShowsSelected(place.DefinitionId, selected))
            return;
        body.ShowSelected(place.DefinitionId, selected, frame);
    }

    /// <summary>
    /// Advances one enemy's typed animation (#641). Every enemy whose processing animates it has a typed animation; one
    /// without fails here.
    /// </summary>
    internal static void AdvanceAnimation(
        RoomEnemyPlace place,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera,
        GameplayAction controllerInput)
    {
        if (place.Entity is not IEnemyTypedAnimation animation)
        {
            throw new InvalidDataException(
                $"Enemy {place.DefinitionId} animates but has no typed animation.");
        }
        animation.AdvanceTypedAnimation(new EnemyAnimationCueContext
            { Samus = samus, Level = level, Camera = camera, ControllerInput = controllerInput });
    }
}
