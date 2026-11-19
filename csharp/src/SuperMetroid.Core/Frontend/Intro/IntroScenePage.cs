namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening cinematic's four scene pages (#1404). The installed background page each is stored as is written out in
/// the import's artwork format, so the members' declaration order carries nothing (#2567).
/// </summary>
internal enum IntroScenePage
{
    /// <summary>The flashback room where Samus fights Mother Brain.</summary>
    MotherBrainRoom,
    /// <summary>The room where Samus discovers the Baby.</summary>
    BabyDiscoveryRoom,
    /// <summary>The laboratory where Samus delivers the Baby to the scientists.</summary>
    ScientistDelivery,
    /// <summary>The laboratory where the scientists examine the Baby.</summary>
    ScientistExamination,
}
