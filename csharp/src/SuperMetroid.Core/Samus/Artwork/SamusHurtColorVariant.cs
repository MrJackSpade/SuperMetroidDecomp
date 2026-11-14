namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The two mutually exclusive full-body palettes of Samus's hurt flash.</summary>
public enum SamusHurtColorVariant
{
    /// <summary>The bright hurt flash, selected on hurt-counter calls one, three, and five.</summary>
    Hurt,
    /// <summary>The gray-tinted restoration of the intro's colours, selected on even hurt-counter calls below seven while a cinematic is active; ordinary gameplay restores the equipped suit instead.</summary>
    Intro,
}
