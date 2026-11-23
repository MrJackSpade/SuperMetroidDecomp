using Engine.Core;

namespace Engine.Tests.Core;

/// <summary>The engine's stand-in game for its own checks: it starts in a <see cref="StandInMode"/>.</summary>
public partial class StandInDefinition : GameDefinition
{
    /// <summary>How many updates the first mode runs before handing over.</summary>
    public int UpdatesBeforeHandOver { get; set; } = 3;

    /// <inheritdoc/>
    public override GameMode CreateFirstMode() => new StandInMode { Name = "First", UpdatesBeforeHandOver = UpdatesBeforeHandOver };

    /// <inheritdoc/>
    public override Engine.Actors.Player CreatePlayer() => new() { Name = "Player" };
}
