namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The species and facing a Fune or Namihe authors in its population parameter one (#1461), decoded once from the
/// word's low byte: the variant in the low nibble, any nonzero variant selecting Namihe, and the facing in the upper
/// nibble, where any set bit selects right-facing art. This is the one place that byte is read.
/// </summary>
/// <param name="Variant">The variant nibble, 0 through 15; zero is Fune and every other value Namihe.</param>
/// <param name="FacesRight">Whether any facing bit is set, selecting the right-facing animations.</param>
[EncodingOwner("Fune and Namihe population parameter one")]
internal readonly record struct FuneNamiheSetup(int Variant, bool FacesRight)
{
    /// <summary>The low nibble the variant occupies.</summary>
    private const int VariantBits = 0x000f;

    /// <summary>The low byte's upper nibble, any bit of which selects right-facing art.</summary>
    private const int FacingBits = 0x00f0;

    /// <summary>Whether the variant selects Namihe: the native initializer and fireball spawn test the nibble for any value.</summary>
    internal bool IsNamihe => Variant != 0;

    /// <summary>The setup an authored parameter one holds.</summary>
    /// <param name="parameter1">The population record's parameter one.</param>
    internal static FuneNamiheSetup Of(int parameter1) =>
        new(parameter1 & VariantBits, (parameter1 & FacingBits) != 0);
}
