namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native cinematic actor slots and their instruction-list behavior.</summary>
internal enum EndingSpriteRole
{
    /// <summary>First right-moving cloud in escape scene A.</summary>
    CloudRightA,
    /// <summary>First left-moving cloud in escape scene A.</summary>
    CloudLeftA,
    /// <summary>Second right-moving cloud in escape scene A.</summary>
    CloudRightB,
    /// <summary>Second left-moving cloud in escape scene A.</summary>
    CloudLeftB,
    /// <summary>Upper cloud actor in the second escape backdrop.</summary>
    CloudTopA,
    /// <summary>Other upper cloud actor in the second escape backdrop.</summary>
    CloudTopB,
    /// <summary>Lower cloud actor in the second escape backdrop.</summary>
    CloudBottomA,
    /// <summary>Other lower cloud actor in the second escape backdrop.</summary>
    CloudBottomB,
    /// <summary>Primary planet actor whose instruction pointer controls subordinate explosion layers.</summary>
    ExplodingZebes,
    /// <summary>Lava layer deleted when the primary planet actor ends.</summary>
    ExplosionLava,
    /// <summary>Glow layer deleted with the primary planet actor.</summary>
    ExplosionGlow,
    /// <summary>Initial star layer deleted with the primary planet actor.</summary>
    ExplosionStars,
    /// <summary>Silhouette spawned by the explosion instruction stream.</summary>
    ExplosionSilhouette,
    /// <summary>Right starfield in its fixed native slot during flyaway.</summary>
    ExplosionStarsRight,
    /// <summary>Left starfield in its fixed native slot during flyaway.</summary>
    ExplosionStarsLeft,
    /// <summary>Final afterglow in its fixed native slot.</summary>
    ExplosionAfterglow,
    /// <summary>First gunship completion-text actor.</summary>
    OperationWasText,
    /// <summary>Second gunship completion-text actor.</summary>
    CompletedSuccessfullyText,
    /// <summary>Clear-time label preceding the digit actors.</summary>
    ClearTimeText,
    /// <summary>One clear-time digit or colon selected by the text opcode.</summary>
    ClearTimeDigit,
    /// <summary>One body or head component of the selected post-credits reward.</summary>
    RewardSamus,
    /// <summary>Animals' escape pod actor admitted by the saved escape flag.</summary>
    AnimalEscape,
}
