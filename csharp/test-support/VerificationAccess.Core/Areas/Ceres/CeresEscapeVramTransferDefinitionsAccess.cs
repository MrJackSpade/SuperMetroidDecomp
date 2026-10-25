using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresEscapeVramTransferDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeVramTransferDefinitionsAccess
{
    /// <summary>Recognizes bytes occupied by Ceres escape VRAM descriptors and their terminators.</summary>
    extension(CeresEscapeVramTransferDefinitions)
    {
        /// <summary>Escape timer sprite transfers beginning at $A6:C4CB.</summary>
        internal static ushort TimerSprites => 0xc4cb;
        /// <summary>Ceres escape timer BG1/BG2 and door transfers beginning at $A6:C4FE.</summary>
        internal static ushort TimerBackgrounds => 0xc4fe;
        /// <summary>Japanese-language warning overlay transfers beginning at $A6:C3B8.</summary>
        internal static ushort JapaneseOverlay => 0xc3b8;
        /// <summary>First warning-text character transfer at $A6:C4D9.</summary>
        internal static ushort WarningTextFirstTransfer => 0xc4d9;

        /// <summary>Recognizes the three native list endings without treating them as transfer records.</summary>
        internal static bool IsTerminator(ushort pointer) => pointer is 0xc3d4 or 0xc4fc or 0xc536;

        /// <summary>Resolves a bank-$A6 record pointer to its fixed DMA metadata.</summary>
        internal static bool TryGet(ushort pointer, out TileTransferDefinition transfer)
        {
            foreach (TileTransferDefinition candidate in CeresEscapeVramTransferDefinitions.Records)
            {
                if (candidate.Pointer == pointer)
                {
                    transfer = candidate;
                    return true;
                }
            }
            transfer = default;
            return false;
        }

        /// <summary>Recognizes bytes in compiled seven-byte VRAM records or their terminator words.</summary>
        internal static bool IsDescriptorByteAddress(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
                return false;
            ushort offset = unchecked((ushort)address);
            foreach (TileTransferDefinition record in CeresEscapeVramTransferDefinitions.Records)
                if (offset >= record.Pointer && offset < record.Pointer + 7)
                    return true;
            return CeresEscapeVramTransferDefinitions.IsTerminator(offset) ||
                CeresEscapeVramTransferDefinitions.IsTerminator(unchecked((ushort)(offset - 1)));
        }
    }
}
