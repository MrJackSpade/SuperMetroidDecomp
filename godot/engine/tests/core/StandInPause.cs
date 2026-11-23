using Engine.Core;
using Engine.Input;

namespace Engine.Tests.Core;

/// <summary>A game's own pause screen, for checks: the engine's, with Cancel quitting to a stand-in mode when the definition says so.</summary>
public partial class StandInPause : PausedMode
{
    private readonly TwoRoomDefinition _definition;

    /// <summary>A pause laid over <paramref name="playing"/>.</summary>
    public StandInPause(PlayingMode playing, TwoRoomDefinition definition) : base(playing) => _definition = definition;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input) =>
        _definition.QuitFromPause && input.WasPressed(GameAction.Cancel) ? new StandInMode { Name = "Title" } : base.Step(input);
}
