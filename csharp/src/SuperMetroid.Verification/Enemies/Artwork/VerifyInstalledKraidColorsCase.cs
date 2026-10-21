
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The consumer and cgram index of one verify installed kraid colors case.</summary>
/// <param name="Consumer">The consumer.</param>
/// <param name="CgramIndex">The cgram index.</param>
internal readonly record struct VerifyInstalledKraidColorsCase(EnemyTileArtworkKraidColorsVerification.KraidPaletteConsumer Consumer, int CgramIndex);
