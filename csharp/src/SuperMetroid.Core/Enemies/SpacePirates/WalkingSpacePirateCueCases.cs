namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What each case of a <see cref="WalkingSpacePirateCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="FireLaser">Makes a <see cref="WalkingSpacePirateCue.FireLaser"/> into the result.</param>
/// <param name="Begin">Makes a <see cref="WalkingSpacePirateCue.Begin"/> into the result.</param>
/// <param name="BeginHeading">Makes a <see cref="WalkingSpacePirateCue.BeginHeading"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="WalkingSpacePirateCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record WalkingSpacePirateCueCases<TContext, TResult>(Func<WalkingSpacePirateCue.FireLaser, TContext, TResult> FireLaser, Func<WalkingSpacePirateCue.Begin, TContext, TResult> Begin, Func<WalkingSpacePirateCue.BeginHeading, TContext, TResult> BeginHeading, Func<WalkingSpacePirateCue.Signal, TContext, TResult> Signal);
