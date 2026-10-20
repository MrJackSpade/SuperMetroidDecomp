using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable visual compositions for the six star, station, asteroid, and vortex
/// frames shared by the Ceres approach and destruction scenes. Animation timing
/// and actor motion remain in their compiled owners.
/// </summary>
public sealed class CeresFlightSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Installed Ceres compositions keyed by their native bank-$8C spritemap pointers.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Canonical identity of all selected decoded visual frames, preserving ordered OAM parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(CeresFlightSpritePresentation), frames);

    /// <summary>Retains the validated composition map shared by approach and destruction scenes.</summary>
    private CeresFlightSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Appends an installed Ceres flight composition to OAM in authored part order, choosing the caller's native origin-clipping path without advancing actor motion or animation.</summary>
    /// <param name="pointer">Bank-$8C spritemap identity for one of the six installed star, station, asteroid, or vortex drawings.</param>
    /// <param name="objects">Destination OAM buffer for the ordered sprite parts.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Encoded OBJ palette bits applied only to parts authored to inherit the drawing owner's palette.</param>
    /// <param name="originIsOnScreen">True for ordinary on-screen origin clipping; false for the native off-screen path with opposite Y-wrap clipping for negative origins.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ceres flight sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.Draw(objects, origin, paletteBits);
        else
            frame.DrawOffScreen(objects, origin, paletteBits);
    }

    /// <summary>Loads exactly the six named visual compositions, rejecting duplicate properties, unsupported versions, missing frames, and invalid OAM parts; compiles selected artwork without importing actor timing or motion.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the versioned Ceres flight sprite document.</param>
    /// <returns>The immutable bank-$8C-keyed presentation shared by approach and destruction scenes.</returns>
    public static CeresFlightSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        CeresFlightSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresFlightSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ceres flight sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres flight sprite JSON.", error);
        }
        IReadOnlyList<CeresFlightSpriteFrameDefinition> definitions =
            CeresFlightSpriteDefinitions.Frames;
        if (document.Version != CeresFlightSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException("Ceres flight requires exactly six named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (CeresFlightSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException($"Ceres flight sprite {definition.Name} is missing.");
            var compiled = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            compiled = CeresStarPointParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresLargeAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresStationParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresSmallAsteroidParts.CalculateIfMatching(definition.Pointer, compiled);
            compiled = CeresVortexParts.CalculateIfMatching(definition.Pointer, compiled,
                frames.GetValueOrDefault(CeresFlightSpriteDefinitions.Stars));
            frames.Add(definition.Pointer, compiled);
        }
        return new CeresFlightSpritePresentation(frames);
    }

    /// <summary>Serializes a sprite document using the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing all six required named visual frames.</param>
    public static void Write(Stream json, CeresFlightSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
