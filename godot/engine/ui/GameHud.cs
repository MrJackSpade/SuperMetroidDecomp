using Engine.Core;
using Engine.World;
using Godot;

namespace Engine.Ui;

/// <summary>
/// What is shown over play: the player's energy, what they carry, where they are. The engine keeps a game's own one
/// on screen while its game is played and tells it when to show itself again; it changes nothing it shows.
/// </summary>
public abstract partial class GameHud : CanvasLayer
{
    /// <summary>Called once as play begins, before the first room is set up.</summary>
    public virtual void Present(GameSession session)
    {
    }

    /// <summary>Called as each room is set up and after each update of play, to show things as they now are.</summary>
    public abstract void Refresh(GameSession session, Room room);
}
