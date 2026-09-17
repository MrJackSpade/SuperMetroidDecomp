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
    private readonly ControllerInputState _controller = new();
    private ISnesAddressSpace? _activeBus;
    private int _nextOpeningRadiusPixels;
    private int _nextClosingRadiusPixels;
    private bool? _closingConfirmationResult;
    private bool _gunshipCompletion;
    private bool _savingSoundRequested;
    private int _savingFramesRemaining;
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
        GameplayMessagePanelPresentation? panels = null)
    {
        titlePresentation = presentation;
        panelPresentation = panels;
        if (panels?.Contains(MessageId) == true)
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
    public void Begin(
        ISnesAddressSpace bus,
        GameplayMessageId messageId,
        ushort shootBinding = (ushort)SnesButton.X,
        ushort runBinding = (ushort)SnesButton.B,
        string sourceContext = nameof(GameplayMessageBoxState))
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte rawMessageId = (byte)messageId;
        if ((messageId is < GameplayMessageId.EnergyTank or > GameplayMessageId.GravitySuit) &&
            messageId != GameplayMessageId.GunshipSaveConfirmation)
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
        if (panelPresentation?.Contains(messageId) == true)
        {
            _tilemap = panelPresentation.Build(messageId);
            PatchInstalledPanelButton(messageId);
        }
        else if (titlePresentation?.Contains(messageId) == true)
            _tilemap = titlePresentation.Build(messageId);
        else
            BuildCartridgeTilemap(bus, messageId, shootBinding, runBinding);

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
        DrawSaveConfirmationSelection();
        Phase = GameplayMessageBoxPhase.Opening;
    }

    private void BuildCartridgeTilemap(
        ISnesAddressSpace bus,
        GameplayMessageId messageId,
        ushort shootBinding,
        ushort runBinding)
    {
        byte rawMessageId = (byte)messageId;
        int definition = GameplayMessageRomData.Assets.DefinitionTable +
            (rawMessageId - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
        ushort modifyFunction = ReadWord(bus, definition);
        ushort drawFunction = ReadWord(bus, definition + 2);
        ushort contentPointer = ReadWord(bus, definition + 4);
        ushort nextContentPointer = ReadWord(bus, definition + 10);
        int contentByteCount = nextContentPointer - contentPointer;
        if (contentByteCount <= 0 ||
            (contentByteCount % (GameplayMessageRomData.Layout.TilemapWidth * 2)) != 0)
        {
            throw new InvalidDataException(
                $"Message {messageId} content ${contentPointer:X4}-${nextContentPointer:X4} " +
                "does not contain complete 32-word rows.");
        }

        int borderAddress = drawFunction switch
        {
            GameplayMessageRomData.Routines.DrawSmallTilemap => GameplayMessageRomData.Assets.SmallBorder,
            GameplayMessageRomData.Routines.DrawLargeTilemap => GameplayMessageRomData.Assets.LargeBorder,
            _ => throw new InvalidDataException(
                $"Message {messageId} names unsupported draw routine $85:{drawFunction:X4}."),
        };

        // The draw routine selects border artwork; content height comes from the
        // difference between adjacent definition pointers. Message $14 deliberately
        // combines the small border with three content rows.
        int contentRows = contentByteCount /
            (GameplayMessageRomData.Layout.TilemapWidth * sizeof(ushort));
        _tilemap = new ushort[(contentRows + GameplayMessageRomData.Layout.BorderRows) *
            GameplayMessageRomData.Layout.TilemapWidth];
        for (int column = 0; column < GameplayMessageRomData.Layout.TilemapWidth; column++)
        {
            ushort borderWord = ReadWord(bus, borderAddress + column * sizeof(ushort));
            _tilemap[column] = borderWord;
            _tilemap[_tilemap.Length - GameplayMessageRomData.Layout.TilemapWidth + column] = borderWord;
        }
        for (int word = 0; word < contentByteCount / sizeof(ushort); word++)
            _tilemap[GameplayMessageRomData.Layout.TilemapWidth + word] = ReadWord(
                bus, GameplayMessageRomData.Assets.BankBase | (contentPointer + word * sizeof(ushort)));

        switch (modifyFunction)
        {
            case GameplayMessageRomData.Routines.PatchShootButton:
                PatchConfiguredButton(messageId, shootBinding);
                break;
            case GameplayMessageRomData.Routines.PatchRunButton:
                PatchConfiguredButton(messageId, runBinding);
                break;
            case GameplayMessageRomData.Routines.SetupSmall:
            case GameplayMessageRomData.Routines.SetupLarge:
                break;
            default:
                throw new InvalidDataException(
                    $"Message {messageId} names unsupported setup routine $85:{modifyFunction:X4}.");
        }
    }

    /// <summary>Consumes the completed result of save confirmation message $17.</summary>
    public bool? ConsumeConfirmationResult()
    {
        bool? result = CompletedConfirmationResult;
        CompletedConfirmationResult = null;
        return result;
    }

    /// <summary>Advances one accepted NMI while gameplay remains blocked.</summary>
    public void Step(ushort controllerInput)
    {
        // The save selector calls ReadControllerInput and then consumes Controller1New.
        // Latch on every opening/closing NMI as well as the selection loop so a direction
        // held before the box finishes opening is not manufactured into a new edge.
        _controller.Latch(controllerInput);
        ConfirmationSelectionChangedThisFrame = false;

        switch (Phase)
        {
            case GameplayMessageBoxPhase.Inactive:
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
                    // Bank $85's save selector owns the same synchronous window as item
                    // boxes. Horizontal input changes the two-choice cursor; A confirms the
                    // highlighted choice and B is the retail cancellation shortcut.
                    ushort newlyPressed = _controller.NewlyPressed;
                    ushort horizontal = unchecked((ushort)(
                        newlyPressed & ((ushort)SnesButton.Left | (ushort)SnesButton.Right |
                            (ushort)SnesButton.Select)));
                    if (horizontal != 0)
                    {
                        ConfirmationSelectionYes = !ConfirmationSelectionYes;
                        DrawSaveConfirmationSelection();
                        ConfirmationSelectionChangedThisFrame = true;
                    }
                    if ((newlyPressed & (ushort)SnesButton.B) != 0)
                    {
                        _closingConfirmationResult = false;
                        _nextClosingRadiusPixels =
                            GameplayMessageRomData.Timing.MaximumRadiusPixels;
                        Phase = GameplayMessageBoxPhase.Closing;
                    }
                    else if ((newlyPressed & (ushort)SnesButton.A) != 0)
                    {
                        _closingConfirmationResult = ConfirmationSelectionYes;
                        _nextClosingRadiusPixels =
                            GameplayMessageRomData.Timing.MaximumRadiusPixels;
                        Phase = GameplayMessageBoxPhase.Closing;
                    }
                    return;
                }
                // Retail's enabled bug-fix path checks held keys, not only new edges.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                if (controllerInput != 0)
                {
                    _nextClosingRadiusPixels =
                        GameplayMessageRomData.Timing.MaximumRadiusPixels;
                    Phase = GameplayMessageBoxPhase.Closing;
                }
                return;

            case GameplayMessageBoxPhase.GunshipSavingSound:
                if (--_savingFramesRemaining == 0)
                {
                    // $85:80D9 opens the ordinary completion notice only after the
                    // saving sound's synchronous wait. Preserve YES across that message.
                    ISnesAddressSpace bus = _activeBus!;
                    Phase = GameplayMessageBoxPhase.Inactive;
                    Begin(bus, GameplayMessageIds.SaveCompleted);
                    _gunshipCompletion = true;
                }
                return;

            case GameplayMessageBoxPhase.Closing:
                RadiusPixels = _nextClosingRadiusPixels;
                _nextClosingRadiusPixels -= GameplayMessageRomData.Timing.RadiusStepPixels;
                if (_nextClosingRadiusPixels < 0)
                {
                    if (MessageId == GameplayMessageIds.GunshipSaveConfirmation &&
                        _closingConfirmationResult == true)
                    {
                        Phase = GameplayMessageBoxPhase.GunshipSavingSound;
                        _savingFramesRemaining = GameplayMessageRomData.Timing.GunshipSavingSoundFrames;
                        _savingSoundRequested = true;
                        return;
                    }
                    // Radius zero has no visible pixels, so restoring gameplay state at
                    // this point preserves the final native wait without losing artwork.
                    Phase = GameplayMessageBoxPhase.Inactive;
                    CompletedConfirmationResult = _gunshipCompletion ? true : _closingConfirmationResult;
                    _gunshipCompletion = false;
                    _closingConfirmationResult = null;
                    MessageId = GameplayMessageId.None;
                    MinimumDisplayFramesRemaining = 0;
                    _tilemap = [];
                    _activeBus = null;
                }
                return;

            default:
                throw new InvalidDataException($"Unknown gameplay message phase {Phase}.");
        }
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
        ISnesAddressSpace source = _activeBus
            ?? throw new InvalidOperationException(
                "Save confirmation cursor changed without its cartridge address space.");
        if (_tilemap.Length <
            GameplayMessageRomData.Layout.SaveSelectionDestinationWord +
            GameplayMessageRomData.Layout.SaveSelectionRowWords)
        {
            throw new InvalidDataException(
                "Save confirmation tilemap is too short for the native selected YES/NO row.");
        }

        int sourceWord = ConfirmationSelectionYes
            ? GameplayMessageRomData.Layout.SaveSelectionYesSourceWord
            : GameplayMessageRomData.Layout.SaveSelectionNoSourceWord;
        for (int word = 0; word < GameplayMessageRomData.Layout.SaveSelectionRowWords; word++)
        {
            _tilemap[GameplayMessageRomData.Layout.SaveSelectionDestinationWord + word] = ReadWord(
                source,
                GameplayMessageRomData.Assets.SaveSelectionTilemap + (sourceWord + word) * 2);
        }
    }

    private void PatchConfiguredButton(GameplayMessageId messageId, ushort binding)
    {
        int byteOffset = GameplayMessageRomData.Buttons.SpecialGlyphByteOffsets[(byte)messageId - 1];
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

    private static ushort ResolveButtonTilemapWord(ushort binding)
    {
        // BIT tests occur in this exact order at $85:83D1. A malformed multi-bit binding
        // therefore still selects the first native match instead of requiring an enum
        // equality that the cartridge never performed.
        foreach (GameplayMessageButtonGlyph definition in
            GameplayMessageRomData.Buttons.Glyphs)
        {
            if ((binding & (ushort)definition.Button) != 0)
                return definition.Glyph.Raw;
        }
        return GameplayMessageRomData.Buttons.UnknownGlyph.Raw;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>Coroutine phase for <see cref="GameplayMessageBoxState"/>.</summary>
public enum GameplayMessageBoxPhase : byte
{
    Inactive,
    Opening,
    MinimumDisplay,
    AwaitingInput,
    Closing,
    /// <summary>The gunship's $85:8119 sound wait before SAVE COMPLETED.</summary>
    GunshipSavingSound,
}
