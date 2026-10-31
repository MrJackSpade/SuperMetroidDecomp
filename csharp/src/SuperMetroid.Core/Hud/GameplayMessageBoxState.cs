using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The gameplay message box, advanced one update at a time. Every permanent item pickup, station and save prompt shows
/// its message through this one box; the message changes what is shown, not how the box behaves.
/// </summary>
/// <remarks>
/// <para>
/// The box pauses gameplay, opens a window around Y=124, holds the message for a fixed interval, waits for input, and
/// closes the same window before gameplay resumes. What it shows is a <see cref="ShownMessage"/>: the message, the
/// button its panel names and the highlighted save choice, whose installed image the backend draws.
/// </para>
/// <para>
/// This type handles the ordinary messages and the gunship's save confirmation. On YES the gunship branch keeps gameplay
/// paused through the saving sound and the completion notice.
/// </para>
/// </remarks>
/// <param name="saveChoiceInput">When an open save confirmation reads the controller; the session creates it.</param>
public sealed class GameplayMessageBoxState(SaveChoiceInputCadence saveChoiceInput)
{
    /// <summary>When an open save confirmation reads the controller.</summary>
    private readonly SaveChoiceInputCadence _saveChoiceInput = Ensure.NotNull(saveChoiceInput);

    /// <summary>Preserves controller edges across the box's own controller reads.</summary>
    private readonly ControllerInputState _controller = new();
    /// <summary>Radius to publish on the next opening frame, after the current frame's window is drawn.</summary>
    private int _nextOpeningRadiusPixels;
    /// <summary>Radius to publish on the next closing frame.</summary>
    private int _nextClosingRadiusPixels;
    /// <summary>YES or NO selected before the confirmation window starts closing.</summary>
    private bool? _closingConfirmationResult;
    /// <summary>Keeps an accepted gunship save result through its saving sound and completion notice.</summary>
    private bool _gunshipCompletion;
    /// <summary>One-shot request for the sound queued by the accepted gunship save branch.</summary>
    private bool _savingSoundRequested;
    /// <summary>Remaining frames before the gunship's SAVE COMPLETED notice reopens the box.</summary>
    private int _savingFramesRemaining;
    /// <summary>Updates left in the pause before the window opens or after it closes.</summary>
    private int _pauseUpdatesRemaining;

    /// <summary>The audio processing and per-line effect advance of the update <see cref="Step"/> just ran.</summary>
    [field: NonSerialized]
    public MessageBoxFrameAudio LastFrameAudio { get; private set; }
    /// <summary>Current Shoot binding used when panel artwork includes a button glyph.</summary>
    private ControllerButton _shootBinding = ControllerButton.X;
    /// <summary>Current Run binding used when panel artwork includes a button glyph.</summary>
    private ControllerButton _runBinding = ControllerButton.B;

    /// <summary>Consumes the accepted gunship save's one-shot saving sound, independently of gameplay publication.</summary>
    public bool ConsumeSavingSoundRequest()
    {
        bool requested = _savingSoundRequested;
        _savingSoundRequested = false;
        return requested;
    }

    /// <summary>Whether the active message uses the two-choice save cursor and edge-based input polling.</summary>
    private bool IsSaveConfirmation => MessageId is GameplayMessageId.SaveConfirmation or
        GameplayMessageId.GunshipSaveConfirmation;

    /// <summary>Whether the box currently pauses gameplay and owns the message window.</summary>
    public bool IsActive => Phase != GameplayMessageBoxPhase.Inactive;

    /// <summary>The message the box shows, or <see cref="GameplayMessageId.None"/> while none has begun.</summary>
    public GameplayMessageId MessageId { get; private set; }

    /// <summary>Current coroutine segment, exposed so frame-by-frame debugging is useful.</summary>
    public GameplayMessageBoxPhase Phase { get; private set; }

    /// <summary>
    /// Half-height of the scanline window visible in the most recently completed frame.
    /// The radius grows and shrinks by exactly two pixels per update.
    /// </summary>
    public int RadiusPixels { get; private set; }

    /// <summary>Unelapsed mandatory frames before controller input is inspected.</summary>
    public int MinimumDisplayFramesRemaining { get; private set; }

    /// <summary>
    /// What the box shows while a message has begun (#1466): the message, the button its panel names for the configured
    /// shoot or run binding, and the save choice a confirmation highlights; null while no message has begun.
    /// </summary>
    public ShownMessage? Shown => MessageId == GameplayMessageId.None ? null
        : new ShownMessage(MessageId, PanelButton(MessageId), IsSaveConfirmation ? ConfirmationSelectionYes : null);

    /// <summary>Current yes/no choice for save confirmation message.</summary>
    public bool ConfirmationSelectionYes { get; private set; } = true;

    /// <summary>
    /// Result published after confirmation has completely closed. Consumers clear it
    /// through <see cref="ConsumeConfirmationResult"/> at the suspended PLM return seam.
    /// </summary>
    public bool? CompletedConfirmationResult { get; private set; }

    /// <summary>
    /// True only on the update that shows a moved save choice. The runtime
    /// consumes this to queue the cartridge's menu-move sound through the ordinary audio
    /// owner instead of giving the message renderer an unrelated sound dependency.
    /// </summary>
    public bool ConfirmationSelectionChangedThisFrame { get; private set; }

    /// <summary>
    /// Starts one ordinary gameplay message with its authored layout.
    /// Bindings are parameters because the options menu may remap them; their defaults are
    /// the words installed by <c>NewSaveFile</c>.
    /// </summary>
    /// <param name="messageId">The ordinary message or the gunship save confirmation to show.</param>
    /// <param name="controllerRead">
    /// The controller held at the update's last controller read. While the box is shown only
    /// the box itself reads the controller, so the save choice's presses are relative to this.
    /// </param>
    /// <param name="shootBinding">Current binding for Shoot, whose button the applicable item panels name.</param>
    /// <param name="runBinding">Current binding for Run, whose button the applicable item panels name.</param>
    /// <param name="sourceContext">Caller identity included in unsupported-message diagnostics; does not affect message timing or presentation.</param>
    public void Begin(
        GameplayMessageId messageId,
        ControllerButton controllerRead,
        ControllerButton shootBinding = ControllerButton.X,
        ControllerButton runBinding = ControllerButton.B,
        string sourceContext = nameof(GameplayMessageBoxState))
    {
        if (!IsOrdinary(messageId))
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageId),
                messageId,
                $"Gameplay message {messageId} from {sourceContext} is not translated by the ordinary coroutine.");
        }
        if (IsActive)
            throw new InvalidOperationException("A gameplay message box is already active.");

        _shootBinding = shootBinding;
        _runBinding = runBinding;
        MessageId = messageId;
        RadiusPixels = 0;
        MinimumDisplayFramesRemaining = 0;
        _nextOpeningRadiusPixels = 0;
        _nextClosingRadiusPixels = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels;
        _closingConfirmationResult = null;
        _gunshipCompletion = false;
        CompletedConfirmationResult = null;
        ConfirmationSelectionYes = true;
        _controller.Latch(controllerRead);
        // The pause's first update is the gameplay update that opened the box.
        BeginPreparing(GameplayMessageBoxDefinitions.Timing.OpeningPauseUpdates - 1);
    }

    /// <summary>Enters the pause before the window opens, with <paramref name="pauseUpdates"/> updates of it left.</summary>
    private void BeginPreparing(int pauseUpdates)
    {
        _pauseUpdatesRemaining = pauseUpdates;
        Phase = GameplayMessageBoxPhase.Preparing;
    }

    /// <summary>Consumes the completed result of the save confirmation.</summary>
    public bool? ConsumeConfirmationResult()
    {
        bool? result = CompletedConfirmationResult;
        CompletedConfirmationResult = null;
        return result;
    }

    /// <summary>
    /// Runs one update of the box. Gameplay does not run and does not read the controller while the box is shown, so
    /// only the box's own reads see input.
    /// </summary>
    public void Step(ControllerButton controllerInput)
    {
        ConfirmationSelectionChangedThisFrame = false;
        // Each update of the box processes music and sound once, unless noted below.
        LastFrameAudio = MessageBoxFrameAudio.MusicAndSounds;

        switch (Phase)
        {
            case GameplayMessageBoxPhase.Inactive:
                LastFrameAudio = default;
                return;

            case GameplayMessageBoxPhase.Preparing:
                // The first update of the screen preparation processes no audio.
                if (_pauseUpdatesRemaining == GameplayMessageBoxDefinitions.Timing.OpeningPauseUpdates - 1)
                    LastFrameAudio = default;
                if (--_pauseUpdatesRemaining == 0)
                    Phase = GameplayMessageBoxPhase.Opening;
                return;

            case GameplayMessageBoxPhase.Restoring:
                LastFrameAudio = _pauseUpdatesRemaining switch
                {
                    // Restoring the screen takes two updates: the first processes no
                    // audio, the second advances the per-line effects and processes music
                    // and sound.
                    3 => default,
                    2 => MessageBoxFrameAudio.MusicAndSounds with { AdvancesPerLineEffects = true },
                    // The box returns to gameplay, whose own sound processing follows.
                    0 => MessageBoxFrameAudio.MusicAndSounds with { SoundHandlerCalls = 2 },
                    _ => MessageBoxFrameAudio.MusicAndSounds,
                };
                if (_pauseUpdatesRemaining-- == 0)
                    Finish();
                return;

            case GameplayMessageBoxPhase.Opening:
                // Each opening update shows the current radius before it grows. Radius
                // zero is consequently a real update, even though it shows nothing.
                RadiusPixels = _nextOpeningRadiusPixels;
                if (RadiusPixels == GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
                {
                    if (IsSaveConfirmation)
                    {
                        MinimumDisplayFramesRemaining = 0;
                        _saveChoiceInput.Begin();
                        Phase = GameplayMessageBoxPhase.AwaitingInput;
                    }
                    else
                    {
                        // The four completion notices hold for only ten updates;
                        // equipment descriptions hold for 360. Treating
                        // every ordinary message as equipment left Samus locked in a map
                        // station while its access-loop sound continued for six seconds.
                        MinimumDisplayFramesRemaining = MessageId is
                            GameplayMessageIds.MapDataAccessCompleted or
                            GameplayMessageIds.EnergyRechargeCompleted or
                            GameplayMessageIds.MissileRechargeCompleted or
                            GameplayMessageIds.SaveCompleted
                            ? GameplayMessageBoxDefinitions.Timing.StationMinimumDisplayFrames
                            : GameplayMessageBoxDefinitions.Timing.ItemMinimumDisplayFrames;
                        Phase = GameplayMessageBoxPhase.MinimumDisplay;
                    }
                }
                else
                    _nextOpeningRadiusPixels += GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
                return;

            case GameplayMessageBoxPhase.MinimumDisplay:
                if (--MinimumDisplayFramesRemaining == 0)
                    Phase = GameplayMessageBoxPhase.AwaitingInput;
                return;

            case GameplayMessageBoxPhase.AwaitingInput:
                if (IsSaveConfirmation)
                {
                    switch (_saveChoiceInput.Advance())
                    {
                        case SaveChoiceInputCadence.Update.ShowsMovedChoice:
                            // The update showing a moved choice processes no audio; the move
                            // sound is queued on it.
                            LastFrameAudio = default;
                            ConfirmationSelectionChangedThisFrame = true;
                            return;
                        case SaveChoiceInputCadence.Update.Waits:
                            return;
                        case SaveChoiceInputCadence.Update.ReadsController:
                            break;
                        default:
                            throw new InvalidOperationException("Undefined save-choice update.");
                    }
                    _controller.Latch(controllerInput);
                    // A confirms the highlighted choice, then B cancels; only a press with
                    // neither moves the two-choice cursor.
                    ControllerButton newlyPressed = _controller.NewlyPressed;
                    if ((newlyPressed & ControllerButton.A) != ControllerButton.None)
                    {
                        BeginClosing(ConfirmationSelectionYes);
                        return;
                    }
                    if ((newlyPressed & ControllerButton.B) != ControllerButton.None)
                    {
                        BeginClosing(false);
                        return;
                    }
                    ControllerButton horizontal = newlyPressed & (ControllerButton.Left | ControllerButton.Right | ControllerButton.Select);
                    if (horizontal != ControllerButton.None)
                    {
                        // The redrawn row shows the other choice from this update on.
                        ConfirmationSelectionYes = !ConfirmationSelectionYes;
                        _saveChoiceInput.ChoiceMoved();
                    }
                    return;
                }
                // An ordinary message closes on any held button, not a new press.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                if (controllerInput != ControllerButton.None)
                    BeginClosing(null);
                return;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (--_savingFramesRemaining == 0)
                {
                    // The completion notice opens only after the saving sound's wait.
                    // Preserve YES across that message.
                    Phase = GameplayMessageBoxPhase.Inactive;
                    Begin(GameplayMessageIds.SaveCompleted, _controller.Current);
                    // The second box needs no screen preparation or clearing, which
                    // preceded the saving sound.
                    BeginPreparing(GameplayMessageBoxDefinitions.Timing.SecondBoxOpeningPauseUpdates);
                    _gunshipCompletion = true;
                }
                return;

            case GameplayMessageBoxPhase.Closing:
                StepClosing();
                return;

            default:
                throw new InvalidDataException($"Unknown gameplay message phase {Phase}.");
        }
    }

    /// <summary>Accepts the interaction; the window's first closing step runs in the same update.</summary>
    private void BeginClosing(bool? confirmationResult)
    {
        _closingConfirmationResult = confirmationResult;
        _nextClosingRadiusPixels = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels;
        Phase = GameplayMessageBoxPhase.Closing;
        StepClosing();
    }

    /// <summary>Publishes one shrinking window radius, then selects restoration or the gunship saving wait.</summary>
    private void StepClosing()
    {
        RadiusPixels = _nextClosingRadiusPixels;
        _nextClosingRadiusPixels -= GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
        if (_nextClosingRadiusPixels >= 0)
            return;
        if (MessageId == GameplayMessageIds.GunshipSaveConfirmation && _closingConfirmationResult == true)
        {
            // The accepted gunship save clears the box, then waits out the saving sound.
            Phase = GameplayMessageBoxPhase.GunshipSavingSound;
            _savingFramesRemaining = GameplayMessageBoxDefinitions.Timing.ClearingUpdates +
                GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames;
            _savingSoundRequested = true;
            return;
        }
        // Clear the box, restore the screen, then two updates of music and sound.
        _pauseUpdatesRemaining = GameplayMessageBoxDefinitions.Timing.ClosingPauseUpdates;
        Phase = GameplayMessageBoxPhase.Restoring;
    }

    /// <summary>The box returns to the paused gameplay update.</summary>
    private void Finish()
    {
        Phase = GameplayMessageBoxPhase.Inactive;
        CompletedConfirmationResult = _gunshipCompletion ? true : _closingConfirmationResult;
        _gunshipCompletion = false;
        _closingConfirmationResult = null;
        MessageId = GameplayMessageId.None;
        MinimumDisplayFramesRemaining = 0;
        RadiusPixels = 0;
    }

    /// <summary>
    /// The button <paramref name="messageId"/>'s panel names: the configured Shoot or Run binding's, as the glyph shows it;
    /// null for a message whose panel names no button.
    /// </summary>
    private ControllerButton? PanelButton(GameplayMessageId messageId) => GameplayMessageButtons.PanelBinding(messageId) switch
    {
        GameplayMessagePanelButtonBinding.Shoot => GameplayMessageButtons.Shown(_shootBinding),
        GameplayMessagePanelButtonBinding.Run => GameplayMessageButtons.Shown(_runBinding),
        GameplayMessagePanelButtonBinding.None => null,
        _ => throw new InvalidDataException($"Unknown gameplay-message panel binding for {messageId}."),
    };

    /// <summary>Whether the ordinary message coroutine shows <paramref name="messageId"/>.</summary>
    internal static bool IsOrdinary(GameplayMessageId messageId) =>
        messageId != GameplayMessageId.None && Enum.IsDefined(messageId);

    /// <summary>The messages the ordinary message coroutine shows.</summary>
    internal static IEnumerable<GameplayMessageId> OrdinaryMessages => Enum.GetValues<GameplayMessageId>().Where(IsOrdinary);
}
