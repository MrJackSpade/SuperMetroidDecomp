namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The rows of the ending screen's colours that its scenes load, fade and flash (#1404).</summary>
internal static class EndingColourRowDefinitions
{
    /// <summary>The background row the logo's tiles show, also turned white by the explosion's flash.</summary>
    public const int LogoBackgroundRow = 1;
    /// <summary>The sprite row the logo's sprites show.</summary>
    public const int LogoSpriteRow = 7;
    /// <summary>The background row the final gunship's colours load into.</summary>
    public const int FinalGunshipBackgroundRow = 5;
    /// <summary>The sprite row whose first colour the explosion's flash and the flyaway clear with the backdrop.</summary>
    public const int ClearedSpriteRow = 0;
    /// <summary>The sprite row holding the explosion's glow colour.</summary>
    public const int GlowSpriteRow = 7;
    /// <summary>The glow colour within its row.</summary>
    public const int GlowColour = 14;
    /// <summary>The background row the explosion crossfade fades out.</summary>
    public const int ExplosionBackgroundRow = 7;
    /// <summary>The first sprite row the explosion crossfade fades in.</summary>
    public const int ExplosionFirstSpriteRow = 5;
    /// <summary>The second sprite row the explosion crossfade fades in.</summary>
    public const int ExplosionSecondSpriteRow = 7;
    /// <summary>The background row the post-credits waiting backdrop fades in.</summary>
    public const int WaitingBackgroundRow = 2;
    /// <summary>The sprite row the suitless reward fades in.</summary>
    public const int SuitlessRewardSpriteRow = 5;
    /// <summary>The sprite row the suited reward fades in.</summary>
    public const int SuitedRewardSpriteRow = 6;
    /// <summary>The sprite row the post-credits shot fades out.</summary>
    public const int ShootingSpriteRow = 4;
    /// <summary>The sprite row Samus's post-credits colours fade out.</summary>
    public const int SamusSpriteRow = 7;
    /// <summary>The leading colours the post-credits restore keeps.</summary>
    public const int PostCreditsKeptColours = 4;
}
