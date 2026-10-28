
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Pose-specific facing, movement and idle transitions, independent of editable artwork.</summary>
/// <remarks>
/// Every pose has a typed facing and movement here. The three adjacent-code poses FD..FF read the unrelated
/// $91:BE11 X-ray HDMA machine-code routine as their records; their raw facing and movement bytes, and those of
/// the two unused glitch-ball poses whose facing bytes are neither left nor right, are owned by
/// <see cref="SamusPoseRecordCodec"/>, the only place a pose record's bytes exist.
/// </remarks>
internal static class SamusPoseDispatchDefinitions
{
}
