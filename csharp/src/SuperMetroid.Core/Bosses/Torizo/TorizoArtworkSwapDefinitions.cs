using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The artwork each swap the Bomb and Golden Torizo instruction lists make shows (#1406): a semantic identity per swap,
/// whose pictures the installed sheets hold.
/// </summary>
internal static class TorizoArtworkSwapDefinitions
{
    /// <summary>Each swap's characters, in the order the instruction lists were authored (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Sources { get; } =
        [.. Enumerable.Range(0, TorizoArtworkSwap.Count).Select(index => new TorizoArtworkSwapCharacters(index))];
}
