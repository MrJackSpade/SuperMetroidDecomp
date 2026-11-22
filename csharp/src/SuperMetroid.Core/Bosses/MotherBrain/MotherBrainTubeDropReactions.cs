namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the tube collapse does on each <see cref="MotherBrainTubeDrop"/> (#2553).</summary>
/// <typeparam name="TContext">What each reaction reads besides the drop.</typeparam>
/// <param name="Bottom">Drops a bottom tube.</param>
/// <param name="Top">Drops a top tube.</param>
/// <param name="Main">Drops the main tube.</param>
internal sealed record MotherBrainTubeDropReactions<TContext>(
    Action<MotherBrainBottomTubeDrop, TContext> Bottom,
    Action<MotherBrainTopTubeDrop, TContext> Top,
    Action<MotherBrainMainTubeDrop, TContext> Main);
