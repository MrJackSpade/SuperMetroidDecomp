namespace SuperMetroid.Core.Programs;

/// <summary>A catalog that can tell which bank bytes its compiled mechanics words own.</summary>
internal interface ICompiledMechanicsByteProbe
{
    /// <summary>Determines whether a bank address belongs to compiled program mechanics.</summary>
    static abstract bool IsCompiledMechanicsByte(int address);
}
