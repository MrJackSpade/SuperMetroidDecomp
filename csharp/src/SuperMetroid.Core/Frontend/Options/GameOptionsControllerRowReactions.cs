namespace SuperMetroid.Core.Frontend.Options;

/// <summary>What the owner of a <see cref="GameOptionsControllerRow"/> does on each of its cases (#2553).</summary>
/// <param name="AssignsAction">Reacts to a <see cref="GameOptionsControllerRow.AssignsAction"/>.</param>
/// <param name="Exit">Reacts to a <see cref="GameOptionsControllerRow.Exit"/>.</param>
/// <param name="Reset">Reacts to a <see cref="GameOptionsControllerRow.Reset"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record GameOptionsControllerRowReactions<TContext>(Action<GameOptionsControllerRow.AssignsAction, TContext> AssignsAction, Action<GameOptionsControllerRow.Exit, TContext> Exit, Action<GameOptionsControllerRow.Reset, TContext> Reset);
