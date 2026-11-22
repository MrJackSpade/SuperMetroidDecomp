namespace SuperMetroid.Core;

/// <summary>
/// Writes the fields of one object of the game into a game snapshot (#2602). Each type's generated members use it: the
/// count of the fields they write, then each field's name and value, in the order a restore reads them back in.
/// </summary>
public interface IStateGraphWriter
{
}
