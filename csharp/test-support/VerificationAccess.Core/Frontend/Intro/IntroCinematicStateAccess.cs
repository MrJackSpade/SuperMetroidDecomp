using SuperMetroid.Core.Hardware;
﻿using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Verification access to <see cref="IntroCinematicState"/> members production does not use.</summary>
internal static class IntroCinematicStateAccess
{
    /// <summary>Exposes <see cref="IntroCinematicState"/> state that only verification reads.</summary>
    extension(IntroCinematicState self)
    {
        /// <summary>A VRAM holding the image the intro shows.</summary>
        internal SnesVram ShownVram
        {
            get
            {
                var vram = new SnesVram();
                vram.LoadBytes(0, self.Image());
                return vram;
            }
        }

        /// <summary>Composes the Mother Brain room, cinematic actors, Samus, and projectiles in SNES priority order.</summary>
        internal Rgba32[] RenderMotherBrainFlashback()
        {
            Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(self.palette, IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight, new Rgba32[IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight]);
            SpriteFrameBuffer oam = self.PrepareMotherBrainSprites();

            self.CompositeObjPriority(pixels, oam, 0);
            self.CompositeTextPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 1);
            self.CompositeMotherBrainRoomPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 2);
            self.CompositeMotherBrainRoomPriority(pixels, priority: true);
            self.CompositeObjPriority(pixels, oam, 3);
            self.CompositeTextPriority(pixels, priority: true);
            return pixels;
        }
        /// <summary>Composes the SR388 room with its egg, baby, Samus, and projectile layers.</summary>
        internal Rgba32[] RenderBabyDiscovery()
        {
            Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(self.palette, IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight, new Rgba32[IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight]);
            SpriteFrameBuffer oam = self.PrepareBabyDiscoverySprites();

            self.CompositeObjPriority(pixels, oam, 0);
            self.CompositeTextPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 1);
            self.CompositeBabyDiscoveryRoomPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 2);
            self.CompositeBabyDiscoveryRoomPriority(pixels, priority: true);
            self.CompositeObjPriority(pixels, oam, 3);
            self.CompositeTextPriority(pixels, priority: true);
            return pixels;
        }
        /// <summary>Composes the delivery or examination laboratory with its scrolling BG1 and actor priority.</summary>
        internal Rgba32[] RenderScientistCutscene()
        {
            Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(self.palette, IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight, new Rgba32[IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight]);
            SpriteFrameBuffer oam = self.PrepareScientistSprites();

            // TM=$15 uses the same BG1/BG3/OBJ priority ladder as the gameplay flashbacks.
            // BG1SC=$58 selects the first laboratory page; its crossfade pan is real PPU scroll.
            self.CompositeObjPriority(pixels, oam, 0);
            self.CompositeTextPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 1);
            self.CompositeScientistRoomPriority(pixels, priority: false);
            self.CompositeObjPriority(pixels, oam, 2);
            self.CompositeScientistRoomPriority(pixels, priority: true);
            self.CompositeObjPriority(pixels, oam, 3);
            self.CompositeTextPriority(pixels, priority: true);
            return pixels;
        }
        /// <summary>Overlays only the requested laboratory BG1 priority tiles, leaving transparent pixels untouched.</summary>
        internal void CompositeScientistRoomPriority(Span<Rgba32> pixels, bool priority)
        {
            // Composited directly; transparent pixels leave the frame untouched, as a plane would.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.ShownVram, self.palette, new BgVramAddresses { TilemapBaseWord = IntroVram.LaboratoryTilemapWord(self.scientistCutscene?.Scene ??
                    IntroLaboratoryScene.Delivery), CharacterBaseWord = 0 }, new BgViewport { Scroll = self.scientistCutscene?.Background.AsBgPlaneScroll() ?? default, Size = new Extent16<ScreenPixels>(IntroCinematicStateConstants.ScreenWidth, IntroCinematicStateConstants.ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
        }
        /// <summary>Overlays only the requested SR388 BG1 priority tiles, leaving transparent pixels untouched.</summary>
        internal void CompositeBabyDiscoveryRoomPriority(Span<Rgba32> pixels, bool priority)
        {
            // Composited directly; transparent pixels leave the frame untouched, as a plane would.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.ShownVram, self.palette, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.SceneBg2TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, IntroCinematicState.GameplayFlashbackBg1VerticalScroll), Size = new Extent16<ScreenPixels>(IntroCinematicStateConstants.ScreenWidth, IntroCinematicStateConstants.ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
        }
        /// <summary>Overlays only the requested Mother Brain BG1 priority tiles, leaving transparent pixels untouched.</summary>
        internal void CompositeMotherBrainRoomPriority(Span<Rgba32> pixels, bool priority)
        {
            // Composited directly; transparent pixels leave the frame untouched, as a plane would.
            SnesBgTilemapRenderer.Composite4BppViewport(pixels, self.ShownVram, self.palette, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.SceneBg1TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = (self.flashbackMotherBrain?.Background ??
                    new Position16<RoomPixels>(0, IntroCinematicState.GameplayFlashbackBg1VerticalScroll)).AsBgPlaneScroll(), Size = new Extent16<ScreenPixels>(IntroCinematicStateConstants.ScreenWidth, IntroCinematicStateConstants.ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
        }
        /// <summary>Clears reused raster scratch before overlaying narration BG3 tiles of one priority.</summary>
        internal void CompositeTextPriority(Span<Rgba32> pixels, bool priority)
        {
            // The renderer skips tiles of the other priority, so clear the reused plane first.
            Rgba32[] fullText = new Rgba32[IntroCinematicStateConstants.ScreenWidth * 32 * 8];
            fullText.AsSpan().Clear();
            SnesBgTilemapRenderer.Render2Bpp(fullText, self.ShownVram, self.palette, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.NarrationTilemapWord, CharacterBaseWord = IntroCinematicRomData.Layers.FontCharacterBaseWord }, rowCount: 32, transparentColorZero: true, priority: priority);
            SnesLayerCompositor.Composite(pixels, fullText.AsSpan(8 * IntroCinematicStateConstants.ScreenWidth, IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight));
        }
        /// <summary>Overlays OBJ sprites at one SNES priority level into the cinematic frame.</summary>
        internal void CompositeObjPriority(Span<Rgba32> pixels, SpriteFrameBuffer objects, int priority)
        {
            Rgba32[] sprites = new Rgba32[IntroCinematicStateConstants.ScreenWidth * IntroCinematicStateConstants.ScreenHeight];
            SnesObjRendererConstants.Render(
                sprites,
                objects.Draws,
                self.palette,
                size: new Extent16<ScreenPixels>(IntroCinematicStateConstants.ScreenWidth, IntroCinematicStateConstants.ScreenHeight),
                priority: priority);
            SnesLayerCompositor.Composite(pixels, sprites);
        }
    }
}
