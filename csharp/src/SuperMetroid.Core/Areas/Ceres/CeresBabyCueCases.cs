namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What each case of a <see cref="CeresBabyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="ReactsToMotion">Makes a <see cref="CeresBabyCue.ReactsToMotion"/> into the result.</param>
/// <param name="UpdateColors">Makes a <see cref="CeresBabyCue.UpdateColors"/> into the result.</param>
/// <param name="Cries">Makes a <see cref="CeresBabyCue.Cries"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record CeresBabyCueCases<TContext, TResult>(Func<CeresBabyCue.ReactsToMotion, TContext, TResult> ReactsToMotion, Func<CeresBabyCue.UpdateColors, TContext, TResult> UpdateColors, Func<CeresBabyCue.Cries, TContext, TResult> Cries);
