namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatuePlmDefinitions
{
    /// <summary><c>$84:D6D6/$D13F</c>, sleeping Lower Norfair hand and its initial list.</summary>
    public static readonly ChozoStatuePlmDefinition LowerNorfairHand =
        new(ChozoStatuePlmRomData.LowerNorfairHand, 0xd13f);

    /// <summary><c>$84:D6EE/$AAE3</c>, Wrecked Ship hand and its delete list.</summary>
    public static readonly ChozoStatuePlmDefinition WreckedShipHand =
        new(ChozoStatuePlmRomData.WreckedShipHand, 0xaae3);

    /// <summary><c>$84:D6F8/$D3CF</c>, clear-slope-access header and program.</summary>
    public static readonly ChozoStatuePlmDefinition ClearSlopeAccess =
        new(ChozoStatuePlmRomData.ClearSlopeAccess, 0xd3cf);

    /// <summary><c>$84:D6FC/$D3EC</c>, block-slope-access header and program.</summary>
    public static readonly ChozoStatuePlmDefinition BlockSlopeAccess =
        new(ChozoStatuePlmRomData.BlockSlopeAccess, 0xd3ec);

    /// <summary><c>$84:D113/$D0F6</c>, crumbling Lower Norfair plug and its draw program.</summary>
    public static readonly ChozoStatuePlmDefinition CrumblePlug =
        new(ChozoStatuePlmRomData.CrumblePlug, 0xd0f6);

    private static readonly ChozoStatuePlmDefinition[] Definitions =
    [
        LowerNorfairHand,
        WreckedShipHand,
        ClearSlopeAccess,
        BlockSlopeAccess,
        CrumblePlug,
    ];

    /// <summary>Complete five-record domain accepted by the translated spawn seam.</summary>
    public static ReadOnlySpan<ChozoStatuePlmDefinition> All => Definitions;

    /// <summary>Resolves a supported header without interpreting adjacent bank-$84 data.</summary>
    public static ChozoStatuePlmDefinition Resolve(ushort headerPointer) => headerPointer switch
    {
        ChozoStatuePlmRomData.LowerNorfairHand => LowerNorfairHand,
        ChozoStatuePlmRomData.WreckedShipHand => WreckedShipHand,
        ChozoStatuePlmRomData.ClearSlopeAccess => ClearSlopeAccess,
        ChozoStatuePlmRomData.BlockSlopeAccess => BlockSlopeAccess,
        ChozoStatuePlmRomData.CrumblePlug => CrumblePlug,
        _ => throw new InvalidDataException($"Unsupported Chozo PLM ${headerPointer:X4}."),
    };
}

/// <summary>One Chozo terrain PLM header paired with its native initial list.</summary>
internal readonly record struct ChozoStatuePlmDefinition(
    ushort HeaderPointer,
    ushort InstructionListPointer);
