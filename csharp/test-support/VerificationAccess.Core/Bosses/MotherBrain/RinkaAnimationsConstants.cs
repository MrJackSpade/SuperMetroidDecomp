using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="RinkaAnimations"/> that only verification reads.</summary>
internal static class RinkaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RinkaProgram[] programs) =>
        programs.SelectMany(program => RinkaAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<RinkaFrame, RinkaCue>.Frame>()
            .Select(beat => RinkaFrameToolingDefinitions.Spritemap(beat.Shown));
}
