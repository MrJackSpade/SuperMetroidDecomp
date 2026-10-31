namespace SuperMetroid.Diagnostics.Saves;

/// <summary>One Zebes area's part of a legacy save image's packed explored-map field.</summary>
/// <param name="SaveOffset">The area's first byte in the packed field: the sum of the earlier areas' byte counts.</param>
/// <param name="PlaneBytes">The explored-map plane bytes the area packs, in packing order.</param>
public sealed record SaveRamPackedArea(int SaveOffset, IReadOnlyList<int> PlaneBytes);
