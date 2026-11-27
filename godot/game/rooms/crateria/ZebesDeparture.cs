using Engine.Core;
using Engine.Ui;
using Engine.World;

namespace Game.Rooms.Crateria;

/// <summary>
/// Samus leaving Zebes: told to start once her gunship has left the planet, it lets the screen go dark over the
/// room, which is still shaking itself apart, and then the ending follows.
/// </summary>
/// <remarks>
/// The dark is the screen's own darkness, which it shows at its own fade's level on each of its updates, a level
/// darker each update: from fully lit, whatever it stood at before, to black, which it is left at.
/// </remarks>
public partial class ZebesDeparture : RoomSequence
{
    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        ScreenDarkness darkness = room.Session.Darkness;
        darkness.Show(elapsed + 1);
        if (!darkness.IsBlack)
            return false;
        room.Session.Sounds.RequestMusic("");
        room.Session.Sounds.StopAll(SoundPriority.Essential);
        room.Session.Service<Escape>().LeaveZebes();
        return true;
    }
}
