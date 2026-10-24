using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Cartridge sizes of Samus's body frame and definition tables that only verification reads.</summary>
internal static class SamusBodyFrameTableConstants
{
    /// <summary>Bytes in one pose/frame selector: top set/position and bottom set/position.</summary>
    public const int AnimationRecordByteCount = 4;
    /// <summary>Number of bottom-half graphics sets before the animation pointer table.</summary>
    public static readonly int BottomDefinitionSetCount = (SamusSourceDefinitions.AnimationDefinitionListPointers.Value - SamusSourceDefinitions.BottomDefinitionListPointers.Value) / sizeof(ushort);
    /// <summary>Number of top-half graphics sets before the bottom pointer table.</summary>
    public static readonly int TopDefinitionSetCount = (SamusSourceDefinitions.BottomDefinitionListPointers.Value - SamusSourceDefinitions.TopDefinitionListPointers.Value) / sizeof(ushort);
}
