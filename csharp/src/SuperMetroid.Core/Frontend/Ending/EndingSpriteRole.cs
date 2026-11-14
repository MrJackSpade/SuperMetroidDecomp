namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's cinematic actors, by what each shows and how it moves.</summary>
internal enum EndingSpriteRole
{
    /// <summary>The side-cloud escape's right cloud that enters first, moving up and left.</summary>
    SideCloudRightLeading,
    /// <summary>The side-cloud escape's left cloud that enters first, moving down and right.</summary>
    SideCloudLeftLeading,
    /// <summary>The side-cloud escape's right cloud that follows below the leading one.</summary>
    SideCloudRightTrailing,
    /// <summary>The side-cloud escape's left cloud that follows above the leading one.</summary>
    SideCloudLeftTrailing,
    /// <summary>The top-and-bottom-cloud escape's upper cloud pattern, above its edge.</summary>
    TopCloudPattern,
    /// <summary>The edge of the top-and-bottom-cloud escape's upper clouds.</summary>
    TopCloudEdge,
    /// <summary>The edge of the top-and-bottom-cloud escape's lower clouds.</summary>
    BottomCloudEdge,
    /// <summary>The top-and-bottom-cloud escape's lower cloud pattern, below its edge.</summary>
    BottomCloudPattern,
    /// <summary>Primary planet actor whose animation controls subordinate explosion layers.</summary>
    ExplodingZebes,
    /// <summary>Lava layer deleted when the primary planet actor ends.</summary>
    ExplosionLava,
    /// <summary>Glow layer deleted with the primary planet actor.</summary>
    ExplosionGlow,
    /// <summary>Initial star layer deleted with the primary planet actor.</summary>
    ExplosionStars,
    /// <summary>Silhouette spawned by the explosion cue stream.</summary>
    ExplosionSilhouette,
    /// <summary>Right starfield in its fixed slot during flyaway.</summary>
    ExplosionStarsRight,
    /// <summary>Left starfield in its fixed slot during flyaway.</summary>
    ExplosionStarsLeft,
    /// <summary>Final afterglow in its fixed slot.</summary>
    ExplosionAfterglow,
    /// <summary>First gunship completion-text actor.</summary>
    OperationWasText,
    /// <summary>Second gunship completion-text actor.</summary>
    CompletedSuccessfullyText,
    /// <summary>Clear-time label preceding the digit actors.</summary>
    ClearTimeText,
    /// <summary>One clear-time digit or colon selected by the clear-time text step.</summary>
    ClearTimeDigit,
    /// <summary>One body or head component of the selected post-credits reward.</summary>
    RewardSamus,
    /// <summary>Animals' escape pod actor admitted by the saved escape flag.</summary>
    AnimalEscape,
}
