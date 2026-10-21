namespace SuperMetroid.Core.Assets;

/// <summary>One fixed spritemap identity and the installed asset name its editable artwork uses.</summary>
/// <param name="Pointer">The native 16-bit spritemap pointer.</param>
/// <param name="Name">The stable asset name of the visual composition.</param>
internal readonly record struct SpritemapFrameDefinition(ushort Pointer, string Name);
