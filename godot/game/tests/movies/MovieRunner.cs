using Engine.Core;
using Engine.Input;
using Godot;

namespace Game.Tests.Movies;

/// <summary>
/// Plays one recorded movie through the game and checks the game against the cartridge on every update: the scene
/// <c>godot/movie.ps1</c> runs. The game is given one update for each update of the movie, with the movie's buttons;
/// hardware lag is not played. It stops at the first update on which the game and the cartridge differ, and never
/// puts the game right from the movie. What the host of a recording stands in for is the hardware the recording was
/// made on: its sound program answers the game when the cartridge's answered (<see cref="MovieGame"/>).
/// </summary>
/// <remarks>
/// <para>
/// A movie that begins as the console is switched on begins the game as a player's is, on an empty folder of saves.
/// One that begins from a snapshot is brought to the snapshot first (<see cref="MovieStart"/>). A reset of the
/// console inside a movie ends the game and begins it again on the same saves. A message box, which the cartridge
/// plays inside one of its updates reading the controller itself, is played with the movie's own frames
/// (<see cref="MovieFile"/>) for as long as the cartridge's lasted.
/// </para>
/// <para>
/// Arguments, after Godot's <c>--</c>: the trace's folder, and optionally <c>--until N</c> to stop, as agreeing, once
/// the movie's update N has been played and found to agree. It ends with 0 when the movie agrees as far as it was
/// played, 1 at the first difference, 2 on an error, and 3 when every update agrees but kinds of enemy the movie met
/// are not yet compared (<see cref="CartridgeEnemies"/>).
/// </para>
/// </remarks>
public partial class MovieRunner : Node
{
    /// <summary>How many of the latest updates a difference is reported with.</summary>
    private const int Remembered = 12;

    private GameLoop? _loop;
    private Engine.Audio.AudioDirector? _director;
    private MovieController? _controller;

    /// <summary>The states of the cartridge's sound handler in the record of the update last played; null when that update had none.</summary>
    private int[]? _soundStates;

    /// <inheritdoc/>
    public override void _Ready()
    {
        int exitCode;
        try
        {
            exitCode = Run(OS.GetCmdlineUserArgs());
        }
        catch (Exception exception)
        {
            // The process boundary: nothing may escape to the engine, which would log it and carry on.
            Console.Error.WriteLine(exception.ToString());
            exitCode = 2;
        }

        GetTree().Quit(exitCode);
    }

    private GameSession Session => (_loop ?? throw new InvalidOperationException("The game has not been begun.")).Session;

    private int Run(string[] arguments)
    {
        if (arguments.Length == 0)
            throw new ArgumentException("Give the folder of the movie's trace.");
        string folder = arguments[0];
        int until = int.MaxValue;
        for (int index = 1; index < arguments.Length; index++)
        {
            if (arguments[index] == "--until" && index + 1 < arguments.Length)
                until = int.Parse(arguments[++index], System.Globalization.CultureInfo.InvariantCulture);
            else
                throw new ArgumentException($"The argument \"{arguments[index]}\" is not known.");
        }

        using MovieTrace trace = MovieTrace.Open(folder);
        string repository = Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), ".."));
        MovieFile file = MovieFile.Of(trace, repository);
        Console.WriteLine($"MOVIE {trace.Name}: {trace.Updates.Count} updates, {trace.RecordCount} records, layout {trace.Layout}, " +
            (trace.StartsFromReset ? "from switching on" : "from a snapshot"));

        GameDefinition definition = MovieGame.Of(ResourceLoader.Load<GameDefinition>("res://game/game_definition.tres"));
        string saves = Path.Combine(Path.GetTempPath(), "movie-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(saves);
        try
        {
            if (trace.StartsFromReset)
            {
                // The cartridge's saved games, and the slot its choice of saved game begins on, as the game keeps them.
                var kept = new SaveFile(saves, definition.SaveSlots);
                for (int slot = 0; slot < definition.SaveSlots; slot++)
                {
                    if (!CartridgeSave.Holds(file.KeptMemory, slot))
                        continue;
                    CartridgeSave save = CartridgeSave.OfSlot(file.KeptMemory, slot, Engine.Content.ContentLibrary.ForProject());
                    kept.Write(slot + 1, save.Data);
                    Console.WriteLine($"SAVED GAME in slot {slot + 1} of the cartridge: {save.Data.Room} at {save.Data.EntryPoint}, {save.Data.Facts.Count} facts, {save.Data.Explored.Count} cells explored.");
                    foreach (string missing in save.NotCarried)
                        Console.WriteLine("  not carried: " + missing);
                }

                kept.ChosenSlot = CartridgeSave.ChosenSlot(file.KeptMemory) + 1;
                Begin(definition, saves);
            }
            else
            {
                Begin(definition, saves);
                IReadOnlyList<string> missed = MovieStart.FromSnapshot(Session, trace.InitialMemory(), Step);
                Console.WriteLine("BEGUN from the snapshot. Not carried from it yet:");
                foreach (string missing in missed)
                    Console.WriteLine("  " + missing);
            }

            return Play(trace, file, definition, saves, until);
        }
        finally
        {
            End();
            Directory.Delete(saves, recursive: true);
        }
    }

    /// <summary>Begins the game on the saves in <paramref name="saves"/>, as switching the console on does.</summary>
    private void Begin(GameDefinition definition, string saves)
    {
        // The options as the game's own start makes them: the game's standard buttons, and its actions on its controls.
        Options options = Options.Load(Path.Combine(saves, "options.json"), definition.StandardBindings);
        options.AddMissingControls(definition.StandardControls);
        _controller = new MovieController(definition, options);
        _loop = new GameLoop { Name = "MovieLoop" };
        _loop.Start(definition, _controller, Engine.Content.ContentLibrary.ForProject(), new SaveFile(saves, definition.SaveSlots), options);
        AddChild(_loop);
        _loop.SetPhysicsProcess(false);
        _director = new Engine.Audio.AudioDirector(_loop.Session);
        _loop.AddChild(_director);
        _director.SetPhysicsProcess(false);
    }

    /// <summary>Ends the game, as switching the console off does.</summary>
    private void End()
    {
        if (_loop is null)
            return;
        RemoveChild(_loop);
        _loop.Free();
        (_loop, _director, _controller) = (null, null, null);
    }

    /// <summary>Plays one update of the game with <paramref name="input"/>, and gives the sounds' host its turn after it.</summary>
    private void Step(GameInput input)
    {
        Session.Step(input);
        (_director ?? throw new InvalidOperationException("The game has not been begun.")).Step();
    }

    private GameInput InputOf(int held, int pressed) =>
        (_controller ?? throw new InvalidOperationException("The game has not been begun.")).InputOf(held, pressed);

    private int Play(MovieTrace trace, MovieFile file, GameDefinition definition, string saves, int until)
    {
        var check = new MovieCheck(trace);
        var latest = new Queue<string>();
        IReadOnlyList<MovieUpdate> updates = trace.Updates;
        for (int index = 0; index < updates.Count; index++)
        {
            MovieUpdate update = updates[index];
            MovieUpdate? next = index + 1 < updates.Count ? updates[index + 1] : null;
            bool boxContinues = Session.Mode is MessageBoxMode;
            ExpectSounds(trace, definition, update);
            Step(InputOf(update.Buttons, update.Pressed));
            string? box = PlayMessageBox(file, update, next, boxContinues);

            latest.Enqueue($"  update {update.Number} (movie frame {update.SourceFrame}, {update.Kind}) buttons {update.Buttons:X4} new {update.Pressed:X4}: {MovieCheck.Describe(Session)}");
            if (latest.Count > Remembered)
                latest.Dequeue();

            var differences = new List<string>();
            if (box is not null)
                differences.Add(box);
            if (next is { ConsoleReset: true })
            {
                // The cartridge's record at a reset is of the console before its start-up has run: it is not
                // compared, and the game is ended and begun again on the same saves.
                Console.WriteLine($"RESET of the console before update {next.Number} (movie frame {next.SourceFrame}).");
                End();
                Begin(definition, saves);
                _soundStates = null;
            }
            else if (update.ExpectedRecord is { } record)
            {
                trace.ReadTo(record);
                differences.AddRange(check.Differences(Session, update));
            }

            if (differences.Count > 0)
            {
                Console.WriteLine($"DIFFERENCE at update {update.Number} of {trace.Name} (movie frame {update.SourceFrame}, {update.Kind}):");
                foreach (string difference in differences)
                    Console.WriteLine("  " + difference);
                Console.WriteLine("The latest updates, as the game played them:");
                foreach (string line in latest)
                    Console.WriteLine(line);
                ReportUndecided(check);
                return 1;
            }

            if (update.Number >= until)
            {
                Console.WriteLine($"AGREED through update {update.Number} of {trace.Name}, as far as was asked.");
                ReportUndecided(check);
                return 0;
            }
        }

        Console.WriteLine($"AGREED on all {updates.Count} updates of {trace.Name}.");
        if (ReportUndecided(check))
            return 3;
        Console.WriteLine($"PASSED {trace.Name}.");
        return 0;
    }

    /// <summary>
    /// Tells the game's sound program what the recording shows of the update about to be played: the states of the
    /// cartridge's sound handler as it began and as it ended, where the update and the one before it have records.
    /// </summary>
    private void ExpectSounds(MovieTrace trace, GameDefinition definition, MovieUpdate update)
    {
        CapturedSoundProgram program = ((MovieGame)definition).Program ?? throw new InvalidOperationException("The game has not been begun.");
        int[]? before = _soundStates;
        _soundStates = null;
        if (update.ExpectedRecord is { } record)
        {
            trace.ReadTo(record);
            _soundStates = [.. Enumerable.Range(0, Cartridge.SoundLibraries).Select(library => trace.Word(Cartridge.SoundStates + library) & 0xFF)];
        }

        program.Expect(before, _soundStates);
    }

    /// <summary>
    /// Plays the frames of a message box the cartridge played inside <paramref name="update"/>, with the movie's own
    /// buttons on each, and returns how the game's box differed from the cartridge's; null when it did not.
    /// </summary>
    private string? PlayMessageBox(MovieFile file, MovieUpdate update, MovieUpdate? next, bool boxContinues)
    {
        if (Session.Mode is not MessageBoxMode)
        {
            if (update.MessageBoxStartFrame is { } opened)
                return $"the cartridge opened a message box on movie frame {opened}; the game opened none";
            if (update.MessageBoxEndFrame is { } closed)
                return $"the cartridge closed a message box on movie frame {closed}; the game had none open";
            return null;
        }

        if (boxContinues && update.MessageBoxStartFrame is not null)
            return "the cartridge opened a new message box while the game's was still open";
        if (!boxContinues && update.MessageBoxStartFrame is null)
            return "the game opened a message box; the cartridge opened none in this update";
        int start = boxContinues ? update.SourceFrame : update.MessageBoxStartFrame!.Value;

        // The cartridge's box returns on its last frame; frames of the update past it are hardware lag.
        int limit = update.MessageBoxEndFrame is { } end ? end + 1 : update.EndSourceFrame;
        int frame = start + 1;
        for (; frame < limit && Session.Mode is MessageBoxMode; frame++)
            Step(InputOf(file.ButtonsOn(frame), file.ButtonsOn(frame) & ~file.ButtonsOn(frame - 1)));

        if (update.MessageBoxEndFrame is { } last)
        {
            if (Session.Mode is MessageBoxMode)
                return $"the cartridge's message box closed on movie frame {last}; the game's is still open after it";
            if (frame != last + 1)
                return $"the cartridge's message box closed on movie frame {last}; the game's closed after movie frame {frame - 1}";
            return null;
        }

        // A box that asks a question reads the controller in an update of its own, which follows.
        if (Session.Mode is not MessageBoxMode)
            return $"the cartridge's message box is open past movie frame {update.EndSourceFrame - 1}; the game's closed after movie frame {frame - 1}";
        if (next is not { Kind: MovieUpdate.Continuation })
            return "the cartridge's message box is open past this update, and the next is not an update that goes on inside it";
        return null;
    }

    /// <summary>Prints the kinds of enemy the movie has met that are not compared yet, and returns whether there are any.</summary>
    private static bool ReportUndecided(MovieCheck check)
    {
        if (check.UndecidedEnemyKinds.Count == 0)
            return false;
        Console.WriteLine($"NOT COMPARED: {check.UndecidedEnemyKinds.Count} kinds of enemy met in play are not decided in CartridgeEnemies.cs: " +
            string.Join(" ", check.UndecidedEnemyKinds.Select(kind => $"${kind:X4}")));
        return true;
    }
}
