namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A post-credits text typed as far as its first <see cref="Characters"/> characters, spaces included (#2488).</summary>
/// <param name="Characters">The characters typed so far, one or more.</param>
internal readonly record struct EndingTypewriterTyping(int Characters);
