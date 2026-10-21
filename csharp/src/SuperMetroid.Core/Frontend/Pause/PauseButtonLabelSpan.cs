namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>A run of tilemap words of a pause menu button label.</summary>
/// <param name="Word">The word.</param>
/// <param name="Count">The count.</param>
internal readonly record struct PauseButtonLabelSpan(int Word, int Count);
