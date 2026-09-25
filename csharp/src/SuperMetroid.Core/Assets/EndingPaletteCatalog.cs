using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Independent native color resources selected by the ending coroutine.</summary>
public enum EndingPaletteId
{
    Escape,
    PostCredits,
    Credits,
    Explosion,
    FinalGunship,
    LogoInitial,
    LogoCrossfade,
}

/// <summary>One exact BGR555 color image, independently editable as RGB5 JSON.</summary>
public sealed class EndingPalette
{
    private readonly byte[] nativeBytes;

    private EndingPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;

    public ReadOnlyMemory<byte> Transfer => nativeBytes;
    public int ColorCount => nativeBytes.Length / sizeof(ushort);

    public ushort Color(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return BinaryPrimitives.ReadUInt16LittleEndian(
            nativeBytes.AsSpan(index * sizeof(ushort)));
    }

    /// <summary>Preserves the cartridge's partial source/destination CGRAM transfers.</summary>
    public void LoadTo(SnesCgram cgram, int sourceColor, int count, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (sourceColor < 0 || count < 0 || sourceColor > ColorCount - count)
            throw new ArgumentOutOfRangeException(nameof(sourceColor));
        cgram.LoadBytes(nativeBytes.AsSpan(sourceColor * sizeof(ushort),
            count * sizeof(ushort)), destinationColor);
    }

    public static EndingPalette Load(Stream json, EndingPaletteId id)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingPaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<EndingPaletteDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException($"Ending {id} palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid ending {id} palette JSON.", error);
        }
        int count = EndingPaletteDefinitions.ColorCount(id);
        if (document.Version != EndingPaletteDefinitions.Version ||
            document.Colors is null || document.Colors.Length != count)
            throw new InvalidDataException($"Ending {id} palette requires {count} RGB5 colors.");
        var native = new byte[count * sizeof(ushort)];
        for (int index = 0; index < count; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ending {id} palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)),
                (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
        }
        return new EndingPalette(native);
    }

    public static void Write(Stream json, EndingPaletteId id, EndingPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), id);
        json.Write(bytes);
    }
}

public sealed record EndingPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Independent static and animated color resources used by the ending.</summary>
public sealed class EndingPaletteCatalog
{
    private readonly EndingPalette[] palettes;

    public EndingPaletteCatalog(EndingPalette escape, EndingPalette postCredits,
        EndingPalette credits, EndingPalette explosion, EndingPalette finalGunship,
        EndingPalette logoInitial, EndingPalette logoCrossfade)
    {
        palettes = [escape, postCredits, credits, explosion, finalGunship,
            logoInitial, logoCrossfade];
    }

    public EndingPalette this[EndingPaletteId id] => palettes[(int)id];
}

/// <summary>Native source addresses, exact sizes, and file identities for ending colors.</summary>
public static class EndingPaletteDefinitions
{
    // The document shape is unchanged; only the installation manifest gains files.
    // Keeping version one lets existing player-authored palette overrides survive.
    public const int Version = 1;
    public const string ManifestFileName = "ending-palettes-manifest.json";

    public static int SourceAddress(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape => EndingCreditsRomData.Assets.EscapePalette,
        EndingPaletteId.PostCredits => EndingCreditsRomData.Assets.PostCreditsPalette,
        EndingPaletteId.Credits => EndingCreditsRomData.Assets.CreditsPalette,
        EndingPaletteId.Explosion => EndingCreditsRomData.Assets.ExplosionPalette,
        EndingPaletteId.FinalGunship => EndingCreditsRomData.Assets.FinalGunshipPalette,
        EndingPaletteId.LogoInitial => EndingLogoDefinitions.InitialPalette,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    public static int ColorCount(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape or EndingPaletteId.PostCredits or EndingPaletteId.Credits or
            EndingPaletteId.Explosion => SnesCgram.ColorCount,
        EndingPaletteId.FinalGunship => 16,
        EndingPaletteId.LogoInitial => 16,
        EndingPaletteId.LogoCrossfade => EndingLogoDefinitions.PaletteSteps * 2 * 16,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    public static string FileName(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape => "ending-escape-palette.json",
        EndingPaletteId.PostCredits => "ending-post-credits-palette.json",
        EndingPaletteId.Credits => "ending-credits-palette.json",
        EndingPaletteId.Explosion => "ending-explosion-palette.json",
        EndingPaletteId.FinalGunship => "ending-final-gunship-palette.json",
        EndingPaletteId.LogoInitial => "ending-logo-initial-palette.json",
        EndingPaletteId.LogoCrossfade => "ending-logo-crossfade-palette.json",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
