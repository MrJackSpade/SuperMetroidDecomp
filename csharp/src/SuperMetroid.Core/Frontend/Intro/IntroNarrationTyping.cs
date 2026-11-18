namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>A narration page typed as far as its first <see cref="Letters"/> characters, spaces included (#2488).</summary>
/// <param name="Letters">The characters typed so far, one or more.</param>
internal readonly record struct IntroNarrationTyping(int Letters);
