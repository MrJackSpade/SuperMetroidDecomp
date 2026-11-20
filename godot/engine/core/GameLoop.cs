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

    /// <summary>Starts a session of <paramref name="definition"/>'s game, taking its input from <paramref name="source"/>.</summary>
    public void Start(GameDefinition definition, IInputSource source)
    {
        if (_session is not null)
            throw new InvalidOperationException("The game loop has already been started.");
        _input = new InputSampler(source);
        _session = new GameSession(definition, this);
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

            var devices = new DeviceInputSource();
            AddChild(devices);
            devices.Watch(GameAction.Standard.Concat(definition.Actions.Select(name => new GameAction(name))));
            Start(definition, devices);
        }
        catch (Exception exception)
        {
            Fail(exception);
        }
    }

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
