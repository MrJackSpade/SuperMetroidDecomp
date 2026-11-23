using Engine.Core;
using Engine.Shared;
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

    /// <summary>This game's outcomes follow the cartridge's own sequence of random numbers.</summary>
    public override IRandomSequence CreateRandomSequence() => new Compatibility.CartridgeRandom();
}
