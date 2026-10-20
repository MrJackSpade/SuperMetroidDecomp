using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable OAM compositions for the 37 post-credits Samus reward frames.</summary>
public sealed class EndingRewardSpritePresentation : IIntroCinematicSpritePresentation
{
    /// <summary>Validated bank-$8C frame pointers mapped to editable OAM compositions.</summary>
    private readonly Dictionary<ushort, SpriteComposition> frames;

    /// <summary>Retains compiled compositions after all published reward frames are checked.</summary>
    private EndingRewardSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingRewardSpritePresentation), frames);

    /// <summary>Draws an installed post-credits Samus composition by its bank-$8C identity.</summary>
    /// <param name="pointer">Bank-relative native OAM record pointer.</param>
    /// <param name="objects">Destination OAM buffer.</param>
    /// <param name="origin">Native wrapped sixteen-bit screen origin in pixels.</param>
    /// <param name="paletteBits">Packed OBJ palette bits inherited by applicable parts.</param>
    /// <param name="originIsOnScreen">Whether the origin uses on-screen wrapping rules.</param>
    public void Draw(ushort pointer, ScreenSprites objects, Position16<OamPixels> origin,
        ushort paletteBits, bool originIsOnScreen) =>
        SpriteComposition.DrawInstalled(frames, "Ending reward sprite", pointer, objects, origin, paletteBits, originIsOnScreen);

    /// <summary>Loads and validates all 37 named post-credits Samus visual frames.</summary>
    public static EndingRewardSpritePresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        EndingRewardSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingRewardSpriteDocument>(
                ArtworkJson.Options) ??
                throw new InvalidDataException("Ending reward sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending reward sprite JSON.", error);
        }
        IReadOnlyList<SpriteFrameDefinition> definitions =
            EndingRewardSpriteDefinitions.Frames;
        if (document.Version != EndingRewardSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending reward requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (SpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending reward frame {definition.Name} is missing.");
            SpriteComposition composition = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            composition = EndingRewardHeadParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardStandingParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardPrepareJumpParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardJumpParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardSuitlessGridParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardSuitlessStandingParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardShootingSceneParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardArmParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardHairParts.CalculateIfMatching(definition.Pointer, composition);
            frames.Add(definition.Pointer, composition);
        }
        return new EndingRewardSpritePresentation(frames);
    }

    /// <summary>Validates and writes an ending-reward sprite document as JSON.</summary>
    public static void Write(Stream json, EndingRewardSpriteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
