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
/// This type handles the ordinary messages and the gunship's save confirmation. On YES the gunship's save keeps gameplay
/// paused through the saving sound and the completion notice.
/// </para>
/// </remarks>
/// <param name="saveChoiceInput">When an open save confirmation reads the controller; the session creates it.</param>
/// <param name="pauses">The updates gameplay stays paused before the window opens and after it closes; the session creates it.</param>
public sealed class GameplayMessageBoxState(SaveChoiceInputCadence saveChoiceInput, MessageBoxPauses pauses)
{
    /// <summary>When an open save confirmation reads the controller.</summary>
    private readonly SaveChoiceInputCadence _saveChoiceInput = Ensure.NotNull(saveChoiceInput);

    /// <summary>The updates gameplay stays paused before the window opens and after it closes, and what each runs.</summary>
    private readonly MessageBoxPauses _pauses = Ensure.NotNull(pauses);

    /// <summary>Preserves controller edges across the box's own controller reads.</summary>
    private readonly ControllerInputState _controller = new();
    /// <summary>Radius to publish on the next opening frame, after the current frame's window is drawn.</summary>
    private int _nextOpeningRadiusPixels;
    /// <summary>Radius to publish on the next closing frame.</summary>
    private int _nextClosingRadiusPixels;
    /// <summary>The answer chosen before the confirmation window starts closing; null for a message offering no choice.</summary>
    private SaveChoice? _closingConfirmationResult;
    /// <summary>Keeps an accepted gunship save result through its saving sound and completion notice.</summary>
    private bool _gunshipCompletion;
    /// <summary>Remaining frames before the gunship's SAVE COMPLETED notice reopens the box.</summary>
    private int _savingFramesRemaining;

    /// <summary>The audio processing and liquid wave and statue advance of the update <see cref="Step"/> just ran.</summary>
    public MessageBoxFrameAudio LastFrameAudio { get; private set; }
    /// <summary>The controller bindings when the message began, whose Shoot or Dash button a panel's glyph shows.</summary>
    private ControllerBindings _bindings;

    /// <summary>Whether the active message uses the two-choice save cursor and edge-based input polling.</summary>
    private bool IsSaveConfirmation => MessageId is GameplayMessageId.SaveConfirmation or
        GameplayMessageId.GunshipSaveConfirmation;

    /// <summary>Whether the box currently pauses gameplay and owns the message window.</summary>
    public bool IsActive => Phase != GameplayMessageBoxPhase.Inactive;

    /// <summary>The message the box shows; null while none has begun.</summary>
    public GameplayMessageId? MessageId { get; private set; }

    /// <summary>The message box's current step.</summary>
    public GameplayMessageBoxPhase Phase { get; private set; }

    /// <summary>
    /// Half-height of the row window visible in the most recently completed frame.
    /// The radius grows and shrinks by exactly two pixels per update.
    /// </summary>
    public int RadiusPixels { get; private set; }

    /// <summary>Unelapsed mandatory frames before controller input is inspected.</summary>
    public int MinimumDisplayFramesRemaining { get; private set; }

    /// <summary>
    /// What the box shows while a message has begun (#1466): the message, the button its panel names for the configured
    /// shoot or run binding, and the save choice a confirmation highlights; null while no message has begun.
    /// </summary>
    public ShownMessage? Shown => MessageId is not { } message ? null
        : new ShownMessage(message, PanelButton(message), IsSaveConfirmation ? ConfirmationSelection : null);

    /// <summary>
    /// The message the box's window exposes in the most recently completed frame (#2505): what the box shows, with the
    /// window's half-height; null while the window shows nothing: before a message begins, while its window is closed
    /// and once it has finished.
    /// </summary>
    public ExposedMessage? Exposed => RadiusPixels > 0 && Shown is { } shown ? new ExposedMessage(shown, RadiusPixels) : null;

    /// <summary>The answer a save confirmation highlights.</summary>
    public SaveChoice ConfirmationSelection { get; private set; } = SaveChoice.Yes;

    /// <summary>
    /// Starts one ordinary gameplay message with its authored layout.
    /// </summary>
    /// <param name="messageId">The ordinary message or the gunship save confirmation to show.</param>
    /// <param name="controllerRead">
    /// The physical buttons held at the update's last controller read. While the box is shown only
    /// the box itself reads the controller, so the save choice's presses are relative to this.
    /// </param>
    /// <param name="bindings">
    /// The current controller bindings: the applicable item panels name the button bound to Shoot or to Dash, as the
    /// options menu may have remapped them.
    /// </param>
    public void Begin(
        GameplayMessageId messageId,
        ControllerButton controllerRead,
        ControllerBindings bindings)
    {
        if (!Enum.IsDefined(messageId))
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageId),
                messageId,
                $"The ordinary message box shows no gameplay message {messageId}.");
        }
        if (IsActive)
            throw new InvalidOperationException("A gameplay message box is already active.");

        _bindings = bindings;
        MessageId = messageId;
        RadiusPixels = 0;
        MinimumDisplayFramesRemaining = 0;
        _nextOpeningRadiusPixels = 0;
        _nextClosingRadiusPixels = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels;
        _closingConfirmationResult = null;
        _gunshipCompletion = false;
        ConfirmationSelection = SaveChoice.Yes;
        _controller.Latch(controllerRead);
        // The pause's first update is the gameplay update that opened the box.
        _pauses.BeginBeforeOpening();
        Phase = GameplayMessageBoxPhase.PausedBeforeOpening;
    }

    /// <summary>
    /// Runs one update of the box. Gameplay does not run and does not read the controller while the box is shown, so
    /// only the box's own reads see input.
    /// </summary>
    /// <returns>
    /// What the update did (#2514): stayed shown, showed a moved save choice, started the gunship's saving sound, or closed
    /// with the answer the box hands to what waited on it.
    /// </returns>
    public MessageBoxStep Step(ControllerButton controllerInput)
    {
        // Each update of the box processes music and sound once, unless noted below.
        LastFrameAudio = MessageBoxFrameAudio.MusicAndSounds;

        switch (Phase)
        {
            case GameplayMessageBoxPhase.Inactive:
                LastFrameAudio = default;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.PausedBeforeOpening:
                LastFrameAudio = _pauses.Advance();
                if (_pauses.IsOver)
                    Phase = GameplayMessageBoxPhase.Opening;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.PausedAfterClosing:
                LastFrameAudio = _pauses.Advance();
                return _pauses.IsOver ? Finish() : MessageBoxStep.NothingAsked.Instance;

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
                        MinimumDisplayFramesRemaining = Ensure.Listed(GameplayMessageBoxDefinitions.Timing.MinimumDisplayFrames,
                            MessageId ?? throw new InvalidOperationException("An opening message box shows a message."));
                        Phase = GameplayMessageBoxPhase.MinimumDisplay;
                    }
                }
                else
                    _nextOpeningRadiusPixels += GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.MinimumDisplay:
                if (--MinimumDisplayFramesRemaining == 0)
                    Phase = GameplayMessageBoxPhase.AwaitingInput;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.AwaitingInput:
                if (IsSaveConfirmation)
                {
                    switch (_saveChoiceInput.Advance())
                    {
                        case SaveChoiceInputCadence.Update.ShowsMovedChoice:
                            // The update showing a moved choice processes no audio; the move
                            // sound is queued on it.
                            LastFrameAudio = default;
                            return MessageBoxStep.ChoiceMoved.Instance;
                        case SaveChoiceInputCadence.Update.Waits:
                            return MessageBoxStep.NothingAsked.Instance;
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
                        return BeginClosing(ConfirmationSelection);
                    if ((newlyPressed & ControllerButton.B) != ControllerButton.None)
                        return BeginClosing(SaveChoice.No);
                    ControllerButton horizontal = newlyPressed & (ControllerButton.Left | ControllerButton.Right | ControllerButton.Select);
                    if (horizontal != ControllerButton.None)
                    {
                        // The redrawn row shows the other choice from this update on.
                        ConfirmationSelection = ConfirmationSelection == SaveChoice.Yes ? SaveChoice.No : SaveChoice.Yes;
                        _saveChoiceInput.ChoiceMoved();
                    }
                    return MessageBoxStep.NothingAsked.Instance;
                }
                // An ordinary message closes on any held button, not a new press.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                return controllerInput != ControllerButton.None ? BeginClosing(null) : MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (--_savingFramesRemaining == 0)
                {
                    // The completion notice opens only after the saving sound's wait.
                    // Preserve YES across that message.
                    Phase = GameplayMessageBoxPhase.Inactive;
                    Begin(GameplayMessageId.SaveCompleted, _controller.Current, _bindings);
                    // The second box's pause is shorter: the screen stayed set up for the first.
                    _pauses.BeginBeforeCompletionNotice();
                    _gunshipCompletion = true;
                }
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.Closing:
                return StepClosing();

            default:
                throw new InvalidDataException($"Unknown gameplay message phase {Phase}.");
        }
    }

    /// <summary>Accepts the interaction; the window's first closing step runs in the same update.</summary>
    private MessageBoxStep BeginClosing(SaveChoice? confirmationResult)
    {
        _closingConfirmationResult = confirmationResult;
        _nextClosingRadiusPixels = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels;
        Phase = GameplayMessageBoxPhase.Closing;
        return StepClosing();
    }

    /// <summary>
    /// Publishes one shrinking window radius, then selects restoration or the gunship saving wait, whose saving sound
    /// starts on the update that selects it.
    /// </summary>
    private MessageBoxStep StepClosing()
    {
        RadiusPixels = _nextClosingRadiusPixels;
        _nextClosingRadiusPixels -= GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
        if (_nextClosingRadiusPixels >= 0)
            return MessageBoxStep.NothingAsked.Instance;
        if (MessageId == GameplayMessageId.GunshipSaveConfirmation && _closingConfirmationResult == SaveChoice.Yes)
        {
            // The accepted gunship save pauses one update after closing, then waits out the saving sound.
            Phase = GameplayMessageBoxPhase.GunshipSavingSound;
            _savingFramesRemaining = MessageBoxPauses.UpdatesBeforeGunshipSavingWait +
                GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames;
            return MessageBoxStep.SavingSoundStarted.Instance;
        }
        _pauses.BeginAfterClosing();
        Phase = GameplayMessageBoxPhase.PausedAfterClosing;
        return MessageBoxStep.NothingAsked.Instance;
    }

    /// <summary>
    /// The box returns to the paused gameplay update, a save confirmation handing over its answer: the gunship's accepted
    /// save answers YES once its completion notice has closed.
    /// </summary>
    private MessageBoxStep.Closed Finish()
    {
        Phase = GameplayMessageBoxPhase.Inactive;
        SaveChoice? answer = _gunshipCompletion ? SaveChoice.Yes : _closingConfirmationResult;
        _gunshipCompletion = false;
        _closingConfirmationResult = null;
        MessageId = null;
        MinimumDisplayFramesRemaining = 0;
        RadiusPixels = 0;
        return answer is { } choice ? new MessageBoxStep.Answered(choice) : MessageBoxStep.Closed.WithoutAnswer;
    }

    /// <summary>
    /// The button <paramref name="messageId"/>'s panel names: the configured Shoot or Run binding's, as the glyph shows it;
    /// null for a message whose panel names no button.
    /// </summary>
    private ControllerButton? PanelButton(GameplayMessageId messageId) => GameplayMessageButtons.PanelBinding(messageId) switch
    {
        GameplayMessagePanelButtonBinding.Shoot => GameplayMessageButtons.Shown(_bindings.Shoot),
        GameplayMessagePanelButtonBinding.Run => GameplayMessageButtons.Shown(_bindings.Dash),
        GameplayMessagePanelButtonBinding.None => null,
        _ => throw new InvalidDataException($"Unknown gameplay-message panel binding for {messageId}."),
    };
}
