namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A PLM family's typed instruction program (#1296), run in place of the bank-$84 interpreter. The slot keeps the
/// cartridge's timer gate: the handler advances the program on the pass its native timer expires, so each draw lasts
/// exactly as long as the native one did.
/// </summary>
internal abstract class PlmAnimation
{
    /// <summary>
    /// Runs the program from where it rests until it draws, sleeps or deletes the PLM, as one pass of the native
    /// instruction interpreter does.
    /// </summary>
    internal abstract void Advance(PlmSlot slot, PlmAnimationContext context);
}
