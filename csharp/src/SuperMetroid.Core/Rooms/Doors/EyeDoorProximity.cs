namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A <c>$84:8D41</c> proximity branch's operands: how many blocks Samus may stand from the PLM, across and down, for the
/// program to continue at <paramref name="Target"/>.
/// </summary>
/// <param name="Reach">The most blocks Samus may stand to either side (X) and above or below (Y).</param>
/// <param name="Target">The program Samus's nearness continues at.</param>
internal readonly record struct EyeDoorProximity(Extent16<RoomBlocks> Reach, EyeDoorPlmProgram Target);
