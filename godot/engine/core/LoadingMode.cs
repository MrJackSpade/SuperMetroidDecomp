using Engine.Input;

namespace Engine.Core;

/// <summary>
/// Starting play from the session's save slot, or from the beginning when the slot is empty. The session's game begins
/// again, the saved game is put into it, and play follows in the room the save resumes in.
/// </summary>
public partial class LoadingMode : GameMode
{
    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Session.BeginAgain();
        if (!Session.Saves.Has(Session.Slot))
            return new PlayingMode();
        SaveData save = Session.Saves.Read(Session.Slot);
        save.ApplyTo(Session);
        return new PlayingMode(save.Room, save.EntryPoint);
    }
}
