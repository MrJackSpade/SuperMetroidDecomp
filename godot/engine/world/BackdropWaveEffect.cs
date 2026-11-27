using Engine.Ui;
using Godot;

namespace Engine.World;

/// <summary>
/// A room effect that waves the rows of what lies behind the room: its backdrops read their pictures a pixel or so
/// further on from one row to the next, by a table gone round a row at a time, as what lies behind water wavers
/// below its surface and what lies behind lava and acid sways with the heat. It plays the steps of its data file
/// (<see cref="RowWaveSteps"/>): each step shows for its updates and the next draws the table a row further up,
/// or further down. Like every room effect it is stepped before the player and the enemies act, and it affects play
/// in no way.
/// </summary>
public partial class BackdropWaveEffect : RoomEffect
{
    /// <summary>The data file of its wave.</summary>
    [Export(PropertyHint.File)]
    public string WaveFile { get; set; } = "";

    private RowWaveSteps? _steps;

    /// <summary>The step that shows, and how many updates it has shown for.</summary>
    private int _phase, _shown;

    private RowWaveSteps Steps => _steps ?? throw new InvalidOperationException($"The backdrops' wave \"{Name}\" has not been set up.");

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        if (WaveFile.Length == 0)
            throw new InvalidOperationException($"The backdrops' wave \"{Name}\" names no file of its wave.");
        _steps = RowWaveSteps.Read(room.Content.ReadData(WaveFile));
        (_phase, _shown) = (0, _steps.ShownAtStart);
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room) => (_phase, _shown) = Steps.Next(_phase, _shown);

    /// <summary>
    /// The wave of a backdrop whose first row lies <paramref name="backdropTop"/> down the room, seen by a view whose
    /// top is <paramref name="viewTop"/> down it, in the backdrop's own rows; null where nothing is waved, as below
    /// the surface of a room with no liquid. The table is gone round a row at a time from the first row waved, the
    /// row below the liquid's surface or the view's top row, starting from the step that shows and the row of the
    /// backdrop's picture at the view's top.
    /// </summary>
    internal RowWave? WaveOver(Room room, int viewTop, int backdropTop)
    {
        RowWaveSteps steps = Steps;
        if (steps.BelowSurface && room.Liquid is null)
            return null;
        int from = steps.BelowSurface ? room.Liquid!.Surface.Whole + 1 : viewTop;
        int phase = (steps.MovesDown ? -_phase : _phase) + viewTop - from;
        return new RowWave(steps.Offsets, steps.Down, Mathf.PosMod(phase, steps.Offsets.Count), from - backdropTop);
    }
}
