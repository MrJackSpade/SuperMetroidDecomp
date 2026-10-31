namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The species and facing a Fune or Namihe placement names (#1461, #1472): the variant, any nonzero variant selecting
/// Namihe, and whether it faces right.
/// </summary>
/// <param name="Variant">The variant, 0 through 15; zero is Fune and every other value Namihe.</param>
/// <param name="FacesRight">Whether it faces right, selecting the right-facing animations.</param>
internal readonly record struct FuneNamiheSetup(int Variant, bool FacesRight)
{
    /// <summary>Whether the variant selects Namihe: the initializer and fireball spawn test the variant for any value.</summary>
    internal bool IsNamihe => Variant != 0;

    /// <summary>The setup a placement's spawn settings name.</summary>
    internal static FuneNamiheSetup Of(FuneNamiheSpawnSettings settings) =>
        new(Ensure.NotNull(settings).Variant, settings.FacesRight);
}
