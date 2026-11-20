using System.Reflection;
using Engine.Shared;
using Godot;

namespace Engine.Content;

/// <summary>
/// The one mechanism by which values come from data files. Every type that holds values uses it; none reads its own
/// file its own way, and none knows whether its values came from a file.
/// </summary>
public static class DataValues
{
    /// <summary>
    /// Fills <paramref name="target"/>'s values from its data file, if it names one: each property marked
    /// <see cref="DataValueAttribute"/> from the value of that name, then whatever the target reads for itself. A target
    /// that names no file keeps the values it was saved with.
    /// </summary>
    /// <exception cref="ContentException">The file, or a value in it, is missing, misspelt or of the wrong kind.</exception>
    public static void Fill(IHasValues target, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(content);
        if (target.DataFile.Length == 0)
            return;

        DataNode data = content.ReadData(target.DataFile);
        foreach ((PropertyInfo property, DataValueAttribute value) in ValueProperties(target.GetType()))
            property.SetValue(target, Convert(data.Get(value.Name), property.PropertyType));

        target.ReadValues(data);
    }

    /// <summary>
    /// Adjusts how the editor treats one of <paramref name="target"/>'s properties: while the target names a data file,
    /// each of its values is shown read-only and is not saved. Called from the target's own property validation.
    /// </summary>
    public static void Validate(IHasValues target, Godot.Collections.Dictionary property)
    {
        if (target.DataFile.Length == 0)
            return;
        string name = property["name"].AsStringName();
        if (!ValueProperties(target.GetType()).Any(value => value.Property.Name == name))
            return;
        var usage = property["usage"].As<PropertyUsageFlags>();
        property["usage"] = (int)((usage | PropertyUsageFlags.ReadOnly) & ~PropertyUsageFlags.Storage);
    }

    /// <summary>The properties of <paramref name="type"/> that are values, with each one's name in a data file.</summary>
    private static IEnumerable<(PropertyInfo Property, DataValueAttribute Value)> ValueProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(property => (Property: property, Value: property.GetCustomAttribute<DataValueAttribute>()))
            .Where(pair => pair.Value is not null)
            .Select(pair => (pair.Property, pair.Value!));

    /// <summary>A value from a data file as the kind of value a property holds.</summary>
    private static object Convert(DataNode value, Type type)
    {
        if (type == typeof(int))
            return value.AsInt();
        if (type == typeof(bool))
            return value.AsFlag();
        if (type == typeof(string))
            return value.AsText();
        if (type == typeof(Pixels))
            return value.AsPixels();
        if (type == typeof(double))
            return value.AsPixels().ToNumber();
        if (type.IsEnum)
            return value.AsEnum(type);
        if (type == typeof(string[]))
            return value.Items().Select(item => item.AsText()).ToArray();
        if (type == typeof(int[]))
            return value.Items().Select(item => item.AsInt()).ToArray();
        throw new NotSupportedException(
            $"A value of type {type.Name} cannot be read from a data file as a simple value; its owner reads it in ReadValues.");
    }
}
