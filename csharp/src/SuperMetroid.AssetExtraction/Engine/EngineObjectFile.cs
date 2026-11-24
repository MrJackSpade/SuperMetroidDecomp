using System.Text.Json.Nodes;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The data file of something of the Godot game that is not an enemy: a room object, begun from what its type reads
/// by name and given its colliders and timelines a part at a time.
/// </summary>
internal sealed partial class EngineObjectFile
{
    private readonly EngineAssetTree _tree;
    private readonly string _file;
    private readonly JsonObject _timelines = [];
    private readonly JsonObject _colliders = [];

    /// <summary>The data file <paramref name="file"/>, which the Godot type <paramref name="type"/> reads.</summary>
    internal EngineObjectFile(EngineAssetTree tree, string file, string type)
    {
        _tree = tree;
        _file = file;
        EngineDefaults.Fill(Data, type);
    }

    /// <summary>The file's values so far.</summary>
    internal JsonObject Data { get; } = [];

    /// <summary>Gives one of the values its type reads by name.</summary>
    /// <exception cref="InvalidDataException">The type reads no value of that name.</exception>
    internal EngineObjectFile Value(string name, JsonNode? value)
    {
        if (!Data.ContainsKey(name))
            throw new InvalidDataException($"The data file {_file} is given the value {name}, which its type does not read by that name.");
        Data[name] = value;
        return this;
    }

    /// <summary>Gives a group of its data that is not one of its type's named values.</summary>
    internal EngineObjectFile Group(string name, JsonNode value)
    {
        Data[name] = value;
        return this;
    }

    /// <summary>Sizes one of the scene's colliders, from the object's place.</summary>
    internal EngineObjectFile Collider(string name, int left, int top, int width, int height)
    {
        _colliders[name] = new JsonObject { ["left"] = left, ["top"] = top, ["width"] = width, ["height"] = height };
        return this;
    }

    /// <summary>Adds a timeline, under the name the Godot game plays it by.</summary>
    internal EngineObjectFile Timeline(string name, JsonObject timeline)
    {
        _timelines[name] = timeline;
        return this;
    }

    /// <summary>Writes the file.</summary>
    internal void Write()
    {
        if (_colliders.Count > 0)
            Data["colliders"] = _colliders;
        if (_timelines.Count > 0)
            Data["timelines"] = _timelines;
        _tree.WriteData(_file, Data);
    }
}
