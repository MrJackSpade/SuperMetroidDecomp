using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Cinematic texts typed one glyph per step (#1513): each typing animation shows its poses in order, each pose the one before
/// it and one more glyph. A sheet pictures each text's glyphs once, and a typed pose is drawn as the glyphs typed so far,
/// so no step is pictured whole.
/// </summary>
internal sealed class CinematicTypedTexts
{
    /// <summary>Each typed pose's step, text by text in typing order.</summary>
    internal readonly Dictionary<CinematicSpritePose, CinematicTypedStep> steps = [];

    /// <summary>The texts <paramref name="segments"/> type, each pose its animation shows a step of it.</summary>
    /// <exception cref="InvalidDataException">Two texts show one pose.</exception>
    internal CinematicTypedTexts(IEnumerable<CinematicSpriteSegment> segments)
    {
        foreach (CinematicSpriteSegment segment in Ensure.NotNull(segments))
        {
            var typed = new List<CinematicSpritePose>();
            foreach (AnimationBeat<CinematicSpritePose, CinematicSpriteCue> beat in CinematicSpriteAnimations.Segments.Beats(segment))
            {
                if (beat is not AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame shown
                    || shown.Shown == CinematicSpritePose.Blank || typed.Contains(shown.Shown))
                    continue;
                if (!steps.TryAdd(shown.Shown, new CinematicTypedStep(segment, typed.Count)))
                    throw new InvalidDataException($"Pose {shown.Shown.Index} is a step of two typed texts.");
                typed.Add(shown.Shown);
            }
        }
    }

    /// <summary>The step <paramref name="pose"/> shows, or none for a pose that is no step of a typed text.</summary>
    internal CinematicTypedStep? Find(CinematicSpritePose pose) => steps.TryGetValue(pose, out CinematicTypedStep step) ? step : null;
}
