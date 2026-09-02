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
    private const int DefinitionTableAddress = 0x85869b;
    private const int LargeBorderAddress = 0x858000;
    private const int SmallBorderAddress = 0x858040;
    private const ushort DrawLargeTilemapFunction = 0x825a;
    private const ushort DrawSmallTilemapFunction = 0x8289;
    private const ushort PatchShootButtonFunction = 0x83c5;
    private const ushort PatchRunButtonFunction = 0x83cc;
    private const ushort SetupSmallFunction = 0x8436;
    private const ushort SetupLargeFunction = 0x8441;
    private const int TilemapWidth = 32;
    private const int MaximumRadiusPixels = 24;
    private const int RadiusStepPixels = 2;
    private const int ItemMinimumDisplayFrames = 360;

    // Byte offsets inside MessageBoxTilemap for the configurable glyph in definitions
    // 1-27. These are the literal words at $85:8749, named by the upstream disassembly.
    private static readonly ushort[] SpecialButtonByteOffsets =
    [
        0x000, 0x12a, 0x12a, 0x12c, 0x12c, 0x12c, 0x000, 0x000,
        0x000, 0x000, 0x000, 0x000, 0x120, 0x000, 0x000, 0x000,
        0x000, 0x000, 0x12a, 0x000, 0x000, 0x000, 0x000, 0x000,
        0x000, 0x000, 0x000,
    ];

    private ushort[] _tilemap = [];
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
    public int TilemapRowCount => _tilemap.Length / TilemapWidth;

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

        int definition = DefinitionTableAddress + (messageId - 1) * 6;
        ushort modifyFunction = ReadWord(bus, definition);
        ushort drawFunction = ReadWord(bus, definition + 2);
        ushort contentPointer = ReadWord(bus, definition + 4);
        ushort nextContentPointer = ReadWord(bus, definition + 10);
        int contentByteCount = nextContentPointer - contentPointer;
        if (contentByteCount <= 0 || (contentByteCount % (TilemapWidth * 2)) != 0)
        {
            throw new InvalidDataException(
                $"Message {messageId} content ${contentPointer:X4}-${nextContentPointer:X4} " +
                "does not contain complete 32-word rows.");
        }

        int borderAddress;
        switch (drawFunction)
        {
            case DrawSmallTilemapFunction:
                borderAddress = SmallBorderAddress;
                break;
            case DrawLargeTilemapFunction:
                borderAddress = LargeBorderAddress;
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
        int contentRows = contentByteCount / (TilemapWidth * 2);

        _tilemap = new ushort[(contentRows + 2) * TilemapWidth];
        for (int column = 0; column < TilemapWidth; column++)
        {
            ushort borderWord = ReadWord(bus, borderAddress + column * 2);
            _tilemap[column] = borderWord;
            _tilemap[_tilemap.Length - TilemapWidth + column] = borderWord;
        }
        for (int word = 0; word < contentByteCount / 2; word++)
            _tilemap[TilemapWidth + word] = ReadWord(bus, 0x850000 | (contentPointer + word * 2));

        switch (modifyFunction)
        {
            case PatchShootButtonFunction:
                PatchConfiguredButton(messageId, shootBinding);
                break;
            case PatchRunButtonFunction:
                PatchConfiguredButton(messageId, runBinding);
                break;
            case SetupSmallFunction:
            case SetupLargeFunction:
                break;
            default:
                throw new InvalidDataException(
                    $"Message {messageId} names unsupported setup routine $85:{modifyFunction:X4}.");
        }

        MessageId = messageId;
        RadiusPixels = 0;
        MinimumDisplayFramesRemaining = 0;
        _nextOpeningRadiusPixels = 0;
        _nextClosingRadiusPixels = MaximumRadiusPixels;
        _closingConfirmationResult = null;
        CompletedConfirmationResult = null;
        ConfirmationSelectionYes = true;
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
        switch (Phase)
        {
            case GameplayMessageBoxPhase.Inactive:
                return;

            case GameplayMessageBoxPhase.Opening:
                // OpenMessageBox_Async waits for NMI first, then publishes the current
                // radius before the for-loop increment. Radius zero is consequently a
                // real accepted frame, even though it exposes no pixels.
                RadiusPixels = _nextOpeningRadiusPixels;
                if (RadiusPixels == MaximumRadiusPixels)
                {
                    if (MessageId == 0x17)
                    {
                        MinimumDisplayFramesRemaining = 0;
                        Phase = GameplayMessageBoxPhase.AwaitingInput;
                    }
                    else
                    {
                        MinimumDisplayFramesRemaining = ItemMinimumDisplayFrames;
                        Phase = GameplayMessageBoxPhase.MinimumDisplay;
                    }
                }
                else
                    _nextOpeningRadiusPixels += RadiusStepPixels;
                return;

            case GameplayMessageBoxPhase.MinimumDisplay:
                if (--MinimumDisplayFramesRemaining == 0)
                    Phase = GameplayMessageBoxPhase.AwaitingInput;
                return;

            case GameplayMessageBoxPhase.AwaitingInput:
                if (MessageId == 0x17)
                {
                    // Bank $85's save selector owns the same synchronous window as item
                    // boxes. Horizontal input changes the two-choice cursor; A confirms the
                    // highlighted choice and B is the retail cancellation shortcut.
                    ushort horizontal = unchecked((ushort)(
                        controllerInput & ((ushort)SnesButton.Left | (ushort)SnesButton.Right)));
                    if (horizontal != 0)
                        ConfirmationSelectionYes = !ConfirmationSelectionYes;
                    if ((controllerInput & (ushort)SnesButton.B) != 0)
                    {
                        _closingConfirmationResult = false;
                        _nextClosingRadiusPixels = MaximumRadiusPixels;
                        Phase = GameplayMessageBoxPhase.Closing;
                    }
                    else if ((controllerInput & (ushort)SnesButton.A) != 0)
                    {
                        _closingConfirmationResult = ConfirmationSelectionYes;
                        _nextClosingRadiusPixels = MaximumRadiusPixels;
                        Phase = GameplayMessageBoxPhase.Closing;
                    }
                    return;
                }
                // Retail's enabled bug-fix path checks held keys, not only new edges.
                // This is observable when a player begins holding a button during the
                // mandatory 360-frame item fanfare.
                if (controllerInput != 0)
                {
                    _nextClosingRadiusPixels = MaximumRadiusPixels;
                    Phase = GameplayMessageBoxPhase.Closing;
                }
                return;

            case GameplayMessageBoxPhase.Closing:
                RadiusPixels = _nextClosingRadiusPixels;
                _nextClosingRadiusPixels -= RadiusStepPixels;
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
                }
                return;

            default:
                throw new InvalidDataException($"Unknown gameplay message phase {Phase}.");
        }
    }

    private void PatchConfiguredButton(byte messageId, ushort binding)
    {
        int byteOffset = SpecialButtonByteOffsets[messageId - 1];
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
        if ((binding & (ushort)SnesButton.A) != 0) return 0x28e0;
        if ((binding & (ushort)SnesButton.B) != 0) return 0x3ce1;
        if ((binding & (ushort)SnesButton.X) != 0) return 0x2cf7;
        if ((binding & (ushort)SnesButton.Y) != 0) return 0x38f8;
        if ((binding & (ushort)SnesButton.Select) != 0) return 0x38d0;
        if ((binding & (ushort)SnesButton.L) != 0) return 0x38eb;
        if ((binding & (ushort)SnesButton.R) != 0) return 0x38f1;
        return 0x284e;
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
