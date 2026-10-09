using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Frame-steppable owner of bank-$85's gameplay message-box coroutine. Permanent item
/// PLMs all enter this shared path; the message identity changes ROM data, not behavior.
/// </summary>
/// <remarks>
/// <para>
/// <c>$85:8080-$85:8619</c> temporarily maps a ROM-authored tilemap onto BG3, opens a
/// scanline window around Y=124, blocks gameplay for a fixed interval, waits for input,
/// and closes the same window. Keeping the wait as explicit state lets the desktop host
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
    private ushort[] _tilemap = [];
    [NonSerialized] private GameplayMessageTitlePresentation? titlePresentation;
    [NonSerialized] private GameplayMessagePanelPresentation? panelPresentation;
    [NonSerialized] private GameplayMessageNoticePresentation? noticePresentation;
    private readonly ControllerInputState _controller = new();
    private ISnesAddressSpace? _activeBus;
    private int _nextOpeningRadiusPixels;
    private int _nextClosingRadiusPixels;
    private bool? _closingConfirmationResult;
    private bool _gunshipCompletion;
    private bool _savingSoundRequested;
    private int _savingFramesRemaining;
    // Lag frames left in the current Preparing/Restoring stretch, or until the save
    // selector's next ReadControllerInput.
    private int _lagFramesRemaining;

    /// <summary>
    /// Toggle_Save_Confirmation_Selection's own <c>Wait_for_Lag_Frame</c> ($85:8532) is due:
    /// one frame, without the audio handlers, before the redrawn row uploads and $37 queues.
    /// </summary>
    private bool _selectionRedrawWaitPending;

    /// <summary>The bank-$85 audio and HDMA-object calls made by the frame <see cref="Step"/> just ran.</summary>
    [field: NonSerialized]
    public MessageBoxFrameAudio LastFrameAudio { get; private set; }
    private ushort _shootBinding = (ushort)SnesButton.X;
    private ushort _runBinding = (ushort)SnesButton.B;

    /// <summary>Consumes $85:811B's one-shot saving sound, independently of gameplay publication.</summary>
    public bool ConsumeSavingSoundRequest()
    {
        bool requested = _savingSoundRequested;
        _savingSoundRequested = false;
        return requested;
    }

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

    /// <summary>Number of consecutive 32-tile rows in the ROM-built box.</summary>
    public int TilemapRowCount => _tilemap.Length / GameplayMessageRomData.Layout.TilemapWidth;

    /// <summary>Read-only final tilemap after border and configured-button substitution.</summary>
    public ReadOnlySpan<ushort> Tilemap => _tilemap;

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
    /// Rebinds host-owned editable content after installation reload or debugger-state restore.
    /// An active supported title is rebuilt without resetting its coroutine phase.
    /// </summary>
    public void BindPresentation(
        GameplayMessageTitlePresentation? presentation,
        GameplayMessagePanelPresentation? panels = null,
        GameplayMessageNoticePresentation? notices = null)
    {
        titlePresentation = presentation;
        panelPresentation = panels;
        noticePresentation = notices;
        if (notices?.Contains(MessageId) == true)
        {
            _tilemap = notices.Build(MessageId);
            if (IsSaveConfirmation)
                notices.ApplySelection(MessageId, _tilemap, ConfirmationSelectionYes);
        }
        else if (panels?.Contains(MessageId) == true)
        {
            _tilemap = panels.Build(MessageId);
            PatchInstalledPanelButton(MessageId);
        }
        else if (presentation?.Contains(MessageId) == true)
            _tilemap = presentation.Build(MessageId);
    }

    /// <summary>
    /// Starts one ordinary gameplay message using its native config and tilemap records.
    /// Bindings are parameters because the options menu may remap them; their defaults are
    /// the words installed by <c>NewSaveFile</c>.
    /// </summary>
    /// <param name="bus">Runtime address space retained while this coroutine owns gameplay, including the gunship's completion-notice reopen.</param>
    /// <param name="messageId">One-based native message identity 1..26 or gunship save confirmation 28; requires installed title, panel, or notice artwork.</param>
    /// <param name="controllerRead">
    /// The controller held at the dispatch's last ReadControllerInput. During the box the
    /// NMI handler runs in lag mode, so the save selector's presses are relative to this.
    /// </param>
    /// <param name="shootBinding">Current native controller-bit binding for Shoot, used to substitute the corresponding button glyph in applicable item panels.</param>
    /// <param name="runBinding">Current native controller-bit binding for Run, used to substitute the corresponding button glyph in applicable item panels.</param>
    /// <param name="sourceContext">Caller identity included in unsupported-message diagnostics; does not affect message timing or presentation.</param>
    public void Begin(
        ISnesAddressSpace bus,
        GameplayMessageId messageId,
        ushort controllerRead,
        ushort shootBinding = (ushort)SnesButton.X,
        ushort runBinding = (ushort)SnesButton.B,
        string sourceContext = nameof(GameplayMessageBoxState))
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte rawMessageId = (byte)messageId;
        if (messageId is (< GameplayMessageId.EnergyTank or > GameplayMessageId.GravitySuit) and
            not GameplayMessageId.GunshipSaveConfirmation)
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageId),
                messageId,
                $"Gameplay message ${rawMessageId:X2} from {sourceContext} is not translated by the ordinary coroutine.");
        }
        if (IsActive)
            throw new InvalidOperationException("A gameplay message box is already active.");

        _shootBinding = shootBinding;
        _runBinding = runBinding;
        if (noticePresentation?.Contains(messageId) == true)
            _tilemap = noticePresentation.Build(messageId);
        else if (panelPresentation?.Contains(messageId) == true)
        {
            _tilemap = panelPresentation.Build(messageId);
            PatchInstalledPanelButton(messageId);
        }
        else if (titlePresentation?.Contains(messageId) == true)
            _tilemap = titlePresentation.Build(messageId);
        else
            throw new InvalidOperationException(
                $"Gameplay message {messageId} requires installed presentation assets.");

        MessageId = messageId;
        _activeBus = bus;
        RadiusPixels = 0;
        MinimumDisplayFramesRemaining = 0;
        _nextOpeningRadiusPixels = 0;
        _nextClosingRadiusPixels = GameplayMessageRomData.Timing.MaximumRadiusPixels;
        _closingConfirmationResult = null;
        _gunshipCompletion = false;
        CompletedConfirmationResult = null;
        ConfirmationSelectionYes = true;
        _selectionRedrawWaitPending = false;
        DrawSaveConfirmationSelection();
        _controller.Latch(controllerRead);
        // The routine's first lag wait belongs to the dispatch that opened the box.
        BeginPreparing(GameplayMessageRomData.Timing.PreOpenLagFrames - 1);
    }

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

    /// <summary>Advances one accepted NMI while gameplay remains blocked.</summary>
    /// <summary>
    /// Runs one frame of the bank-$85 routine, which waits on lag frames: the NMI handler
    /// reads no controller while it runs, so only the routine's own reads see input.
    /// </summary>
    public void Step(ushort controllerInput)
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
                if (_lagFramesRemaining == GameplayMessageRomData.Timing.PreOpenLagFrames - 1)
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
                    2 => MessageBoxFrameAudio.MusicAndSounds with { RunsHdmaObjects = true },
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
                if (RadiusPixels == GameplayMessageRomData.Timing.MaximumRadiusPixels)
                {
                    if (IsSaveConfirmation)
                    {
                        MinimumDisplayFramesRemaining = 0;
                        _lagFramesRemaining = GameplayMessageRomData.Timing.SaveSelectionReadLagFrames;
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
                            ? GameplayMessageRomData.Timing.StationMinimumDisplayFrames
                            : GameplayMessageRomData.Timing.ItemMinimumDisplayFrames;
                        Phase = GameplayMessageBoxPhase.MinimumDisplay;
                    }
                }
                else
                    _nextOpeningRadiusPixels += GameplayMessageRomData.Timing.RadiusStepPixels;
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
                    _lagFramesRemaining = GameplayMessageRomData.Timing.SaveSelectionReadLagFrames - 1;
                    // $85:84D8: A confirms the highlighted choice, then B cancels; only a
                    // press with neither moves the two-choice cursor.
                    ushort newlyPressed = _controller.NewlyPressed;
                    if ((newlyPressed & (ushort)SnesButton.A) != 0)
                    {
                        BeginClosing(ConfirmationSelectionYes);
                        return;
                    }
                    if ((newlyPressed & (ushort)SnesButton.B) != 0)
                    {
                        BeginClosing(false);
                        return;
                    }
                    ushort horizontal = unchecked((ushort)(
                        newlyPressed & ((ushort)SnesButton.Left | (ushort)SnesButton.Right |
                            (ushort)SnesButton.Select)));
                    if (horizontal != 0)
                    {
                        ConfirmationSelectionYes = !ConfirmationSelectionYes;
                        DrawSaveConfirmationSelection();
                        _selectionRedrawWaitPending = true;
                    }
                    return;
                }
                // $85:84A3 polls the joypad registers for any held key, not a new edge.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                if (controllerInput != 0)
                    BeginClosing(null);
                return;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (--_savingFramesRemaining == 0)
                {
                    // $85:80D9 opens the ordinary completion notice only after the
                    // saving sound's synchronous wait. Preserve YES across that message.
                    ISnesAddressSpace bus = _activeBus!;
                    Phase = GameplayMessageBoxPhase.Inactive;
                    Begin(bus, GameplayMessageIds.SaveCompleted, _controller.Current);
                    // The second box skips Initialise_PPU_for_MessageBoxes and the clear,
                    // which preceded the saving sound.
                    BeginPreparing(GameplayMessageRomData.Timing.ReopenLagFrames);
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
        _nextClosingRadiusPixels = GameplayMessageRomData.Timing.MaximumRadiusPixels;
        Phase = GameplayMessageBoxPhase.Closing;
        StepClosing();
    }

    private void StepClosing()
    {
        RadiusPixels = _nextClosingRadiusPixels;
        _nextClosingRadiusPixels -= GameplayMessageRomData.Timing.RadiusStepPixels;
        if (_nextClosingRadiusPixels >= 0)
            return;
        if (MessageId == GameplayMessageIds.GunshipSaveConfirmation && _closingConfirmationResult == true)
        {
            // $85:80D3 clears the tilemap, then waits out the saving sound.
            Phase = GameplayMessageBoxPhase.GunshipSavingSound;
            _savingFramesRemaining = GameplayMessageRomData.Timing.ClearTilemapLagFrames +
                GameplayMessageRomData.Timing.GunshipSavingSoundFrames;
            _savingSoundRequested = true;
            return;
        }
        // $85:80AA-$80B4: clear the tilemap, restore the PPU, two music/sound frames.
        _lagFramesRemaining = GameplayMessageRomData.Timing.RestoreLagFrames;
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
        _tilemap = [];
        _activeBus = null;
    }

    /// <summary>
    /// Copies the native selected YES/NO row into the already-built message tilemap.
    /// `$85:8507` uses byte offsets $40/$80 into the three-row table at $85:9581 and
    /// writes 32 words at native message-buffer word $180 (local word $80).
    /// </summary>
    private void DrawSaveConfirmationSelection()
    {
        if (!IsSaveConfirmation && MessageId != GameplayMessageId.None)
            return;
        if (noticePresentation?.Contains(MessageId) == true)
        {
            noticePresentation.ApplySelection(
                MessageId, _tilemap, ConfirmationSelectionYes);
            return;
        }
        throw new InvalidOperationException("Save confirmation requires installed notice artwork.");
    }

    private void PatchConfiguredButton(GameplayMessageId messageId, ushort binding)
    {
        int byteOffset = GameplayMessageRomData.Buttons.SpecialGlyphByteOffset(messageId);
        if ((byteOffset & 1) != 0 || byteOffset + 1 >= _tilemap.Length * 2)
        {
            throw new InvalidDataException(
                $"Message {messageId} configurable-button offset ${byteOffset:X4} is outside its tilemap.");
        }

        _tilemap[byteOffset / 2] = ResolveButtonTilemapWord(binding);
    }

    private void PatchInstalledPanelButton(GameplayMessageId messageId)
    {
        switch (GameplayMessagePanelDefinitions.ButtonBinding(messageId))
        {
            case GameplayMessagePanelButtonBinding.Shoot:
                PatchConfiguredButton(messageId, _shootBinding);
                break;
            case GameplayMessagePanelButtonBinding.Run:
                PatchConfiguredButton(messageId, _runBinding);
                break;
            case GameplayMessagePanelButtonBinding.None:
                throw new InvalidDataException(
                    $"Installed gameplay-message panel {messageId} has no compiled button binding.");
            default:
                throw new InvalidDataException(
                    $"Unknown gameplay-message panel binding for {messageId}.");
        }
    }

    private static ushort ResolveButtonTilemapWord(ushort binding) =>
        GameplayMessageRomData.Buttons.ResolveGlyphWord(binding);

}

/// <summary>Audio and HDMA-object handler calls made by one bank-$85 frame.</summary>
/// <param name="RunsHdmaObjects">Whether the host must run the HDMA-object handler for this accepted message update, notably at the PPU restoration seam.</param>
/// <param name="MusicHandlerCalls">Number of native music-queue handler calls represented by this update; zero suppresses the ordinary host music call.</param>
/// <param name="SoundHandlerCalls">Number of native sound-handler calls represented by this update, including the extra dispatch call when the coroutine returns.</param>
public readonly record struct MessageBoxFrameAudio(bool RunsHdmaObjects, int MusicHandlerCalls, int SoundHandlerCalls)
{
    /// <summary>Ordinary message-update descriptor: one music handler, one sound handler, and no HDMA-object handler; describes calls for the host rather than executing them.</summary>
    public static MessageBoxFrameAudio MusicAndSounds { get; } = new(false, 1, 1);
}

/// <summary>Coroutine phase for <see cref="GameplayMessageBoxState"/>.</summary>
public enum GameplayMessageBoxPhase : byte
{
    /// <summary>No message owns gameplay or the BG3 window; after restoration, any completed save-confirmation result remains available for consumption by the suspended caller.</summary>
    Inactive,
    /// <summary>$85:844C Open_MessageBox: publish scanline-window radii from zero through 24 pixels in two-pixel steps while gameplay remains suspended.</summary>
    Opening,
    /// <summary>Fully opened ordinary message's mandatory authored wait: 360 updates for item descriptions or ten for station completion notices, without controller inspection.</summary>
    MinimumDisplay,
    /// <summary>Fully opened input wait: ordinary messages dismiss on any held button, while save confirmation polls newly pressed A/B and left/right/select at its native read cadence.</summary>
    AwaitingInput,
    /// <summary>$85:8589 Close_MessageBox: contract the window through radius zero before PPU restoration, or before the accepted gunship-save sound/completion sequence.</summary>
    Closing,
    /// <summary>The gunship's $85:8119 sound wait before SAVE COMPLETED.</summary>
    GunshipSavingSound,
    /// <summary>Lag frames before Open_MessageBox: PPU, tilemap and HDMA setup.</summary>
    Preparing,
    /// <summary>Lag frames after Close_MessageBox: tilemap clear and PPU restore.</summary>
    Restoring,
}
