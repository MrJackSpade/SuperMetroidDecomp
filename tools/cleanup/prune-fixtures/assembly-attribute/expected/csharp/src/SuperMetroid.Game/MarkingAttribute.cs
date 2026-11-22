namespace SuperMetroid.Game;

/// <summary>Marks the assembly, as an assembly attribute only the assembly's own attributes name.</summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class MarkingAttribute : Attribute
{
    /// <summary>Whether the assembly is marked.</summary>
    public bool Marked { get; init; }
}
