namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>A family of cinematic compositions keyed by pose, each drawn from its screen's sprite sheet (#1392).</summary>
public enum CinematicSpriteFamily
{
    /// <summary>The intro's egg and confused Baby.</summary>
    IntroDiscoveryActor,
    /// <summary>The intro's egg fragments and slime drops.</summary>
    IntroEggEffect,
    /// <summary>The intro flashback's Rinkas.</summary>
    IntroRinka,
    /// <summary>The intro's scientist-scene Baby.</summary>
    IntroScientist,
    /// <summary>The intro flashback's Mother Brain.</summary>
    IntroMotherBrain,
    /// <summary>The intro flashback's Mother Brain explosions.</summary>
    IntroMotherBrainExplosion,
    /// <summary>The intro narration's caret.</summary>
    IntroCaret,
    /// <summary>The Ceres approach's stars and rear-view gunship and station.</summary>
    CeresFlight,
    /// <summary>The Ceres destruction scene's blasts, backdrop and Zebes reveal.</summary>
    CeresDestruction,

    /// <summary>The ending's atmospheric clouds.</summary>
    EndingCloud,

    /// <summary>The ending's Zebes explosion.</summary>
    EndingExplosion,

    /// <summary>The ending's completion message and clear time.</summary>
    EndingCompletionText,

    /// <summary>The post-credits Samus.</summary>
    EndingReward,

    /// <summary>The ending's assembling logo.</summary>
    EndingLogo,

    /// <summary>The rescued animals' escape pod.</summary>
    EndingAnimalEscape,
}
