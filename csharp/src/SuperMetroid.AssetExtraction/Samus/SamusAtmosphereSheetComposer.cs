using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of Samus's atmospheric effects (#1392) over the standard OBJ sheet: the footstep and shared
/// lava-and-dust small sprites in each frame, as their installed attributes name them, and the diving splash's and
/// bubbles' spritemaps, coloured as Samus's other sheets are.
/// </summary>
internal static class SamusAtmosphereSheetComposer
{
    /// <summary>Every frame the sheet holds.</summary>
    internal static IEnumerable<string> Frames =>
        SamusAtmosphereSheetDefinitions.FootstepSplash.Concat(SamusAtmosphereSheetDefinitions.LavaAndDust)
            .Concat(SamusAtmosphereSheetDefinitions.DivingSplash).Concat(SamusAtmosphereSheetDefinitions.Bubbles)
            .Select(SamusAtmosphereSheetDefinitions.FrameName);

    /// <summary>What the atmosphere sheet composes with the body's spritemaps and atmospheric attributes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs) =>
        Sources(inputs, Ensure.NotNull(inputs).SamusBody.Body.Spritemaps, inputs.SamusBody.Body.Atmosphere);

    /// <summary>What the atmosphere sheet composes with <paramref name="spritemaps"/> and <paramref name="atmosphere"/>.</summary>
    private static SpriteSheetSources Sources(SamusSheetInputs inputs, SamusSpritemapSources spritemaps,
        SamusAtmosphericSourceArtwork atmosphere)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(atmosphere);
        var frames = new List<SheetFrameSource>();
        // Each picture of a small-sprite effect shows the effect's appearance at its own frame of the effect's animation.
        frames.AddRange(SamusAtmosphereSheetDefinitions.FootstepSplash.Select((picture, frame) =>
            Direct(inputs, atmosphere, SamusAtmosphereSheetDefinitions.FrameName(picture), SamusAtmosphericPictures.FootstepSplash, frame)));
        frames.AddRange(SamusAtmosphereSheetDefinitions.LavaAndDust.Select((picture, frame) =>
            Direct(inputs, atmosphere, SamusAtmosphereSheetDefinitions.FrameName(picture), SamusAtmosphericPictures.LavaAndDust, frame)));
        foreach (SamusAtmosphereFrame frame in SamusAtmosphereSheetDefinitions.DivingSplash.Concat(SamusAtmosphereSheetDefinitions.Bubbles))
            frames.Add(Spritemap(inputs, spritemaps, SamusAtmosphereSheetDefinitions.FrameName(frame)));
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades);
    }

    /// <summary>A small sprite whose installed attributes name its character, palette, depth and flips.</summary>
    private static SheetFrameSource Direct(SamusSheetInputs inputs, SamusAtmosphericSourceArtwork atmosphere, string name,
        SamusAtmosphericPictures type, int frame)
    {
        if (!atmosphere.TryResolve(type, frame, out SpritePart appearance))
            throw new InvalidDataException($"Atmospheric type {type} frame {frame} has no installed sprite.");
        return new(name, SpritePartFrames.SmallObject(appearance, 0), inputs.StandardObjects);
    }

    /// <summary>
    /// The Samus spritemap the atmosphere frame <paramref name="name"/> shows over the standard OBJ sheet. A frame the import
    /// decoded no composition for names direct-page scratch the port does not model and draws nothing (owner decision on
    /// #1318), so its frame is empty.
    /// </summary>
    private static SheetFrameSource Spritemap(SamusSheetInputs inputs, SamusSpritemapSources spritemaps, string name) =>
        new(name, spritemaps.TryGetAtmosphereFrame(name, out SpriteFrame? frame) ? frame : new SpriteFrame(name, []), inputs.StandardObjects);
}
