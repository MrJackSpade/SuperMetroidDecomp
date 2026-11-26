using Godot;

namespace Engine.World;

/// <summary>
/// A height the view's top is kept at or below for as long as this holds: once the view has followed the player each
/// update, it is brought down to here if it stands higher. Told to "release", it holds no more; told to "hold", it
/// holds again.
/// </summary>
[Tool]
public partial class ViewFloor : Node2D, IPlaced, ITriggerTarget
{
    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>Whether it is holding the view right now.</summary>
    public bool IsHolding { get; private set; }

    /// <summary>The highest the view's top may stand while this holds, in room pixels.</summary>
    public int LowestTop => Mathf.FloorToInt(Position.Y);

    /// <summary>Puts it in its starting state: holding the view.</summary>
    internal void Setup() => IsHolding = true;

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        IsHolding = what switch
        {
            "hold" => true,
            "release" => false,
            _ => throw new ArgumentException($"A view floor can be told to \"hold\" or \"release\", not \"{what}\".", nameof(what)),
        };
    }
}
