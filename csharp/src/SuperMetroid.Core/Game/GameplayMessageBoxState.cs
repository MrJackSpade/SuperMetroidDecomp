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
/// This type handles ordinary messages (IDs 1-22 and 24-26). Save confirmation
/// IDs 23/28 have a separate selection loop and are intentionally not accepted here.
/// </para>
/// </remarks>
public sealed class GameplayMessageBoxState
{
    private ushort[] _tilemap = [];
    private readonly ControllerInputState _controller = new();
    private ISnesAddressSpace? _activeBus;
    private int _nextOpeningRadiusPixels;
    private int _nextClosingRadiusPixels;
    private bool? _closingConfirmationResult;

    /// <summary>Whether bank-$85 currently owns the gameplay main loop and BG3 window.</summary>
    public bool IsActive => Phase != GameplayMessageBoxPhase.Inactive;

    /// <summary>One-based index into <c>$85:869B</c>, matching WRAM <c>$1C1F</c>.</summary>
    public byte MessageId { get; private set; }

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
    /// Starts one ordinary gameplay message using its native config and tilemap records.
    /// Bindings are parameters because the options menu may remap them; their defaults are
    /// the words installed by <c>NewSaveFile</c>.
    /// </summary>
    public void Begin(
        ISnesAddressSpace bus,
        byte messageId,
        ushort shootBinding = (ushort)SnesButton.X,
        ushort runBinding = (ushort)SnesButton.B)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!(messageId is >= 1 and <= 26))
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageId),
                messageId,
                "Gameplay message IDs are 1-26; later IDs use separate ending code.");
        }
        if (IsActive)
            throw new InvalidOperationException("A gameplay message box is already active.");

        int definition = GameplayMessageRomData.Assets.DefinitionTable +
            (messageId - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
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

        int borderAddress;
        switch (drawFunction)
        {
            case GameplayMessageRomData.Routines.DrawSmallTilemap:
                borderAddress = GameplayMessageRomData.Assets.SmallBorder;
                break;
            case GameplayMessageRomData.Routines.DrawLargeTilemap:
                borderAddress = GameplayMessageRomData.Assets.LargeBorder;
                break;
            default:
                throw new InvalidDataException(
                    $"Message {messageId} names unsupported draw routine $85:{drawFunction:X4}.");
        }

        // `$85:8289` and `$85:825A` select the small and large *border artwork*.
        // Neither routine fixes the content height. Both tail-call `$85:82C1`, whose
        // copy length is the difference between this definition's content pointer and
        // the following definition's pointer. Message $14 (the map-station message) is
        // the important retail counterexample: it deliberately uses the small border
        // routine around three content rows. Treating "small" as "one row" made valid
        // cartridge data fail as soon as the first map station opened.
        int contentRows = contentByteCount / (GameplayMessageRomData.Layout.TilemapWidth * 2);

        _tilemap = new ushort[
            (contentRows + GameplayMessageRomData.Layout.BorderRows) *
            GameplayMessageRomData.Layout.TilemapWidth];
        for (int column = 0; column < GameplayMessageRomData.Layout.TilemapWidth; column++)
        {
            ushort borderWord = ReadWord(bus, borderAddress + column * 2);
            _tilemap[column] = borderWord;
            _tilemap[_tilemap.Length - GameplayMessageRomData.Layout.TilemapWidth + column] =
                borderWord;
        }
        for (int word = 0; word < contentByteCount / 2; word++)
            _tilemap[GameplayMessageRomData.Layout.TilemapWidth + word] = ReadWord(
                bus,
                GameplayMessageRomData.Assets.BankBase | (contentPointer + word * 2));

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

        MessageId = messageId;
        _activeBus = bus;
        RadiusPixels = 0;
        MinimumDisplayFramesRemaining = 0;
        _nextOpeningRadiusPixels = 0;
        _nextClosingRadiusPixels = GameplayMessageRomData.Timing.MaximumRadiusPixels;
        _closingConfirmationResult = null;
        CompletedConfirmationResult = null;
        ConfirmationSelectionYes = true;
        DrawSaveConfirmationSelection();
        Phase = GameplayMessageBoxPhase.Opening;
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
                    if (MessageId == GameplayMessageIds.SaveConfirmation)
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
                if (MessageId == GameplayMessageIds.SaveConfirmation)
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

            case GameplayMessageBoxPhase.Closing:
                RadiusPixels = _nextClosingRadiusPixels;
                _nextClosingRadiusPixels -= GameplayMessageRomData.Timing.RadiusStepPixels;
                if (_nextClosingRadiusPixels < 0)
                {
                    // Radius zero has no visible pixels, so restoring gameplay state at
                    // this point preserves the final native wait without losing artwork.
                    Phase = GameplayMessageBoxPhase.Inactive;
                    CompletedConfirmationResult = _closingConfirmationResult;
                    _closingConfirmationResult = null;
                    MessageId = 0;
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
        if (MessageId != GameplayMessageIds.SaveConfirmation && MessageId != 0)
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

    private void PatchConfiguredButton(byte messageId, ushort binding)
    {
        int byteOffset = GameplayMessageRomData.Buttons.SpecialGlyphByteOffsets[messageId - 1];
        if ((byteOffset & 1) != 0 || byteOffset + 1 >= _tilemap.Length * 2)
        {
            throw new InvalidDataException(
                $"Message {messageId} configurable-button offset ${byteOffset:X4} is outside its tilemap.");
        }

        _tilemap[byteOffset / 2] = ResolveButtonTilemapWord(binding);
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
}
