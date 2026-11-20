using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Verification's native view of the room-effect identities (#1415): the cartridge routine and header words naming
/// each member. Core's values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class FxNativeWords
{
    /// <summary>A <see cref="RoomFxType"/> value naming no effect, standing for a dispatcher byte that names none.</summary>
    internal const RoomFxType UndefinedFxType = (RoomFxType)ushort.MaxValue;

    /// <summary>A <see cref="RoomEffectTint"/> value naming no blend, standing for a selector byte that names none.</summary>
    internal const RoomEffectTint UndefinedPaletteBlend = (RoomEffectTint)byte.MaxValue;

    /// <summary>
    /// The effect a native record's dispatcher byte <paramref name="word"/> names, or <see cref="UndefinedFxType"/> when it
    /// names none: a domain sweep over native bytes keeps testing rejection now that the identity's values are arbitrary.
    /// </summary>
    internal static RoomFxType FxType(int word) =>
        word is >= 0 and <= ushort.MaxValue && SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.TryDecode((ushort)word, out RoomFxType type)
            ? type : UndefinedFxType;

    /// <summary>The palette blend native selector byte <paramref name="word"/> names, or <see cref="UndefinedPaletteBlend"/>.</summary>
    internal static RoomEffectTint PaletteBlend(int word) =>
        word is >= 0 and <= ushort.MaxValue && SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.TryDecode((ushort)word, out RoomEffectTint blend)
            ? blend : UndefinedPaletteBlend;

    /// <summary>
    /// The bank-$87 object header an area's animated-tile bit names when it starts no animation: a still object with no
    /// loop, whose blocks keep their own pictures (#2054) — the vertical spikes in every area, Crateria's lake, Brinstar's
    /// plant and the Wrecked Ship's screen — or, selecting no object of the game (#1686), the unused Crateria lava for
    /// Crateria's fourth bit, which no record sets, and the empty object for every other bit no area uses.
    /// </summary>
    internal static ushort UnselectedAnimatedTileObject(int nativeAreaIndex, int bit) =>
        (bit, nativeAreaIndex < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count
            ? SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(nativeAreaIndex, "Block-animation area")
            : (SuperMetroid.Core.Progression.AreaId?)null) switch
        {
            (1, _) => 0x8251,
            (2, SuperMetroid.Core.Progression.AreaId.Crateria) => 0x825d,
            (3, SuperMetroid.Core.Progression.AreaId.Crateria) => 0x8263,
            (2, SuperMetroid.Core.Progression.AreaId.Brinstar) => 0x8281,
            (4, SuperMetroid.Core.Progression.AreaId.WreckedShip) => 0x826f,
            _ => 0x824b,
        };

    /// <summary>The bank-$87 object header naming each <see cref="BlockAnimation"/>.</summary>
    internal static readonly NativeWordIdentities<BlockAnimation> AnimatedTileObjects = new(new Dictionary<BlockAnimation, ushort>
    {
        [BlockAnimation.TourianStatuePhantoon] = 0x854c,
        [BlockAnimation.TourianStatueRidley] = 0x8552,
        [BlockAnimation.TourianStatueKraid] = 0x8558,
        [BlockAnimation.TourianStatueDraygon] = 0x855e,
        [BlockAnimation.HorizontalSpikes] = 0x8257,
        [BlockAnimation.MaridiaSandCeiling] = 0x8287,
        [BlockAnimation.MaridiaSandFalling] = 0x828d,
        [BlockAnimation.WreckedShipTreadmillRightwards] = 0x8275,
        [BlockAnimation.WreckedShipTreadmillLeftwards] = 0x827b,
        [BlockAnimation.Lava] = 0x82ab,
        [BlockAnimation.Acid] = 0x82c9,
        [BlockAnimation.Rain] = 0x82e7,
        [BlockAnimation.Spores] = 0x82fd,
    });

    /// <summary>The bank-$8D pre-instruction routine of each <see cref="NativePaletteFxPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<NativePaletteFxPreInstruction> PaletteFxPreInstructions = new(new Dictionary<NativePaletteFxPreInstruction, ushort>
    {
        [NativePaletteFxPreInstruction.Null] = 0xc526,
        [NativePaletteFxPreInstruction.Cleared] = 0xc5e3,
        [NativePaletteFxPreInstruction.Intro] = 0xe20b,
        [NativePaletteFxPreInstruction.DeleteWhenEnemyZeroDies] = 0xe2e0,
        [NativePaletteFxPreInstruction.Heat] = 0xe379,
        [NativePaletteFxPreInstruction.SwitchAboveY380] = 0xec59,
        [NativePaletteFxPreInstruction.DeleteWhenAreaMiniBossDies] = 0xeec5,
    });

    /// <summary>
    /// The colour effect condition <paramref name="preInstruction"/> stands for (#1554): the three routines that do
    /// nothing stand for none, and each lightning switch for its program's restart while Samus is high.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="preInstruction"/> is not a defined routine.</exception>
    internal static ColourEffectCondition? ConditionOf(NativePaletteFxPreInstruction preInstruction) => preInstruction switch
    {
        NativePaletteFxPreInstruction.Null or NativePaletteFxPreInstruction.Cleared or NativePaletteFxPreInstruction.Intro => null,
        NativePaletteFxPreInstruction.DeleteWhenEnemyZeroDies => ColourEffectCondition.EndsWhenTorizoDies.Instance,
        NativePaletteFxPreInstruction.Heat => ColourEffectCondition.BurnsSamusInHeat.Instance,
        NativePaletteFxPreInstruction.SwitchAboveY380 => new ColourEffectCondition.RestartsWhileSamusIsHigh(
            CrateriaLightningDefinitions.SamusHighOnSurfaceHeight),
        NativePaletteFxPreInstruction.DeleteWhenAreaMiniBossDies => ColourEffectCondition.EndsWhenAreaMiniBossDies.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(preInstruction), preInstruction, "Undefined palette-FX pre-instruction."),
    };

    /// <summary>
    /// The bank-$8D pre-instruction routine of the deleted adjacent-slot inspection (#1526): Tourian's glowing blocks and
    /// orbs set it to delete their object when the slot two below holds one. It only decided whether the glow kept
    /// showing, a presentation quirk the port does not reproduce, so no <see cref="NativePaletteFxPreInstruction"/> names it.
    /// </summary>
    internal const ushort DeletedSlotInspectionPreInstruction = 0xf621;

    /// <summary>
    /// The condition a recorded pre-instruction routine <paramref name="word"/> stands for in the port: the deleted
    /// adjacent-slot inspection stands for none, which the port's glow keeps from its start; any other word must name a
    /// routine.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="word"/> names no routine and is not the deleted inspection.</exception>
    internal static ColourEffectCondition? RecordedColourEffectCondition(ushort word) =>
        word == DeletedSlotInspectionPreInstruction
            ? null
            : ConditionOf(PaletteFxPreInstructions.Decode(word, "palette-FX pre-instruction"));

    /// <summary>The bank-$8D setup routine of each <see cref="NativePaletteFxSetup"/>.</summary>
    internal static readonly NativeWordIdentities<NativePaletteFxSetup> PaletteFxSetups = new(new Dictionary<NativePaletteFxSetup, ushort>
    {
        [NativePaletteFxSetup.Null] = 0xc685,
        [NativePaletteFxSetup.Intro] = 0xe204,
        [NativePaletteFxSetup.Norfair] = 0xe440,
        [NativePaletteFxSetup.Brinstar] = 0xf730,
    });

    /// <summary>
    /// How an effect whose native setup is <paramref name="setup"/> starts (#1554): the intro's setup only installs a
    /// pre-instruction that does nothing, so it starts plainly.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="setup"/> is not a defined setup.</exception>
    internal static ColourEffectStart StartOf(NativePaletteFxSetup setup) => setup switch
    {
        NativePaletteFxSetup.Null or NativePaletteFxSetup.Intro => ColourEffectStart.Plain,
        NativePaletteFxSetup.Norfair => ColourEffectStart.ChoosesHeatAnimationForSuit,
        NativePaletteFxSetup.Brinstar => ColourEffectStart.SkippedOnceAreaMiniBossIsDead,
        _ => throw new ArgumentOutOfRangeException(nameof(setup), setup, "Undefined palette-FX setup."),
    };
}
