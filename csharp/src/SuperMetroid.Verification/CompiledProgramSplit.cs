/// <summary>
/// How a compiled instruction program exposes its mechanics/presentation split to a lookup-stream
/// check (#633): the ordered mechanics words, the ordered presentation operands, the direct
/// mechanics reader, byte ownership, and the visual classification of each program word.
/// </summary>
internal sealed record CompiledProgramSplit(
    Func<int, (ushort Address, ushort Value)> Mechanics,
    Func<int, ushort> Presentation,
    Func<ushort, ushort> Read,
    Func<int, bool> Owns,
    Func<int, bool> IsVisual);
