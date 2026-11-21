using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Hardware;

/// <summary>Verification checks on <see cref="CpuBusCell"/>.</summary>
internal static class CpuBusCellAccess
{
    /// <summary>Checks a bus cell's shape.</summary>
    extension(CpuBusCell self)
    {
        /// <summary>Requires this to be a <paramref name="region"/> cell of exactly <paramref name="byteCount"/> bytes.</summary>
        internal void Require(CpuBusRegion region, int byteCount)
        {
            if (self.Region != region || self.ByteCount != byteCount)
                throw new ArgumentException($"{self} is not a {byteCount}-byte {region} cell.");
        }
    }
}
