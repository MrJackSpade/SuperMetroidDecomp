using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Evir.EvirInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>Checks that Evir instruction selectors resolve to installed frames with cartridge-equivalent OAM.</summary>
internal static class EvirVisualsVerification
{
    /// <summary>
    /// Checks every pose the Evir's programs show against editable stock spritemaps with cartridge-equivalent OAM, and
    /// that running each program with the stock artwork shows exactly its authored poses.
    /// </summary>
    internal static void VerifyInstalledEvirVisuals(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        var shownFrames = EvirAnimationsConstants.Spritemaps(Enum.GetValues<EvirSegment>()).ToHashSet();
        foreach (ushort pointer in shownFrames)
        {
            AssertTrue((stock.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    EvirVisualDefinitions.Bank, pointer,
                    out EnemySpritemapParts installedParts),
                $"Evir pose $A8:{pointer:X4} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = EvirVisualDefinitions.Bank, Pointer = pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<ScreenPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Evir pose $A8:{pointer:X4} installed OAM matches cartridge");
        }
        AssertEqual(EvirVisualDefinitions.FrameCount, shownFrames.Count,
            "the Evir programs show 24 distinct editable OAM frames");
        AssertTrue(EvirVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(shownFrames),
            "every exported Evir frame is shown by a program");
        foreach (EvirSegment loop in new[] { EvirSegment.BodyFacingLeft, EvirSegment.ArmsFacingLeft, EvirSegment.BodyFacingRight, EvirSegment.ArmsFacingRight })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(),
                definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            PrepareEvirBodyLoop(place, loop);
            Animation<EvirSegment> animation = EvirAnimations.StartingWith(loop);
            AssertEqual(EnemyInstructionRunner.ExpectedAnimationLoop(EvirAnimations.Segments, animation, EvirFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.AnimationLoopUpdates(EvirAnimations.Segments, animation)),
                $"installed Evir {loop} shows its authored poses");
        }
        Console.WriteLine(
            "  Evir visuals: 24 editable OAM frames match the cartridge and every installed loop shows its authored poses.");
    }
}
