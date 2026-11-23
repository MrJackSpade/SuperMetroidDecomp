using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room's body of liquid: its surface level, its rise and fall, and the answer to "is this point under it". It
/// applies no physics and no damage; the player and enemies ask it and act on the answer. It can be told to "rise"
/// and "fall" between its two levels.
/// </summary>
[Tool]
public partial class Liquid : Node2D, IPlaced, ITriggerTarget
{
    /// <summary>The data file of its liquid type.</summary>
    [Export(PropertyHint.File)]
    public string TypeFile { get; set; } = "";

    /// <summary>The surface's height when lowered, in pixels down from the room's top.</summary>
    [Export]
    public int LowSurface { get; set; }

    /// <summary>The surface's height when raised, in pixels down from the room's top.</summary>
    [Export]
    public int HighSurface { get; set; }

    /// <summary>How far the surface moves each update while rising or falling, as a number of pixels.</summary>
    [Export]
    public double MoveSpeed { get; set; } = 1;

    /// <summary>Whether the liquid starts raised.</summary>
    [Export]
    public bool StartsRaised { get; set; }

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>The liquid's type, once set up.</summary>
    private LiquidType? _type;

    /// <summary>Where the surface is heading.</summary>
    private Pixels _target;

    /// <summary>The liquid's type.</summary>
    public LiquidType Type => _type ?? throw new InvalidOperationException($"The liquid \"{Name}\" has not been set up.");

    /// <summary>The surface's height right now, down from the room's top.</summary>
    public Pixels Surface { get; private set; }

    /// <summary>Loads the liquid's type and puts its surface at its starting level.</summary>
    internal void Setup(ContentLibrary content)
    {
        if (TypeFile.Length == 0)
            throw new InvalidOperationException($"The liquid \"{Name}\" names no liquid type.");
        _type = LiquidType.Read(content.ReadData(TypeFile));
        Surface = Pixels.FromWhole(StartsRaised ? HighSurface : LowSurface);
        _target = Surface;
    }

    /// <summary>Whether <paramref name="point"/> lies under the surface.</summary>
    public bool IsSubmerged(PixelPosition point) => point.Y >= Surface;

    /// <summary>Moves the surface one update toward where it is heading.</summary>
    internal void Step()
    {
        if (Surface == _target)
            return;
        Pixels step = Pixels.FromNumber(MoveSpeed);
        Surface = Surface < _target
            ? (Surface + step > _target ? _target : Surface + step)
            : (Surface - step < _target ? _target : Surface - step);
        QueueRedraw();
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        _target = what switch
        {
            "rise" => Pixels.FromWhole(HighSurface),
            "fall" => Pixels.FromWhole(LowSurface),
            _ => throw new ArgumentException($"A liquid can be told to \"rise\" or \"fall\", not \"{what}\".", nameof(what)),
        };
    }
}
