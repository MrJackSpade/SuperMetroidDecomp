using Engine.Content;
using Engine.Input;
using Godot;

namespace Engine.Core;

/// <summary>
/// The one owner of advancing the game. Godot calls it sixty times a second; each call it takes the update's input and
/// plays one update of the session. Nothing else in gameplay advances itself from Godot's own callbacks, and Godot's
/// node order never decides the order of play. It is also the one place a failure is caught: the error is printed in
/// full and the program ends with a failing exit code.
/// </summary>
public partial class GameLoop : Node
{
    /// <summary>The project setting naming the game definition to run.</summary>
    public const string GameDefinitionSetting = "engine/game_definition";

    /// <summary>The running game, once started.</summary>
    private GameSession? _session;

    /// <summary>Makes each update's input, once started.</summary>
    private InputSampler? _input;

    /// <summary>
    /// Starts a session of <paramref name="definition"/>'s game, taking its input from <paramref name="source"/> and
    /// its assets from <paramref name="content"/>.
    /// </summary>
    public void Start(GameDefinition definition, IInputSource source, ContentLibrary content, SaveFile saves, Options options)
    {
        if (_session is not null)
            throw new InvalidOperationException("The game loop has already been started.");
        ArgumentNullException.ThrowIfNull(definition);
        definition.CheckRememberedNames();
        _input = new InputSampler(source);
        _session = new GameSession(definition, this, content, saves, options);
    }

    /// <summary>The running game.</summary>
    /// <exception cref="InvalidOperationException">The loop has not been started.</exception>
    public GameSession Session => _session ?? throw new InvalidOperationException("The game loop has not been started.");

    /// <summary>Plays one update: takes the update's input and steps the session.</summary>
    public void Step()
    {
        InputSampler input = _input ?? throw new InvalidOperationException("The game loop has not been started.");
        Session.Step(input.Sample());
    }

    /// <summary>
    /// When the loop is the program's main scene, starts the game the project setting names, played from the player's
    /// devices. A loop created in code is started by <see cref="Start"/> and does nothing here.
    /// </summary>
    public override void _Ready()
    {
        if (_session is not null)
            return;
        try
        {
            string path = ProjectSettings.GetSetting(GameDefinitionSetting, "").AsString();
            if (path.Length == 0)
                throw new InvalidOperationException($"The project setting \"{GameDefinitionSetting}\" names no game definition.");
            GameDefinition definition = ResourceLoader.Load<GameDefinition>(path)
                ?? throw new InvalidOperationException($"\"{path}\" is not a game definition.");

#if !TOOLS
            // An exported game with no assets installed has nothing of the game to show. One whose definition says
            // how its assets are made has them made first, when there are none or they are another version's, and
            // begins after; any other says where they belong and starts nothing.
            if (definition.AssetsMaker is { } maker)
            {
                if (AssetsSetup.IsNeeded(maker, ContentLibrary.InstalledFolder))
                {
                    AddChild(AssetsSetup.Of(maker, ContentLibrary.InstalledFolder, () => Begin(definition)));
                    return;
                }
            }
            else if (!Directory.Exists(ContentLibrary.InstalledFolder))
            {
                ShowNoAssets(ContentLibrary.InstalledFolder);
                return;
            }
#endif

            Begin(definition);
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    /// <summary>Begins the game the project names, played from the player's devices, its assets being there.</summary>
    private void Begin(GameDefinition definition)
    {
        try
        {
            IReadOnlyList<GameAction> actions = definition.AllActions;
            Options options = Options.Load(ProjectSettings.GlobalizePath("user://options.json"), definition.StandardBindings);
            options.AddMissingControls(definition.StandardControls);
            options.ApplyBindings(actions);
            var devices = new DeviceInputSource();
            AddChild(devices);
            devices.Watch(actions);
            var saves = new SaveFile(ProjectSettings.GlobalizePath("user://saves"), definition.SaveSlots);
            Start(definition, devices, ContentLibrary.ForProject(), saves, options);
            AddChild(new Engine.Audio.AudioDirector(Session));
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

#if !TOOLS
    /// <summary>Fills the screen with where the game's assets belong, in the engine's own font.</summary>
    private void ShowNoAssets(string folder)
    {
        Vector2 size = GetViewport().GetVisibleRect().Size;
        var words = new Label
        {
            Name = "NoAssets",
            Text = string.Join(
                "\n\n",
                "The game's assets are not installed.",
                "Make them from your own ROM on a computer and copy them to:",
                folder),
            AutowrapMode = TextServer.AutowrapMode.Arbitrary,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = Vector2.Zero,
            Size = size,
        };
        words.AddThemeFontSizeOverride("font_size", 8);
        AddChild(words);
        GD.PrintErr($"No assets are installed at \"{folder}\".");
    }
#endif

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        if (_session is null)
            return;
        try
        {
            Step();
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

    /// <summary>
    /// The process boundary. Godot would log an escaping exception and carry on with half an update done, so the error
    /// is printed in full and the program ends with a failing exit code.
    /// </summary>
    private void Fail(Exception exception)
    {
        Console.Error.WriteLine(exception.ToString());
        SetPhysicsProcess(false);
        GetTree().Quit(1);
    }
}
