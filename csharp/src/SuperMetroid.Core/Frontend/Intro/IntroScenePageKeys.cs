namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The installed key of each <see cref="IntroScenePage"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class IntroScenePageKeys
{
    /// <summary>Each IntroScenePage member's installed key.</summary>
    internal static InstalledKeys<IntroScenePage> Installed { get; } = new(new Dictionary<IntroScenePage, string>
    {
        [IntroScenePage.MotherBrainRoom] = "MotherBrainRoom",
        [IntroScenePage.BabyDiscoveryRoom] = "BabyDiscoveryRoom",
        [IntroScenePage.ScientistDelivery] = "ScientistDelivery",
        [IntroScenePage.ScientistExamination] = "ScientistExamination",
    });
}
