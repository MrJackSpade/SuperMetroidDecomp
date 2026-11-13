using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy animation player: common cues, owner-private cue dispatch,
/// visual selectors and mechanics, the artwork animations show, and the body frames bosses show on their bodies.
/// </summary>
[RoomLifetime]
internal sealed class EnemyInstructionInterpreter
{
    /// <summary>Holds the body the room's boss shows its parts' frames on.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Captures the presentation for cue execution.</summary>
    internal EnemyInstructionInterpreter(EnemyPresentation presentation) => _presentation = presentation;

    /// <summary>
    /// Shows the body frame the already resolved visual frame shows on its boss's body (#1466, #1515, #1540), on the update its
    /// cue selects a new frame: from then on it is the frame the slot's part of the boss shows. The common writer draws
    /// its sprite half, which may be empty for a pose shown only on the body. Selecting a pose with no body frame leaves
    /// the part's frame as it was.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed body sheet lacks the frame.</exception>
    /// <exception cref="InvalidOperationException">
    /// The frame is shown before its boss attached the body it shows on, or by a part its boss does not draw there.
    /// </exception>
    internal void DrawBossBodyFrame(RoomEnemySlot slot, EnemyFrameId selected)
    {
        if ((slot.Entity as IBossBodyFrames)?.FindBodyFrame(selected) is not { } frame)
            return;
        if (!slot.AnimationClock.ShowsNewFrame)
            return;
        // Every boss that shows a body attaches it at initialization; Mother Brain's shows only once her second phase begins.
        (_presentation.BossBody ?? throw new InvalidOperationException("A boss body frame was shown before its owner attached the body it shows on."))
            .Show(slot.DefinitionId, frame);
    }

    /// <summary>
    /// Advances one enemy's typed animation (#641). Every enemy that runs an animation script runs a typed program; one
    /// without is a missing conversion and fails here.
    /// </summary>
    internal static void RunsAnimationScript(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera,
        ControllerButton controllerInput)
    {
        if ((slot.Entity as IEnemyTypedAnimation)?.FindTypedAnimation() is not { } advance)
        {
            throw new InvalidDataException(
                $"Enemy {slot.DefinitionId} processes instructions but has no typed animation.");
        }
        advance(slot, new EnemyInstructionContext
            { Samus = samus, Level = level, Camera = camera, ControllerInput = controllerInput });
    }
}
