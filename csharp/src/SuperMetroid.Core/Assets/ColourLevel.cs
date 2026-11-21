namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour shown at a level between black and itself (#1512, #2563): the opening's cross-fader and the ending's fades show
/// each colour <see cref="Level"/> of the way up from black, which the backend shows (<see cref="IColourBlend.AtLevel"/>).
/// </summary>
/// <param name="Level">How far up from black the colour shows.</param>
public sealed partial record ColourLevel(FadeLevel Level) : ColourEffect;
