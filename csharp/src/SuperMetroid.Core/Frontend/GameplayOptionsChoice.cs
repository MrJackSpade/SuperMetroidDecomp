namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The options every selected game of a session starts with: unchosen until the options menu first closes, when a selected
/// game starts on the default options and keeps the options its save loaded, then the choices the menu closed with, which
/// replace the save's.
/// </summary>
internal abstract record GameplayOptionsChoice
{
    /// <summary>No options chosen yet: the options menu has not closed this session.</summary>
    internal static GameplayOptionsChoice Unchosen { get; } = new UnchosenGameplayOptions();

    /// <summary>The options a selected game's runtime is created with.</summary>
    internal abstract SelectedGameplayOptions StartingOptions { get; }

    /// <summary>No options chosen yet: a selected game starts on the default options.</summary>
    private sealed record UnchosenGameplayOptions : GameplayOptionsChoice
    {
        /// <inheritdoc/>
        internal override SelectedGameplayOptions StartingOptions => SelectedGameplayOptions.Default;
    }
}
