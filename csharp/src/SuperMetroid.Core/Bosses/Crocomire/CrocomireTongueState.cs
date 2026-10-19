namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The state of Crocomire's tongue actor. Its offset is a value the cartridge keeps in the tongue's common record, so it
/// is emptied with the record when the tongue is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class CrocomireTongueState
{
    /// <summary>The tongue's horizontal offset from the body, in pixels, added to the body's X every update.</summary>
    public int BodyXOffset
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the values the cartridge keeps in the tongue's common record.</summary>
    internal void ClearWithRecord() => BodyXOffset = 0;
}
