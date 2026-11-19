namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed key of each <see cref="CinematicSpriteFamily"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CinematicSpriteFamilyKeys
{
    /// <summary>Each CinematicSpriteFamily member's installed key.</summary>
    internal static InstalledKeys<CinematicSpriteFamily> Installed { get; } = new(new Dictionary<CinematicSpriteFamily, string>
    {
        [CinematicSpriteFamily.IntroDiscoveryActor] = "IntroDiscoveryActor",
        [CinematicSpriteFamily.IntroEggEffect] = "IntroEggEffect",
        [CinematicSpriteFamily.IntroRinka] = "IntroRinka",
        [CinematicSpriteFamily.IntroScientist] = "IntroScientist",
        [CinematicSpriteFamily.IntroMotherBrain] = "IntroMotherBrain",
        [CinematicSpriteFamily.IntroMotherBrainExplosion] = "IntroMotherBrainExplosion",
        [CinematicSpriteFamily.IntroCaret] = "IntroCaret",
        [CinematicSpriteFamily.IntroNextPageArrow] = "IntroNextPageArrow",
        [CinematicSpriteFamily.CeresFlight] = "CeresFlight",
        [CinematicSpriteFamily.CeresDestruction] = "CeresDestruction",
        [CinematicSpriteFamily.EndingCloud] = "EndingCloud",
        [CinematicSpriteFamily.EndingExplosion] = "EndingExplosion",
        [CinematicSpriteFamily.EndingCompletionText] = "EndingCompletionText",
        [CinematicSpriteFamily.EndingReward] = "EndingReward",
        [CinematicSpriteFamily.EndingLogo] = "EndingLogo",
        [CinematicSpriteFamily.EndingAnimalEscape] = "EndingAnimalEscape",
    });
}
