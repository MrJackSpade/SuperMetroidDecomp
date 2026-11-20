namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship asks for its save prompt (#2562): ordinary restoration has finished with Samus inside, and the runtime opens
/// the prompt's message box, which carries <paramref name="Prompt"/> to the answer.
/// </summary>
/// <param name="Prompt">The prompt, which resumes the ship that asked.</param>
internal sealed record GunshipSavePromptRequested(GunshipSavePrompt Prompt) : EnemyPassEvent;
