using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>The native tile character indexes of the gameplay-message title glyph set.</summary>
public enum GameplayMessageTitleGlyph : ushort
{
    /// <summary>Native tile character index for a space.</summary>
    Space = 0x04e,
    /// <summary>Native tile character index for a hyphen.</summary>
    Hyphen = 0x0cf,
    /// <summary>Native tile character index for A.</summary>
    A = 0x0e0,
    /// <summary>Native tile character index for B.</summary>
    B = 0x0e1,
    /// <summary>Native tile character index for C.</summary>
    C = 0x0e2,
    /// <summary>Native tile character index for D.</summary>
    D = 0x0e3,
    /// <summary>Native tile character index for E.</summary>
    E = 0x0e4,
    /// <summary>Native tile character index for F.</summary>
    F = 0x0e5,
    /// <summary>Native tile character index for G.</summary>
    G = 0x0e6,
    /// <summary>Native tile character index for H.</summary>
    H = 0x0e7,
    /// <summary>Native tile character index for I.</summary>
    I = 0x0e8,
    /// <summary>Native tile character index for J.</summary>
    J = 0x0e9,
    /// <summary>Native tile character index for K.</summary>
    K = 0x0ea,
    /// <summary>Native tile character index for L.</summary>
    L = 0x0eb,
    /// <summary>Native tile character index for M.</summary>
    M = 0x0ec,
    /// <summary>Native tile character index for N.</summary>
    N = 0x0ed,
    /// <summary>Native tile character index for O.</summary>
    O = 0x0ee,
    /// <summary>Native tile character index for P.</summary>
    P = 0x0ef,
    /// <summary>Native tile character index for Q.</summary>
    Q = 0x0f0,
    /// <summary>Native tile character index for R.</summary>
    R = 0x0f1,
    /// <summary>Native tile character index for S.</summary>
    S = 0x0f2,
    /// <summary>Native tile character index for T.</summary>
    T = 0x0f3,
    /// <summary>Native tile character index for U.</summary>
    U = 0x0f4,
    /// <summary>Native tile character index for V.</summary>
    V = 0x0f5,
    /// <summary>Native tile character index for W.</summary>
    W = 0x0f6,
    /// <summary>Native tile character index for X.</summary>
    X = 0x0f7,
    /// <summary>Native tile character index for Y.</summary>
    Y = 0x0f8,
    /// <summary>Native tile character index for Z.</summary>
    Z = 0x0f9,
    /// <summary>Native tile character index for a period.</summary>
    Period = 0x0fa,
    /// <summary>Native tile character index for a question mark.</summary>
    QuestionMark = 0x0fe,
}

/// <summary>Schema and cartridge glyph identities for editable one-row gameplay messages.</summary>
public static class GameplayMessageTitleDefinitions
{
    /// <summary>Supported gameplay-message title document schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing the editable one-row item and status titles.</summary>
    public const string FileName = "gameplay-message-titles.json";
    /// <summary>Number of transparent tilemap columns preceding the visible title.</summary>
    public const int OuterLeftColumns = 6;
    /// <summary>Number of transparent tilemap columns following the visible title.</summary>
    public const int OuterRightColumns = 7;
    /// <summary>Width in tilemap cells of the centered visible title region.</summary>
    public const int VisibleColumns = 19;
    /// <summary>Total words in the native three-row, 32-column message tilemap.</summary>
    public const int TilemapWords = GameplayMessageRomData.Layout.TilemapWidth * 3;
    /// <summary>Transparent character word used outside the visible title region.</summary>
    public const ushort TransparentWord = 0x000e;
    /// <summary>BG priority bit applied to compiled title glyph words.</summary>
    public const ushort PriorityWord = 0x2000;

    private static readonly GameplayMessageId[] SupportedMessageIds =
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

    /// <summary>Determines whether a character belongs to the supported title glyph alphabet.</summary>
    public static bool IsSupportedGlyph(char character) =>
        character is ' ' or '-' or '.' or '?' or >= 'A' and <= 'Z';
}
