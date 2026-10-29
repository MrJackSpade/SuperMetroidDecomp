using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy instruction interpreter: common bank-$A0 instructions, owner-private instruction dispatch,
/// visual selectors and mechanics words, instruction VRAM transfers, and the body frames bosses draw on their body layer.
/// </summary>
[RoomLifetime]
internal sealed class EnemyInstructionInterpreter
{
    /// <summary>The body canvases bosses assemble besides the shown one, which a body frame is also drawn into.</summary>
    private readonly IBossBodyCopies _bossCopies;
    /// <summary>Holds the body layer the room's boss shows.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Captures the presentation and the bosses' body canvases for instruction execution.</summary>
    internal EnemyInstructionInterpreter(IBossBodyCopies bossCopies, EnemyPresentation presentation)
    {
        _bossCopies = bossCopies;
        _presentation = presentation;
    }

    /// <summary>
    /// Draws the body frame the already resolved visual frame draws on its boss's body layer (#1466), on the update its
    /// instruction selects a new frame. The common writer draws its sprite half, which may be empty for a pose drawn only
    /// on the body layer. Selecting a pose with no body frame draws nothing; it does not invent a clear.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed body sheet lacks the frame.</exception>
    /// <exception cref="InvalidOperationException">The frame is drawn before its boss attached the body layer it draws on.</exception>
    internal void DrawBossBodyFrame(RoomEnemySlot slot, EnemyFrameId selected)
    {
        if ((slot.Entity as IBossBodyFrames)?.FindBodyFrame(selected) is not { } frame)
            return;
        if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
            return;
        _bossCopies.Draw(frame);
        // A boss that shows a body layer draws into it; every owner attaches it at initialization. Mother Brain only
        // stages her body until her phase-two transition shows it (#1356).
        if (_presentation.BossBody is { } body)
            body.Draw(frame);
        else if (!_bossCopies.StagesBody)
            throw new InvalidOperationException("A boss body frame was drawn before its owner attached the body layer it draws on.");
    }

    /// <summary>
    /// Advances one enemy's typed animation (#641). Every enemy that processes instructions runs a typed program; one
    /// without is a missing conversion and fails here rather than reading cartridge instruction lists.
    /// </summary>
    internal static void ProcessInstructions(
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
