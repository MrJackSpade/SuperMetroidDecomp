using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of Samus's atmospheric effects (#1392) over the standard OBJ sheet: the footstep and shared
/// lava-and-dust small sprites in each frame, as their installed attributes name them, and the diving splash's and
/// bubbles' spritemaps, coloured as Samus's other sheets are.
/// </summary>
internal static class SamusAtmosphereSheetComposer
{
    /// <summary>The atmospheric type whose list the footstep splash draws.</summary>
    private const byte FootstepType = (byte)SamusAtmosphericEffectType.FootstepSplash;

    /// <summary>The atmospheric type whose shared list the lava spray and dust draw.</summary>
    private const byte LavaAndDustType = (byte)SamusAtmosphericEffectType.LavaSurfaceDamage;

    /// <summary>What the atmosphere sheet composes with the body's spritemaps and atmospheric attributes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs) =>
        Sources(inputs, Ensure.NotNull(inputs).SamusBody.Body.Spritemaps.Source, inputs.SamusBody.Body.Atmosphere.Source);

    /// <summary>What the atmosphere sheet composes with <paramref name="spritemaps"/> and <paramref name="atmosphere"/>.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SamusSpritemapSources spritemaps,
        SamusAtmosphericSourceArtwork atmosphere)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(spritemaps);
        Ensure.NotNull(atmosphere);
        var frames = new List<SheetFrameSource>();
        for (int frame = 0; frame < SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount; frame++)
        {
            frames.Add(Direct(inputs, atmosphere, SamusAtmosphereSheetDefinitions.FootstepFrame(frame), FootstepType, frame));
            frames.Add(Direct(inputs, atmosphere, SamusAtmosphereSheetDefinitions.LavaAndDustFrame(frame), LavaAndDustType, frame));
        }
        for (int frame = 0; frame < SamusAtmosphereSheetDefinitions.DivingSplashFrameCount; frame++)
        {
            frames.Add(Spritemap(inputs, spritemaps, SamusAtmosphereSheetDefinitions.DivingSplashFrame(frame),
                SamusAtmosphereSheetDefinitions.DivingSplashFirstSpritemap + frame));
        }
        for (int frame = 0; frame < SamusAtmosphereSheetDefinitions.BubblesFrameCount; frame++)
        {
            frames.Add(Spritemap(inputs, spritemaps, SamusAtmosphereSheetDefinitions.BubblesFrame(frame),
                SamusAtmosphereSheetDefinitions.BubblesFirstSpritemap + frame));
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }

    /// <summary>A small sprite whose installed attributes name its character, palette, priority and flips.</summary>
    private static SheetFrameSource Direct(SamusSheetInputs inputs, SamusAtmosphericSourceArtwork atmosphere, string name,
        int type, int frame)
    {
        if (!atmosphere.TryResolve(type, frame, out SpritePart appearance))
            throw new InvalidDataException($"Atmospheric type {type} frame {frame} has no installed sprite.");
        return new(name, SpritePartFrames.SmallObject(appearance, 0), inputs.StandardObjects);
    }

    /// <summary>
    /// A Samus spritemap over the standard OBJ sheet. A zero entry in the installed pointer table names direct-page
    /// scratch the port does not model and draws nothing (owner decision on #1318), so its frame is empty.
    /// </summary>
    private static SheetFrameSource Spritemap(SamusSheetInputs inputs, SamusSpritemapSources spritemaps, string name, int index) =>
        new(name, spritemaps.TryGetFrame(index, out SpriteFrame? frame) ? frame : new SpriteFrame(name, []), inputs.StandardObjects);
}
