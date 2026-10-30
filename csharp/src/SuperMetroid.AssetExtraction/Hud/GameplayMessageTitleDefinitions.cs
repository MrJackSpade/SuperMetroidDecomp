using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Schema and cartridge glyph identities for editable one-row gameplay messages.</summary>
public static class GameplayMessageTitleDefinitions
{
    /// <summary>Supported gameplay-message title document schema revision.</summary>
    public const int Version = 2;
    /// <summary>Native tile character index of A; B through Z follow contiguously.</summary>
    public const int FirstLetterTile = 0x0e0;
    /// <summary>The tile that draws each punctuation character a title spells: a space, a hyphen, a period and a question mark.</summary>
    public static FrozenDictionary<char, int> PunctuationTiles { get; } = new Dictionary<char, int>
    {
        [' '] = 0x04e,
        ['-'] = 0x0cf,
        ['.'] = 0x0fa,
        ['?'] = 0x0fe,
    }.ToFrozenDictionary();
    /// <summary>Letters in the contiguous A-Z tile range.</summary>
    public const int LetterCount = 26;

    /// <summary>JSON filename containing the editable one-row item and status titles.</summary>
    public const string FileName = "gameplay-message-titles.json";
    /// <summary>Number of transparent tilemap columns preceding the visible title.</summary>
    public const int OuterLeftColumns = 6;
    /// <summary>Number of transparent tilemap columns following the visible title.</summary>
    public const int OuterRightColumns = 7;
    /// <summary>Width in tilemap cells of the centered visible title region.</summary>
    public const int VisibleColumns = 19;
    /// <summary>Total cells in the three-row, 32-column message.</summary>
    public const int TilemapWords = GameplayMessageBoxDefinitions.Layout.TilemapWidth * 3;
    /// <summary>The transparent tile shown outside the visible title region.</summary>
    public static BackgroundTile TransparentTile { get; } = new(0x00e, 0, highPriority: false);

    /// <summary>Fifteen item and status messages with a centered one-row title that the artwork document may replace.</summary>
    internal static readonly GameplayMessageId[] SupportedMessageIds =
    [
        GameplayMessageId.EnergyTank,
        GameplayMessageId.VariaSuit,
        GameplayMessageId.SpringBall,
        GameplayMessageId.MorphBall,
        GameplayMessageId.ScrewAttack,
        GameplayMessageId.HiJumpBoots,
        GameplayMessageId.SpaceJump,
        GameplayMessageId.ChargeBeam,
        GameplayMessageId.IceBeam,
        GameplayMessageId.WaveBeam,
        GameplayMessageId.SpazerBeam,
        GameplayMessageId.PlasmaBeam,
        GameplayMessageId.SaveCompleted,
        GameplayMessageId.ReserveTank,
        GameplayMessageId.GravitySuit,
    ];

    /// <summary>Gets the 15 gameplay messages whose centered title text is editable.</summary>
    public static ReadOnlySpan<GameplayMessageId> MessageIds => SupportedMessageIds;
}
