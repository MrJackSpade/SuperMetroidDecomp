using Engine.Content;
using Godot;

namespace Engine.Core;

/// <summary>
/// What an exported game shows when its definition says how its assets are made (<see cref="GameDefinition.AssetsMaker"/>)
/// and they are not there, or were made by another version of the maker. It asks the player for the file they are
/// made from with the system's own file chooser, keeps that file, has the assets made off the main thread, shows how
/// far the making has come as a bar, the step under way and a count that moves with the work, and then tells whoever
/// showed it that the game can begin. With the file kept from an earlier time it asks nothing and makes them at
/// once: a new version of the game makes its assets again by itself. A making that fails is written to the log in
/// full and said on the screen, and the player may choose a file again.
/// </summary>
/// <remarks>
/// Beside the assets' folder it keeps two files, named as the folder is: one with <c>.source</c> after, the file the
/// player chose, and one with <c>.made</c> after, the version of the maker the assets there were made by.
/// </remarks>
public partial class AssetsSetup : Control
{
    private AssetsMaker? _maker;
    private string _folder = "";
    private Action? _made;
    private Label? _words;
    private ProgressBar? _bar;

    /// <summary>The making under way; null while there is none.</summary>
    private System.Threading.Tasks.Task? _making;

    /// <summary>Whether the file chooser is open.</summary>
    private bool _choosing;

    /// <summary>How far the making last said it had come: written from its threads, read from the main one.</summary>
    private volatile MakingProgress? _progress;

    /// <summary>The step last written to the log.</summary>
    private string _logged = "";

    /// <summary>
    /// Whether the assets in <paramref name="folder"/> have to be made before the game can begin: there are none, or
    /// those there were made by another version of <paramref name="maker"/>.
    /// </summary>
    public static bool IsNeeded(AssetsMaker maker, string folder)
    {
        ArgumentNullException.ThrowIfNull(maker);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        return !Directory.Exists(folder) || !File.Exists(MadeFile(folder)) || File.ReadAllText(MadeFile(folder)) != maker.Version;
    }

    /// <summary>A setup that has <paramref name="maker"/> make the assets into <paramref name="folder"/> and then calls <paramref name="made"/>.</summary>
    /// <param name="maker">How the game's assets are made.</param>
    /// <param name="folder">The folder the game reads its assets from.</param>
    /// <param name="made">Called on the main thread once the assets are there; the setup has taken itself away by then.</param>
    public static AssetsSetup Of(AssetsMaker maker, string folder, Action made)
    {
        ArgumentNullException.ThrowIfNull(maker);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        ArgumentNullException.ThrowIfNull(made);
        return new AssetsSetup { Name = "AssetsSetup", _maker = maker, _folder = folder, _made = made };
    }

    private AssetsMaker Maker => _maker ?? throw new InvalidOperationException("The assets' setup was given no maker.");

    /// <summary>The file that says which version of the maker made the assets in a folder.</summary>
    private static string MadeFile(string folder) => folder + ".made";

    /// <summary>The kept file the assets in a folder are made from.</summary>
    private static string SourceFile(string folder) => folder + ".source";

    /// <inheritdoc/>
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // The words fill the screen above the bar, which lies across its lower part.
        _words = new Label
        {
            Name = "Words",
            AutowrapMode = TextServer.AutowrapMode.Arbitrary,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ClipText = true,
            AnchorRight = 1,
            AnchorBottom = 0.75f,
        };
        _words.AddThemeFontSizeOverride("font_size", 8);
        AddChild(_words);
        _bar = new ProgressBar
        {
            Name = "Bar",
            MinValue = 0,
            MaxValue = 1,
            Step = 0,
            ShowPercentage = false,
            Visible = false,
            AnchorLeft = 0.1f,
            AnchorRight = 0.9f,
            AnchorTop = 0.8f,
            AnchorBottom = 0.85f,
        };
        AddChild(_bar);
        if (File.Exists(SourceFile(_folder)))
            Make();
        else
            Ask();
    }

    /// <summary>Shows what the player is asked for.</summary>
    private void Ask() => Say("The game's assets are not installed.", Maker.Asks, "Press any button.");

    /// <summary>Shows lines of words, a blank line between each two.</summary>
    private void Say(params string[] lines)
    {
        if (_words is not null)
            _words.Text = string.Join("\n\n", lines.Where(line => line.Length > 0));
    }

    /// <inheritdoc/>
    public override void _Input(InputEvent @event)
    {
        if (_making is not null || _choosing)
            return;
        bool pressed = @event is InputEventKey { Pressed: true, Echo: false } or InputEventJoypadButton { Pressed: true }
            or InputEventScreenTouch { Pressed: true } or InputEventMouseButton { Pressed: true };
        if (!pressed)
            return;

        _choosing = true;
        Error shown = DisplayServer.FileDialogShow(
            Maker.Asks, "", "", false, DisplayServer.FileDialogMode.OpenFile, [.. Maker.Filters], Callable.From<bool, string[], int>(Chosen));
        if (shown != Error.Ok)
        {
            _choosing = false;
            Fail($"The file chooser could not be shown: {shown}.");
        }
    }

    /// <summary>Takes the file the player chose, or their leaving the chooser without one, keeps it and starts the making.</summary>
    private void Chosen(bool chose, string[] files, int filter)
    {
        _choosing = false;
        if (!chose || files.Length == 0)
        {
            Ask();
            return;
        }

        // The chooser may name the file by an address only the engine's own file access can open, so it is read here
        // and kept as a plain file.
        GD.Print($"Making the assets from \"{files[0]}\".");
        byte[] source = Godot.FileAccess.GetFileAsBytes(files[0]);
        if (source.Length == 0)
        {
            Fail($"The chosen file could not be read: {Godot.FileAccess.GetOpenError()}.");
            return;
        }

        File.WriteAllBytes(SourceFile(_folder), source);
        Make();
    }

    /// <summary>Starts the making from the kept file, off the main thread.</summary>
    private void Make()
    {
        AssetsMaker maker = Maker;
        string folder = _folder, source = SourceFile(folder);

        // What says the assets are made goes first: from here until the making ends they are no version's.
        File.Delete(MadeFile(folder));
        _progress = null;
        _making = System.Threading.Tasks.Task.Run(() => maker.Make(source, folder, new Steps(this)));
    }

    /// <inheritdoc/>
    public override void _Process(double delta)
    {
        if (_making is not { } making)
            return;
        if (!making.IsCompleted)
        {
            ShowProgress(_progress);
            return;
        }

        _making = null;
        if (_bar is not null)
            _bar.Visible = false;
        if (making.Exception is { } failure)
        {
            Console.Error.WriteLine(failure.ToString());
            GD.PrintErr(failure.ToString());
            Fail((failure.InnerException ?? failure).Message);
            return;
        }

        File.WriteAllText(MadeFile(_folder), Maker.Version);
        GD.Print("The game's assets are made.");
        Action made = _made ?? throw new InvalidOperationException("The assets' setup was given nothing to do once they are made.");
        GetParent().RemoveChild(this);
        QueueFree();
        made();
    }

    /// <summary>Shows how far the making has come: the bar, the step under way and its count.</summary>
    private void ShowProgress(MakingProgress? progress)
    {
        const string what = "Making the game's assets. This is done once for each version of the game.";
        if (progress is null)
        {
            Say(what);
            return;
        }

        if (progress.Doing != _logged)
        {
            // Each step goes to the log as well, where a making that stalls or fails can be followed.
            _logged = progress.Doing;
            GD.Print($"Making the assets: {progress.Doing} ({progress.Done:P0})");
        }

        if (_bar is not null)
        {
            _bar.Visible = true;
            _bar.Value = progress.Done;
        }

        Say(what, progress.Doing, $"{progress.Done:P0}   {progress.Detail}");
    }

    /// <summary>Says a failure on the screen and in the log, and lets the player choose a file again.</summary>
    private void Fail(string what)
    {
        GD.PrintErr($"The assets could not be made: {what}");
        Say("The assets could not be made.", what, "Press any button to choose the file again.");
    }

    /// <summary>Keeps how far the making last said it had come, for the main thread to show.</summary>
    private sealed class Steps(AssetsSetup setup) : IProgress<MakingProgress>
    {
        /// <inheritdoc/>
        public void Report(MakingProgress value) => setup._progress = value;
    }
}
