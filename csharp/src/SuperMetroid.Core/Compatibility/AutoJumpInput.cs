namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The one-shot auto-jump a landing or a turn installs as Samus's pose input for her next update (#2562): that update's
/// input phase substitutes a fresh Jump press for a jump still held below the auto-jump limit, once, and reinstates her
/// ordinary input. A shinespark's start and the attract demo reinstate it without the substitution.
/// </summary>
/// <remarks>
/// Evidence: the game's landing and turn transitions install the auto-jump as Samus's input, whose next run adds the
/// Jump press and installs her ordinary input again; a recording's import and the showcase comparison read the installed
/// auto-jump as the game's own state.
/// Lifetime: Samus's.
/// </remarks>
public sealed partial class AutoJumpInput
{
    /// <summary>Whether the auto-jump is Samus's pose input.</summary>
    public bool IsInstalled { get; private set; }

    /// <summary>A landing or a turn installs the auto-jump.</summary>
    internal void Install() => IsInstalled = true;

    /// <summary>A shinespark's start or the attract demo reinstates her ordinary input.</summary>
    internal void Reinstate() => IsInstalled = false;

    /// <summary>Her input phase runs the auto-jump: whether it was installed, which her ordinary input then replaces.</summary>
    internal bool Take()
    {
        bool installed = IsInstalled;
        IsInstalled = false;
        return installed;
    }
}
