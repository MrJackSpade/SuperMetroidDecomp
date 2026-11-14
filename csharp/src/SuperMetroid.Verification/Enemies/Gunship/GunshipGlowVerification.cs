using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// Checks the landed gunship's glowing lights (#1733) against the cartridge: the top hull's initialisation spawns the glow
/// on every one of its paths, over the last colour of the sprite row all three pieces draw in; loading the Landing Site
/// starts it; every gameplay update shows the cartridge list's record and lays it over all three pieces, whose draws show
/// the installed shade of that record; and each installed record shade shows the record's colour.
/// </summary>
internal static class GunshipGlowVerification
{
    /// <summary>The top hull's spawn of the glow at its initialisation's merge, <c>$A2:A6C1</c>.</summary>
    private const int GlowSpawn = 0xa2a6c1;
    /// <summary>The post-Ceres path's branch to the merge, <c>$A2:A686</c>.</summary>
    private const int PostCeresBranch = 0xa2a686;
    /// <summary>The ordinary path's branch to the merge, <c>$A2:A69B</c>.</summary>
    private const int LandedBranch = 0xa2a69b;
    /// <summary>The operand of the top hull's palette load, at <c>$A2:A65F</c>.</summary>
    private const int TopPaletteOperand = 0xa2a660;
    /// <summary>The operand of the bottom hull's and pad's palette load, at <c>$A2:A700</c>.</summary>
    private const int BottomPaletteOperand = 0xa2a701;
    /// <summary>The glow's definition in bank $8D.</summary>
    private const ushort GlowDefinition = 0xe1c0;
    /// <summary>The glow's list, which opens with its colour index command.</summary>
    private const ushort GlowList = 0xca4e;
    /// <summary>The palette word of the sprite row the gunship's pieces draw in: the eighth.</summary>
    private const ushort PiecesPalette = 0x0e00;
    /// <summary>The bits of an enemy's palette word that select its sprite row.</summary>
    private const int PaletteRowShift = 9;
    /// <summary>The branch-always opcode.</summary>
    private const byte BranchAlways = 0x80;
    /// <summary>The gameplay updates the check runs: two of the glow's loops and a few more.</summary>
    private const int Updates = 150;

    /// <summary>
    /// The glow's wiring, its start as the Landing Site loads, the record on every update and on all three pieces and
    /// their draws, and the installed record shades.
    /// </summary>
    internal static void VerifyGunshipGlow(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        CeresColourEffectsVerification.AssertSpawn(cartridge, GlowSpawn, GlowDefinition, GunshipLightDefinitions.Glow, "the top hull's initialisation");
        foreach (int branch in new[] { PostCeresBranch, LandedBranch })
        {
            AssertEqual(BranchAlways, CeresColourEffectsVerification.Byte(cartridge, branch), $"the path at ${branch:X6} branches to the merge");
            AssertEqual(GlowSpawn, branch + 2 + (sbyte)CeresColourEffectsVerification.Byte(cartridge, branch + 1), $"the path at ${branch:X6} reaches the glow's spawn");
        }
        AssertEqual(PiecesPalette, cartridge.ReadCartridgeWord(TopPaletteOperand), "the top hull draws in the eighth sprite row");
        AssertEqual(PiecesPalette, cartridge.ReadCartridgeWord(BottomPaletteOperand), "the bottom hull and the pad draw in the eighth sprite row");
        AssertEqual((ushort)PaletteFxInstruction.SetColorIndex, cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | GlowList), "the glow's list opens with its colours");
        int row = PiecesPalette >> PaletteRowShift;
        int written = cartridge.ReadCartridgeWord(PaletteFxListWords.ListBank | (GlowList + sizeof(ushort))) / sizeof(ushort);
        AssertEqual(GameplayColourPositionDefinitions.SpriteSetsFirst + (row + 1) * GameplayColourPositionDefinitions.SetColours - 1, written,
            "the glow writes the last colour of the pieces' sprite row");

        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        RoomEnemySlot[] pieces = [.. runtime.Enemies.SlotTable.Slots.Where(slot => slot.DefinitionId is EnemyDefinitionId.ShipTop or EnemyDefinitionId.ShipBottomEntrance)];
        AssertEqual(3, pieces.Length, "the Landing Site places the gunship's three pieces");
        // Samus stands beside the landed ship, so the camera shows it.
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0x0400, 0x0400));
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot top = pieces.Single(piece => piece.DefinitionId == EnemyDefinitionId.ShipTop);
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(top.Position.X.Whole, (ushort)(top.Position.Y.Whole - 30)));
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(runtime.RoomPaletteFx.IsRunning(GunshipLightDefinitions.Glow), "loading the Landing Site starts the gunship's glow");
        AssertTrue(runtime.RoomPaletteFx.FrameOf(ColourEffectColours.GunshipGlow) is null, "the glow shows its first record on the first update");

        var glow = new NativeColourEffectRun(cartridge, GlowDefinition);
        var images = new HashSet<SpriteImage>();
        for (int update = 0; update < Updates; update++)
        {
            runtime.StepFrame(0);
            glow.Step();
            ColourEffectFrame frame = runtime.RoomPaletteFx.FrameOf(ColourEffectColours.GunshipGlow)
                ?? throw new InvalidOperationException($"Landing Site update {update} shows no gunship glow.");
            AssertEqual(glow.Record, NativeColourEffectRun.RecordOf(frame), $"Landing Site update {update} shows the record the cartridge's glow list shows");
            PictureShading lit = PictureShading.Shaded(ColourEffectShades.Of(ColourEffectColours.GunshipGlow, frame));
            foreach (RoomEnemySlot piece in pieces)
                AssertEqual(lit, piece.Shading, $"Landing Site update {update} lays the glow's record over gunship piece {piece.SlotIndex}");
            PictureShade glowShade = ColourEffectShades.Of(ColourEffectColours.GunshipGlow, frame);
            ImageSpriteDraw[] draws = [.. runtime.Sprites.Draws.OfType<ImageSpriteDraw>()
                .Where(draw => draw.Shading is { } shading && shading.Shades.Contains(glowShade))];
            AssertTrue(draws.Length > 0, $"Landing Site update {update} draws the gunship with the glow's record");
            foreach (ImageSpriteDraw draw in draws)
                images.Add(draw.Image.Sheet);
        }

        var native = new ReferencePaletteFxColorSource(bus);
        SuperMetroid.AssetExtraction.Fx.Artwork.IPaletteFxColorSource installed = InstalledFixtures.ImportMaps.RoomPaletteFx;
        int first = row * GameplayColourPositionDefinitions.SetColours;
        // A piece whose pictures draw no light holds the shades and recolours nothing.
        int recoloured = 0;
        foreach (SpriteImage image in images)
        {
            recoloured += CeresColourEffectsVerification.InstalledShadeRecolourings(image,
                [.. PaletteFxSpriteShades.Of(GunshipLightDefinitions.Glow, ColourEffectColours.GunshipGlow, first, installed)],
                [.. PaletteFxSpriteShades.Of(GunshipLightDefinitions.Glow, ColourEffectColours.GunshipGlow, first, native)], $"the gunship's glow on {image.Name}");
        }
        AssertTrue(recoloured > 0, "the gunship's installed pictures show its glow");
        Console.WriteLine($"  Gunship glow: every initialisation path spawns it over the pieces' last row colour; the Landing Site starts it, " +
            $"shows the cartridge list's record on {Updates} updates over all three pieces, and the {images.Count} sheets they draw from hold its record shades.");
    }

    /// <summary>Runs the gunship glow check against the retail cartridge.</summary>
    [VerificationCommand("--gunship-glow")]
    internal static int GunshipGlowCommand()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyGunshipGlow), () => VerifyGunshipGlow(bus));
        return 0;
    }
}
