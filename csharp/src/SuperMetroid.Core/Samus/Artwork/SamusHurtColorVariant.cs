namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The two mutually exclusive full-body palettes in Samus's hurt handler.</summary>
public enum SamusHurtColorVariant
{
    /// <summary>Bright flash from <c>SamusPalettes_HurtFlash</c>, selected on hurt-counter calls one, three, and five.</summary>
    Hurt,
    /// <summary>Gray-tinted restoration from <c>SamusPalettes_Intro</c>, selected on even hurt-counter calls below seven while a cinematic is active; ordinary gameplay restores the equipped suit instead.</summary>
    Intro,
}
