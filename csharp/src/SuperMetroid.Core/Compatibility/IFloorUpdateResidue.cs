namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What Samus's delay over-reads see where Ceres Ridley's floor updates leave their descriptors (#1338, #1406): the
/// session's residue, or a recording verification supplies.
/// </summary>
[CompatibilityService("bounded over-read: the delays Samus's running stage five and the invalid poses read past their tables")]
public interface IFloorUpdateResidue
{
    /// <summary>The residue byte <paramref name="index"/> bytes from its start, 0 through <see cref="FloorUpdateResidue.ByteCount"/> less one.</summary>
    /// <exception cref="InvalidDataException">The index lies outside the residue.</exception>
    byte ByteAt(int index);
}
