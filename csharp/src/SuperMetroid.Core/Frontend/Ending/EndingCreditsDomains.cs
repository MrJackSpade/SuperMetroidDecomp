namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Domain operations over the ending's closed phase and sprite-role sets.</summary>
internal static class EndingCreditsDomains
{

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
