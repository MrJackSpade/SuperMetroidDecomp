using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="ShaktoolSegmentDefinitions"/> that only verification reads.</summary>
internal static class ShaktoolSegmentDefinitionsConstants
{
    /// <summary>$AA:DEDB ShaktoolPieceData_functionPointer, seven callbacks also used by reset.</summary>
    public const int NativeCallbackAddress = 0xaadedb;
    /// <summary>$AA:DEB1 ShaktoolPieceData_initialNeighborAngle, seven initial joint angles.</summary>
    public const int NativeInitialAngleAddress = 0xaadeb1;
    /// <summary>$AA:DEBF ShaktoolPieceData_initialInstListPointer, seven initial lists.</summary>
    public const int NativeInstructionAddress = 0xaadebf;
    /// <summary>$AA:DECD ShaktoolPieceData_layerControl, seven drawing layers.</summary>
    public const int NativeLayerAddress = 0xaadecd;
    /// <summary>$AA:DEA3 ShaktoolPieceData_RAMOffset, seven owner-relative enemy offsets.</summary>
    public const int NativeOwnerOffsetAddress = 0xaadea3;
    /// <summary>$AA:DE95 ShaktoolPieceData_properties, seven property words.</summary>
    public const int NativePropertiesAddress = 0xaade95;

    /// <summary>Provides native Shaktool segment callback, angle, instruction, layer, owner, and property references.</summary>
    extension(ShaktoolSegmentDefinitions)
    {
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeCallbackAddress"/>
        internal static int NativeCallbackAddress => ShaktoolSegmentDefinitionsConstants.NativeCallbackAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeInitialAngleAddress"/>
        internal static int NativeInitialAngleAddress => ShaktoolSegmentDefinitionsConstants.NativeInitialAngleAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeInstructionAddress"/>
        internal static int NativeInstructionAddress => ShaktoolSegmentDefinitionsConstants.NativeInstructionAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeLayerAddress"/>
        internal static int NativeLayerAddress => ShaktoolSegmentDefinitionsConstants.NativeLayerAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativeOwnerOffsetAddress"/>
        internal static int NativeOwnerOffsetAddress => ShaktoolSegmentDefinitionsConstants.NativeOwnerOffsetAddress;
        /// <inheritdoc cref="ShaktoolSegmentDefinitionsConstants.NativePropertiesAddress"/>
        internal static int NativePropertiesAddress => ShaktoolSegmentDefinitionsConstants.NativePropertiesAddress;
    }
}
