using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro's demo-input objects (#1300); each initializer does nothing.</summary>
public static class IntroDemoInputObjectDefinitions
{
    /// <summary>The intro Mother Brain flashback's object.</summary>
    public static DemoInputObjectDefinition IntroMotherBrainFlashback { get; } =
        new(DemoInputFrameBehavior.ReturnOnly, DemoInputProgram.IntroMotherBrainFlashback);

    /// <summary>The intro Baby Metroid discovery's object.</summary>
    public static DemoInputObjectDefinition IntroBabyDiscovery { get; } =
        new(DemoInputFrameBehavior.IntroBabyRunningLeft, DemoInputProgram.IntroBabyDiscoveryRunningLeft);
}
