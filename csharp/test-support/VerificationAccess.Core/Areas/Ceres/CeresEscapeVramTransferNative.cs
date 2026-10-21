using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification's native view of the Ceres escape transfer records: the bank-$A6 address of each.</summary>
internal static class CeresEscapeVramTransferNative
{
    /// <summary>A record's bank-$A6 address.</summary>
    extension(TileTransferDefinition record)
    {
        /// <summary>The bank-$A6 address of the record's seven bytes.</summary>
        internal ushort Pointer
        {
            get
            {
                int index = -1;
                for (int candidate = 0; candidate < CeresEscapeVramTransferDefinitions.Records.Count; candidate++)
                    if (CeresEscapeVramTransferDefinitions.Records[candidate] == record)
                        index = candidate;
                return index switch
                {
                    < 0 => throw new InvalidDataException("The record is not one of the Ceres escape transfers."),
                    < 4 => (ushort)(0xc3b8 + 7 * index),
                    < 6 => (ushort)(0xc4cb + 7 * (index - 4)),
                    < 11 => (ushort)(0xc4d9 + 7 * (index - 6)),
                    _ => (ushort)(0xc4fe + 7 * (index - 11)),
                };
            }
        }
    }
}
