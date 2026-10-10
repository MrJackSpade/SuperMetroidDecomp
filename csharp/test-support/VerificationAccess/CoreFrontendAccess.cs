using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Verification access to <see cref="AttractDemoRomData"/> members production does not use.</summary>
internal static class AttractDemoRomDataAccess
{
    extension(AttractDemoRomData)
    {
        /// <summary>VerifySRAM compares the twelve letters without a terminator; the selected-slot word follows.</summary>
        internal static ReadOnlySpan<byte> CompletionMarker => SuperMetroid.Core.Game.SaveRamLayout.CompletionMarker;
    }
}

/// <summary>Verification access to <see cref="CeresDepartureState"/> members production does not use.</summary>
internal static class CeresDepartureStateAccess
{
    extension(CeresDepartureState self)
    {
        /// <summary>Applies the current master-brightness nibble to a software-rendered frame.</summary>
        internal void ApplyBrightness(Span<Rgba32> pixels)
        {
            MasterBrightnessFilter.Apply(pixels, PrivateState.Field<byte>(self, "<Brightness>k__BackingField"));
        }
    }
}

/// <summary>Verification access to <see cref="CeresDestructionCinematicState"/> members production does not use.</summary>
internal static class CeresDestructionCinematicStateAccess
{
    extension(CeresDestructionCinematicState self)
    {
        internal ushort Zoom => PrivateState.Field<ushort>(self, "zoom");

        internal ushort BackgroundX => PrivateState.Field<ushort>(self, "backgroundX");

        internal ushort BackgroundY => PrivateState.Field<ushort>(self, "backgroundY");

        internal int ActiveActorCount => PrivateState.Field<List<IntroDiscoverySprite>>(self, "actors").Count;

        /// <summary>
        /// Reads the low-byte Mode-7 map selected by the cinematic's current native transfers.
        /// </summary>
        internal byte ReadMode7MapByte(int mapByteIndex)
        {
            if ((uint)mapByteIndex >= CeresDestructionRomData.Vram.Mode7MapCapacityBytes)
                throw new ArgumentOutOfRangeException(nameof(mapByteIndex));
            return PrivateState.Field<SnesVram>(self, "vram").ReadByte(mapByteIndex * 2);
        }
    }
}

/// <summary>Verification access to <see cref="CeresExplosionDefinitions"/> members production does not use.</summary>
internal static class CeresExplosionDefinitionsAccess
{
    extension(CeresExplosionDefinitions)
    {
        /// <summary><c>$8B:CF33</c>, invisible actor whose list owns the three-wave schedule.</summary>
        /// <remarks>Native list $8B:CE35..CE4A owns the three-wave schedule translated by StepCeresActors. The native list is translated to EventsAtFrame, with a mutable repeat countdown reset by fade completion. Initial/final instruction timing and repeat-before-final ordering are independently verified under #1165.</remarks>
        internal static CeresExplosionActorDefinition SpawnerActor =>
            new(0x93d9, 0xce35);
    }
}

/// <summary>Verification access to <see cref="CreditsObjectState"/> members production does not use.</summary>
internal static class CreditsObjectStateAccess
{
    extension(CreditsObjectState self)
    {
        internal int DestinationRow => PrivateState.Field<int>(self, "destinationRow");

        internal ReadOnlySpan<ushort> Tilemap => PrivateState.Field<ushort[]>(self, "tilemap");
    }
}

/// <summary>Verification access to <see cref="EndingRewardJump"/> members production does not use.</summary>
internal static class EndingRewardJumpAccess
{
    extension(EndingRewardJump self)
    {
        internal int VerticalVelocity => PrivateState.Field<int>(self, "velocity");
    }
}

/// <summary>Verification access to <see cref="FileSelectMapEntry"/> members production does not use.</summary>
internal static class FileSelectMapEntryAccess
{
    extension(FileSelectMapEntry self)
    {
        /// <summary>BG1/OBJ/subscreen are visible inside window one; the cleared BG2 covers its exterior.</summary>
        internal Rgba32[] Render(ReadOnlySpan<Rgba32> areaScene) => self.Render(areaScene, new Rgba32[areaScene.Length]);
    }
}

/// <summary>Verification access to <see cref="FileSelectMapWindowCompositor"/> members production does not use.</summary>
internal static class FileSelectMapWindowCompositorAccess
{
    extension(FileSelectMapWindowCompositor)
    {
        /// <summary>
        /// Keeps the area scene outside the window and the empty room frame inside it.
        /// Inputs must already have their own color math applied; this does not scale pixels.
        /// </summary>
        internal static Rgba32[] Composite(ReadOnlySpan<Rgba32> area, ReadOnlySpan<Rgba32> roomFrame,
            FileSelectMapWindow window) =>
            FileSelectMapWindowCompositor.Composite(area, roomFrame, window, new Rgba32[FrontendFrame.Width * FrontendFrame.Height]);
    }
}

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    extension(FileSelectMenuState self)
    {
        /// <summary>
        /// Read-only access for cartridge-layout verification. Gameplay code uploads this same
        /// buffer to BG1; exposing a span avoids adding a second, test-only tilemap builder.
        /// </summary>
        internal ReadOnlySpan<ushort> BackgroundTilemap => PrivateState.Field<ushort[]>(self, "bg1Tilemap");

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal int SelectedHelmetFrame => PrivateState.Field<int>(self, "helmetAnimationFrame");
    }
}

/// <summary>Verification access to <see cref="GameOverMenuState"/> members production does not use.</summary>
internal static class GameOverMenuStateAccess
{
    extension(GameOverMenuState self)
    {
        /// <summary>Currently displayed Baby Metroid frame, exposed for deterministic replay.</summary>
        internal ushort BabySpritemap => PrivateState.Field<ushort>(self, "babySpritemap");

        /// <summary>Current bank-$82 Baby instruction pointer, exposed for debugger inspection.</summary>
        internal ushort BabyInstructionPointer => PrivateState.Field<ushort>(self, "babyInstructionPointer");
    }
}

/// <summary>Verification access to <see cref="GameOverRomData.BabyAnimation"/> members production does not use.</summary>
internal static class GameOverRomDataBabyAnimationAccess
{
    extension(GameOverRomData.BabyAnimation)
    {
        /// <summary>Maps a bank-$82 animation opcode to its library-qualified cry.</summary>
        /// <remarks>
        /// Issues #625 and #970: pinned NTSC J/U v1.0 ROM and bank_82.asm match
        /// all three native instructions at $82:BC0C+9*i for i=0..2. Each loads
        /// library-3 effect $23, $26, or $27 and calls the same sound queue.
        /// The game-over Baby stream references those opcodes once each at
        /// $82:BC5D, BCEF, and BD69. Opcode spacing is regular, but the effect
        /// identities select three distinct sound operations; use the named cases and
        /// reject unknown opcodes.
        /// </remarks>
        internal static SoundEffectId ResolveCry(ushort opcode) => opcode switch
        {
            GameOverRomData.BabyAnimation.CryOpcode23 => GameOverRomData.BabyAnimation.Cry23,
            GameOverRomData.BabyAnimation.CryOpcode26 => GameOverRomData.BabyAnimation.Cry26,
            GameOverRomData.BabyAnimation.CryOpcode27 => GameOverRomData.BabyAnimation.Cry27,
            _ => throw new InvalidDataException(
                $"Unknown game-over Baby instruction $82:{opcode:X4}."),
        };
    }
}

/// <summary>Verification access to <see cref="IntroBabyDiscoveryState"/> members production does not use.</summary>
internal static class IntroBabyDiscoveryStateAccess
{
    extension(IntroBabyDiscoveryState self)
    {
        internal int ActiveEggParticleCount => PrivateState.Field<List<IntroEggParticle>>(self, "eggParticles").Count(particle => particle.IsActive);
    }
}

/// <summary>Verification access to <see cref="IntroCinematicRomData.Palette.Regions"/> members production does not use.</summary>
internal static class IntroCinematicRomDataPaletteRegionsAccess
{
    extension(IntroCinematicRomData.Palette.Regions self)
    {
        internal bool IsEmpty => self.Count == 0;
    }
}

/// <summary>Verification access to <see cref="IntroEggParticle"/> members production does not use.</summary>
internal static class IntroEggParticleAccess
{
    extension(IntroEggParticle self)
    {
        internal bool IsActive => PrivateState.Field<IntroDiscoverySprite>(self, "sprite").IsActive;
    }
}

/// <summary>Verification access to <see cref="IntroEggSlimeDrop"/> members production does not use.</summary>
internal static class IntroEggSlimeDropAccess
{
    extension(IntroEggSlimeDrop self)
    {
        internal bool IsActive => PrivateState.Field<IntroDiscoverySprite>(self, "sprite").IsActive;
    }
}

/// <summary>Verification access to <see cref="IntroMotherBrainExplosionSystem"/> members production does not use.</summary>
internal static class IntroMotherBrainExplosionSystemAccess
{
    extension(IntroMotherBrainExplosionSystem self)
    {
        /// <summary>All eight actors remain allocated until the later page-two crossfade.</summary>
        internal int ActiveCount => PrivateState.Field<global::System.Collections.Generic.IReadOnlyList<object>>(self, "actors").Count(actor => PrivateState.Property<bool>(actor, "IsActive"));
    }
}

/// <summary>Verification access to <see cref="IntroCeresFlightState"/> members production does not use.</summary>
internal static class IntroCeresFlightStateAccess
{
    extension(IntroCeresFlightState self)
    {
        /// <summary>
        /// One standalone game frame: the flight's dispatch, then the music queue handler the
        /// game runs after it, so the flight's own music wait drains as in production.
        /// </summary>
        internal void StepFrame()
        {
            self.Step();
            PrivateState.Field<SuperMetroid.Core.Audio.CartridgeAudioState>(self, "audio")
                .AdvanceFrame(PrivateState.Field<SuperMetroid.Core.Hardware.ISnesAddressSpace>(self, "bus"), default);
        }
    }
}

/// <summary>Verification access to <see cref="IntroRinkaSystem"/> members production does not use.</summary>
internal static class IntroRinkaSystemAccess
{
    extension(IntroRinkaSystem self)
    {
        /// <summary>Rinkas holding a cinematic slot, in the native descending slot order.</summary>
        internal IReadOnlyList<IntroDiscoverySprite> LiveRinkas
        {
            get
            {
                IntroDiscoverySprite?[] slots = PrivateState.Field<IntroDiscoverySprite?[]>(self, "slots");
                IntroDiscoverySprite spawner = PrivateState.Field<IntroDiscoverySprite>(self, "spawner");
                return slots.Reverse().OfType<IntroDiscoverySprite>()
                    .Where(actor => !ReferenceEquals(actor, spawner)).ToArray();
            }
        }

        internal int ActiveCount => self.LiveRinkas.Count(static rinka => rinka.IsActive);

        /// <summary>The live Rinka spawned with init parameter <paramref name="parameter"/>.</summary>
        internal IntroDiscoverySprite Rinka(int parameter) =>
            self.LiveRinkas.Single(rinka => rinka.GeneralTimer == parameter);
    }
}

/// <summary>Verification access to <see cref="MapScrollControls"/> members production does not use.</summary>
internal static class MapScrollControlsAccess
{
    extension(MapScrollControls)
    {
        internal static ushort[] Buttons =>
            [.. Enumerable.Range(1, MapScrollControls.DirectionCount).Select(direction => MapScrollControls.ButtonFor((MapScrollDirection)direction))];
    }
}

/// <summary>Verification access to <see cref="PauseMenuState"/> members production does not use.</summary>
internal static class PauseMenuStateAccess
{
    extension(PauseMenuState self)
    {
        /// <summary>True while a map/equipment page change is fading or loading; page input waits for it.</summary>
        internal bool IsPageTransitionActive => PrivateState.Field<PauseMenuTransition>(self, "transition") != default;

        /// <summary>Low byte of the native category/item selector word.</summary>
        internal PauseEquipmentCategory SelectedCategory => PrivateState.Field<PauseEquipmentCategory>(self, "selectedCategory");

        /// <summary>High byte of the native category/item selector word.</summary>
        internal int SelectedItem => PrivateState.Field<int>(self, "selectedItem");

        /// <summary>Read-only diagnostic timing for the native equipment-selector animation.</summary>
        internal (int Frame, int Timer) ItemSelectorAnimationState => (PrivateState.Field<int>(self, "itemSelectorAnimationFrame"), PrivateState.Field<int>(self, "itemSelectorAnimationTimer"));

        /// <summary>Number of cartridge OBJ records emitted by the most recent render.</summary>
        internal int LastRenderedSpriteCount => PrivateState.Field<OamBuffer>(self, "oam").LastFinalizedSpriteCount;

        /// <summary>Last bank-$82 menu spritemap ID selected by a pause draw routine.</summary>
        internal ushort LastIndicatorSpritemapId => PrivateState.Field<ushort>(self, "lastIndicatorSpritemapId");

        /// <summary>
        /// Reads one native $7E:3000-relative button-label word after its queued-equivalent
        /// upload. This narrow friend-test seam proves the bottom pause chrome reached VRAM;
        /// callers cannot mutate the private menu PPU.
        /// </summary>
        internal ushort ReadPauseButtonLabelWord(int nativeWordIndex)
        {
            const int firstUploadedNativeWordIndex = 0x0320;
            int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
            if ((uint)uploadedWordOffset >= PauseMenuLayout.ButtonRowsByteCount / 2)
                throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
            int byteAddress = (PauseMenuLayout.ButtonRowsDestinationWord + uploadedWordOffset) * 2;
            return unchecked((ushort)(
                PrivateState.Field<SnesVram>(self, "vram").ReadByte(byteAddress) | (PrivateState.Field<SnesVram>(self, "vram").ReadByte(byteAddress + 1) << 8)));
        }

        /// <summary>
        /// Reads one of the three reserve-supply digits from the mutable equipment tilemap.
        /// This exposes rendered menu state to focused tests without exposing mutation.
        /// </summary>
        internal SnesBgTilemapWord ReadReserveSupplyDigit(int digitIndex)
        {
            if ((uint)digitIndex >= PauseMenuLayout.ReserveSupplyDigitCount)
                throw new ArgumentOutOfRangeException(nameof(digitIndex));

            int byteOffset = PauseMenuLayout.ReserveSupplyDigitsByteOffset + digitIndex * 2;
            return unchecked((ushort)(
                PrivateState.Field<byte[]>(self, "equipmentTilemap")[byteOffset] | (PrivateState.Field<byte[]>(self, "equipmentTilemap")[byteOffset + 1] << 8)));
        }
    }
}

/// <summary>Verification access to <see cref="SuperMetroidGame"/> members production does not use.</summary>
internal static class SuperMetroidGameAccess
{
    extension(SuperMetroidGame self)
    {
        internal string? MapPresentationIdentity => PrivateState.Field<AreaMapPresentationCatalog?>(self, "mapPresentation")?.ContentIdentity;

        internal ushort DispatcherRandomNumber => PrivateState.Property<Bank80SystemState>(self, "FrontendRandomOwner").RandomNumber;

        /// <summary>
        /// Initialize the ordinary gameplay runtime for a friend verifier that
        /// directly loads a retail room. This uses the same runtime setup as a new
        /// game, then omits only the title/intro/fade dispatcher frames. It is not
        /// an alternate playable entry point or a substitute for the separate
        /// full-startup integration test.
        /// </summary>
        internal void InitializeDirectRoomVerification()
        {
            if (PrivateState.Field<SuperMetroidRuntime?>(self, "runtime") is not null || self.GameState != SuperMetroidGameState.Reset)
                throw new InvalidOperationException(
                    "Direct-room verification requires a fresh frontend.");
            if (!((bool)(PrivateState.Invoke(self, "SetupSelectedGame"))!))
                throw new InvalidOperationException(
                    "New-game runtime setup did not select the Ceres arrival path.");
            PrivateState.SetProperty(self, "GameState", SuperMetroidGameState.MainGameplay);
        }

        /// <summary>
        /// Exposes the cartridge coroutine phase only to the friend verification/debug hosts.
        /// Production UI code continues to consume the coarser public game state.
        /// </summary>
        internal DoorTransitionPhase DoorTransitionPhaseForVerification => PrivateState.Field<DoorTransitionState>(self, "doorTransition").Phase;
    }
}

/// <summary>Verification access to <see cref="TitleSequenceState"/> members production does not use.</summary>
internal static class TitleSequenceStateAccess
{
    extension(TitleSequenceState self)
    {
        /// <summary>Current INIDISP brightness nibble; zero is black and fifteen is full.</summary>
        internal byte Brightness => (byte)PrivateState.Field<int>(self, "brightness");

        /// <summary>Current title CGRAM, including cartridge palette-animation writes.</summary>
        internal ReadOnlySpan<Bgr555> PaletteColors => PrivateState.Field<SnesCgram>(self, "cgram").Colors;

        /// <summary>Current native Mode-7 A/D scalar, exposed for transform regression audits.</summary>
        internal ushort Mode7MatrixScale => unchecked((ushort)PrivateState.Field<int>(self, "zoom"));

        /// <summary>
        /// NTSC demo countdown. Retail initializes this to $0384 (900 frames); PAL uses $02D0
        /// so both revisions hold the title for approximately fifteen seconds.
        /// </summary>
        internal int TitleScreenFramesRemaining => PrivateState.Field<TitleSequencePhase>(self, "<Phase>k__BackingField") == TitleSequencePhase.TitleScreen ? PrivateState.Field<int>(self, "phaseTimer") : 0;
    }
}
