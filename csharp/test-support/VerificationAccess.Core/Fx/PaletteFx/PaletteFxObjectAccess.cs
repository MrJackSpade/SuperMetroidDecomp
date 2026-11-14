using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Verification's native view of palette-FX objects: each object's bank-$8D definition address, so the ROM oracles can
/// spawn, query and compare objects by the words the cartridge stores.
/// </summary>
internal static class PaletteFxObjectAccess
{
    /// <summary>Each object's bank-$8D definition address.</summary>
    private static readonly Dictionary<ColourEffectId, ushort> Addresses = new()
    {
        [ColourEffectId.FadeInSuperMetroidTitleLogo] = 0xe194,
        [ColourEffectId.FadeInNintendoCopyright] = 0xe19c,
        [ColourEffectId.TitleScreenBabyMetroidTubeLight] = 0xe1a0,
        [ColourEffectId.TitleScreenFlickeringDisplays] = 0xe1a4,
        [ColourEffectId.CutsceneGunshipEngineFlicker] = 0xe1a8,
        [ColourEffectId.CutsceneCeresNavigationLightsSprite] = 0xe1ac,
        [ColourEffectId.FadeInPlanetZebesText] = 0xe1b0,
        [ColourEffectId.FadeOutPlanetZebesText] = 0xe1b4,
        [ColourEffectId.CutsceneCeresNavigationLightsBackground] = 0xe1b8,
        [ColourEffectId.OldMotherBrainFightBackgroundLights] = 0xe1bc,
        [ColourEffectId.GunshipGlow] = 0xe1c0,
        [ColourEffectId.FadeOutZoomedOutExplodingZebes] = 0xe1c4,
        [ColourEffectId.WidePartOfZebesExplosionForeground] = 0xe1c8,
        [ColourEffectId.ZebesExplosionFinale] = 0xe1cc,
        [ColourEffectId.WhiteOutSpaceDuringZebesExplosion] = 0xe1d0,
        [ColourEffectId.ZebesExplosionPlanetAfterglow] = 0xe1d4,
        [ColourEffectId.ExplodingZebesLava] = 0xe1d8,
        [ColourEffectId.FadeOutExplodingZebesCrust] = 0xe1dc,
        [ColourEffectId.FadeOutExplodingZebesGreyClouds] = 0xe1e0,
        [ColourEffectId.GunshipEmergingFromZebesExplosion] = 0xe1e4,
        [ColourEffectId.WidePartOfZebesExplosionBackground] = 0xe1e8,
        [ColourEffectId.HyperBeam] = 0xe1f0,
        [ColourEffectId.SamusLoadingPowerSuit] = 0xe1f4,
        [ColourEffectId.SamusLoadingVariaSuit] = 0xe1f8,
        [ColourEffectId.SamusLoadingGravitySuit] = 0xe1fc,
        [ColourEffectId.PostCreditsSuperMetroidIcon] = 0xe200,
        [ColourEffectId.GreyOutTourianStatueDraygon] = 0xf749,
        [ColourEffectId.GreyOutTourianStatueKraid] = 0xf74d,
        [ColourEffectId.GreyOutTourianStatueRidley] = 0xf751,
        [ColourEffectId.GreyOutTourianStatuePhantoon] = 0xf755,
        [ColourEffectId.BombTorizoBelly] = 0xf759,
        [ColourEffectId.GoldenTorizoBelly] = 0xf75d,
        [ColourEffectId.SamusInHeat] = 0xf761,
        [ColourEffectId.CrateriaLightning] = 0xf765,
        [ColourEffectId.WreckedShipGreenLights] = 0xf76d,
        [ColourEffectId.BrinstarBlueBackgroundSpores] = 0xf775,
        [ColourEffectId.SporeSpawnBlueBackgroundSpores] = 0xf779,
        [ColourEffectId.BrinstarRedBackgroundGlow] = 0xf77d,
        [ColourEffectId.BeaconFlashing] = 0xf781,
        [ColourEffectId.NorfairForegroundAndHeatPhase] = 0xf785,
        [ColourEffectId.NorfairGreenForegroundHeatGlow] = 0xf789,
        [ColourEffectId.NorfairPurpleForegroundHeatGlow] = 0xf78d,
        [ColourEffectId.NorfairDarkForegroundHeatGlow] = 0xf791,
        [ColourEffectId.MaridiaSandPits] = 0xf795,
        [ColourEffectId.MaridiaSandFalls] = 0xf799,
        [ColourEffectId.MaridiaBackgroundWaterfalls] = 0xf79d,
        [ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs] = 0xf7a1,
        [ColourEffectId.TourianShutterRedFlashing] = 0xffc9,
        [ColourEffectId.TourianBackgroundRedFlashing] = 0xffcd,
        [ColourEffectId.TourianGeneralLevelRedFlashing] = 0xffd1,
        [ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs] = 0xffd5,
        [ColourEffectId.OldTourianRedFlashing] = 0xffd9,
        [ColourEffectId.OldTourianBackgroundRailingsFlashYellow] = 0xffdd,
        [ColourEffectId.OldTourianBackgroundPanelsFlashYellow] = 0xffe1,
        [ColourEffectId.UpperCrateriaRedFlashing] = 0xffe5,
        [ColourEffectId.CrateriaEscapeYellowLightning] = 0xffe9,
        [ColourEffectId.ParlorEscapeSkyFlicker] = 0xffed,
    };

    /// <summary>Each address's object.</summary>
    private static readonly Dictionary<ushort, ColourEffectId> Objects = Addresses.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The bank-$8D definition address of <paramref name="paletteFxObject"/>.</summary>
    internal static ushort NativeAddress(ColourEffectId paletteFxObject) => Addresses[paletteFxObject];

    /// <summary>The object whose definition is at <paramref name="address"/>.</summary>
    internal static ColourEffectId FromNative(ushort address) => Objects.TryGetValue(address, out ColourEffectId paletteFxObject)
        ? paletteFxObject
        : throw new InvalidDataException($"Palette-FX definition $8D:{address:X4} is outside the compiled retail domain.");

    /// <summary>Every object, in definition-address order.</summary>
    internal static IEnumerable<ColourEffectId> All => Addresses.Keys;

    /// <summary>Starting and querying effects by definition address.</summary>
    extension(ColourEffectSystem self)
    {
        /// <summary>Starts the effect whose definition is at <paramref name="definition"/>.</summary>
        internal void Start(ushort definition, SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated = false) =>
            self.Start(FromNative(definition), equippedItems, areaMiniBossDefeated);

        /// <summary>
        /// Starts the Torizo belly glow whose definition is at <paramref name="definition"/> lighting <paramref name="torizo"/>,
        /// as the Torizo's fight start does (#1700).
        /// </summary>
        internal void StartTorizoBellyGlow(ushort definition, ILitTorizo torizo, SamusEquipmentFlags equippedItems) =>
            self.StartTorizoBellyGlow(FromNative(definition), torizo, equippedItems);

        /// <summary>Whether the effect whose definition is at <paramref name="definition"/> is running.</summary>
        internal bool IsRunning(ushort definition) => self.IsRunning(FromNative(definition));
    }

    /// <summary>A running effect's native definition word.</summary>
    extension(RoomColourEffect effect)
    {
        /// <summary>The definition address of the effect.</summary>
        internal ushort Id => NativeAddress(effect.Effect);
    }

    /// <summary>Definitions by address.</summary>
    extension(ColourEffectSelection)
    {
        /// <summary>The definition at <paramref name="pointer"/>.</summary>
        internal static ColourEffectDefinition Get(ushort pointer) => ColourEffectSelection.Get(FromNative(pointer));
    }

    /// <summary>Native view of a Tourian statue release.</summary>
    extension(TourianStatueAnimationDefinition self)
    {
        /// <summary>The bank-$8D definition address of the palette-FX object the release spawns.</summary>
        internal ushort PaletteFxDefinition => NativeAddress(self.ColourEffect);
    }

    /// <summary>Native view of a <see cref="CinematicGlowPaletteFxColorDefinition"/>.</summary>
    extension(CinematicGlowPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/>.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/>.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/>.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="SamusLoadingSuitPaletteFxColorDefinition"/>.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="TorizoBellyPaletteFxColorDefinition"/>.</summary>
    extension(TorizoBellyPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="TourianEscapeRedFlashPaletteFxColorDefinition"/>.</summary>
    extension(TourianEscapeRedFlashPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/>.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }

    /// <summary>Native view of a <see cref="ZebesExplosionLayerFadePaletteFxColorDefinition"/>.</summary>
    extension(ZebesExplosionLayerFadePaletteFxColorDefinition self)
    {
        /// <summary>The bank-$8D definition address of the program's object.</summary>
        internal ushort DefinitionPointer => NativeAddress(self.Object);
    }
}
