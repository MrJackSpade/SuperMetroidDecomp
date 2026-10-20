namespace SuperMetroid.Core.Programs;

/// <summary>The result of advancing a timeline: a newly shown frame, or a hold.</summary>
[IndependentFlag("whether the program held its frame this tick, a fact of the step")]
internal readonly record struct AnimationAdvance<TFrame>(bool Held, TFrame Frame, int Duration)
    where TFrame : struct;
