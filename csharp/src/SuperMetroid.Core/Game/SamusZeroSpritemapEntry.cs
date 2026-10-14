namespace SuperMetroid.Core.Game;

/// <summary>One part of the live zero-pointer Samus spritemap: encoded X word, Y byte and OAM attributes.</summary>
internal readonly record struct SamusZeroSpritemapEntry(ushort EncodedX, byte EncodedY, ushort Attributes);
