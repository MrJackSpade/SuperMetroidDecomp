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
    private readonly byte[]? nativeBytes;
    private readonly EndingLogoPaletteFade? logoFade;

    private EndingPalette(byte[] nativeBytes) => this.nativeBytes = nativeBytes;
    private EndingPalette(EndingLogoPaletteFade logoFade) => this.logoFade = logoFade;

    /// <summary>Transfer/export view. A computed fade is materialized only for this
    /// request, never stored as a generated lookup cache; gameplay calls Color directly.</summary>
    public ReadOnlyMemory<byte> Transfer
    {
        get
        {
            if (nativeBytes is not null) return nativeBytes;
            var bytes = new byte[ColorCount * sizeof(ushort)];
            WriteColors(bytes, 0, ColorCount);
            return bytes;
        }
    }
    public int ColorCount => nativeBytes is not null ? nativeBytes.Length / sizeof(ushort) : EndingLogoPaletteFade.ColorCount;

    public ushort Color(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (logoFade is not null) return logoFade.Color(index);
        return BinaryPrimitives.ReadUInt16LittleEndian(
            nativeBytes!.AsSpan(index * sizeof(ushort)));
    }

    /// <summary>Preserves the cartridge's partial source/destination CGRAM transfers.</summary>
    public void LoadTo(SnesCgram cgram, int sourceColor, int count, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (sourceColor < 0 || count < 0 || sourceColor > ColorCount - count)
            throw new ArgumentOutOfRangeException(nameof(sourceColor));
        if (nativeBytes is not null)
            cgram.LoadBytes(nativeBytes.AsSpan(sourceColor * sizeof(ushort),
                count * sizeof(ushort)), destinationColor);
        else
        {
            Span<byte> transfer = stackalloc byte[count * sizeof(ushort)];
            WriteColors(transfer, sourceColor, count);
            cgram.LoadBytes(transfer, destinationColor);
        }
    }

    private void WriteColors(Span<byte> bytes, int sourceColor, int count)
    {
        for (int i = 0; i < count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.Slice(i * sizeof(ushort)), Color(sourceColor + i));
    }

    public static EndingPalette Load(Stream json, EndingPaletteId id)
    {
        EndingPaletteDocument document = JsonAssetDocument.Read<EndingPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, $"ending {id} palette");
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
        if (id == EndingPaletteId.LogoCrossfade && EndingLogoPaletteFade.TryCreate(native) is { } fade)
            return new EndingPalette(fade);
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
    private readonly EndingPalette escape, postCredits, credits, explosion, finalGunship, logoInitial, logoCrossfade;

    public EndingPaletteCatalog(EndingPalette escape, EndingPalette postCredits,
        EndingPalette credits, EndingPalette explosion, EndingPalette finalGunship,
        EndingPalette logoInitial, EndingPalette logoCrossfade)
    {
        this.escape = escape;
        this.postCredits = postCredits;
        this.credits = credits;
        this.explosion = explosion;
        this.finalGunship = finalGunship;
        this.logoInitial = logoInitial;
        this.logoCrossfade = logoCrossfade;
    }

    public EndingPalette this[EndingPaletteId id] => id switch
    {
        EndingPaletteId.Escape => escape,
        EndingPaletteId.PostCredits => postCredits,
        EndingPaletteId.Credits => credits,
        EndingPaletteId.Explosion => explosion,
        EndingPaletteId.FinalGunship => finalGunship,
        EndingPaletteId.LogoInitial => logoInitial,
        EndingPaletteId.LogoCrossfade => logoCrossfade,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Identity of all selected static colors and every ordered logo-crossfade color.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingPaletteCatalog), content =>
    {
        content.Append("palette-count", 7);
        content.Append("palette", escape.Transfer.Span);
        content.Append("palette", postCredits.Transfer.Span);
        content.Append("palette", credits.Transfer.Span);
        content.Append("palette", explosion.Transfer.Span);
        content.Append("palette", finalGunship.Transfer.Span);
        content.Append("palette", logoInitial.Transfer.Span);
        content.Append("palette", logoCrossfade.Transfer.Span);
    });
}

/// <summary>Native source addresses, exact sizes, and file identities for ending colors.</summary>
public static class EndingPaletteDefinitions
{
    // The document shape is unchanged; only the installation manifest gains files.
    // Keeping version one lets existing player-authored palette overrides survive.
    public const int Version = 1;
    public const string ManifestFileName = "ending-palettes-manifest.json";

    /// <summary>Six named contiguous resources selected by native ending load operations.
    /// Crossfade has two interleaved sources per step and deliberately has no single address.</summary>
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

    /// <summary>Full editable resource sizes, not individual CGRAM transfer lengths:
    /// four 256-color bank-$8C allocations, two 16-color palettes, and sixteen pairs
    /// of 16-color crossfade palettes. Native credits transfers can select a subset.</summary>
    public static int ColorCount(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape or EndingPaletteId.PostCredits or EndingPaletteId.Credits or
            EndingPaletteId.Explosion => SnesCgram.ColorCount,
        EndingPaletteId.FinalGunship => 16,
        EndingPaletteId.LogoInitial => 16,
        EndingPaletteId.LogoCrossfade => EndingLogoDefinitions.PaletteSteps * 2 * 16,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Published asset-file identities selected by semantic palette role.
    /// Explicit cases preserve independently editable stock and override filenames.</summary>
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
