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
    /// <summary>Silhouette spawned by the explosion's cues.</summary>
    ExplosionSilhouette,
    /// <summary>The right starfield during the flyaway, the last sole actor to run and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    ExplosionStarsRight,
    /// <summary>The left starfield during the flyaway, a sole actor that runs and draws just before the right one.</summary>
    ExplosionStarsLeft,
    /// <summary>The exploded planet's final afterglow, the first sole actor to run and draw.</summary>
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
    /// <summary>The animals' escape pod actor, shown when the animals were saved.</summary>
    AnimalEscape,
}
