using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Multiviola: a ball of fire that travels in a straight line and bounces off whatever it meets. A wall turns it
/// back across, a floor or ceiling up or down; meeting a wall it does not move up or down in that update.
/// </summary>
/// <remarks>Its timeline is "flying".</remarks>
public partial class Multiviola : Enemy
{
    /// <summary>The way it sets off, in 256ths of a turn anticlockwise from straight right.</summary>
    [Export]
    public int Heading { get; set; }

    /// <summary>How far it travels each update, in pixels.</summary>
    [Export]
    public int Speed { get; set; } = 1;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Pixels speed = Pixels.FromWhole(Speed);
        Velocity = new PixelVelocity(Turns.Cosine(Heading, speed), -Turns.Sine(Heading, speed));
        Play("flying");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (MoveAcrossBy(Velocity.X))
        {
            Velocity = Velocity.WithX(-Velocity.X);
            return;
        }

        if (MoveDownBy(Velocity.Y))
            Velocity = Velocity.WithY(-Velocity.Y);
    }
}
