using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One horizontal strip of a PLM block draw: where the next strip starts and the strip's cells.</summary>
/// <param name="Next">The next strip's offset, in blocks; zero after the last strip.</param>
/// <param name="Cells">The strip's level words, left to right.</param>
internal readonly record struct PlmBlockStrip(Offset16<RoomBlocks> Next, RoomLevelWord[] Cells);
