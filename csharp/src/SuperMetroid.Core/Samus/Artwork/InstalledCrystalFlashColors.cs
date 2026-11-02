using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Editable Crystal Flash body and bubble colors. Timers, frame cursors,
/// and completion still belong to <see cref="SamusCrystalFlashState"/>.
/// </summary>
public sealed class InstalledCrystalFlashColors
{
    /// <summary>Complete edited body frames, retained only when any supplied color differs from the calculated pulse.</summary>
    internal readonly RgbColor[][]? body;
    /// <summary>Bubble samples that differ from the rotating ramp, including the independently painted stock white.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> bubble;

    /// <summary>Compares all supplied frames with color rules before retaining only independent edits.</summary>
    /// <param name="body">Every supplied body frame when one color differs from the pulse, otherwise null.</param>
    /// <param name="bubble">The bubble samples the import found differing from the rotating ramp.</param>
    internal InstalledCrystalFlashColors(RgbColor[][]? body, Dictionary<ColourCell, RgbColor> bubble)
    {
        this.body = body;
        this.bubble = bubble;
    }

    /// <summary>Returns one editable body color for a playback record, calculating stock frames when they require no stored overrides.</summary>
    /// <param name="frame">Zero-based body record index, 0 through 9 in playback order.</param>
    /// <param name="color">Zero-based color within the ten-color body portion, including transparent color zero.</param>
    /// <returns>RGB555 color word, with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the body dimensions.</exception>
    public RgbColor ResolveBody(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BodyFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BodyColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return body is null ? CalculateBody(frame, color) : body[frame][color];
    }

    /// <summary>
    /// The body colors, selected by ten playback records: the first
    /// frame is neutral grey16; subsequent frames pulse 19,23,27,23 twice and
    /// return to19. All nine visible body colors share that intensity. The
    /// transparent color is the fixed RGB5 backdrop (0,0,14) in every frame.
    /// </summary>
    internal static RgbColor CalculateBody(int frame, int color)
    {
        if (color == 0)
            return new RgbColor(0, 0, 14);
        int grey = frame == 0 ? 16 : 27 - 4 * Math.Abs((frame - 1) % 4 - 2);
        return new RgbColor(grey, grey, grey);
    }
    /// <summary>Returns one editable bubble color from the independently cycling six-frame sequence, preserving supplied edits and the painted white sample.</summary>
    /// <param name="frame">Zero-based bubble palette index, 0 through 5.</param>
    /// <param name="color">Zero-based color within the six-color bubble portion, 0 through 5.</param>
    /// <returns>RGB555 color word, with red in bits 0..4, green in bits 5..9, and blue in bits 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame or color index is outside the bubble dimensions.</exception>
    public RgbColor ResolveBubble(int frame, int color)
    {
        if ((uint)frame >= CrystalFlashColorFormat.BubbleFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= CrystalFlashColorFormat.BubbleColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return bubble.TryGetValue(new ColourCell(frame, color), out RgbColor supplied)
            ? supplied : CalculateBubble(frame, color);
    }

    /// <summary>
    /// The bubble frames rotate six pink-to-white levels one color per frame.
    /// Red stays31; green/blue descend from31 to25 with nearest-integer interpolation
    /// across five intervals. Frame5/color0 is independently painted white
    /// rather than the ramp's 31711. The uniform five-tick traversal
    /// and six-word copy supply no phase-specific
    /// operation generating that choice. Only that sample remains supplied stock data;
    /// a formula exception would merely reencode its frame/color identity and value.
    /// </summary>
    internal static RgbColor CalculateBubble(int frame, int color)
    {
        int phase = (color - frame + CrystalFlashColorFormat.BubbleColorCount)
            % CrystalFlashColorFormat.BubbleColorCount;
        int channel = 31 - (6 * phase + 2) / 5;
        return new RgbColor(31, channel, channel);
    }

    /// <summary>Shows one body frame as Samus's Crystal Flash body colours without changing the bubble colors or advancing any gameplay timer.</summary>
    /// <param name="colours">Samus's colours.</param>
    /// <param name="frame">Zero-based body playback record index, 0 through 9.</param>
    /// <exception cref="ArgumentNullException"><paramref name="colours"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The body frame index is outside the sequence.</exception>
    public void ApplyBody(SamusColours colours, int frame)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            colours.CrystalFlashBody.Write(color,
                ResolveBody(frame, color));
    }
    /// <summary>Shows one bubble frame as Samus's Crystal Flash bubble colours without changing the body colors or advancing any gameplay timer.</summary>
    /// <param name="colours">Samus's colours.</param>
    /// <param name="frame">Zero-based bubble palette index, 0 through 5.</param>
    /// <exception cref="ArgumentNullException"><paramref name="colours"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The bubble frame index is outside the sequence.</exception>
    public void ApplyBubble(SamusColours colours, int frame)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
            colours.CrystalFlashBubble.Write(color,
                ResolveBubble(frame, color));
    }
}
