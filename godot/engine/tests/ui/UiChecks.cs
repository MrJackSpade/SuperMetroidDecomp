using Engine.Audio;
using Engine.Content;
using Engine.Core;
using Engine.Tests.Content;
using Engine.Tests.Core;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Engine.Tests.Ui;

/// <summary>Checks of the building blocks for screens: fades, typed text, the map, and the audio director.</summary>
public sealed class UiChecks
{
    private const string CavesMap = """
        { "name": "caves", "revealed_by": "caves map seen",
          "sheet": "maps/tiles.png", "tile_size": 8,
          "tiles": { "corridor": { "column": 0, "row": 0 }, "save": { "column": 1, "row": 0 } },
          "cells": [ { "column": 3, "row": 2, "shows": "corridor" },
                     { "column": 4, "row": 2, "shows": "corridor" },
                     { "column": 5, "row": 2, "shows": "corridor", "secret": true } ],
          "markers": [ { "column": 4, "row": 2, "shows": "save" } ] }
        """;

    [Check]
    public void AFadeCoversTheScreenOverItsUpdatesThenUncoversIt()
    {
        var fade = new ScreenFade();
        fade.FadeOut(4);
        Expect.Equal(0, fade.Covered);
        fade.Step();
        Expect.Equal(1, fade.Covered);
        Expect.False(fade.IsDone);
        fade.Step();
        fade.Step();
        fade.Step();
        Expect.True(fade.IsCovered);
        fade.Step();
        Expect.Equal(4, fade.Covered);
        Expect.Equal(1f, fade.GetNode<ColorRect>("Cover").Color.A);

        fade.FadeIn(2);
        Expect.False(fade.IsCovered);
        Expect.Equal(2, fade.Covered);
        fade.Step();
        fade.Step();
        Expect.True(fade.IsDone);
        Expect.Equal(0, fade.Covered);
        Expect.Equal(0f, fade.GetNode<ColorRect>("Cover").Color.A);
        Expect.Throws<ArgumentOutOfRangeException>(() => fade.FadeOut(0));
        fade.Free();
    }

    [Check]
    public void TypedTextShowsACharacterAtATimeWithASoundForEachThatIsNotASpace()
    {
        var text = new TypedText { UpdatesPerCharacter = 2, Sound = "tick" };
        var sounds = new SoundRequests(new Engine.Tests.Core.ImmediateSoundFlow());
        text.Type("ab c");
        Expect.Equal(0, text.Shown);

        text.Step(sounds);
        Expect.Equal(1, text.Shown);
        text.Step(sounds);
        Expect.Equal(1, text.Shown);
        text.Step(sounds);
        Expect.Equal(2, text.Shown);
        Expect.Equal(2, text.VisibleCharacters);
        for (int update = 0; update < 4; update++)
            text.Step(sounds);
        Expect.True(text.IsDone);
        Expect.Equal(3, sounds.TakeAccepted().Count);

        text.Type("again");
        text.Finish();
        Expect.True(text.IsDone);
        Expect.Equal(5, text.VisibleCharacters);
        text.Free();
    }

    [Check]
    public void TheMapShowsWhatIsExploredAndWhatIsRevealedButNeverASecretCellUnexplored()
    {
        using var assets = MapAssets();
        Area area = Area.Read(assets.Library.ReadData("areas/caves.json"));
        var progress = new Progress();
        var map = new MapView();
        map.Present(area, progress, assets.Library);
        Expect.Equal(0, map.CellsDrawn().Count);
        Expect.Equal(0, map.MarkersDrawn().Count);

        progress.MarkExplored("caves", 3, 2);
        Expect.Equal(1, map.CellsDrawn().Count);
        Expect.Equal(0, map.MarkersDrawn().Count);

        progress.Record("caves map seen");
        Expect.Equal(2, map.CellsDrawn().Count);
        Expect.Equal(1, map.MarkersDrawn().Count);
        Expect.False(map.IsExplored(area.CellAt(4, 2)!));

        progress.MarkExplored("caves", 5, 2);
        Expect.Equal(3, map.CellsDrawn().Count);
        map.Free();
    }

    [Check]
    public void TheMapDrawsOnlyTheCellsInsideItsWindow()
    {
        using var assets = MapAssets();
        Area area = Area.Read(assets.Library.ReadData("areas/caves.json"));
        var progress = new Progress();
        progress.Record("caves map seen");
        var map = new MapView { Window = new Vector2I(3, 3), Centre = new Vector2I(2, 2) };
        map.Present(area, progress, assets.Library);

        Expect.Equal(1, map.CellsDrawn().Count);
        Expect.Equal(new Vector2I(3, 2), map.CellsDrawn()[0].Place);
        map.Centre = new Vector2I(4, 2);
        Expect.Equal(2, map.CellsDrawn().Count);
        map.Free();
    }

    [Check]
    public void AnAreaKnowsWhereOnItsSheetEachPictureIs()
    {
        using var assets = MapAssets();
        Area area = Area.Read(assets.Library.ReadData("areas/caves.json"));
        Expect.Equal(new Rect2I(8, 0, 8, 8), area.PictureOf("save"));
        Expect.True(area.CellAt(9, 9) is null);
        Expect.True(area.CellAt(5, 2)!.Secret);
    }

    [Check]
    public void AnAreaWhoseCellShowsAPictureItDoesNotHaveIsRefused()
    {
        using var assets = new TempAssets();
        assets.With("area.json", """
            { "name": "caves", "sheet": "maps/tiles.png", "tile_size": 8, "tiles": {},
              "cells": [ { "column": 0, "row": 0, "shows": "corridor" } ] }
            """);
        Expect.Throws<ContentException>(() => Area.Read(assets.Library.ReadData("area.json")));
    }

    [Check]
    public void TheAudioDirectorPlaysWhatWasAcceptedAndFollowsTheMusic()
    {
        using var game = new StandInGame();
        game.Assets.WithBytes("sounds/beep.wav", TinyWav()).WithBytes("music/theme.wav", TinyWav());
        var director = new AudioDirector(game.Session);
        // Sound plays only from inside the scene tree, which the check's own game is not in.
        TestRunner.Stage.AddChild(director);

        game.Session.Sounds.Request("beep", channel: "player");
        game.Session.Sounds.RequestMusic("theme");
        director.Step();
        Expect.Equal(0, game.Session.Sounds.TakeAccepted().Count);
        Expect.True(director.GetNode<AudioStreamPlayer>("Channel0").Stream is not null);
        Expect.True(director.GetNode<AudioStreamPlayer>("Music").Stream is not null);

        game.Session.Sounds.RequestMusic("");
        director.Step();

        game.Session.Sounds.Request("no such sound");
        Expect.Throws<ContentException>(director.Step);
        director.GetParent().RemoveChild(director);
        director.Free();
    }

    /// <summary>Assets holding the caves map and its sheet of small pictures.</summary>
    private static TempAssets MapAssets()
    {
        var assets = new TempAssets();
        assets.With("areas/caves.json", CavesMap);
        Directory.CreateDirectory(Path.Combine(assets.Folder, "maps"));
        Image.CreateEmpty(16, 8, false, Image.Format.Rgba8).SavePng(Path.Combine(assets.Folder, "maps", "tiles.png"));
        return assets;
    }

    /// <summary>A very short sound file: eight silent samples.</summary>
    private static byte[] TinyWav()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        const int dataBytes = 16;
        writer.Write("RIFF"u8);
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(8000);
        writer.Write(16000);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataBytes);
        writer.Write(new byte[dataBytes]);
        writer.Flush();
        return stream.ToArray();
    }
}
