using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Actors;

/// <summary>
/// How fast the player moves across while doing one thing in one medium: the speed gained each update while steering,
/// the most speed steering reaches, the speed lost each update while slowing, and what happens to their speed while
/// they are not steering at all.
/// </summary>
[GlobalClass]
public partial class MoveValues : Resource
{
    /// <summary>What becomes of the player's speed across in an update in which they steer neither way.</summary>
    public enum NotSteering
    {
        /// <summary>It is lost a little each update, by the deceleration.</summary>
        Slows,

        /// <summary>It is lost at once.</summary>
        Stops,

        /// <summary>It is kept.</summary>
        Coasts,
    }

    /// <summary>The speed gained each update while steering, as a number of pixels.</summary>
    [Export]
    public double Acceleration { get; set; }

    /// <summary>The most speed steering reaches.</summary>
    [Export]
    public double TopSpeed { get; set; }

    /// <summary>The speed lost each update while slowing, and while steering against the way the player is moving.</summary>
    [Export]
    public double Deceleration { get; set; }

    /// <summary>What becomes of the player's speed while they steer neither way.</summary>
    [Export]
    public NotSteering WhenNotSteering { get; set; }

    /// <summary>The speed gained each update, exactly.</summary>
    public Pixels Gain => Pixels.FromNumber(Acceleration);

    /// <summary>The most speed steering reaches, exactly.</summary>
    public Pixels Top => Pixels.FromNumber(TopSpeed);

    /// <summary>The speed lost each update, exactly.</summary>
    public Pixels Loss => Pixels.FromNumber(Deceleration);

    /// <summary>Reads move values from a data file's group.</summary>
    /// <remarks><code>{ "acceleration": 0.1875, "top_speed": 2.75, "deceleration": 0.5, "when_not_steering": "stops" }</code></remarks>
    public static MoveValues Read(DataNode data) => new()
    {
        Acceleration = data.Distance("acceleration").ToNumber(),
        TopSpeed = data.Distance("top_speed").ToNumber(),
        Deceleration = data.Distance("deceleration").ToNumber(),
        WhenNotSteering = data.Has("when_not_steering") ? data.Get("when_not_steering").AsEnum<NotSteering>() : NotSteering.Slows,
    };
}
