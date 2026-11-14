namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Domain operations over the ending's closed phase and sprite-role sets.</summary>
internal static class EndingCreditsDomains
{

    /// <summary>The artwork and animation family of a sprite role.</summary>
    internal static EndingSpriteFamily Family(this EndingSpriteRole role) => role switch
    {
        EndingSpriteRole.SideCloudRightLeading or EndingSpriteRole.SideCloudLeftLeading or EndingSpriteRole.SideCloudRightTrailing or
            EndingSpriteRole.SideCloudLeftTrailing or EndingSpriteRole.TopCloudPattern or EndingSpriteRole.TopCloudEdge or
            EndingSpriteRole.BottomCloudEdge or EndingSpriteRole.BottomCloudPattern => EndingSpriteFamily.Cloud,
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
