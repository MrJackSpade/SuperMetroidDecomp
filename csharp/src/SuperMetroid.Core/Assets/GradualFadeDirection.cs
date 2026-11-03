namespace SuperMetroid.Core.Assets;

/// <summary>Which way a <see cref="GradualColourFade"/> runs between the colour shown and its fixed colour (#1512).</summary>
public enum GradualFadeDirection
{
    /// <summary>The colour shown fades toward the fixed colour, as a room's colours fade out to black.</summary>
    Toward,

    /// <summary>The fixed colour fades toward the colour shown, as a room's colours fade in from black.</summary>
    From,
}
