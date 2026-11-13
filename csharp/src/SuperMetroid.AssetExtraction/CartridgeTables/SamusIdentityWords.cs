using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming Samus's X-ray reveal commands (#1411).</summary>
[EncodingOwner("X-ray reveal command words")]
internal static class SamusIdentityWords
{
    /// <summary>The bank-$91 revealed-block command an X-ray reveal table entry names.</summary>
    internal static readonly NativeWordIdentities<XrayRevealShape> XrayRevealShapes = new(new Dictionary<XrayRevealShape, ushort>
    {
        [XrayRevealShape.VerticalExtension] = 0xce79,
        [XrayRevealShape.HorizontalExtension] = 0xcebb,
        [XrayRevealShape.CopyOne] = 0xcf36,
        [XrayRevealShape.CopyBrinstar] = 0xcf3e,
        [XrayRevealShape.CopyWide] = 0xcf4e,
        [XrayRevealShape.CopyTall] = 0xcf62,
        [XrayRevealShape.CopySquare] = 0xcf6f,
    });
}
