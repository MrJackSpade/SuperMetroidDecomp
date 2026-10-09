using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable egg-fragment and slime OAM art, without physical motion rules.</summary>
public sealed class IntroEggEffectSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Installed visual compositions indexed by their original bank-$8C frame pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroEggEffectSpritePresentation), frames);

    /// <summary>Creates a presentation backed by the validated compositions produced by <see cref="Load"/>.</summary>
    /// <param name="frames">Compiled compositions keyed by each frame's native bank-$8C pointer.</param>
    private IntroEggEffectSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed shell-fragment or slime composition to OAM without advancing the actor's motion or animation.</summary>
    /// <param name="pointer">Bank-$8C frame identity: $8F7E..$8FA1 for six fragments or $8FA8..$8FC4 for five slime frames, in seven-byte native record steps.</param>
    /// <param name="oam">Destination object-attribute buffer; parts retain their authored draw order.</param>
    /// <param name="x">Screen-space horizontal origin in pixels, including wrapping unsigned representations of negative coordinates.</param>
    /// <param name="y">Screen-space vertical origin in pixels, including wrapping unsigned representations of negative coordinates.</param>
    /// <param name="paletteBits">Drawing actor's OBJ palette bits, used only by parts whose palette is inherited.</param>
    /// <param name="originIsOnScreen">Whether to use the ordinary origin clipping path; false preserves the negative-origin Y-wrap clipping used by cinematic sprites.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Intro egg effect frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    /// <summary>Validates and compiles all eleven named egg-effect frames from editable JSON into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; the caller retains ownership and it is left open.</param>
    /// <returns>Installed artwork keyed by the original bank-$8C frame identities; later document edits cannot change its compiled parts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or has invalid OAM visual fields.</exception>
    public static IntroEggEffectSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroEggEffectSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<IntroEggEffectSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Intro egg effect sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid intro egg effect sprite JSON.", error);
        }
        IReadOnlyList<IntroEggEffectSpriteFrameDefinition> definitions =
            IntroEggEffectSpriteDefinitions.Frames;
        if (document.Version != IntroEggEffectSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                "Intro egg effects require eleven named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (IntroEggEffectSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Intro egg effect frame {definition.Name} is missing.");
            frames.Add(definition.Pointer,
                IntroEggEffectParts.CalculateIfMatching(definition.Pointer,
                    IntroCinematicSpriteCompiler.Compile(visual, definition.Name)));
        }
        return new IntroEggEffectSpritePresentation(frames);
    }

    /// <summary>Serializes and validates the complete egg-effect document before writing any bytes to the destination.</summary>
    /// <param name="json">Writable destination at its current position; the caller retains ownership and it is left open.</param>
    /// <param name="document">Editable frame set using the supported version and all eleven canonical frame names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The serialized document does not satisfy the loader's schema and visual-field constraints.</exception>
    public static void Write(Stream json, IntroEggEffectSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Mutable JSON authoring data for SR388's six shell fragments, moving slime, and four slime-impact frames; actor physics and instruction timing are not stored here.</summary>
public sealed record IntroEggEffectSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="IntroEggEffectSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned frame arrays keyed by fragment-0..fragment-5, slime-moving, and slime-impact-0..slime-impact-3; array order determines OAM draw order.</summary>
    /// <remarks>Parts use pixel offsets and 8- or 16-pixel OBJ sizes within a 16-column by 32-row tile atlas. A null palette inherits the drawing actor's selector. Loading compiles and copies these values rather than retaining the mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>Installed file identity for the eleven SR388 egg-effect frames.</summary>
public static class IntroEggEffectSpriteFormat
{
    /// <summary>Supported JSON schema revision for the eleven named visual compositions; not a native animation or motion version.</summary>
    public const int Version = 1;
    /// <summary>Installation-relative JSON filename selected by the opening cinematic artwork loader for shell-fragment and slime OAM definitions.</summary>
    public const string FileName = "intro-egg-effect-sprites.json";
}
