namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's ordinary restoration has finished and it asks for the save-confirmation prompt (#2562), whose answer
/// reaches <paramref name="Prompt"/>, the ship that asked (#2554).
/// </summary>
/// <param name="Prompt">The prompt the ship asked for.</param>
internal sealed record GunshipSavePromptRequested(GunshipSavePrompt Prompt) : EnemyPassEvent;
