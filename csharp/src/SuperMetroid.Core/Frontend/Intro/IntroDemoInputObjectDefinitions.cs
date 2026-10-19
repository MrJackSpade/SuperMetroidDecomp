using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro's bank-$91 demo-input objects (#1300); each initializer is the return-only <c>$91:83BF</c>.</summary>
public static class IntroDemoInputObjectDefinitions
{
    /// <summary><c>$91:8784</c>: the intro Mother Brain flashback's object.</summary>
    public static DemoInputObjectDefinition IntroMotherBrainFlashback { get; } =
        new(DemoInputPreInstruction.ReturnOnly, DemoInputProgram.IntroMotherBrainFlashback);

    /// <summary><c>$91:877E</c>: the intro Baby Metroid discovery's object.</summary>
    public static DemoInputObjectDefinition IntroBabyDiscovery { get; } =
        new(DemoInputPreInstruction.IntroBabyRunningLeft, DemoInputProgram.IntroBabyDiscoveryRunningLeft);
}
