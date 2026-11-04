using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming Samus's X-ray reveal commands (#1411).</summary>
[EncodingOwner("X-ray reveal command words")]
internal static class SamusIdentityWords
{
    /// <summary>The bank-$91 revealed-block command an X-ray reveal table entry names.</summary>
    internal static readonly NativeWordIdentities<XrayRevealCommand> XrayRevealCommands = new(new Dictionary<XrayRevealCommand, ushort>
    {
        [XrayRevealCommand.VerticalExtension] = 0xce79,
        [XrayRevealCommand.HorizontalExtension] = 0xcebb,
        [XrayRevealCommand.CopyOne] = 0xcf36,
        [XrayRevealCommand.CopyBrinstar] = 0xcf3e,
        [XrayRevealCommand.CopyWide] = 0xcf4e,
        [XrayRevealCommand.CopyTall] = 0xcf62,
        [XrayRevealCommand.CopySquare] = 0xcf6f,
    });
}
