namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// A gunship's save prompt, which its top hull asks for once Samus is restored inside it (#2562): the message box it opens
/// carries the prompt, and its answer resumes that ship (#2554).
/// </summary>
/// <param name="top">The top hull of the ship that asked.</param>
internal sealed partial class GunshipSavePrompt(GunshipTop top)
{
    /// <summary>Resumes the ship once the message box has its answer; the ship's exit is the same for either answer.</summary>
    internal void Answer() => GunshipBehavior.AnswerGunshipSavePrompt(top);
}
