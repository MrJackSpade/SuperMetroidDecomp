namespace SuperMetroid.Verification.Fx;

/// <summary>One pre-conversion room-FX atlas segment, as captured from the supported ROM.</summary>
/// <param name="Source">The cartridge address of the segment's characters.</param>
/// <param name="ByteCount">The segment's bytes.</param>
internal readonly record struct OriginalRoomFxAtlasSegment(int Source, int ByteCount);
