using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One actor of the ending's assembling logo (#1979): the animation it runs and where it starts.</summary>
/// <param name="Animation">The animation the actor runs.</param>
/// <param name="Origin">Where the actor starts, before the logo's camera is subtracted.</param>
internal sealed record EndingLogoActorDefinition(EndingLogoAnimation Animation, Offset16<ScreenPixels> Origin);
