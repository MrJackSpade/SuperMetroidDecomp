using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable egg-fragment and slime OAM art, without physical motion rules.</summary>
public sealed class IntroEggEffectSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Compiled shell and slime OAM compositions keyed by their original bank-$8C pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(IntroEggEffectSpritePresentation), frames);

    /// <summary>Retains the validated eleven-frame set after authoring arrays have been copied.</summary>
    private IntroEggEffectSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed shell-fragment or slime composition to OAM without advancing the actor's motion or animation.</summary>
    /// <param name="pointer">Bank-$8C frame identity: $8F7E..$8FA1 for six fragments or $8FA8..$8FC4 for five slime frames, in seven-byte native record steps.</param>
    /// <param name="objects">Destination object-attribute buffer; parts retain their authored draw order.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Drawing actor's OBJ palette bits, used only by parts whose palette is inherited.</param>
    /// <param name="originIsOnScreen">Whether to use the ordinary origin clipping path; false preserves the negative-origin Y-wrap clipping used by cinematic sprites.</param>
    /// <exception cref="InvalidDataException">The requested frame identity is not installed.</exception>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Intro egg effect", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Validates and compiles all eleven named egg-effect frames from editable JSON into independently owned visual compositions.</summary>
    /// <param name="json">Readable JSON stream at its current position; the caller retains ownership and it is left open.</param>
    /// <returns>Installed artwork keyed by the original bank-$8C frame identities; later document edits cannot change its compiled parts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is malformed, contains duplicate properties, has an unsupported version or frame set, or has invalid OAM visual fields.</exception>
    public static IntroEggEffectSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroEggEffectSpriteDocument document;
        document = JsonAssetDocument.Read<IntroEggEffectSpriteDocument>(json, ArtworkJson.Options, "intro egg effect sprite");
        IReadOnlyList<SpritemapFrameDefinition> definitions =
            IntroEggEffectSpriteDefinitions.Frames;
        if (document.Version != IntroEggEffectSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                "Intro egg effects require eleven named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (SpritemapFrameDefinition definition in definitions)
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
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
