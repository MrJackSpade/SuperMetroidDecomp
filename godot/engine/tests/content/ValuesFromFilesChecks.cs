using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Tests.Content;

/// <summary>
/// Checks of values from data files: that they are read, that mistakes in a file are named, and that the editor shows a
/// file's values read-only and does not save them.
/// </summary>
public sealed class ValuesFromFilesChecks
{
    private const string Good = """
        {
          // A comment, which a person editing the file may leave.
          "health": 20,
          "speed": 1.25,
          "facing": "left",
          "tags": ["one", "two"],
        }
        """;

    [Check]
    public void ValuesAreReadFromTheNamedFile()
    {
        using var assets = new TempAssets().With("enemies/stand_in.json", Good);
        var node = new StandInValues { DataFile = "enemies/stand_in.json" };

        DataValues.Fill(node, assets.Library);

        Expect.Equal(20, node.Health);
        Expect.Equal(Pixels.FromParts(1, Pixels.FractionsPerPixel / 4), Pixels.FromNumber(node.Speed));
        Expect.Equal(Facing.Left, node.Facing);
        Expect.True(node.Tags.SequenceEqual(["one", "two"]));
        node.Free();
    }

    [Check]
    public void WithNoFileNamedTheSavedValuesAreKept()
    {
        using var assets = new TempAssets();
        var node = new StandInValues { Health = 7 };
        DataValues.Fill(node, assets.Library);
        Expect.Equal(7, node.Health);
        node.Free();
    }

    [Check]
    public void AMissingValueIsNamedWithItsFile()
    {
        using var assets = new TempAssets().With("broken.json", """{ "health": 20, "facing": "left", "tags": [] }""");
        var node = new StandInValues { DataFile = "broken.json" };
        string message = MessageOf(() => DataValues.Fill(node, assets.Library));
        Expect.True(message.Contains("broken.json"));
        Expect.True(message.Contains("speed"));
        node.Free();
    }

    [Check]
    public void AValueOfTheWrongKindIsNamedWithItsFile()
    {
        using var assets = new TempAssets().With(
            "broken.json", """{ "health": "lots", "speed": 1, "facing": "left", "tags": [] }""");
        var node = new StandInValues { DataFile = "broken.json" };
        string message = MessageOf(() => DataValues.Fill(node, assets.Library));
        Expect.True(message.Contains("broken.json"));
        Expect.True(message.Contains("health"));
        node.Free();
    }

    [Check]
    public void AMisspeltNameIsNamedWithTheNamesItCouldBe()
    {
        using var assets = new TempAssets().With(
            "broken.json", """{ "health": 1, "speed": 1, "facing": "sideways", "tags": [] }""");
        var node = new StandInValues { DataFile = "broken.json" };
        string message = MessageOf(() => DataValues.Fill(node, assets.Library));
        Expect.True(message.Contains("sideways"));
        Expect.True(message.Contains("left, right"));
        node.Free();
    }

    [Check]
    public void AMissingFileAndABadlyFormedFileAreNamed()
    {
        using var assets = new TempAssets().With("bad.json", "{ \"health\": ");
        Expect.True(MessageOf(() => assets.Library.ReadData("absent.json")).Contains("absent.json"));
        Expect.True(MessageOf(() => assets.Library.ReadData("bad.json")).Contains("bad.json"));
        Expect.Throws<ContentException>(() => assets.Library.ReadData("../outside.json"));
    }

    [Check]
    public void WithAFileNamedTheEditorShowsValuesReadOnlyAndDoesNotSaveThem()
    {
        var node = new StandInValues { DataFile = "enemies/stand_in.json" };
        Expect.True(UsageOf(node, "Health").HasFlag(PropertyUsageFlags.ReadOnly));
        Expect.False(UsageOf(node, "Health").HasFlag(PropertyUsageFlags.Storage));
        Expect.True(UsageOf(node, "Speed").HasFlag(PropertyUsageFlags.ReadOnly));
        Expect.False(UsageOf(node, "Label").HasFlag(PropertyUsageFlags.ReadOnly));
        Expect.True(UsageOf(node, "Label").HasFlag(PropertyUsageFlags.Storage));
        Expect.True(UsageOf(node, "DataFile").HasFlag(PropertyUsageFlags.Storage));
        node.Free();
    }

    [Check]
    public void WithNoFileNamedTheEditorEditsAndSavesValues()
    {
        var node = new StandInValues();
        Expect.False(UsageOf(node, "Health").HasFlag(PropertyUsageFlags.ReadOnly));
        Expect.True(UsageOf(node, "Health").HasFlag(PropertyUsageFlags.Storage));
        node.Free();
    }

    [Check]
    public void ASavedSceneHoldsTheFileNameButNotTheFilesValues()
    {
        using var assets = new TempAssets().With("enemies/stand_in.json", Good);
        var node = new StandInValues { Name = "StandIn", DataFile = "enemies/stand_in.json", Label = "kept" };
        DataValues.Fill(node, assets.Library);

        string saved = SaveAsScene(node, assets.Folder);

        Expect.True(saved.Contains("enemies/stand_in.json"));
        Expect.True(saved.Contains("kept"));
        Expect.False(saved.Contains("Health"));
        Expect.False(saved.Contains("Speed"));
        node.Free();
    }

    [Check]
    public void ASavedSceneHoldsValuesWhenNoFileIsNamed()
    {
        using var assets = new TempAssets();
        var node = new StandInValues { Name = "StandIn", Health = 33 };
        string saved = SaveAsScene(node, assets.Folder);
        Expect.True(saved.Contains("Health = 33"));
        node.Free();
    }

    /// <summary>Saves a node as a scene file and returns the file's text.</summary>
    private static string SaveAsScene(Node node, string folder)
    {
        var scene = new PackedScene();
        Expect.Equal(Error.Ok, scene.Pack(node));
        string file = Path.Combine(folder, "saved.tscn");
        Expect.Equal(Error.Ok, ResourceSaver.Save(scene, file));
        return File.ReadAllText(file);
    }

    /// <summary>How the editor is told to treat a property of <paramref name="target"/>.</summary>
    private static PropertyUsageFlags UsageOf(GodotObject target, string property)
    {
        foreach (Godot.Collections.Dictionary entry in target.GetPropertyList())
        {
            if (entry["name"].AsString() == property)
                return entry["usage"].As<PropertyUsageFlags>();
        }

        throw new CheckFailedException($"{target.GetType().Name} has no property named {property}.");
    }

    /// <summary>The message of the content error <paramref name="action"/> must raise.</summary>
    private static string MessageOf(Action action)
    {
        try
        {
            action();
        }
        catch (ContentException exception)
        {
            return exception.Message;
        }

        throw new CheckFailedException("Expected a content error naming the file.");
    }
}
