using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>The installed artwork a queued native transfer selects, as the verification queue records it over the OBJ characters.</summary>
/// <param name="SourceAddress">The native transfer's source address.</param>
/// <param name="ByteCount">The native transfer's byte count.</param>
internal sealed record QueuedTransferCharacters(int SourceAddress, int ByteCount) : SpriteArtworkSource, IVerificationArtworkSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The resolver serves no native transfers, or none of this one.</exception>
    public ReadOnlyMemory<byte> ResolveCharacters(IInstalledArtworkContent resolver) =>
        (Ensure.NotNull(resolver) as IInstalledArtworkTransferSource ?? throw new InvalidOperationException(
            $"{resolver.GetType().Name} serves no native transfers."))
        .TryResolve(SourceAddress, ByteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"No installed artwork serves the transfer from ${SourceAddress:X6} ({ByteCount} bytes).");

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"transfer ${SourceAddress:X6}+{ByteCount}";
}
