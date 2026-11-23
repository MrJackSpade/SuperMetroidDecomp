using Engine.Core;
using Engine.Shared;
using Game.Cinematics;
using Godot;

namespace Game;

/// <summary>
/// Super Metroid, as the engine sees it: the one thing the engine is pointed at. It names this game's actions and their
/// buttons; the player's scene and everything else that is a setting is in its resource, <c>game_definition.tres</c>.
/// </summary>
[GlobalClass]
public partial class SuperMetroid : GameDefinition
{
    /// <summary>The definition, with this game's actions.</summary>
    public SuperMetroid() => Actions = [.. Data.Actions.All.Select(action => action.Name)];

    /// <inheritdoc/>
    public override Dictionary<string, List<string>> StandardBindings => Data.Actions.StandardBindings;

    /// <summary>The game starts on its title screen.</summary>
    public override GameMode CreateFirstMode() => FirstMode is null ? new Menus.TitleMode() : base.CreateFirstMode();

    /// <summary>Pausing brings up the map and equipment screens.</summary>
    public override GameMode CreatePauseMode(PlayingMode playing) => new PauseScreen(playing);

    /// <summary>A new game begins with the opening.</summary>
    public override GameMode CreateModeForNewGame() => new Intro();

    /// <summary>After a death comes the game over screen.</summary>
    public override GameMode CreateModeAfterDeath() => new Menus.GameOverMode();

    /// <summary>This game adds the escape countdown.</summary>
    public override IReadOnlyList<ISessionService> CreateServices() => [new Escape()];

    /// <summary>This game's outcomes follow the cartridge's own sequence of random numbers.</summary>
    public override IRandomSequence CreateRandomSequence() => new Compatibility.CartridgeRandom();
}
