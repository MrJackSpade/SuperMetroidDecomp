using System.Globalization;
using System.Text.Json;
using Engine.Shared;

namespace Engine.Content;

/// <summary>
/// One value, or one group of named values, read from a data file. Every read that fails says which file and which
/// value, so a person who changed the file can find their mistake.
/// </summary>
public sealed class DataNode
{
    /// <summary>The value as parsed.</summary>
    private readonly JsonElement _element;

    internal DataNode(string file, string location, JsonElement element)
    {
        File = file;
        Location = location;
        _element = element;
    }

    /// <summary>The data file, as a path within the assets tree.</summary>
    public string File { get; }

    /// <summary>Where in the file this value is, for messages; empty for the file's top.</summary>
    public string Location { get; }

    /// <summary>Whether this group has a value named <paramref name="name"/>.</summary>
    public bool Has(string name) => RequireGroup().TryGetProperty(name, out _);

    /// <summary>The value named <paramref name="name"/>.</summary>
    /// <exception cref="ContentException">This is not a group, or it has no such value.</exception>
    public DataNode Get(string name) =>
        RequireGroup().TryGetProperty(name, out JsonElement value)
            ? new DataNode(File, Location.Length == 0 ? name : $"{Location}.{name}", value)
            : throw Problem($"has no value named \"{name}\"");

    /// <summary>The names and values of this group, in the order the file lists them.</summary>
    public IEnumerable<KeyValuePair<string, DataNode>> Entries() =>
        RequireGroup().EnumerateObject().Select(property => new KeyValuePair<string, DataNode>(
            property.Name, new DataNode(File, Location.Length == 0 ? property.Name : $"{Location}.{property.Name}", property.Value)));

    /// <summary>The items of this list, in order.</summary>
    /// <exception cref="ContentException">This is not a list.</exception>
    public IReadOnlyList<DataNode> Items()
    {
        if (_element.ValueKind != JsonValueKind.Array)
            throw Problem("is not a list");
        return _element.EnumerateArray().Select((item, index) => new DataNode(File, $"{Location}[{index}]", item)).ToList();
    }

    /// <summary>This value as a whole number.</summary>
    public int AsInt() =>
        _element.ValueKind == JsonValueKind.Number && _element.TryGetInt32(out int value) ? value : throw Problem("is not a whole number");

    /// <summary>This value as text.</summary>
    public string AsText() => _element.ValueKind == JsonValueKind.String ? _element.GetString() ?? "" : throw Problem("is not text");

    /// <summary>This value as yes or no.</summary>
    public bool AsFlag() => _element.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => throw Problem("is not true or false"),
    };

    /// <summary>
    /// This value as an exact distance, written as a number of pixels such as 2 or 1.25. It is taken to the nearest
    /// part of a pixel the game can hold.
    /// </summary>
    public Pixels AsPixels()
    {
        if (_element.ValueKind != JsonValueKind.Number || !_element.TryGetDecimal(out decimal pixels))
            throw Problem("is not a number of pixels");
        decimal fractions = Math.Round(pixels * Pixels.FractionsPerPixel, MidpointRounding.AwayFromZero);
        decimal whole = Math.Floor(fractions / Pixels.FractionsPerPixel);
        if (whole < int.MinValue || whole > int.MaxValue)
            throw Problem("is too large a number of pixels");
        return Pixels.FromParts((int)whole, (int)(fractions - whole * Pixels.FractionsPerPixel));
    }

    /// <summary>This value as one of <typeparamref name="TEnum"/>'s names.</summary>
    public TEnum AsEnum<TEnum>() where TEnum : struct, Enum => (TEnum)AsEnum(typeof(TEnum));

    /// <summary>This value as one of the names of the enum <paramref name="type"/>.</summary>
    public object AsEnum(Type type)
    {
        string name = AsText();
        return Enum.TryParse(type, name, ignoreCase: true, out object? value) && Enum.IsDefined(type, value)
            ? value
            : throw Problem($"is \"{name}\", which is not one of: {string.Join(", ", Enum.GetNames(type))}");
    }

    /// <summary>The whole number named <paramref name="name"/>.</summary>
    public int Int(string name) => Get(name).AsInt();

    /// <summary>The text named <paramref name="name"/>.</summary>
    public string Text(string name) => Get(name).AsText();

    /// <summary>The yes or no named <paramref name="name"/>.</summary>
    public bool Flag(string name) => Get(name).AsFlag();

    /// <summary>The distance named <paramref name="name"/>.</summary>
    public Pixels Distance(string name) => Get(name).AsPixels();

    /// <summary>The list named <paramref name="name"/>.</summary>
    public IReadOnlyList<DataNode> List(string name) => Get(name).Items();

    /// <summary>An error naming this file and this value.</summary>
    public ContentException Problem(string what) =>
        new(File, Location.Length == 0 ? $"the file {what}." : $"\"{Location}\" {what}.");

    /// <summary>This value, which must be a group of named values.</summary>
    private JsonElement RequireGroup() =>
        _element.ValueKind == JsonValueKind.Object ? _element : throw Problem("is not a group of named values");

    /// <summary>The value as the file wrote it, for messages.</summary>
    public override string ToString() => _element.ToString() ?? "";

    /// <summary>Parses a data file's text.</summary>
    /// <exception cref="ContentException">The text is not well formed.</exception>
    internal static DataNode Parse(string file, string text)
    {
        try
        {
            var options = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
            using JsonDocument document = JsonDocument.Parse(text, options);
            return new DataNode(file, "", document.RootElement.Clone());
        }
        catch (JsonException exception)
        {
            string line = exception.LineNumber is long number ? (number + 1).ToString(CultureInfo.InvariantCulture) : "?";
            throw new ContentException(file, $"is not well formed at line {line}: {exception.Message}");
        }
    }
}
