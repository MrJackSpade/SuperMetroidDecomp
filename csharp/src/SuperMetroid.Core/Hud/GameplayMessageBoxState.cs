using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The gameplay message box, advanced one update at a time. Every permanent item pickup, station and save prompt shows
/// its message through this one box; the message changes what is shown, not how the box behaves.
/// </summary>
/// <remarks>
/// <para>
/// The box pauses gameplay, opens a window around its centre line, holds the message for a fixed interval, waits for input, and
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
public sealed partial class GameplayMessageBoxState(SaveChoiceInputCadence saveChoiceInput, MessageBoxPauses pauses)
{
    /// <summary>When an open save confirmation reads the controller.</summary>
    private readonly SaveChoiceInputCadence _saveChoiceInput = Ensure.NotNull(saveChoiceInput);

    /// <summary>The updates gameplay stays paused before the window opens and after it closes, and what each runs.</summary>
    private readonly MessageBoxPauses _pauses = Ensure.NotNull(pauses);

    /// <summary>Preserves controller edges across the box's own controller reads.</summary>
    private readonly ControllerInputState _controller = new();
    /// <summary>
    /// The window's half-height the next opening or closing update shows, after the current update's window is drawn: it
    /// grows from zero while the window opens and shrinks from the full height while it closes.
    /// </summary>
    private int _nextRadiusPixels;
    /// <summary>
    /// The answer the box hands to what waited on it as it closes: the save choice a confirmation closed with, which the
    /// gunship's accepted save keeps through its saving sound and completion notice; null for a message that asks nothing.
    /// </summary>
    private SaveChoice? _answer;
    /// <summary>The wait before the gunship's SAVE COMPLETED notice reopens the box.</summary>
    private UpdateCountdown _savingWait = UpdateCountdown.Ended();

    /// <summary>The audio processing and liquid wave and statue advance of the update <see cref="Step"/> just ran.</summary>
    public MessageBoxFrameAudio LastFrameAudio { get; private set; }
    /// <summary>The message the box shows with the bindings it began under; null while none has begun.</summary>
    private BegunMessage? _begun;

    /// <summary>Whether the box currently pauses gameplay and owns the message window.</summary>
    public bool IsActive => Phase != GameplayMessageBoxPhase.Inactive;

    /// <summary>The message the box shows; null while none has begun.</summary>
    public GameplayMessageId? MessageId => _begun?.Id;

    /// <summary>The message box's current step.</summary>
    internal GameplayMessageBoxPhase Phase { get; set; }

    /// <summary>
    /// Half-height of the row window visible in the most recently completed frame.
    /// The radius grows and shrinks by exactly its radius step each update.
    /// </summary>
    internal int RadiusPixels { get; set; }

    /// <summary>The mandatory display before controller input is inspected.</summary>
    internal UpdateCountdown MinimumDisplay { get; set; } = UpdateCountdown.Ended();

    /// <summary>
    /// What the box shows while a message has begun (#1466): the message, the button its panel names for the configured
    /// shoot or run binding, and the save choice a confirmation highlights; null while no message has begun.
    /// </summary>
    internal ShownMessage? Shown => _begun is not { } begun ? null
        : new ShownMessage(begun.Id, PanelButton(begun),
            ConfirmationOf(begun.Id) != GameplayMessageConfirmation.None ? ConfirmationSelection : null);

    /// <summary>
    /// The message the box's window exposes in the most recently completed frame (#2505): what the box shows, with the
    /// window's half-height; null while the window shows nothing: before a message begins, while its window is closed
    /// and once it has finished.
    /// </summary>
    public ExposedMessage? Exposed => RadiusPixels > 0 && Shown is { } shown ? new ExposedMessage(shown, RadiusPixels) : null;

    /// <summary>The answer a save confirmation highlights.</summary>
    internal SaveChoice ConfirmationSelection { get; set; } = SaveChoice.Yes;

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
        if (IsActive)
            throw new InvalidOperationException("A gameplay message box is already active.");

        Ensure.NotNull(bindings);
        _answer = null;
        Show(new BegunMessage(messageId, bindings), controllerRead);
        // The pause's first update is the gameplay update that opened the box.
        _pauses.BeginBeforeOpening();
    }

    /// <summary>
    /// Sets the box up to show <paramref name="message"/> from a closed window, paused before it opens: a message that
    /// begins, or the gunship's completion notice after its saving sound.
    /// </summary>
    /// <param name="message">The message to show, with the bindings it began under.</param>
    /// <param name="controllerRead">The physical buttons held at the last controller read, which the box's own reads follow.</param>
    private void Show(BegunMessage message, ControllerButton controllerRead)
    {
        _begun = message;
        RadiusPixels = 0;
        MinimumDisplay = UpdateCountdown.Ended();
        _nextRadiusPixels = 0;
        ConfirmationSelection = SaveChoice.Yes;
        _controller.Latch(controllerRead);
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
        // A box that shows no message is inactive, and its update does nothing; every other step runs on the message the
        // box shows, which its showing set and its finish clears together with the box's phase.
        if (_begun is not { } begun)
        {
            LastFrameAudio = default;
            return MessageBoxStep.NothingAsked.Instance;
        }

        GameplayMessageId message = begun.Id;
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
                RadiusPixels = _nextRadiusPixels;
                if (RadiusPixels == GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
                {
                    // A message that asks for a confirmation has no minimum display: it takes input as soon as its window opens.
                    if (GameplayMessageBoxDefinitions.Message(message).MinimumDisplayFrames is { } minimumFrames)
                    {
                        MinimumDisplay = UpdateCountdown.Lasting(minimumFrames);
                        Phase = GameplayMessageBoxPhase.MinimumDisplay;
                    }
                    else
                    {
                        MinimumDisplay = UpdateCountdown.Ended();
                        _saveChoiceInput.Begin();
                        Phase = GameplayMessageBoxPhase.AwaitingInput;
                    }
                }
                else
                    _nextRadiusPixels += GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.MinimumDisplay:
                if (MinimumDisplay.StepEnds())
                    Phase = GameplayMessageBoxPhase.AwaitingInput;
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.AwaitingInput:
                if (ConfirmationOf(message) != GameplayMessageConfirmation.None)
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
                        return Answer(message, ConfirmationSelection);
                    if ((newlyPressed & ControllerButton.B) != ControllerButton.None)
                        return Answer(message, SaveChoice.No);
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
                // mandatory item fanfare.
                return controllerInput != ControllerButton.None ? BeginClosing(message) : MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (_savingWait.StepEnds())
                {
                    // The completion notice opens only after the saving sound's wait, the accepted YES kept across it.
                    // Its pause is shorter: the screen stayed set up for the first box.
                    Show(begun with { Id = GameplayMessageId.SaveCompleted }, _controller.Current);
                    _pauses.BeginBeforeCompletionNotice();
                }
                return MessageBoxStep.NothingAsked.Instance;

            case GameplayMessageBoxPhase.Closing:
                return StepClosing(message);

            default:
                throw new InvalidDataException($"Unknown gameplay message phase {Phase}.");
        }
    }

    /// <summary>Accepts a confirmation's <paramref name="answer"/> and begins closing its window.</summary>
    private MessageBoxStep Answer(GameplayMessageId message, SaveChoice answer)
    {
        _answer = answer;
        return BeginClosing(message);
    }

    /// <summary>Accepts the interaction; the window's first closing step runs in the same update.</summary>
    private MessageBoxStep BeginClosing(GameplayMessageId message)
    {
        _nextRadiusPixels = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels;
        Phase = GameplayMessageBoxPhase.Closing;
        return StepClosing(message);
    }

    /// <summary>
    /// Publishes one shrinking window radius, then selects restoration or the gunship saving wait, whose saving sound
    /// starts on the update that selects it.
    /// </summary>
    private MessageBoxStep StepClosing(GameplayMessageId message)
    {
        RadiusPixels = _nextRadiusPixels;
        _nextRadiusPixels -= GameplayMessageBoxDefinitions.Timing.RadiusStepPixels;
        if (_nextRadiusPixels >= 0)
            return MessageBoxStep.NothingAsked.Instance;
        if (ConfirmationOf(message) == GameplayMessageConfirmation.SaveThenCompletionNotice && _answer == SaveChoice.Yes)
        {
            // The accepted gunship save pauses one update after closing, then waits out the saving sound.
            Phase = GameplayMessageBoxPhase.GunshipSavingSound;
            _savingWait = UpdateCountdown.Lasting(MessageBoxPauses.UpdatesBeforeGunshipSavingWait +
                GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames);
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
        SaveChoice? answer = _answer;
        _answer = null;
        _begun = null;
        MinimumDisplay = UpdateCountdown.Ended();
        RadiusPixels = 0;
        return answer is { } choice ? new MessageBoxStep.Answered(choice) : MessageBoxStep.Closed.WithoutAnswer;
    }

    /// <summary>What <paramref name="messageId"/> asks the player to confirm.</summary>
    private static GameplayMessageConfirmation ConfirmationOf(GameplayMessageId messageId) =>
        GameplayMessageBoxDefinitions.Message(messageId).Confirmation;

    /// <summary>
    /// The button <paramref name="message"/>'s panel names: the Shoot or Run binding's it began under, as the glyph shows it;
    /// null for a message whose panel names no button.
    /// </summary>
    private static ControllerButton? PanelButton(BegunMessage message)
    {
        GameplayMessagePanelButtonBinding? binding = GameplayMessageButtons.PanelBinding(message.Id);
        if (binding == GameplayMessagePanelButtonBinding.Shoot)
            return GameplayMessageButtons.Shown(message.Bindings[ControllerAction.Shoot]);
        if (binding == GameplayMessagePanelButtonBinding.Run)
            return GameplayMessageButtons.Shown(message.Bindings[ControllerAction.Dash]);
        // A panel that names no button.
        return null;
    }

    /// <summary>A message the box shows, with the controller bindings it began under.</summary>
    /// <param name="Id">The message.</param>
    /// <param name="Bindings">The controller bindings when the message began, whose Shoot or Dash button a panel's glyph shows.</param>
    private sealed partial record BegunMessage(GameplayMessageId Id, ControllerBindings Bindings);
}
