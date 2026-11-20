namespace SuperMetroid.Core.Frontend;

/// <summary>The choices the options menu closed with, which every selected game starts with and applies over its save's.</summary>
/// <param name="Options">The choices.</param>
internal sealed record ChosenGameplayOptions(SelectedGameplayOptions Options) : GameplayOptionsChoice
{
    /// <inheritdoc/>
    internal override SelectedGameplayOptions StartingOptions => Options;
}
