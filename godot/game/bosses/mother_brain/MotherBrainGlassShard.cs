using System.Numerics;
using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A shard thrown as a pane of Mother Brain's glass bursts. It flies up and falls back through everything, and is gone
/// once it has fallen out of the room's top screen. Each update it may shed a sparkle somewhere about it.
/// </summary>
public partial class MotherBrainGlassShard : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.125;

    /// <summary>How far down the room it is gone, in pixels.</summary>
    [Export, DataValue("bottom")]
    public int Bottom { get; set; } = 256;

    /// <summary>The effect of a sparkle it sheds, by the data file of its timeline.</summary>
    [Export, DataValue("sparkle_effect")]
    public string SparkleEffect { get; set; } = "";

    /// <summary>How many pixels across, and down, the places a sparkle may show at span: a power of two.</summary>
    [Export, DataValue("sparkle_spread")]
    public int SparkleSpread { get; set; } = 32;

    /// <summary>How far into that span its own place lies, each way.</summary>
    [Export, DataValue("sparkle_centre")]
    public int SparkleCentre { get; set; } = 16;

    private PixelVelocity _speed;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        if (SparkleSpread < 2 || !BitOperations.IsPow2(SparkleSpread))
            throw new InvalidOperationException($"A shard of Mother Brain's glass has a sparkle spread of {SparkleSpread}, which is not a power of two.");
        // It moves itself, so that it sheds its sparkles from where it has just moved to.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Position += _speed.OverOneUpdate;
        if (Position.Y.Whole >= Bottom)
        {
            Leave(room);
            return;
        }

        _speed = _speed.WithY(_speed.Y + Pixels.FromNumber(Gravity));
        // One update in four it sheds a sparkle: when the sixth and eleventh binary digits of a fresh number are both clear.
        GameRandom random = room.Session.Random;
        RandomDraw roll = random.Draw();
        if (roll.Part(5, 1) != 0 || roll.Part(10, 1) != 0)
            return;
        int bits = BitOperations.Log2((uint)SparkleSpread);
        int across = random.Draw().Part(0, bits) - SparkleCentre, down = random.Draw().Part(0, bits) - SparkleCentre;
        room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(SparkleEffect)), Position + PixelOffset.FromWhole(across, down)));
    }
}
