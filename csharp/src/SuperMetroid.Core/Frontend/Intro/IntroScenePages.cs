namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Which scene page each part of the opening cinematic shows (#1404).</summary>
internal static class IntroScenePages
{
    /// <summary>The laboratory page <paramref name="scene"/> shows.</summary>
    internal static IntroScenePage Laboratory(IntroLaboratoryScene scene) => scene switch
    {
        IntroLaboratoryScene.Delivery => IntroScenePage.ScientistDelivery,
        IntroLaboratoryScene.Examination => IntroScenePage.ScientistExamination,
        _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, "Undefined intro laboratory scene."),
    };
}
