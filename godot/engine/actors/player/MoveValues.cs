using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Actors;

/// <summary>
/// How fast the player moves across while doing one thing in one medium: the speed gained each update while steered,
/// the most speed that reaches, and the speed lost each update while slowing.
/// </summary>
[GlobalClass]
public partial class MoveValues : Resource
{
    /// <summary>The speed gained each update while steering, as a number of pixels.</summary>
    [Export]
    public double Acceleration { get; set; }

    /// <summary>The most speed steering reaches.</summary>
    [Export]
    public double TopSpeed { get; set; }

    /// <summary>The speed lost each update while slowing: unsteered on the ground, and reversing.</summary>
    [Export]
    public double Deceleration { get; set; }

    /// <summary>The speed gained each update, exactly.</summary>
    public Pixels Gain => Pixels.FromNumber(Acceleration);

    /// <summary>The most speed steering reaches, exactly.</summary>
    public Pixels Top => Pixels.FromNumber(TopSpeed);

    /// <summary>The speed lost each update, exactly.</summary>
    public Pixels Loss => Pixels.FromNumber(Deceleration);

    /// <summary>Reads move values from a data file's group.</summary>
    /// <remarks><code>{ "acceleration": 0.1875, "top_speed": 2.75, "deceleration": 0.5 }</code></remarks>
    public static MoveValues Read(DataNode data) => new()
    {
        Acceleration = data.Distance("acceleration").ToNumber(),
        TopSpeed = data.Distance("top_speed").ToNumber(),
        Deceleration = data.Distance("deceleration").ToNumber(),
    };
}
