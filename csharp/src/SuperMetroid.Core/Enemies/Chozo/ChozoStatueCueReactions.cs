namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What the owner of a <see cref="ChozoStatueCue"/> does on each of its cases (#2553).</summary>
/// <param name="Footstep">Reacts to a <see cref="ChozoStatueCue.Footstep"/>.</param>
/// <param name="Move">Reacts to a <see cref="ChozoStatueCue.Move"/>.</param>
/// <param name="Signal">Reacts to a <see cref="ChozoStatueCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record ChozoStatueCueReactions<TContext>(Action<ChozoStatueCue.Footstep, TContext> Footstep, Action<ChozoStatueCue.Move, TContext> Move, Action<ChozoStatueCue.Signal, TContext> Signal);
