using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// The colours Ceres's doors take once Ridley has gone. A door is given them as its room is set up, so it shows
/// them from the first moment the room is seen, and a door that was there while he left keeps the colours it had
/// until the room is entered again. The colours are changed where the doors' pictures are drawn; no picture is kept
/// twice.
/// </summary>
public partial class CeresDoorColours : RoomSequence
{
    /// <summary>The data file of the change of colours, place by place.</summary>
    [Export(PropertyHint.File)]
    public string ColoursFile { get; set; } = "";

    /// <summary>The doors of the room.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> Doors { get; set; } = [];

    /// <inheritdoc/>
    protected override void OnStarted()
    {
        ColourChange colours = ColourChange.Read(Room.Content.ReadData(ColoursFile).Get("colours"));
        foreach (NodePath path in Doors)
        {
            Node door = GetNodeOrNull(path)
                ?? throw new InvalidOperationException($"The door colours \"{Name}\" are for \"{path}\", which is not in the room.");
            IRecoloured[] visuals = [.. door.FindChildren("*", owned: false).Prepend(door).OfType<IRecoloured>()];
            if (visuals.Length == 0)
                throw new InvalidOperationException($"The door colours \"{Name}\" are for \"{path}\", which has nothing that can be recoloured.");
            foreach (IRecoloured visual in visuals)
                colours.ShowOn(visual, Name);
        }
    }

    /// <summary>There is nothing to play: the colours were given as it started.</summary>
    protected override bool Play(Room room, int elapsed) => true;
}
