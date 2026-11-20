namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The save prompt a gunship asked for (#2554): its answer resumes that ship, whose entrance pad opens for either answer.
/// </summary>
/// <param name="pieces">The pieces of the ship that asked.</param>
/// <param name="ship">The state of the ship that asked.</param>
internal sealed class GunshipSavePrompt(GunshipPieces pieces, GunshipState ship)
{
    /// <summary>Resumes the ship once the message box has its answer; the ship's exit is the same for either answer.</summary>
    internal void Answer() => GunshipBehavior.AnswerGunshipSavePrompt(pieces, ship);
}
