using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Cinematic texts typed one glyph per step (#1513): each typing program shows its poses in order, each pose the one before
/// it and one more glyph. A sheet pictures each text's glyphs once, and a typed pose is drawn as the glyphs typed so far,
/// so no step is pictured whole.
/// </summary>
internal sealed class CinematicTypedTexts
{
    /// <summary>Each typed pose's step, text by text in typing order.</summary>
    internal readonly Dictionary<CinematicSpritePose, CinematicTypedStep> steps = [];

    /// <summary>The texts <paramref name="programs"/> type, each pose its program shows a step of it.</summary>
    /// <exception cref="InvalidDataException">Two texts show one pose.</exception>
    internal CinematicTypedTexts(IEnumerable<CinematicSpriteProgram> programs)
    {
        foreach (CinematicSpriteProgram program in Ensure.NotNull(programs))
        {
            var typed = new List<CinematicSpritePose>();
            foreach (AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> step in CinematicSpriteAnimations.Programs.Steps(program))
            {
                if (step is not AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.ShowStep shown
                    || shown.Frame == CinematicSpritePose.Blank || typed.Contains(shown.Frame))
                    continue;
                if (!steps.TryAdd(shown.Frame, new CinematicTypedStep(program, typed.Count)))
                    throw new InvalidDataException($"Pose {shown.Frame.Index} is a step of two typed texts.");
                typed.Add(shown.Frame);
            }
        }
    }

    /// <summary>The step <paramref name="pose"/> shows, or none for a pose that is no step of a typed text.</summary>
    internal CinematicTypedStep? Find(CinematicSpritePose pose) => steps.TryGetValue(pose, out CinematicTypedStep step) ? step : null;
}
