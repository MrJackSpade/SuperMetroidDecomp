using Engine.Core;
using Engine.Input;

namespace Engine.Ui;

/// <summary>
/// A scripted sequence with no gameplay: an introduction, an ending, credits. It plays update by update and, when it
/// ends or is skipped, hands over to the mode that follows it. It runs no rules of play.
/// </summary>
public abstract partial class Cinematic : GameMode
{
    /// <summary>How many updates the cinematic has played.</summary>
    public int Elapsed { get; private set; }

    /// <summary>Whether confirming skips to the end.</summary>
    protected virtual bool CanBeSkipped => true;

    /// <inheritdoc/>
    public sealed override GameMode Step(GameInput input)
    {
        bool finished = (CanBeSkipped && Elapsed > 0 && input.WasPressed(GameAction.Confirm)) || Play(Elapsed);
        Elapsed++;
        return finished ? Next() : this;
    }

    /// <summary>
    /// Plays the update that comes <paramref name="elapsed"/> updates after the cinematic began, and returns whether
    /// that was its last.
    /// </summary>
    protected abstract bool Play(int elapsed);

    /// <summary>Makes the mode that follows the cinematic.</summary>
    protected abstract GameMode Next();
}
