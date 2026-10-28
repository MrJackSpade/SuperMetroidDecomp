using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The part of a verification queue's transfer that reaches the sprite characters, from <paramref name="ByteOffset"/> bytes
/// into the transfer's artwork: a transfer that begins below the characters shows only its rest there.
/// </summary>
/// <param name="Characters">The whole transfer's artwork.</param>
/// <param name="ByteOffset">The bytes of it before the sprite characters begin.</param>
internal sealed record OffsetSpriteCharacters(SpriteArtworkSource Characters, int ByteOffset) : SpriteArtworkSource, IVerificationArtworkSource
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> ResolveCharacters(IInstalledArtworkContent resolver) => Characters.Resolve(resolver)[ByteOffset..];

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{Characters} from byte {ByteOffset}";
}
