namespace SuperMetroid.Core.Enemies;

/// <summary>The gunship's save prompt and health handler as the runtime and message box see them.</summary>
internal interface IGunshipSession
{
    /// <summary>Restoration requested message box $1C and awaits the answer.</summary>
    bool SavePromptPending { get; }

    /// <summary>Supplies the message box's Yes/No answer at its completion point.</summary>
    void AnswerSavePrompt(bool save);

    /// <summary>The gunship's health handler owns Samus's energy refill this frame.</summary>
    bool HealthHandlerActive { get; }
}
