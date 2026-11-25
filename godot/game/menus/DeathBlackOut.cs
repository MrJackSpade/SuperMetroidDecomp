using Engine.Core;
using Engine.Input;

namespace Game.Menus;

/// <summary>
/// The last black-out of a death the screen is already black for, as Ceres blowing up with Samus aboard and time
/// running out on Zebes end: one update on which the update's random number is drawn and nothing else is done, and
/// what follows a death begins on the next.
/// </summary>
public partial class DeathBlackOut : GameMode
{
    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Session.OpenUpdate();
        return new GameOverMode();
    }
}
