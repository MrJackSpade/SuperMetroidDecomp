using Engine.Core;
using Engine.Input;

namespace Engine.Tests.Core;

/// <summary>A mode for checks that asks the yes-or-no message "ask" on its first update and records the answer.</summary>
public partial class AskingMode : GameMode
{
    private bool _asked;

    /// <summary>The answer given, once one has been.</summary>
    public bool? Answer { get; private set; }

    /// <summary>How many times control has come back to this mode.</summary>
    public int TimesResumed { get; private set; }

    /// <inheritdoc/>
    protected internal override void Resume() => TimesResumed++;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (_asked)
            return this;
        _asked = true;
        return new MessageBoxMode(this, "ask", answer => Answer = answer);
    }
}
