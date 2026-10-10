using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Independent native color resources selected by the ending coroutine.</summary>
public enum EndingPaletteId
{
    /// <summary>$8C:EDE9, Palettes_CloudSpritesInZebesExplosionScene: 256-color image installed for the initial escape view and its atmospheric cloud sprites.</summary>
    Escape,
    /// <summary>$8C:E7E9, Palettes_PostCredits: 256-color reward-scene image; post-credits restoration preserves CGRAM colors 0 through 3.</summary>
    PostCredits,
    /// <summary>$8C:E9E9, Palettes_Credits: 256-color resource whose first 128 colors supply the scrolling credits backgrounds and text.</summary>
    Credits,
    /// <summary>$8C:EBE9, Palettes_ZebesExplosionScene: 256-color resource whose BG and OBJ halves are transferred separately during destruction and flyaway scenes.</summary>
    Explosion,
    /// <summary>$8B:DE43, .greyGunshipPalette: 16 BG colors restored at CGRAM index 80 for the final gunship view before the operation text.</summary>
    FinalGunship,
    /// <summary>$8C:EFE9, Palettes_EndingSuperMetroidIconFadingToGrey_Sprite_0: 16 colors initially loaded into OBJ palette 7 for the assembling logo.</summary>
    LogoInitial,
    /// <summary>$8B:E5E7 palette-pointer pairs consumed by E58A: sixteen successive BG/OBJ palette pairs, flattened as [step][BG then OBJ][16 colors].</summary>
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
            var bytes = new byte[ColorCount * Bgr555.ByteCount];
            WriteColors(bytes, 0, ColorCount);
            return bytes;
        }
    }
    /// <summary>Number of color words in the complete resource, including all 512 ordered colors when this is the sixteen-step logo crossfade.</summary>
    public int ColorCount => nativeBytes is not null ? nativeBytes.Length / Bgr555.ByteCount : EndingLogoPaletteFade.ColorCount;

    /// <summary>Reads one selected color, evaluating a recognized logo fade directly rather than materializing its transfer image.</summary>
    /// <param name="index">Zero-based color position within <see cref="ColorCount"/>; crossfade positions use (step * 2 + palette) * 16 + color, with BG palette 0 and OBJ palette 1.</param>
    /// <returns>A native BGR555 word: red in bits 0..4, green in 5..9 and blue in 10..14.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this resource.</exception>
    public Bgr555 Color(int index)
    {
        if ((uint)index >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (logoFade is not null) return logoFade.Color(index);
        return Bgr555.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(
            nativeBytes!.AsSpan(index * Bgr555.ByteCount)));
    }

    /// <summary>Preserves the cartridge's partial source/destination CGRAM transfers.</summary>
    /// <param name="cgram">Destination color memory; colors outside the requested transfer are left unchanged.</param>
    /// <param name="sourceColor">Zero-based starting color in this resource, not a byte offset.</param>
    /// <param name="count">Number of consecutive color words to transfer; zero is allowed.</param>
    /// <param name="destinationColor">Zero-based starting CGRAM color index; the complete transfer must fit its 256 colors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The source range or destination range is outside its color memory.</exception>
    public void LoadTo(SnesCgram cgram, int sourceColor, int count, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (sourceColor < 0 || count < 0 || sourceColor > ColorCount - count)
            throw new ArgumentOutOfRangeException(nameof(sourceColor));
        if (nativeBytes is not null)
            cgram.LoadBytes(nativeBytes.AsSpan(sourceColor * Bgr555.ByteCount,
                count * Bgr555.ByteCount), destinationColor);
        else
        {
            Span<byte> transfer = stackalloc byte[count * Bgr555.ByteCount];
            WriteColors(transfer, sourceColor, count);
            cgram.LoadBytes(transfer, destinationColor);
        }
    }

    private void WriteColors(Span<byte> bytes, int sourceColor, int count)
    {
        for (int i = 0; i < count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.Slice(i * Bgr555.ByteCount), Color(sourceColor + i).ToWord());
    }

    /// <summary>Validates RGB5 JSON against the chosen resource's exact color count and compiles an independent native color representation.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <param name="id">Palette role determining the required full resource size, not the size of an individual CGRAM transfer.</param>
    /// <returns>Selected colors copied from the document; a logo crossfade matching the interpolation rule is represented by its endpoints without changing any supplied colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The JSON is invalid or ambiguous, the version or color count is wrong, or a color is null or has a channel outside 0..31.</exception>
    public static EndingPalette Load(Stream json, EndingPaletteId id)
    {
        EndingPaletteDocument document = JsonAssetDocument.Read<EndingPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, $"ending {id} palette");
        int count = EndingPaletteDefinitions.ColorCount(id);
        if (document.Version != EndingPaletteDefinitions.Version ||
            document.Colors is null || document.Colors.Length != count)
            throw new InvalidDataException($"Ending {id} palette requires {count} RGB5 colors.");
        var native = new byte[count * Bgr555.ByteCount];
        for (int index = 0; index < count; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ending {id} palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * Bgr555.ByteCount),
                color.ToBgr555().ToWord());
        }
        if (id == EndingPaletteId.LogoCrossfade && EndingLogoPaletteFade.TryCreate(native) is { } fade)
            return new EndingPalette(fade);
        return new EndingPalette(native);
    }

    /// <summary>Serializes and validates the entire selected palette before writing its UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="id">Palette role used to validate the document's color count.</param>
    /// <param name="document">Authored color collection read for serialization, not retained by the writer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or RGB5 validation.</exception>
    public static void Write(Stream json, EndingPaletteId id, EndingPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false), id);
        json.Write(bytes);
    }
}

/// <summary>Editable RGB5 color document for one externally selected ending palette role; the color array remains caller-mutable until compilation.</summary>
public sealed record EndingPaletteDocument
{
    /// <summary>Schema revision; loading requires <see cref="EndingPaletteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Ordered, nonnull RGB5 entries with each channel in 0..31; the role requires 256, 16 or 512 colors. Logo crossfade order is sixteen steps of 16 BG colors followed by 16 OBJ colors.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Independent static and animated color resources used by the ending.</summary>
public sealed class EndingPaletteCatalog
{
    private readonly EndingPalette escape, postCredits, credits, explosion, finalGunship, logoInitial, logoCrossfade;

    /// <summary>Groups seven independently selected immutable palette resources without copying them or validating their sizes.</summary>
    /// <param name="escape">Full palette for escape and cloud imagery.</param>
    /// <param name="postCredits">Full palette for the post-credits reward sequence.</param>
    /// <param name="credits">Full credits resource, of which scrolling text uses the BG half.</param>
    /// <param name="explosion">Full destruction/flyaway resource with separately transferred BG and OBJ halves.</param>
    /// <param name="finalGunship">Sixteen colors for the final gunship's BG palette 5.</param>
    /// <param name="logoInitial">Sixteen initial assembling-logo colors for OBJ palette 7.</param>
    /// <param name="logoCrossfade">Sixteen ordered BG/OBJ palette pairs for the final logo transition.</param>
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

    /// <summary>Returns the retained palette resource selected by its ending-scene role.</summary>
    /// <param name="id">Independent static palette or complete logo-crossfade resource to retrieve.</param>
    /// <returns>The same palette instance supplied for that role at construction.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
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
    /// <summary>Revision 1 of the RGB5 palette document, retained for compatibility with existing authored overrides.</summary>
    public const int Version = 1;
    /// <summary>Installation manifest filename identifying the independently selected ending palette files.</summary>
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
