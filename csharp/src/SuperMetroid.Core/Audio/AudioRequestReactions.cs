namespace SuperMetroid.Core.Audio;

/// <summary>What the audio session does on each <see cref="AudioRequest"/> (#2553).</summary>
/// <typeparam name="TContext">What each reaction reads besides the request.</typeparam>
/// <param name="PlayMusicTrack">Plays a music track.</param>
/// <param name="StopMusic">Stops the music.</param>
/// <param name="ChangeMusicDataSet">Changes the music data set.</param>
/// <param name="RequestSound">Asks a library for a sound.</param>
/// <param name="ClearSoundRequest">Withdraws a library's request.</param>
internal sealed partial record AudioRequestReactions<TContext>(
    Action<AudioRequest.PlayMusicTrack, TContext> PlayMusicTrack,
    Action<AudioRequest.StopMusic, TContext> StopMusic,
    Action<AudioRequest.ChangeMusicDataSet, TContext> ChangeMusicDataSet,
    Action<AudioRequest.RequestSound, TContext> RequestSound,
    Action<AudioRequest.ClearSoundRequest, TContext> ClearSoundRequest);
