using Engine.Core;
using Godot;

namespace Game.Tests.Movies;

/// <summary>
/// Plays one recorded movie through the game and checks the game against the cartridge on every update: the scene
/// <c>godot/movie.ps1</c> runs. The game is begun as a player's is, on an empty folder of saves, and is given one
/// update for each update of the movie, with the movie's buttons; hardware lag is not played. It stops at the first
/// update on which the game and the cartridge differ, and never puts the game right from the movie.
/// </summary>
/// <remarks>
/// Arguments, after Godot's <c>--</c>: the trace's folder, and optionally <c>--until N</c> to stop, as passed, once
/// the movie's update N has been played and found to agree.
/// </remarks>
public partial class MovieRunner : Node
{
    /// <summary>How many of the latest updates a difference is reported with.</summary>
    private const int Remembered = 12;

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
        Console.WriteLine($"MOVIE {trace.Name}: {trace.Updates.Count} updates, {trace.RecordCount} records");

        GameDefinition definition = ResourceLoader.Load<GameDefinition>("res://game/game_definition.tres");
        string saves = Path.Combine(Path.GetTempPath(), "movie-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(saves);
        try
        {
            return Play(trace, definition, saves, until);
        }
        finally
        {
            Directory.Delete(saves, recursive: true);
        }
    }

    private int Play(MovieTrace trace, GameDefinition definition, string saves, int until)
    {
        var options = Options.Load(Path.Combine(saves, "options.json"));
        var controller = new MovieController(definition, options);
        var loop = new GameLoop { Name = "MovieLoop" };
        loop.Start(definition, controller, Engine.Content.ContentLibrary.ForProject(), new SaveFile(saves, definition.SaveSlots), options);
        AddChild(loop);
        loop.SetPhysicsProcess(false);
        var director = new Engine.Audio.AudioDirector(loop.Session);
        loop.AddChild(director);
        director.SetPhysicsProcess(false);

        var check = new MovieCheck(trace, loop.Session);
        var latest = new Queue<string>();
        foreach (MovieUpdate update in trace.Updates)
        {
            if (update.ConsoleReset)
                throw new NotSupportedException($"Update {update.Number} follows a reset of the console, which this does not play yet.");
            controller.Held = update.Buttons;
            loop.Step();
            director.Step();

            latest.Enqueue($"  update {update.Number} (movie frame {update.SourceFrame}, {update.Kind}) buttons {update.Buttons:X4}: {check.Describe()}");
            if (latest.Count > Remembered)
                latest.Dequeue();

            if (update.ExpectedRecord is { } record)
            {
                trace.ReadTo(record);
                if (check.Differences() is { Count: > 0 } differences)
                {
                    Console.WriteLine($"DIFFERENCE at update {update.Number} of {trace.Name} (movie frame {update.SourceFrame}, {update.Kind}):");
                    foreach (string difference in differences)
                        Console.WriteLine("  " + difference);
                    Console.WriteLine("The latest updates, as the game played them:");
                    foreach (string line in latest)
                        Console.WriteLine(line);
                    return 1;
                }
            }

            if (update.Number >= until)
            {
                Console.WriteLine($"AGREED through update {update.Number} of {trace.Name}, as far as was asked.");
                return 0;
            }
        }

        Console.WriteLine($"PASSED {trace.Name}: all {trace.Updates.Count} updates agree.");
        return 0;
    }
}
