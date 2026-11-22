namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What each case of a <see cref="NinjaSpacePirateCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="NinjaSpacePirateCue.PlaySound"/> into the result.</param>
/// <param name="SetShading">Makes a <see cref="NinjaSpacePirateCue.SetShading"/> into the result.</param>
/// <param name="ThrowClaw">Makes a <see cref="NinjaSpacePirateCue.ThrowClaw"/> into the result.</param>
/// <param name="Begin">Makes a <see cref="NinjaSpacePirateCue.Begin"/> into the result.</param>
/// <param name="BeginHeading">Makes a <see cref="NinjaSpacePirateCue.BeginHeading"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="NinjaSpacePirateCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record NinjaSpacePirateCueCases<TContext, TResult>(Func<NinjaSpacePirateCue.PlaySound, TContext, TResult> PlaySound, Func<NinjaSpacePirateCue.SetShading, TContext, TResult> SetShading, Func<NinjaSpacePirateCue.ThrowClaw, TContext, TResult> ThrowClaw, Func<NinjaSpacePirateCue.Begin, TContext, TResult> Begin, Func<NinjaSpacePirateCue.BeginHeading, TContext, TResult> BeginHeading, Func<NinjaSpacePirateCue.Signal, TContext, TResult> Signal);
