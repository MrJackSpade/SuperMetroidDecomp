namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship session over the gunship's save prompt.</summary>
/// <param name="gunship">The gunship.</param>
internal sealed class GunshipSession(GunshipBehavior gunship) : IGunshipSession
{
    /// <inheritdoc/>
    public bool SavePromptPending => gunship.GunshipSavePromptPending;

    /// <inheritdoc/>
    public void AnswerSavePrompt(bool save) => gunship.AnswerGunshipSavePrompt(save);
}
