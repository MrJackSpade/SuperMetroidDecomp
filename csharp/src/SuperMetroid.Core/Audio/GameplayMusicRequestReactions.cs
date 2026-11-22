namespace SuperMetroid.Core.Audio;

/// <summary>What the owner of a <see cref="GameplayMusicRequest"/> does on each of its cases (#2553).</summary>
/// <param name="Scheduled">Reacts to a <see cref="GameplayMusicRequest.Scheduled"/>.</param>
/// <param name="PermanentItemFanfare">Reacts to a <see cref="GameplayMusicRequest.PermanentItemFanfare"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record GameplayMusicRequestReactions<TContext>(Action<GameplayMusicRequest.Scheduled, TContext> Scheduled, Action<GameplayMusicRequest.PermanentItemFanfare, TContext> PermanentItemFanfare);
