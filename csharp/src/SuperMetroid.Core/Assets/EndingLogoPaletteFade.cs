using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>Sixteen-step logo RGB5 interpolation, stored as two caller-supplied endpoints.
/// Original $8C:EFE9..F3E8 channels use biased truncation: divide the endpoint times
/// remaining steps by15, rounding up only a remainder of14. Every remainder0..14
/// occurs in the original data and has this same result across both palette families.
/// Thus (channel*remaining+1)/15 reproduces all1536 channels without correction entries.
/// This identifies the exact quantizer, not the historical tool that generated it.</summary>
internal sealed class EndingLogoPaletteFade
{
    internal const int ColorCount = 16 * 2 * 16;
    private readonly ushort[] spriteEndpoint;
    private readonly ushort[]? backgroundEndpoint;
    private readonly EndingLogoBackgroundPalette? backgroundGradient;

    private EndingLogoPaletteFade(ReadOnlySpan<ushort> endpoints)
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
        if (bytes.Length != ColorCount * sizeof(ushort)) return null;
        Span<ushort> endpoints = stackalloc ushort[32];
        for (int color = 0; color < 16; color++)
        {
            endpoints[color] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice((15 * 32 + color) * 2));
            endpoints[16 + color] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice((16 + color) * 2));
        }
        var fade = new EndingLogoPaletteFade(endpoints);
        for (int index = 0; index < ColorCount; index++)
            if (fade.Color(index) != BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * 2)))
                return null;
        return fade;
    }

    /// <summary>Flattened [step0..15][BG,OBJ][color0..15]. BG gains brightness while
    /// OBJ loses brightness. Each channel is evaluated directly; no sample cache exists.</summary>
    internal ushort Color(int index)
    {
        if ((uint)index >= ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        int step = index / 32;
        int palette = index / 16 % 2;
        int color = index % 16;
        ushort endpoint = palette == 1 ? spriteEndpoint[color] :
            backgroundGradient is not null ? backgroundGradient.Color(color) : backgroundEndpoint![color];
        int remaining = palette == 0 ? step : 15 - step;
        int red = ((endpoint & 31) * remaining + 1) / 15;
        int green = (((endpoint >> 5) & 31) * remaining + 1) / 15;
        int blue = (((endpoint >> 10) & 31) * remaining + 1) / 15;
        return (ushort)(red | green << 5 | blue << 10);
    }
}
