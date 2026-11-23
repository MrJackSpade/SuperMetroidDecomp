using Engine.Content;
using Godot;

namespace Engine.Tests.Content;

/// <summary>A node with values, for the engine's own checks of values from files.</summary>
[Tool]
public partial class StandInValues : Node, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>A whole-number value.</summary>
    [Export, DataValue("health")]
    public int Health { get; set; } = 1;

    /// <summary>A distance value, shown as a number of pixels.</summary>
    [Export, DataValue("speed")]
    public double Speed { get; set; }

    /// <summary>A value that is one of a set of names.</summary>
    [Export, DataValue("facing")]
    public Engine.Shared.Facing Facing { get; set; }

    /// <summary>Something that is not a value: it is always edited and saved in the editor.</summary>
    [Export]
    public string Label { get; set; } = "";

    /// <summary>The names read by <see cref="ReadValues"/>.</summary>
    public List<string> Tags { get; } = [];

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
        Tags.Clear();
        Tags.AddRange(data.List("tags").Select(tag => tag.AsText()));
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);
}
