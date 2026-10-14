namespace SuperMetroid.Core.Game;

/// <summary>Delayed music queue request emitted by Botwoon's completed death sequence.</summary>
/// <param name="Command">Full cartridge music-command word; normal Botwoon completion selects track three.</param>
/// <param name="Delay">The queue admission delay; normal completion uses the authored eight-frame delay.</param>
public readonly record struct BotwoonMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
