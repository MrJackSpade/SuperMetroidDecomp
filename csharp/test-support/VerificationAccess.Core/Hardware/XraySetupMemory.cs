using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Hardware;

/// <summary>The frozen X-ray reveal map, as verification reads it.</summary>
internal static class XraySetupMemory
{
    /// <summary>Reads the frozen reveal map's shown blocks through the production X-ray view (#1431).</summary>
    public static RevealedBlock?[] ReadReveal(SuperMetroid.Core.World.XrayReveal xray) => xray.RevealedBlocks.ToArray();
}
