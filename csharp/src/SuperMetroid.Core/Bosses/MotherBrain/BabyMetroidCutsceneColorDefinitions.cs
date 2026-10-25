namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The shape of the cutscene Baby Metroid's colours: its initial image and its fade to black.</summary>
public static class BabyMetroidCutsceneColorDefinitions
{
    /// <summary>The colours of the initial image, filling the Baby's colour set after its transparent first colour.</summary>
    public const int InitialColorCount = 15;

    /// <summary>The colours each fade image rewrites, the Baby's colours one through fourteen; the initial black colour fifteen is not rewritten by the fade.</summary>
    public const int FadeColorCount = 14;

    /// <summary>The displayed fade images, indexed one through six; the cutscene signals completion at index seven rather than showing a seventh image.</summary>
    public const int FadeFrameCount = 6;
}
