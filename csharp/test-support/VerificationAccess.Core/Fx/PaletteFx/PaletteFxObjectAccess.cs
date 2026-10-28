using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification's native view of palette-FX objects: each object's bank-$8D definition address, so the ROM oracles can
/// spawn, query and compare objects by the words the cartridge stores.
/// </summary>
internal static class PaletteFxObjectAccess
{
    /// <summary>Each object's bank-$8D definition address.</summary>
    private static readonly Dictionary<PaletteFxObject, ushort> Addresses = new()
    {
        [PaletteFxObject.FadeInSuperMetroidTitleLogo] = 0xe194,
        [PaletteFxObject.FadeInNintendoBootLogo] = 0xe198,
        [PaletteFxObject.FadeInNintendoCopyright] = 0xe19c,
        [PaletteFxObject.TitleScreenBabyMetroidTubeLight] = 0xe1a0,
        [PaletteFxObject.TitleScreenFlickeringDisplays] = 0xe1a4,
        [PaletteFxObject.CutsceneGunshipEngineFlicker] = 0xe1a8,
        [PaletteFxObject.CutsceneCeresNavigationLightsSprite] = 0xe1ac,
        [PaletteFxObject.FadeInPlanetZebesText] = 0xe1b0,
        [PaletteFxObject.FadeOutPlanetZebesText] = 0xe1b4,
        [PaletteFxObject.CutsceneCeresNavigationLightsBackground] = 0xe1b8,
        [PaletteFxObject.OldMotherBrainFightBackgroundLights] = 0xe1bc,
        [PaletteFxObject.GunshipGlow] = 0xe1c0,
        [PaletteFxObject.FadeOutZoomedOutExplodingZebes] = 0xe1c4,
        [PaletteFxObject.WidePartOfZebesExplosionForeground] = 0xe1c8,
        [PaletteFxObject.ZebesExplosionFinale] = 0xe1cc,
        [PaletteFxObject.WhiteOutSpaceDuringZebesExplosion] = 0xe1d0,
        [PaletteFxObject.ZebesExplosionPlanetAfterglow] = 0xe1d4,
        [PaletteFxObject.ExplodingZebesLava] = 0xe1d8,
        [PaletteFxObject.FadeOutExplodingZebesCrust] = 0xe1dc,
        [PaletteFxObject.FadeOutExplodingZebesGreyClouds] = 0xe1e0,
        [PaletteFxObject.GunshipEmergingFromZebesExplosion] = 0xe1e4,
        [PaletteFxObject.WidePartOfZebesExplosionBackground] = 0xe1e8,
        [PaletteFxObject.UnusedCinematicFade] = 0xe1ec,
        [PaletteFxObject.HyperBeam] = 0xe1f0,
        [PaletteFxObject.SamusLoadingPowerSuit] = 0xe1f4,
        [PaletteFxObject.SamusLoadingVariaSuit] = 0xe1f8,
        [PaletteFxObject.SamusLoadingGravitySuit] = 0xe1fc,
        [PaletteFxObject.PostCreditsSuperMetroidIcon] = 0xe200,
        [PaletteFxObject.Nothing] = 0xf745,
        [PaletteFxObject.GreyOutTourianStatueDraygon] = 0xf749,
        [PaletteFxObject.GreyOutTourianStatueKraid] = 0xf74d,
        [PaletteFxObject.GreyOutTourianStatueRidley] = 0xf751,
        [PaletteFxObject.GreyOutTourianStatuePhantoon] = 0xf755,
        [PaletteFxObject.BombTorizoBelly] = 0xf759,
        [PaletteFxObject.GoldenTorizoBelly] = 0xf75d,
        [PaletteFxObject.SamusInHeat] = 0xf761,
        [PaletteFxObject.CrateriaLightning] = 0xf765,
        [PaletteFxObject.UnusedDarkLightning] = 0xf769,
        [PaletteFxObject.WreckedShipGreenLights] = 0xf76d,
        [PaletteFxObject.UnusedWreckedShipGreenLightsDuplicate] = 0xf771,
        [PaletteFxObject.BrinstarBlueBackgroundSpores] = 0xf775,
        [PaletteFxObject.SporeSpawnBlueBackgroundSpores] = 0xf779,
        [PaletteFxObject.BrinstarRedBackgroundGlow] = 0xf77d,
        [PaletteFxObject.BeaconFlashing] = 0xf781,
        [PaletteFxObject.NorfairForegroundAndHeatPhase] = 0xf785,
        [PaletteFxObject.NorfairForegroundPalette4] = 0xf789,
        [PaletteFxObject.NorfairForegroundPalette5] = 0xf78d,
        [PaletteFxObject.NorfairForegroundPalette6] = 0xf791,
        [PaletteFxObject.MaridiaSandPits] = 0xf795,
        [PaletteFxObject.MaridiaSandFalls] = 0xf799,
        [PaletteFxObject.MaridiaBackgroundWaterfalls] = 0xf79d,
        [PaletteFxObject.TourianGlowingArkanoidBlocksAndRedOrbs] = 0xf7a1,
        [PaletteFxObject.UnusedTourianGlowClone] = 0xf7a5,
        [PaletteFxObject.TourianShutterRedFlashing] = 0xffc9,
        [PaletteFxObject.TourianBackgroundRedFlashing] = 0xffcd,
        [PaletteFxObject.TourianGeneralLevelRedFlashing] = 0xffd1,
        [PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs] = 0xffd5,
        [PaletteFxObject.OldTourianRedFlashing] = 0xffd9,
        [PaletteFxObject.OldTourianBackgroundRailingsFlashYellow] = 0xffdd,
        [PaletteFxObject.OldTourianBackgroundPanelsFlashYellow] = 0xffe1,
        [PaletteFxObject.UpperCrateriaRedFlashing] = 0xffe5,
        [PaletteFxObject.CrateriaEscapeYellowLightning] = 0xffe9,
        [PaletteFxObject.CrateriaEscapeCreBlockPixel] = 0xffed,
    };

    /// <summary>Each address's object.</summary>
    private static readonly Dictionary<ushort, PaletteFxObject> Objects = Addresses.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The bank-$8D definition address of <paramref name="paletteFxObject"/>.</summary>
    internal static ushort NativeAddress(PaletteFxObject paletteFxObject) => Addresses[paletteFxObject];

    /// <summary>The object whose definition is at <paramref name="address"/>.</summary>
    internal static PaletteFxObject FromNative(ushort address) => Objects.TryGetValue(address, out PaletteFxObject paletteFxObject)
        ? paletteFxObject
        : throw new InvalidDataException($"Palette-FX definition $8D:{address:X4} is outside the compiled retail domain.");

    /// <summary>Every object, in definition-address order.</summary>
    internal static IEnumerable<PaletteFxObject> All => Addresses.Keys;

    /// <summary>Spawning and querying objects by definition address.</summary>
    extension(RoomPaletteFxSystem self)
    {
        /// <summary>Spawns the object whose definition is at <paramref name="definition"/>.</summary>
        internal void SpawnDefinition(ushort definition, SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated = false) =>
            self.SpawnDefinition(FromNative(definition), equippedItems, areaMiniBossDefeated);

        /// <summary>Whether the object whose definition is at <paramref name="definition"/> owns a slot.</summary>
        internal bool IsDefinitionActive(ushort definition) => self.IsDefinitionActive(FromNative(definition));
    }

    /// <summary>A slot's native definition word.</summary>
    extension(RoomPaletteFxSystem.PaletteFxSlot slot)
    {
        /// <summary>The definition address of the slot's object, zero for a free slot.</summary>
        internal ushort Id
        {
            get => slot.Object is { } paletteFxObject ? NativeAddress(paletteFxObject) : (ushort)0;
            set => slot.Object = value == 0 ? null : FromNative(value);
        }
    }

    /// <summary>Definitions by address.</summary>
    extension(RoomPaletteFxSelection)
    {
        /// <summary>The definition at <paramref name="pointer"/>.</summary>
        internal static RoomPaletteFxDefinition Get(ushort pointer) => RoomPaletteFxSelection.Get(FromNative(pointer));
    }

    /// <summary>Native view of a Tourian statue release.</summary>
    extension(TourianStatueAnimatedTileProgramDefinition self)
    {
        /// <summary>The bank-$8D definition address of the palette-FX object the release spawns.</summary>
        internal ushort PaletteFxDefinition => NativeAddress(self.PaletteFx);
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
