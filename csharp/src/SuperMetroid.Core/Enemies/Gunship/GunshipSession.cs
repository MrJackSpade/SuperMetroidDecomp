namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship session over the gunship and its health handler.</summary>
/// <param name="gunship">The gunship.</param>
/// <param name="health">The gunship's health handler.</param>
internal sealed class GunshipSession(GunshipBehavior gunship, GunshipHealthBehavior health) : IGunshipSession
{
    /// <inheritdoc/>
    public bool SavePromptPending => gunship.GunshipSavePromptPending;

    /// <inheritdoc/>
    public void AnswerSavePrompt(bool save) => gunship.AnswerGunshipSavePrompt(save);

    /// <inheritdoc/>
    public bool HealthHandlerActive => health.HasGunshipHealthHandler;
}
