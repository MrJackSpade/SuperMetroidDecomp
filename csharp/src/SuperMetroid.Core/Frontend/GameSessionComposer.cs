using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The one place hosts compose a game session, from the services they supply through narrow interfaces. Lifetimes: the
/// installed <see cref="IInstalledSessionContent"/> is immutable, host-owned and shared; the game (its saves, audio,
/// display, menus and compatibility services) lives until the host restarts it or replaces it with another game; a gameplay
/// runtime lives inside it from a game or demo start to title, game over or soft reset (<see cref="GameplayRuntimeFactory"/>).
/// </summary>
public static class GameSessionComposer
{
    /// <summary>Creates a game running on its own timing, with its installed content bound before the first frame.</summary>
    /// <param name="saves">The saves the session owns.</param>
    /// <param name="assists">The assists the game plays under; <see cref="GameplayAssists.None"/> for the unassisted game.</param>
    /// <param name="content">The installed content.</param>
    /// <param name="storage">Where the saves are kept after the game changes them; null when a session's saves are not kept, as a replay's are not.</param>
    /// <returns>A reset-state game ready to step.</returns>
    public static SuperMetroidGame Create(
        SaveSlotStore saves,
        GameplayAssists assists,
        IInstalledSessionContent content,
        ISessionSaveStorage? storage)
    {
        Ensure.NotNull(content);
        var game = new SuperMetroidGame(saves, assists);
        Bind(game, content, storage);
        return game;
    }

    /// <summary>
    /// Binds a game to the installed content it draws and plays, and to the storage that keeps its saves. Creation binds
    /// every new game; a host binds a game it brings back the same way. The clips bind first, then the artwork, then the
    /// save storage: a game whose session never bound clips is rejected before anything else is bound.
    /// </summary>
    /// <param name="game">The game to bind.</param>
    /// <param name="content">The installed content.</param>
    /// <param name="storage">Where the saves are kept after the game changes them; null when a session's saves are not kept, as a replay's are not.</param>
    public static void Bind(SuperMetroidGame game, IInstalledSessionContent content, ISessionSaveStorage? storage)
    {
        Ensure.NotNull(game);
        Ensure.NotNull(content);
        game.BindAudio(content.AudioClips);
        game.BindArtwork(content.Artwork);
        if (storage is not null)
            game.SavesChanged += () => storage.Persist(game.Saves);
    }
}
