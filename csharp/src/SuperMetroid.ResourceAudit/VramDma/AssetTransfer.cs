using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>An artwork asset and the length transferred.</summary>
/// <param name="Asset">The asset.</param>
/// <param name="Count">The count.</param>
internal readonly record struct AssetTransfer(VramAssetId Asset, int Count);
