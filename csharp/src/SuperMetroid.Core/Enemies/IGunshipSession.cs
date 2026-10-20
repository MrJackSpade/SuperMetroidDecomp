namespace SuperMetroid.Core.Enemies;

/// <summary>The gunship's save prompt as the runtime and message box see it.</summary>
internal interface IGunshipSession
{
    /// <summary>Restoration requested message box $1C and awaits the answer.</summary>
    bool SavePromptPending { get; }

    /// <summary>Supplies the message box's Yes/No answer at its completion point.</summary>
    void AnswerSavePrompt(bool save);
}
