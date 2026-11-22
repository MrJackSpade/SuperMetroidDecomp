namespace SuperMetroid.Core.Audio;

/// <summary>What the owner of a <see cref="AudioEvent"/> does on each of its cases (#2553).</summary>
/// <param name="ChangeMusicDataSet">Reacts to a <see cref="AudioEvent.ChangeMusicDataSet"/>.</param>
/// <param name="SelectMusicTrack">Reacts to a <see cref="AudioEvent.SelectMusicTrack"/>.</param>
/// <param name="StopMusic">Reacts to a <see cref="AudioEvent.StopMusic"/>.</param>
/// <param name="ClockStep">Reacts to a <see cref="AudioEvent.ClockStep"/>.</param>
/// <param name="StartSoundEffect">Reacts to a <see cref="AudioEvent.StartSoundEffect"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed partial record AudioEventReactions<TContext>(Action<AudioEvent.ChangeMusicDataSet, TContext> ChangeMusicDataSet, Action<AudioEvent.SelectMusicTrack, TContext> SelectMusicTrack, Action<AudioEvent.StopMusic, TContext> StopMusic, Action<AudioEvent.ClockStep, TContext> ClockStep, Action<AudioEvent.StartSoundEffect, TContext> StartSoundEffect);
