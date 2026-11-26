using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// The quake of rising lava and acid: on every update the room's liquid, where it is one of these, waited to move or
/// moved, the room's shake is kept going and, unless the room's quakes are silent, the quake's rumble is heard in its
/// own uneven rhythm.
/// </summary>
/// <remarks>
/// The rumble's wait is counted down an update at a time and the rumble comes on the update the count has already
/// run out; the next wait is the next of the rhythm's, with nought to three updates more by the last number chance
/// gave, taken as it stands and not drawn, and with one more besides (<see cref="ExtraWait"/>).
/// </remarks>
public partial class RisingLiquidQuake : RoomSequence
{
    /// <summary>The liquids that quake, by their types' names.</summary>
    [Export]
    public string[] Liquids { get; set; } = ["lava", "acid"];

    /// <summary>The rumble, by name.</summary>
    [Export]
    public string Sound { get; set; } = "";

    /// <summary>Whether the room's quakes are silent: it shakes, and no rumble is heard.</summary>
    [Export]
    public bool Silent { get; set; }

    /// <summary>The waits between rumbles, in updates, taken in turn and round again.</summary>
    [Export]
    public int[] SoundWaits { get; set; } = [1, 3, 2, 1, 1, 2, 2, 1];

    /// <summary>
    /// The update added to every wait. The cartridge adds it for a save file whose history holds anything, a
    /// completed game or a start that found no saved game; and every save file that play can make holds one or the
    /// other from its first start on.
    /// </summary>
    [Export]
    public int ExtraWait { get; set; } = 1;

    /// <summary>How hard the room shakes, across.</summary>
    [Export]
    public int ShakeStrength { get; set; } = 2;

    /// <summary>The updates of shaking each such update keeps to come at the least: a power of two.</summary>
    [Export]
    public int ShakeAtLeast { get; set; } = 32;

    private int _wait;
    private int _next;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            (_wait, _next) = (0, 0);
        if (room.Liquid is not { IsAstir: true } liquid || !Liquids.Contains(liquid.Type.Name))
            return false;
        if (!Silent && Sound.Length > 0)
        {
            if (_wait == 0)
            {
                room.Session.Sounds.Request(Sound, channel: "room");
                _wait = SoundWaits[_next] + (int)(room.Session.Random.Last.Number & 3) + ExtraWait;
                _next = (_next + 1) % SoundWaits.Length;
            }
            else
            {
                _wait--;
            }
        }

        room.SustainShake(ShakeStrength, ShakeAtLeast);
        return false;
    }
}
