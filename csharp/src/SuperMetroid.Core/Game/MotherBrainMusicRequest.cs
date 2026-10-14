namespace SuperMetroid.Core.Game;

/// <summary>
/// One typed <c>QueueMusic_Delayed*</c> call. The command retains its complete cartridge
/// word, including unknown commands, without conflating data uploads with track indices.
/// </summary>
/// <param name="Command">The full native music queue word, identifying stop, data load, or track selection.</param>
/// <param name="Delay">The authored music-queue delay before the command is eligible for consumption.</param>
public readonly record struct MotherBrainMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
