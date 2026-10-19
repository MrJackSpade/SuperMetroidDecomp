using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Saves;

/// <summary>A cartridge load point expressed with typed area identity.</summary>
/// <param name="Area">Retail world area that owns the load-station table.</param>
/// <param name="SaveStation">
/// Zero-based load-station index within <paramref name="Area"/>; a JSON number the codec accepts in
/// <c>[0, <see cref="SaveFieldValue.MaximumValue"/>]</c>, the range the slot keeps.
/// </param>
public sealed record SaveCheckpointJsonDocument(
    AreaId Area,
    int SaveStation);
