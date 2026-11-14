namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// What writing a run of bytes over one of the cartridge's progress bit tables' leading bytes decides (#1777): the bits
/// numbered from zero up to <paramref name="Decided"/>, each set when <paramref name="Set"/> lists it and clear
/// otherwise. Bits from <paramref name="Decided"/> on keep their state. Only the import decodes the bit tables.
/// </summary>
/// <param name="Decided">How many bits, from bit zero, the write decides.</param>
/// <param name="Set">The decided bits the write sets, ascending.</param>
public sealed record BitTablePrefix(int Decided, IReadOnlyList<int> Set);
