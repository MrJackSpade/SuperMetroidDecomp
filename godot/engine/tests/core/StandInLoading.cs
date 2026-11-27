using Engine.Core;
using Engine.Input;

namespace Engine.Tests.Core;

/// <summary>
/// The stand-in game's death: play starts from the session's save slot, or from the beginning when the slot is empty.
/// The session's game begins again, the saved game is put into it (<see cref="GameSession.LoadSlot"/>), and play
/// follows in the room the save resumes in.
/// </summary>
public partial class StandInLoading : GameMode
{
    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        return Session.LoadSlot() is { } save
            ? new PlayingMode(save.Room, save.EntryPoint)
            : Session.Definition.CreateModeForNewGame();
    }
}
