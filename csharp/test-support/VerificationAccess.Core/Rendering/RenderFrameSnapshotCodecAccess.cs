namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="RenderFrameSnapshotCodec"/> members production does not use.</summary>
internal static class RenderFrameSnapshotCodecAccess
{
    /// <summary>Exposes a bounded span-based display-packet reader for verification fixtures.</summary>
    extension(RenderFrameSnapshotCodec)
    {
        /// <summary>Rejects oversized fixture packets before copying their bytes into the production snapshot decoder.</summary>
        internal static RenderFrameSnapshot Deserialize(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length > RenderPacketFormat.MaximumPacketBytes)
                throw new InvalidDataException("Display fixture exceeds the bounded packet size.");
            return RenderFrameSnapshotCodec.Deserialize(bytes.ToArray());
        }
    }
}
