namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Which scene page each part of the opening cinematic shows (#1404).</summary>
internal static partial class IntroScenePages
{
    /// <summary>The laboratory of each scene.</summary>
    private static IdentityTable<IntroLaboratoryScene, IntroScenePage> LaboratoryByScene => field ??= new(new Dictionary<IntroLaboratoryScene, IntroScenePage>
    {
        [IntroLaboratoryScene.Delivery] = IntroScenePage.ScientistDelivery,
        [IntroLaboratoryScene.Examination] = IntroScenePage.ScientistExamination,
    });

    /// <summary>The laboratory page <paramref name="scene"/> shows.</summary>
    internal static IntroScenePage Laboratory(IntroLaboratoryScene scene) => LaboratoryByScene[scene];
}
