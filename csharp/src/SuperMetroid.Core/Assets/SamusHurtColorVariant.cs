namespace SuperMetroid.Core.Assets;

/// <summary>The two mutually exclusive full-body palettes in Samus's hurt handler.</summary>
public enum SamusHurtColorVariant
{
    /// <summary>Bright flash from native <c>SamusPalettes_HurtFlash</c> at <c>$9B:A380</c>, selected on hurt-counter calls one, three, and five.</summary>
    Hurt,
    /// <summary>Gray-tinted restoration from native <c>SamusPalettes_Intro</c> at <c>$9B:A3A0</c>, selected on even hurt-counter calls below seven while a cinematic is active; ordinary gameplay restores the equipped suit instead.</summary>
    Intro,
}
