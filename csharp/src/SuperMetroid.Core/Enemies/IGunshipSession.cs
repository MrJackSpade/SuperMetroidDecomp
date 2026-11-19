namespace SuperMetroid.Core.Enemies;

/// <summary>The gunship's save prompt as the runtime and message box see it.</summary>
internal interface IGunshipSession
{
    /// <summary>Resumes the ship once the message box has its answer; the ship's exit is the same for either answer.</summary>
    void AnswerSavePrompt();
}
