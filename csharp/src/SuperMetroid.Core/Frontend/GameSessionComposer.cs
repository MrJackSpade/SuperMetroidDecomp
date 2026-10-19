using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The one place hosts compose a game session. Lifetimes: the installed <see cref="GameArtwork"/> is immutable,
/// host-owned and shared; the game (its saves, audio, display, menus and lag policies) lives until the host restarts or
/// replaces it with a restored state; a gameplay runtime lives inside it from a game or demo start to title, game over or
/// soft reset (<see cref="GameplayRuntimeFactory"/>).
/// </summary>
public static class GameSessionComposer
{
    /// <summary>Creates a lag-free game with its installed artwork bound before the first frame.</summary>
    /// <param name="saves">The battery-backed saves the session owns.</param>
    /// <param name="gameOptions">The host's options.</param>
    /// <param name="artwork">The installed catalogs.</param>
    /// <returns>A reset-state game ready to step.</returns>
    public static SuperMetroidGame Create(SaveSlotStore saves, SuperMetroidGameOptions gameOptions, GameArtwork artwork)
    {
        Ensure.NotNull(artwork);
        var game = new SuperMetroidGame(saves, gameOptions);
        game.BindArtwork(artwork);
        return game;
    }

    /// <summary>Rebinds the installed artwork a restored debugger state omits.</summary>
    /// <param name="restored">The game decoded from a debugger state.</param>
    /// <param name="artwork">The installed catalogs.</param>
    public static void Rebind(SuperMetroidGame restored, GameArtwork artwork)
    {
        Ensure.NotNull(restored);
        Ensure.NotNull(artwork);
        restored.BindArtwork(artwork);
    }
}
