using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Domain operations over the ending's closed phase and sprite-role sets.</summary>
internal static class EndingCreditsDomains
{
    /// <summary>The Mode 7 backdrop a phase displays, or null when it displays none.</summary>
    internal static EndingMode7SceneId? Mode7Scene(this EndingCreditsPhase phase) => phase switch
    {
        EndingCreditsPhase.WaitForEscapeMusic or EndingCreditsPhase.FadeInEscapeSceneA or
            EndingCreditsPhase.EscapeSceneA or EndingCreditsPhase.FadeOutEscapeSceneA => EndingMode7SceneId.EscapeA,
        EndingCreditsPhase.FadeInEscapeSceneB or EndingCreditsPhase.EscapeSceneB or
            EndingCreditsPhase.FadeOutEscapeSceneB => EndingMode7SceneId.EscapeB,
        EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade or
            EndingCreditsPhase.ZebesExplosionTileUpload => EndingMode7SceneId.PlanetExplosion,
        EndingCreditsPhase.SetupEscapeFromZebes or EndingCreditsPhase.ZebesExplosionAnimation or
            EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue or
            EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or
            EndingCreditsPhase.PlanetEscapeAccelerating or EndingCreditsPhase.OperationSuccessfulText or
            EndingCreditsPhase.FadeOutToCredits or EndingCreditsPhase.Credits or EndingCreditsPhase.PostCreditsBlank or
            EndingCreditsPhase.PostCreditsFadeIn or EndingCreditsPhase.PostCreditsShootingStars or
            EndingCreditsPhase.PostCreditsWaitingBackdrop or EndingCreditsPhase.PostCreditsWaitingSamus or
            EndingCreditsPhase.PostCreditsReward or EndingCreditsPhase.PostCreditsCopyright or
            EndingCreditsPhase.PostCreditsGesture or EndingCreditsPhase.PostCreditsJump or
            EndingCreditsPhase.PostCreditsShot or EndingCreditsPhase.PostCreditsWhiteFlash or
            EndingCreditsPhase.PostCreditsLogo or EndingCreditsPhase.ItemPercentage or
            EndingCreditsPhase.ItemPercentageScrollDown or EndingCreditsPhase.SeeYouNextMission => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined ending phase."),
    };

    /// <summary>The artwork and instruction-list family of a sprite role.</summary>
    internal static EndingSpriteFamily Family(this EndingSpriteRole role) => role switch
    {
        EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudRightB or
            EndingSpriteRole.CloudLeftB or EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB or
            EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB => EndingSpriteFamily.Cloud,
        EndingSpriteRole.ExplodingZebes or EndingSpriteRole.ExplosionLava or EndingSpriteRole.ExplosionGlow or
            EndingSpriteRole.ExplosionStars or EndingSpriteRole.ExplosionSilhouette or
            EndingSpriteRole.ExplosionStarsRight or EndingSpriteRole.ExplosionStarsLeft or
            EndingSpriteRole.ExplosionAfterglow => EndingSpriteFamily.Explosion,
        EndingSpriteRole.OperationWasText or EndingSpriteRole.CompletedSuccessfullyText or
            EndingSpriteRole.ClearTimeText or EndingSpriteRole.ClearTimeDigit => EndingSpriteFamily.CompletionText,
        EndingSpriteRole.RewardSamus => EndingSpriteFamily.Reward,
        EndingSpriteRole.AnimalEscape => EndingSpriteFamily.AnimalEscape,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role."),
    };
}
