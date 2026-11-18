namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The SPACE COLONY caption typed so far (#2488): its first <see cref="Letters"/> letters, and which caption shows, the
/// English letters alone or with the katakana caption below them.
/// </summary>
/// <param name="Letters">The caption's visible letters shown, in order, one or more.</param>
/// <param name="Shows">Which caption shows: <see cref="SpaceColonyCaptionText.WithKatakana"/> once the katakana caption shows.</param>
internal readonly record struct SpaceColonyCaptionTyping(int Letters, SpaceColonyCaptionText Shows);
