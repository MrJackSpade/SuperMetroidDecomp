using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Complete compiled palette-FX definition and room-area selection domains used by the
/// retail cartridge. Native pointers remain the external identity stored in room and
/// cinematic programs; fixed callback metadata is application-owned.
/// </summary>
internal static class RoomPaletteFxDefinitions
{
    /// <summary>Eight retail area rows in the room palette-FX selection domain.</summary>
    internal const int AreaCount = 8;
    /// <summary>Eight effect-bit selections within each area row.</summary>
    internal const int DefinitionsPerArea = 8;

    /// <summary>Selects setup callback and initial program by native palette-FX object identity.</summary>
    /// <remarks>The 63 aligned objects occupy E194..E200, F745..F7A5 and FFC9..FFED
    /// in bank8D. Each identity dispatches to one setup operation and animation program.
    /// Named setup cases preserve null/intro/Norfair/Brinstar semantics, including
    /// aliases between distinct objects. Unknown/interior addresses reject.
    /// Both fields are independently checked against supported NTSC J/U v1.0 and
    /// pinned bank_8D.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// No stored definition array or generated cache remains.</remarks>
    internal static RoomPaletteFxDefinition Get(ushort pointer) => pointer switch
    {
        0xE194 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInSuperMetroidTitleLogo),
        0xE198 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInNintendoBootLogo),
        0xE19C => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInNintendoCopyright),
        0xE1A0 => new(PaletteFxSetup.Null, PaletteFxProgram.TitleScreenBabyMetroidTubeLight0),
        0xE1A4 => new(PaletteFxSetup.Null, PaletteFxProgram.TitleScreenFlickeringDisplays0),
        0xE1A8 => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneGunshipEngineFlicker0),
        0xE1AC => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneCeresNavigationLightSprite),
        0xE1B0 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInPlanetZebesText),
        0xE1B4 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutPlanetZebesText),
        0xE1B8 => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneCeresNavigationLightsBG),
        0xE1BC => new(PaletteFxSetup.Intro, PaletteFxProgram.OldMotherBrainFightBGLights0),
        0xE1C0 => new(PaletteFxSetup.Null, PaletteFxProgram.GunshipGlow0),
        0xE1C4 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutZoomedOutExplodingZebes),
        0xE1C8 => new(PaletteFxSetup.Null, PaletteFxProgram.WidePartOfZebesExplosionForeground),
        0xE1CC => new(PaletteFxSetup.Null, PaletteFxProgram.ZebesExplosionFinale),
        0xE1D0 => new(PaletteFxSetup.Null, PaletteFxProgram.WhiteOutSpaceDuringZebesExplosion),
        0xE1D4 => new(PaletteFxSetup.Null, PaletteFxProgram.ZebesExplosionPlanetAfterGlow0),
        0xE1D8 => new(PaletteFxSetup.Null, PaletteFxProgram.ExplodingZebesLava0),
        0xE1DC => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutExplodingZebesCrust),
        0xE1E0 => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutExplodingZebesGreyClouds),
        0xE1E4 => new(PaletteFxSetup.Null, PaletteFxProgram.GunshipEmergingFromZebesExplosion),
        0xE1E8 => new(PaletteFxSetup.Null, PaletteFxProgram.WidePartOfZebesExplosionBackground),
        0xE1EC => new(PaletteFxSetup.Null, PaletteFxProgram.ListD9D0),
        0xE1F0 => new(PaletteFxSetup.Null, PaletteFxProgram.HyperBeam0),
        0xE1F4 => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingPowerSuit0),
        0xE1F8 => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingVairaSuit0),
        0xE1FC => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingGravitySuit0),
        0xE200 => new(PaletteFxSetup.Null, PaletteFxProgram.PostCreditsSuperMetroidIconGlare),
        0xF745 => new(PaletteFxSetup.Null, PaletteFxProgram.Nothing),
        0xF749 => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueDraygon),
        0xF74D => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueKraid),
        0xF751 => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueRidley),
        0xF755 => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatuePhantoon),
        0xF759 => new(PaletteFxSetup.Null, PaletteFxProgram.BombTorizoBelly0),
        0xF75D => new(PaletteFxSetup.Null, PaletteFxProgram.GoldenTorizoBelly0),
        0xF761 => new(PaletteFxSetup.Norfair, PaletteFxProgram.SamusInHeatPowerSuit0),
        0xF765 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria10),
        0xF769 => new(PaletteFxSetup.Null, PaletteFxProgram.ListEC6E),
        0xF76D => new(PaletteFxSetup.Null, PaletteFxProgram.WreckedShip10),
        0xF771 => new(PaletteFxSetup.Null, PaletteFxProgram.WreckedShip10),
        0xF775 => new(PaletteFxSetup.Null, PaletteFxProgram.Brinstar10),
        0xF779 => new(PaletteFxSetup.Brinstar, PaletteFxProgram.Brinstar80),
        0xF77D => new(PaletteFxSetup.Null, PaletteFxProgram.Brinstar20),
        0xF781 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria80Brinstar40),
        0xF785 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair20),
        0xF789 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair40),
        0xF78D => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair80),
        0xF791 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair100),
        0xF795 => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia10),
        0xF799 => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia20),
        0xF79D => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia40),
        0xF7A1 => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian2),
        0xF7A5 => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian4),
        0xFFC9 => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian80),
        0xFFCD => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian100),
        0xFFD1 => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian20),
        0xFFD5 => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian40),
        0xFFD9 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria80),
        0xFFDD => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria100),
        0xFFE1 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria200),
        0xFFE5 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria20),
        0xFFE9 => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria40),
        0xFFED => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria400),
        _ => throw new InvalidDataException(
            $"Palette-FX definition $8D:{pointer:X4} is outside the compiled retail domain."),
    };
    /// <summary>Selects the palette-FX object enabled by one area's effect bit.</summary>
    /// <remarks>Direct semantic area/bit dispatch for all eight areas and eight bits.
    /// Unused selections (including Ceres/debug) select the native empty object.
    /// Validate area before bit, preserving both rejection domains and precedence.
    /// All64 native words are independently verified; no selection matrix remains.</remarks>
    internal static ushort GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return ((AreaId)areaIndex, bitIndex) switch
        {
            (AreaId.Crateria, 0) => 0xF765,
            (AreaId.Crateria, 1) => 0xFFE5,
            (AreaId.Crateria, 2) => 0xFFE9,
            (AreaId.Crateria, 3) => 0xFFD9,
            (AreaId.Crateria, 4) => 0xFFDD,
            (AreaId.Crateria, 5) => 0xFFE1,
            (AreaId.Crateria, 6) => 0xFFED,
            (AreaId.Crateria, 7) => 0xF781,
            (AreaId.Brinstar, 0) => 0xF775,
            (AreaId.Brinstar, 1) => 0xF77D,
            (AreaId.Brinstar, 2) => 0xF781,
            (AreaId.Brinstar, 3) => 0xF779,
            (AreaId.Norfair, 0) => 0xF761,
            (AreaId.Norfair, 1) => 0xF785,
            (AreaId.Norfair, 2) => 0xF789,
            (AreaId.Norfair, 3) => 0xF78D,
            (AreaId.Norfair, 4) => 0xF791,
            (AreaId.WreckedShip, 0) => 0xF76D,
            (AreaId.Maridia, 0) => 0xF795,
            (AreaId.Maridia, 1) => 0xF799,
            (AreaId.Maridia, 2) => 0xF79D,
            (AreaId.Tourian, 0) => 0xF761,
            (AreaId.Tourian, 1) => 0xF7A1,
            (AreaId.Tourian, 2) => 0xF7A5,
            (AreaId.Tourian, 3) => 0xFFC9,
            (AreaId.Tourian, 4) => 0xFFCD,
            (AreaId.Tourian, 5) => 0xFFD1,
            (AreaId.Tourian, 6) => 0xFFD5,
            _ => PaletteFxDeleteDefinitions.EmptyRoomEffectDefinition,
        };
    }

}
