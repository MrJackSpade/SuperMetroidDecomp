using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>The restored game, file metadata and any compatible build-identity warnings.</summary>
internal readonly record struct DebuggerSaveStateLoadResult(
    SuperMetroidGame Game,
    DebuggerSaveStateMetadata Metadata,
    IReadOnlyList<string> Warnings)
{
    /// <summary>
    /// Binds the restored game to the installed content and save storage a debugger state omits, as every host must
    /// before it steps or shows the game (<see cref="GameSessionComposer.Bind"/>). The clips bind first, so a state
    /// whose session never bound them is rejected before the live game is replaced.
    /// </summary>
    /// <param name="content">The installed content the host runs.</param>
    /// <param name="storage">Where the restored session's saves are kept.</param>
    /// <returns>The restored game, bound and ready to step.</returns>
    internal SuperMetroidGame BindGame(IInstalledSessionContent content, ISessionSaveStorage storage)
    {
        Ensure.NotNull(storage);
        GameSessionComposer.Bind(Game, content, storage);
        return Game;
    }
}
