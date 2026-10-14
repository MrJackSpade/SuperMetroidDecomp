namespace SuperMetroid.Core.Assets;

/// <summary>Native pointer, export name, and stock OAM-part count for the visible caret.</summary>
internal readonly record struct IntroCaretFrameDefinition(ushort Pointer, string Name, int StockPartCount);
