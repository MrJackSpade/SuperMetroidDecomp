using System.Buffers.Binary;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Sixteen-step logo RGB5 interpolation, stored as two caller-supplied endpoints.
/// Original $8C:EFE9..F3E8 channels use biased truncation: divide the endpoint times
/// remaining steps by15, rounding up only a remainder of14. Every remainder0..14
/// occurs in the original data and has this same result across both palette families.
/// Thus (channel*remaining+1)/15 reproduces all1536 channels without correction entries.
/// This identifies the exact quantizer, not the historical tool that generated it.</summary>
internal sealed class EndingLogoPaletteFade
{
    /// <summary>Sixteen steps of two sixteen-color palette rows.</summary>
    internal const int ColorCount = 16 * 2 * 16;
    /// <summary>Caller-supplied drawing colors. Stock $8C:EFE9 uses independent yellow
    /// face, blue/cyan edging, orange-sector shades and a dark outline, as confirmed by
    /// the original $99:E089 tiles and final OAM compositions. These selected hues are
    /// retained artwork; numerical cases would merely recite the drawing's palette.
    /// Temporal samples are calculated separately, not covered by this disposition.</summary>
    private readonly RgbColor[] spriteEndpoint;
    /// <summary>Supplied BG colors when they do not match the calculated background gradient.</summary>
    private readonly RgbColor[]? backgroundEndpoint;
    /// <summary>Calculated BG endpoint when the supplied colors follow the stock gradient.</summary>
    private readonly EndingLogoBackgroundPalette? backgroundGradient;

    /// <summary>Separates the supplied OBJ endpoint from the BG gradient or independent BG endpoint.</summary>
    private EndingLogoPaletteFade(ReadOnlySpan<RgbColor> endpoints)
    {
        backgroundGradient = EndingLogoBackgroundPalette.TryCreate(endpoints[..16]);
        if (backgroundGradient is null) backgroundEndpoint = endpoints[..16].ToArray();
        spriteEndpoint = endpoints[16..].ToArray();
    }

    /// <summary>Recognize the complete supplied progression before replacing its samples.
    /// An independently edited sequence that does not follow this rule remains ordinary
    /// caller-owned palette content. Neither path substitutes stock colors for edits.</summary>
    internal static EndingLogoPaletteFade? TryCreate(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != ColorCount * RgbColor.ByteCount) return null;
        Span<RgbColor> endpoints = stackalloc RgbColor[32];
        for (int color = 0; color < 16; color++)
        {
            endpoints[color] = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice((15 * 32 + color) * 2)));
            endpoints[16 + color] = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice((16 + color) * 2)));
        }
        var fade = new EndingLogoPaletteFade(endpoints);
        for (int index = 0; index < ColorCount; index++)
            if (fade.Color(index) != RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * 2))))
                return null;
        return fade;
    }

    /// <summary>Flattened [step0..15][BG,OBJ][color0..15]. BG gains brightness while
    /// OBJ loses brightness. Each channel is evaluated directly; no sample cache exists.</summary>
    internal RgbColor Color(int index)
    {
        if ((uint)index >= ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        int step = index / 32;
        int palette = index / 16 % 2;
        int color = index % 16;
        RgbColor endpoint = palette == 1 ? spriteEndpoint[color] :
            backgroundGradient is not null ? backgroundGradient.Color(color)
            : backgroundEndpoint is not null ? backgroundEndpoint[color]
            : throw new InvalidOperationException("The ending logo fade has neither a background gradient nor endpoint colors.");
        int remaining = palette == 0 ? step : 15 - step;
        int red = ((endpoint.Red) * remaining + 1) / 15;
        int green = ((endpoint.Green) * remaining + 1) / 15;
        int blue = ((endpoint.Blue) * remaining + 1) / 15;
        return new RgbColor(red, green, blue);
    }
}
