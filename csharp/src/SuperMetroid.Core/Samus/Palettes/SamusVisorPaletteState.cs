using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Room visor-colour animation run by <c>HandleVisorPalette</c> at <c>$91:D83F-$91:D8A4</c>: a countdown of
/// palette-handler calls and the byte offset of the next visor colour.
/// </summary>
/// <remarks>
/// This is intentionally not a generic color tween. In rooms whose default layer-blending configuration animates
/// the visor, the routine rotates only Samus OBJ palette-four color four through the last three of the six visor
/// colours. The countdown and the offset are only ever set by this type: the reset installs countdown one at offset
/// six, and every expiry reloads the countdown to the frame delay and moves the offset through six, eight and ten.
/// The countdown therefore holds one through the frame delay between calls and never decrements from zero, which is
/// why the cartridge's word-wide decrement of the two adjacent bytes never borrows into the offset.
/// </remarks>
public sealed class SamusVisorPaletteState
{
    /// <summary>Calls left after the reset before the first visor colour is written.</summary>
    private const int ResetTimer = 1;

    /// <summary>Host-owned visor artwork, excluded from debugger-state serialization.</summary>
    [field: NonSerialized]
    public SamusVisorColorCatalog? PresentationColors { get; set; }

    /// <summary>
    /// Palette-handler calls left before the next visor colour, one through
    /// <see cref="SamusPaletteDefinitions.Visor.FrameDelay"/>; it reads the frame delay on the call that wrote a colour.
    /// </summary>
    public int Timer { get; private set; } = ResetTimer;

    /// <summary>
    /// Byte offset of the next visor colour in the six-colour visor table: six, eight or ten, the three
    /// room-cycle colours. It is a byte offset, not an ordinal.
    /// </summary>
    public int PaletteByteOffset { get; private set; } = SamusPaletteDefinitions.Visor.CycleFirstByteOffset;

    /// <summary>
    /// Advances one palette-handler call and optionally writes CGRAM color 196.
    /// </summary>
    public SamusVisorPaletteStepResult Update(
        SamusColours colours,
        SamusSpecialPaletteType specialSamusPaletteType,
        LayerBlendingConfiguration layerBlendingDefaultConfig)
    {
        Ensure.NotNull(colours);

        // X-ray handler eight owns this same visor color. `$91:D842` returns immediately,
        // preserving the countdown and offset so ordinary room animation resumes where it stopped.
        if (specialSamusPaletteType == SamusSpecialPaletteType.Xray)
        {
            return new SamusVisorPaletteStepResult();
        }

        // Every other room resets the animation on every call. Besides disabling the animation,
        // this primes an immediate color write on the first call after entering a backdrop-
        // color-math room: countdown one expires before the table is consulted.
        if (!layerBlendingDefaultConfig.AnimatesVisor())
        {
            Timer = ResetTimer;
            PaletteByteOffset = SamusPaletteDefinitions.Visor.CycleFirstByteOffset;
            return new SamusVisorPaletteStepResult();
        }

        Timer = DecrementTimer(Timer);
        if (Timer != 0)
        {
            return new SamusVisorPaletteStepResult();
        }

        // `$91:D864` reloads the expired countdown to the frame delay and keeps the table offset.
        Timer = SamusPaletteDefinitions.Visor.FrameDelay;
        int sourceOffset = PaletteByteOffset;
        if (PresentationColors is null ||
            !PresentationColors.TryResolveByteOffset(sourceOffset, out RgbColor color))
            throw new InvalidDataException($"Visor palette offset {sourceOffset} has no installed color.");
        colours.Visor.Write(0, color);

        // Only offsets 6, 8, and 10 belong to the room-backdrop cycle. Offsets 0/2/4 are
        // X-ray widening colors and are selected by the separate X-ray palette handler.
        int nextOffset = sourceOffset + 2;
        PaletteByteOffset = nextOffset < SamusPaletteDefinitions.Visor.CycleEndByteOffset
            ? nextOffset
            : SamusPaletteDefinitions.Visor.CycleFirstByteOffset;

        return new SamusVisorPaletteStepResult();
    }

    /// <summary>
    /// One call's countdown step. The countdown is one or more on every call (see the remarks), so the step never
    /// reaches the cartridge's borrow into the offset byte; a zero countdown is a broken invariant and fails.
    /// </summary>
    private static int DecrementTimer(int timer)
    {
        if (timer <= 0)
            throw new InvalidOperationException($"Visor countdown {timer} was decremented below its expiry.");
        return timer - 1;
    }
}
