namespace SuperMetroid.Core.Fx;

/// <summary>The result of advancing a timeline: a newly shown frame, or a hold.</summary>
internal readonly record struct AnimationAdvance<TFrame>(bool Held, TFrame Frame, int Duration)
    where TFrame : struct;
