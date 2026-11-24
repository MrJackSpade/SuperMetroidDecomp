using Engine.Input;

namespace Engine.Core;

/// <summary>
/// Starting play from the session's save slot, or from the beginning when the slot is empty. The session's game begins
/// again, the saved game is put into it, and play follows in the room the save resumes in. A new game begins in
/// whatever mode its game says a new game does.
/// </summary>
public partial class LoadingMode : GameMode
{
    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Session.BeginAgain();
        if (!Session.Saves.Has(Session.Slot))
            return Session.Definition.CreateModeForNewGame();
        SaveData save = Session.Saves.Read(Session.Slot);
        save.ApplyTo(Session);
        return new PlayingMode(save.Room, save.EntryPoint);
    }
}
