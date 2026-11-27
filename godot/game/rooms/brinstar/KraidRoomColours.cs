using Engine.Content;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Rooms.Brinstar;

/// <summary>
/// The colours of Kraid's room once he is beaten: the room shows the colours his death's last fade ends on from the
/// moment it is set up, so through the door's passage too, as the cartridge shows them as the room loads.
/// </summary>
/// <remarks>
/// They are the last step of the fade his data names "sunk", under "backdrop_fades", shown on the room's pictures.
/// </remarks>
public partial class KraidRoomColours : RoomSequence
{
    /// <summary>The data file of Kraid, whose fades it reads.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <inheritdoc/>
    protected override void OnStarted()
    {
        DataNode fades = Room.Content.ReadData(DataFile).Get("backdrop_fades");
        IReadOnlyList<DataNode> sunk = fades.List("sunk");
        if (sunk.Count == 0)
            throw fades.Problem("must give at least one step of its backdrop's fade \"sunk\"");
        Room.Recolour(Name, ColourChange.Read(sunk[^1]));
    }

    /// <summary>There is nothing to play: the colours were shown as it started.</summary>
    protected override bool Play(Room room, int elapsed) => true;
}
