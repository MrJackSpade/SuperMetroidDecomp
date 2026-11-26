using Engine.Core;
using Engine.Input;
using Engine.Shared;

namespace Engine.Tests.Core;

/// <summary>Checks of the engine's own modes: play, the change of rooms, pausing, messages, death, loading and cinematics.</summary>
public sealed class ModeChecks
{
    [Check]
    public void PlayStartsInTheFirstRoomAtItsEntryPoint()
    {
        using var game = new StandInGame();
        var playing = (PlayingMode)game.Mode;
        Expect.Equal("a", playing.RoomScene);
        Expect.Equal(PixelPosition.FromWhole(40, 80), game.Session.Player.Position);
        game.Step(3);
        Expect.Equal(3L, game.Session.Progress.PlayUpdates);
    }

    [Check]
    public void PausingStopsPlayUntilItIsPressedAgain()
    {
        using var game = new StandInGame();
        GameMode playing = game.Mode;

        game.Press(GameAction.Pause);
        Expect.True(game.Mode is PausedMode);
        long played = game.Session.Progress.PlayUpdates;
        game.Step(5);
        Expect.Equal(played, game.Session.Progress.PlayUpdates);

        game.Press(GameAction.Pause);
        Expect.True(ReferenceEquals(playing, game.Mode));
        Expect.Equal(1, game.Loop.GetChildCount());
        game.Step(2);
        Expect.Equal(played + 2, game.Session.Progress.PlayUpdates);
    }

    [Check]
    public void ASaveAskedForDuringPlayIsWrittenToTheSessionsSlot()
    {
        using var game = new StandInGame();
        game.Session.Progress.Record("before the save");
        game.Session.RequestSave("", "Start");
        Expect.False(game.Saves.Has(1));
        game.Step(1);

        SaveData save = game.Saves.Read(1);
        Expect.Equal("a", save.Room);
        Expect.Equal("Start", save.EntryPoint);
        Expect.True(save.Facts.Contains("before the save"));
    }

    [Check]
    public void DyingReloadsTheSaveAndForgetsWhatCameAfterIt()
    {
        using var game = new StandInGame();
        GameMode firstPlay = game.Mode;
        var firstPlayer = game.Session.Player;
        game.Session.Progress.Record("before the save");
        game.Session.RequestSave("", "Start");
        game.Step(1);
        game.Session.Progress.Record("after the save");

        firstPlayer.TakeHit(new Hit(500, HitSide.Enemies, HitKind.Contact), firstPlayer.Hurtbox);
        game.StepUntil(() => game.Mode is PlayingMode && !ReferenceEquals(game.Mode, firstPlay));

        Expect.True(game.Session.Progress.Has("before the save"));
        Expect.False(game.Session.Progress.Has("after the save"));
        Expect.False(ReferenceEquals(firstPlayer, game.Session.Player));
        Expect.False(Godot.GodotObject.IsInstanceValid(firstPlayer));
        Expect.Equal("a", ((PlayingMode)game.Mode).RoomScene);
        Expect.Equal(1, game.Loop.GetChildCount());
    }

    [Check]
    public void DyingWithNoSaveStartsTheGameAgain()
    {
        using var game = new StandInGame();
        GameMode firstPlay = game.Mode;
        game.Session.Progress.Record("something happened");
        game.Session.Player.TakeHit(new Hit(500, HitSide.Enemies, HitKind.Contact), game.Session.Player.Hurtbox);
        game.StepUntil(() => game.Mode is PlayingMode && !ReferenceEquals(game.Mode, firstPlay));

        Expect.False(game.Session.Progress.Has("something happened"));
        Expect.Equal(PixelPosition.FromWhole(40, 80), game.Session.Player.Position);
    }

    [Check]
    public void LeavingAModeLaidOverPlayForAnotherEndsPlayToo()
    {
        using var game = new StandInGame();
        game.Press(GameAction.Pause);
        var definition = (TwoRoomDefinition)game.Session.Definition;
        Expect.Equal(2, game.Loop.GetChildCount());

        // A game's own pause screen may quit to its title; the engine's hand-over then ends the play beneath it.
        definition.QuitFromPause = true;
        game.Press(GameAction.Cancel);
        Expect.True(game.Mode is StandInMode);
        Expect.Equal(1, game.Loop.GetChildCount());
    }

    [Check]
    public void ACinematicPlaysToItsEndThenHandsOver()
    {
        using var game = new StandInGame(() => new StandInCinematic { Length = 4 });
        var cinematic = (StandInCinematic)game.Mode;
        game.Step(3);
        Expect.True(ReferenceEquals(cinematic, game.Mode));
        game.Step(1);
        Expect.True(game.Mode is StandInMode);
        Expect.True(cinematic.Played.SequenceEqual([0, 1, 2, 3]));
    }

    [Check]
    public void ConfirmingSkipsACinematic()
    {
        using var game = new StandInGame(() => new StandInCinematic { Length = 500 });
        game.Press(GameAction.Confirm);
        Expect.True(game.Mode is StandInMode);
    }
}
