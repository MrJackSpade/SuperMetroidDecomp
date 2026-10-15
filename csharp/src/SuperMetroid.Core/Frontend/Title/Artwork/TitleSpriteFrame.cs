using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>One ROM-selected title OBJ frame; all parts inherit the active title palette.</summary>
public sealed record TitleSpriteFrame
{
    /// <summary>Gets the bank-$8C spritemap pointer used to select this frame.</summary>
    public required int Pointer { get; init; }
    /// <summary>Gets the ordered OBJ parts composing the frame.</summary>
    public required SpriteVisualPart[] Parts { get; init; }
}
