namespace SuperMetroid.Core;

/// <summary>
/// Reads the fields of one object of the game back from a game snapshot (#2602), in the order they were written,
/// checking each field's name against this build's so a snapshot of another layout is refused rather than misread.
/// </summary>
public interface IStateGraphReader
{
}
