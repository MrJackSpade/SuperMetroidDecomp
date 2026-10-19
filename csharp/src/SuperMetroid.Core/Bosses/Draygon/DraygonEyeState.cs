namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The state of Draygon's eye actor. Its function and look angle are values the cartridge keeps in the eye's common
/// record, so they are emptied with the record when the eye is killed (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class DraygonEyeState
{
    /// <summary>The eye's bank-$A5 function; none once a kill has emptied the record.</summary>
    public DraygonEyeFunction? Function { get; internal set; }

    /// <summary>The cartridge angle toward Samus the eye last looked along; only its low byte is compared.</summary>
    public int LookAngle
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the values the cartridge keeps in the eye's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        LookAngle = 0;
    }
}
