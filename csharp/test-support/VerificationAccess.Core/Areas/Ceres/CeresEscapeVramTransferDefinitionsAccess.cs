using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresEscapeVramTransferDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeVramTransferDefinitionsAccess
{
    /// <summary>Recognizes bytes occupied by Ceres escape VRAM descriptors and their terminators.</summary>
    extension(CeresEscapeVramTransferDefinitions)
    {
        /// <summary>Recognizes bytes in compiled seven-byte VRAM records or their terminator words.</summary>
        internal static bool IsDescriptorByteAddress(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
                return false;
            ushort offset = unchecked((ushort)address);
            foreach (CeresEscapeVramTransferDefinition record in CeresEscapeVramTransferDefinitions.Records)
                if (offset >= record.Pointer && offset < record.Pointer + 7)
                    return true;
            return CeresEscapeVramTransferDefinitions.IsTerminator(offset) ||
                CeresEscapeVramTransferDefinitions.IsTerminator(unchecked((ushort)(offset - 1)));
        }
    }
}
