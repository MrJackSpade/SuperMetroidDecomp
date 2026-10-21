namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The physical, streamed, trigger physical and trigger streamed of one downward gate visuals render result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Streamed">The streamed.</param>
/// <param name="TriggerPhysical">The trigger physical.</param>
/// <param name="TriggerStreamed">The trigger streamed.</param>
internal readonly record struct DownwardGateVisualsRenderResult(ushort Physical, ushort Streamed, ushort TriggerPhysical, ushort TriggerStreamed);
