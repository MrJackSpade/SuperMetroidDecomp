using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Frame-steppable owner of bank-$85's gameplay message-box coroutine. Permanent item
/// PLMs all enter this shared path; the message identity changes ROM data, not behavior.
/// </summary>
/// <remarks>
/// <para>
/// <c>$85:8080-$85:8619</c> shows a message over gameplay, opens a scanline window
/// around Y=124, blocks gameplay for a fixed interval, waits for input, and closes the
/// same window. What it shows is a <see cref="ShownMessage"/>: the message, the button its
/// panel names and the highlighted save choice, whose installed image the backend draws. Keeping the wait as explicit state lets the desktop host
/// remain responsive and debuggable without pretending the synchronous 65816 routine
/// returned while its lag-frame loops were running.
/// </para>
/// <para>
/// This type handles definitions 1-26 and the gunship's ID-28 confirmation. On YES the
/// ship branch retains gameplay ownership through the saving sound and completion notice.
/// </para>
/// </remarks>
public sealed class GameplayMessageBoxState
{
    /// <summary>Preserves controller edges across the message routine's own input polls.</summary>
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
    /// <summary>Lag frames left in preparation, restoration, or before the save selector's next controller read.</summary>
    private int _lagFramesRemaining;

    /// <summary>
    /// Toggle_Save_Confirmation_Selection's own <c>Wait_for_Lag_Frame</c> ($85:8532) is due:
    /// one frame, without the audio handlers, before the redrawn row uploads and $37 queues.
    /// </summary>
    private bool _selectionRedrawWaitPending;

    /// <summary>The bank-$85 audio and HDMA-object calls made by the frame <see cref="Step"/> just ran.</summary>
    [field: NonSerialized]
    public MessageBoxFrameAudio LastFrameAudio { get; private set; }
    /// <summary>Current Shoot binding used when panel artwork includes a button glyph.</summary>
    private ControllerButton _shootBinding = ControllerButton.X;
    /// <summary>Current Run binding used when panel artwork includes a button glyph.</summary>
    private ControllerButton _runBinding = ControllerButton.B;

    /// <summary>Consumes $85:811B's one-shot saving sound, independently of gameplay publication.</summary>
    public bool ConsumeSavingSoundRequest()
    {
        bool requested = _savingSoundRequested;
        _savingSoundRequested = false;
        return requested;
    }

    /// <summary>Whether the active message uses the two-choice save cursor and edge-based input polling.</summary>
    private bool IsSaveConfirmation => MessageId is GameplayMessageId.SaveConfirmation or
        GameplayMessageId.GunshipSaveConfirmation;

    /// <summary>Whether bank-$85 currently owns the gameplay main loop and BG3 window.</summary>
    public bool IsActive => Phase != GameplayMessageBoxPhase.Inactive;

    /// <summary>One-based index into <c>$85:869B</c>, matching WRAM <c>$1C1F</c>.</summary>
    public GameplayMessageId MessageId { get; private set; }

    /// <summary>Current coroutine segment, exposed so frame-by-frame debugging is useful.</summary>
    public GameplayMessageBoxPhase Phase { get; private set; }

    /// <summary>
    /// Half-height of the scanline window visible in the most recently completed frame.
    /// The native 8.8 word advances by <c>$0200</c>, so host pixels advance by exactly two.
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

    /// <summary>Current yes/no choice for save confirmation message $17.</summary>
    public bool ConfirmationSelectionYes { get; private set; } = true;

    /// <summary>
    /// Result published after confirmation $17 has completely closed. Consumers clear it
    /// through <see cref="ConsumeConfirmationResult"/> at the suspended PLM return seam.
    /// </summary>
    public bool? CompletedConfirmationResult { get; private set; }

    /// <summary>
    /// True only on the accepted NMI where bank $85 toggled the save cursor. The runtime
    /// consumes this to queue the cartridge's menu-move sound through the ordinary audio
    /// owner instead of giving the message renderer an unrelated sound dependency.
    /// </summary>
    public bool ConfirmationSelectionChangedThisFrame { get; private set; }

    /// <summary>
    /// Starts one ordinary gameplay message using its native config record.
    /// Bindings are parameters because the options menu may remap them; their defaults are
    /// the words installed by <c>NewSaveFile</c>.
    /// </summary>
    /// <param name="messageId">One-based native message identity 1..26 or gunship save confirmation 28.</param>
    /// <param name="controllerRead">
    /// The controller held at the dispatch's last ReadControllerInput. During the box the
    /// NMI handler runs in lag mode, so the save selector's presses are relative to this.
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
        _selectionRedrawWaitPending = false;
        _controller.Latch(controllerRead);
        // The routine's first lag wait belongs to the dispatch that opened the box.
        BeginPreparing(GameplayMessageBoxDefinitions.Timing.PreOpenLagFrames - 1);
    }

    /// <summary>Enters the PPU preparation stretch with the number of waits left at this dispatch boundary.</summary>
    private void BeginPreparing(int lagFrames)
    {
        _lagFramesRemaining = lagFrames;
        Phase = GameplayMessageBoxPhase.Preparing;
    }

    /// <summary>Consumes the completed result of save confirmation message $17.</summary>
    public bool? ConsumeConfirmationResult()
    {
        bool? result = CompletedConfirmationResult;
        CompletedConfirmationResult = null;
        return result;
    }

    /// <summary>
    /// Runs one frame of the bank-$85 routine, which waits on lag frames: the NMI handler
    /// reads no controller while it runs, so only the routine's own reads see input.
    /// </summary>
    public void Step(ControllerButton controllerInput)
    {
        ConfirmationSelectionChangedThisFrame = false;
        // Each routine calls HandleMusicQueue and HandleSounds after its lag wait returns.
        LastFrameAudio = MessageBoxFrameAudio.MusicAndSounds;

        switch (Phase)
        {
            case GameplayMessageBoxPhase.Inactive:
                LastFrameAudio = default;
                return;

            case GameplayMessageBoxPhase.Preparing:
                // Initialise_PPU_for_MessageBoxes waits twice before its audio calls.
                if (_lagFramesRemaining == GameplayMessageBoxDefinitions.Timing.PreOpenLagFrames - 1)
                    LastFrameAudio = default;
                if (--_lagFramesRemaining == 0)
                    Phase = GameplayMessageBoxPhase.Opening;
                return;

            case GameplayMessageBoxPhase.Restoring:
                LastFrameAudio = _lagFramesRemaining switch
                {
                    // Restore_PPU's first wait; its second ends in $88:84B9 (HDMA objects
                    // and music), then HandleSounds.
                    3 => default,
                    2 => MessageBoxFrameAudio.MusicAndSounds with { AdvancesPerLineEffects = true },
                    // The routine returns into the dispatch, whose own HandleSounds follows.
                    0 => MessageBoxFrameAudio.MusicAndSounds with { SoundHandlerCalls = 2 },
                    _ => MessageBoxFrameAudio.MusicAndSounds,
                };
                if (_lagFramesRemaining-- == 0)
                    Finish();
                return;

            case GameplayMessageBoxPhase.Opening:
                // OpenMessageBox_Async waits for NMI first, then publishes the current
                // radius before the for-loop increment. Radius zero is consequently a
                // real accepted frame, even though it exposes no pixels.
                RadiusPixels = _nextOpeningRadiusPixels;
                if (RadiusPixels == GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
                {
                    if (IsSaveConfirmation)
                    {
                        MinimumDisplayFramesRemaining = 0;
                        _lagFramesRemaining = GameplayMessageBoxDefinitions.Timing.SaveSelectionReadLagFrames;
                        Phase = GameplayMessageBoxPhase.AwaitingInput;
                    }
                    else
                    {
                        // `$85:847A-$8490` gives the four completion notices only ten
                        // lag frames. Equipment descriptions use $0168 (360). Treating
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
                    if (_selectionRedrawWaitPending)
                    {
                        // The toggle's wait precedes the loop's two waits, so a cursor move
                        // delays the next ReadControllerInput by one frame. The selection
                        // sound ($85:84EC) is queued after this wait.
                        _selectionRedrawWaitPending = false;
                        LastFrameAudio = default;
                        ConfirmationSelectionChangedThisFrame = true;
                        return;
                    }
                    // $85:84BA waits two lag frames, then calls ReadControllerInput. The
                    // read's frame also holds the next iteration's first wait.
                    if (_lagFramesRemaining > 0)
                    {
                        _lagFramesRemaining--;
                        return;
                    }
                    _controller.Latch(controllerInput);
                    _lagFramesRemaining = GameplayMessageBoxDefinitions.Timing.SaveSelectionReadLagFrames - 1;
                    // $85:84D8: A confirms the highlighted choice, then B cancels; only a
                    // press with neither moves the two-choice cursor.
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
                        // The redrawn row shows the other choice from this frame on.
                        ConfirmationSelectionYes = !ConfirmationSelectionYes;
                        _selectionRedrawWaitPending = true;
                    }
                    return;
                }
                // $85:84A3 polls the joypad registers for any held key, not a new edge.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                if (controllerInput != ControllerButton.None)
                    BeginClosing(null);
                return;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (--_savingFramesRemaining == 0)
                {
                    // $85:80D9 opens the ordinary completion notice only after the
                    // saving sound's synchronous wait. Preserve YES across that message.
                    Phase = GameplayMessageBoxPhase.Inactive;
                    Begin(GameplayMessageIds.SaveCompleted, _controller.Current);
                    // The second box skips Initialise_PPU_for_MessageBoxes and the clear,
                    // which preceded the saving sound.
                    BeginPreparing(GameplayMessageBoxDefinitions.Timing.ReopenLagFrames);
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

    /// <summary>Accepts the interaction; Close_MessageBox's first step runs in the same frame.</summary>
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
            // $85:80D3 clears the tilemap, then waits out the saving sound.
            Phase = GameplayMessageBoxPhase.GunshipSavingSound;
            _savingFramesRemaining = GameplayMessageBoxDefinitions.Timing.ClearTilemapLagFrames +
                GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames;
            _savingSoundRequested = true;
            return;
        }
        // $85:80AA-$80B4: clear the tilemap, restore the PPU, two music/sound frames.
        _lagFramesRemaining = GameplayMessageBoxDefinitions.Timing.RestoreLagFrames;
        Phase = GameplayMessageBoxPhase.Restoring;
    }

    /// <summary>The routine returns into the suspended gameplay dispatch.</summary>
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
