using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Atomic: a spinning ball of energy that closes on Samus a little each update, across and up or down alike.
/// Nothing in the room stops it.
/// </summary>
/// <remarks>Its timelines are the two ways it can spin, "spin_clockwise" and "spin_anticlockwise".</remarks>
public partial class Atomic : Enemy
{
    /// <summary>The timeline of the way it spins.</summary>
    [Export]
    public string Spin { get; set; } = "spin_clockwise";

    /// <summary>How far it closes on Samus along each axis each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play(Spin);

    /// <inheritdoc/>
    protected override void Behave()
    {
        Pixels speed = Pixels.FromNumber(Speed);
        Position += new PixelOffset(
            Target.X.Whole < Position.X.Whole ? -speed : speed,
            Target.Y.Whole < Position.Y.Whole ? -speed : speed);
    }
}
