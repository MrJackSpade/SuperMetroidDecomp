namespace SuperMetroid.Game.Steps;

/// <summary>Notes a type, as tools read an attribute's argument from its applications.</summary>
/// <param name="text">The note.</param>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class NoteAttribute(string text) : Attribute
{
    /// <summary>The note, which no source reads.</summary>
    public string Text { get; } = text;
}
