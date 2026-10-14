namespace SuperMetroid.Core.Fx;

/// <summary>Where a program in an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> resumes.</summary>
/// <param name="Program">The program being run.</param>
/// <param name="Position">The step to run next.</param>
internal readonly record struct AnimationProgramCursor<TProgram>(TProgram Program, int Position)
    where TProgram : struct, Enum
{
    /// <summary>The start of <paramref name="program"/>.</summary>
    internal static AnimationProgramCursor<TProgram> Start(TProgram program) => new(program, 0);
}
