using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The background and sprite colours one step of the opening's cross-fade moves together (#1512).</summary>
/// <param name="Backgrounds">The background colours moved.</param>
/// <param name="Sprites">The sprite colours moved.</param>
public sealed record IntroFadeColours(ImmutableArray<IntroBackgroundFade> Backgrounds, ImmutableArray<IntroSpriteFade> Sprites);
