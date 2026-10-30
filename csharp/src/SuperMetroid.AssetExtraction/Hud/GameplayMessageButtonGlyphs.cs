using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>The button glyph a message panel's words show for a binding, and where in the panel it goes (#1416, #1466).</summary>
public static class GameplayMessageButtonGlyphs
{
    /// <summary>The glyph for <paramref name="binding"/>: that of the button <see cref="GameplayMessageButtons.Shown"/> chooses.</summary>
    public static BackgroundTile ResolveGlyph(ControllerButton binding) =>
        GameplayMessageTilemapDefinitions.Glyphs[GameplayMessageButtons.Shown(binding)];

    /// <summary>The byte offset in <paramref name="messageId"/>'s panel where the button glyph goes; zero for a message without one.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> is no message, or a message the glyph table does not list.</exception>
    public static int SpecialGlyphByteOffset(GameplayMessageId messageId)
    {
        if (!Enum.IsDefined(messageId) || messageId is GameplayMessageId.None or GameplayMessageId.GunshipSaveConfirmation)
            throw new ArgumentOutOfRangeException(nameof(messageId));
        return GameplayMessageTilemapDefinitions.GlyphByteOffsets.TryGetValue(messageId, out int offset) ? offset : 0;
    }
}
