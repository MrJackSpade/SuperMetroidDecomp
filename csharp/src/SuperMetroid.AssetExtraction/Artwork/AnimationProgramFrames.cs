using SuperMetroid.Core.Programs;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The frames an animation program family can show (#641): the import pipeline composes a sheet of every frame a
/// spawned object's programs reach, following each goto, fall-through and counted repeat.
/// </summary>
internal static class AnimationProgramFrames
{
    /// <summary>The frames of one program family.</summary>
    extension<TProgram, TFrame, TCue>(AnimationProgramSet<TProgram, TFrame, TCue> programs)
        where TProgram : struct
        where TFrame : struct
        where TCue : notnull
    {
        /// <summary>Every frame <paramref name="start"/> can show, through each program it goes, falls or repeats into.</summary>
        /// <exception cref="InvalidOperationException">A program holds a step of an unknown kind.</exception>
        internal IEnumerable<TFrame> FramesFrom(TProgram start)
        {
            var visited = new HashSet<TProgram>();
            var pending = new Stack<TProgram>([start]);
            var frames = new List<TFrame>();
            while (pending.TryPop(out TProgram program))
            {
                if (!visited.Add(program))
                    continue;
                foreach (AnimationProgramStep<TProgram, TFrame, TCue> step in programs.Steps(program))
                {
                    switch (step)
                    {
                        case AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep show:
                            frames.Add(show.Frame);
                            break;
                        case AnimationProgramStep<TProgram, TFrame, TCue>.GoToStep go:
                            pending.Push(go.Target);
                            break;
                        case AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep fall:
                            pending.Push(fall.Target);
                            break;
                        case AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep repeat:
                            pending.Push(repeat.Target);
                            break;
                        // These show nothing and lead nowhere else.
                        case AnimationProgramStep<TProgram, TFrame, TCue>.CueStep:
                        case AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep:
                        case AnimationProgramStep<TProgram, TFrame, TCue>.WaitStep:
                        case AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep:
                        case AnimationProgramStep<TProgram, TFrame, TCue>.StopStep:
                            break;
                        default:
                            throw new InvalidOperationException($"Program {program} has an unknown step {step}.");
                    }
                }
            }
            return frames.Distinct();
        }
    }
}
