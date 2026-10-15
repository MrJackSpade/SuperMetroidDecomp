namespace SuperMetroid.Core.Programs;

/// <summary>The result of advancing a program: a new frame, a wait, a hold or a stop.</summary>
/// <param name="Kind">How the advance ended.</param>
/// <param name="Frame">The frame shown, for <see cref="AnimationProgramAdvanceKind.Show"/>.</param>
/// <param name="Duration">The updates until the next advance, for a show or a wait.</param>
internal readonly record struct AnimationProgramAdvance<TFrame>(AnimationProgramAdvanceKind Kind, TFrame Frame, int Duration)
    where TFrame : struct;
