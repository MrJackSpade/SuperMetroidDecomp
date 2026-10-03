namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed PLM header/list identities published by the two Chozo-statue actors.</summary>
internal static class ChozoStatuePlmDefinitions
{
    /// <summary><c>$84:D6D6/$D13F</c>, sleeping Lower Norfair hand and its initial list.</summary>
    public static ChozoStatuePlmDefinition LowerNorfairHand =>
        new(ChozoStatuePlmRomData.LowerNorfairHand, ChozoStatuePlmProgramDefinitions.LowerNorfairHandStart);

    /// <summary><c>$84:D6EE/$AAE3</c>, Wrecked Ship hand and its delete list.</summary>
    public static ChozoStatuePlmDefinition WreckedShipHand =>
        new(ChozoStatuePlmRomData.WreckedShipHand, RoomPlmSharedDeleteProgramDefinitions.Start);

    /// <summary><c>$84:D6F8/$D3CF</c>, clear-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition ClearSlopeAccess =>
        new(ChozoStatuePlmRomData.ClearSlopeAccess, ChozoStatuePlmProgramDefinitions.ClearSlopeStart);

    /// <summary><c>$84:D6FC/$D3EC</c>, block-slope-access header and program.</summary>
    public static ChozoStatuePlmDefinition BlockSlopeAccess =>
        new(ChozoStatuePlmRomData.BlockSlopeAccess, ChozoStatuePlmProgramDefinitions.BlockSlopeStart);

    /// <summary><c>$84:D113/$D0F6</c>, crumbling Lower Norfair plug and its draw program.</summary>
    public static ChozoStatuePlmDefinition CrumblePlug =>
        new(ChozoStatuePlmRomData.CrumblePlug, ChozoStatuePlmProgramDefinitions.CrumblePlugStart);

    /// <summary>Enumerate the five semantic spawn cases in the original published order.</summary>
    public static IEnumerable<ChozoStatuePlmDefinition> All
    {
        get
        {
            yield return LowerNorfairHand;
            yield return WreckedShipHand;
            yield return ClearSlopeAccess;
            yield return BlockSlopeAccess;
            yield return CrumblePlug;
        }
    }
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
