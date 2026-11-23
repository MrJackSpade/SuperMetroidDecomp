namespace Engine.Content;

/// <summary>
/// Marks a property as a value: a number, a name or a word that describes the thing, as opposed to its behaviour or
/// structure. When the thing names a data file, the value is read from it under <see cref="Name"/>, shows read-only in
/// the editor and is not saved with the scene. When it names none, the value is edited and saved in the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataValueAttribute(string name) : Attribute
{
    /// <summary>The value's name in the data file.</summary>
    public string Name { get; } = name;

    /// <summary>
    /// Whether the value has more structure than a single number, name or word, so that its owner reads it itself in
    /// <see cref="IHasValues.ReadValues"/>. The editor still shows it read-only and does not save it while a data file is named.
    /// </summary>
    public bool ReadByOwner { get; init; }
}
