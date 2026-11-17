namespace SuperMetroid.Core.Assets;

/// <summary>Which way a <see cref="ColourFade"/> runs between the colour shown and its endpoint colour (#1512, #2289).</summary>
public enum GradualFadeDirection
{
    /// <summary>The colour shown fades toward the endpoint colour, as a room's colours fade out to black.</summary>
    Toward,

    /// <summary>The endpoint colour fades toward the colour shown, as a room's colours fade in from black.</summary>
    From,
}
