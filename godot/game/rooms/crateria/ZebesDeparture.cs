using Engine.Core;
using Engine.Ui;
using Engine.World;

namespace Game.Rooms.Crateria;

/// <summary>
/// Samus leaving Zebes: told to start once her gunship has left the planet, it lets the screen go dark over the
/// room, which is still shaking itself apart, and then the ending follows.
/// </summary>
public partial class ZebesDeparture : RoomSequence
{
    /// <summary>How many updates the screen takes to go dark.</summary>
    private const int DarkeningUpdates = 15;

    private ScreenFade? _dark;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
        {
            _dark?.QueueFree();
            _dark = new ScreenFade { Name = "Dark", Layer = 10 };
            AddChild(_dark);
            _dark.FadeOut(DarkeningUpdates);
        }

        ScreenFade dark = _dark ?? throw new InvalidOperationException("The departure from Zebes has not begun.");
        dark.Step();
        if (!dark.IsDone)
            return false;
        room.Session.Sounds.RequestMusic("");
        room.Session.Sounds.StopAll(SoundPriority.Essential);
        room.Session.Service<Escape>().LeaveZebes();
        return true;
    }
}
