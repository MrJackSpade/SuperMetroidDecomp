using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The one place hosts compose a game session, from the services they supply through narrow interfaces. Lifetimes: the
/// installed <see cref="IInstalledSessionContent"/> is immutable, host-owned and shared; the game (its saves, audio,
/// display, menus and lag policies) lives until the host restarts or replaces it with a restored state; a gameplay
/// runtime lives inside it from a game or demo start to title, game over or soft reset (<see cref="GameplayRuntimeFactory"/>).
/// </summary>
public static class GameSessionComposer
{
    /// <summary>Creates a lag-free game with its installed content bound before the first frame.</summary>
    /// <param name="saves">The battery-backed saves the session owns.</param>
    /// <param name="gameOptions">The options the game reads.</param>
    /// <param name="content">The installed content.</param>
    /// <param name="storage">Where the saves are kept after the game changes them; null when a session's saves are not kept, as a replay's are not.</param>
    /// <returns>A reset-state game ready to step.</returns>
    public static SuperMetroidGame Create(
        SaveSlotStore saves,
        SuperMetroidGameOptions gameOptions,
        IInstalledSessionContent content,
        ISessionSaveStorage? storage)
    {
        Ensure.NotNull(content);
        var game = new SuperMetroidGame(saves, gameOptions);
        Bind(game, content, storage);
        return game;
    }

    /// <summary>
    /// Binds a game decoded from a debugger state to the installed content and save storage the state omits. The clips
    /// bind first: a state whose session never bound them is rejected before anything else is bound.
    /// </summary>
    /// <param name="restored">The game decoded from a debugger state.</param>
    /// <param name="content">The installed content.</param>
    /// <param name="storage">Where the restored session's saves are kept.</param>
    public static void Restore(SuperMetroidGame restored, IInstalledSessionContent content, ISessionSaveStorage storage)
    {
        Ensure.NotNull(restored);
        Ensure.NotNull(content);
        Ensure.NotNull(storage);
        Bind(restored, content, storage);
    }

    /// <summary>Binds the clips, then the artwork, then the save storage.</summary>
    private static void Bind(SuperMetroidGame game, IInstalledSessionContent content, ISessionSaveStorage? storage)
    {
        game.BindAudio(content.AudioClips);
        game.BindArtwork(content.Artwork);
        if (storage is not null)
            game.SaveRamChanged += () => storage.Persist(game.Saves);
    }
}
