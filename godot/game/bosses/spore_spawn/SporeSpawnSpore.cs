using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of Spore Spawn's spores: it drifts down from the ceiling along a set, wavering path, to one side or the other,
/// and is gone when it reaches the floor. Shot down, it may leave a pickup, as its settings say.
/// </summary>
/// <remarks>Its data lists its path under "drift": the step it takes each update, drifting right, round and round.</remarks>
public partial class SporeSpawnSpore : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far below where it started it is gone, in pixels.</summary>
    [Export, DataValue("fall_distance")]
    public int FallDistance { get; set; } = 248;

    private readonly List<PixelOffset> _drift = [];
    private bool _driftsLeft;
    private int _step;
    private int _startY;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
        _drift.Clear();
        _drift.AddRange(data.List("drift").Select(step => PixelOffset.FromWhole(step.Int("x"), step.Int("y"))));
        if (_drift.Count == 0)
            throw data.Problem("must list at least one drift step");
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        // The speed it is launched with only says which way it drifts; its path does the moving.
        _driftsLeft = Velocity.X.IsNegative;
        Velocity = PixelVelocity.Zero;
        _startY = Position.Y.Whole;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        PixelOffset step = _drift[_step];
        Position += _driftsLeft ? new PixelOffset(-step.X, step.Y) : step;
        _step = (_step + 1) % _drift.Count;
        if (Position.Y.Whole >= _startY + FallDistance)
            Leave(room);
    }
}
