using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingObjectArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares installed post-credits Samus and logo artwork through the native fade, DMA and assembled-logo sequence.</summary>
internal static class PostCreditsCharacterArtworkVerification
{
    /// <summary>Checks native versus installed VRAM and rendered pixels across the suitless post-shot handoff and all four logo actors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry")]
    internal static void VerifyPostCreditsCharacterArtwork(GameInstallation installation)
    {
        EndingObjectArtworkCatalog stock = installation.LoadEndingObjectArt();
        EndingMode7ArtworkCatalog stockMode7 = installation.LoadEndingMode7Art();
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        CreditsPresentation credits = CreditsPresentation.Load(new MemoryStream(
            CreditsPresentationExtractor.Extract(source)));
        foreach (ushort hours in new ushort[] { 2, 3, 10 })
        {
            var nativeBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
            var guardedBus = NewEndingObjectSourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
            var nativeAudio = new CartridgeAudioState();
            var installedAudio = new CartridgeAudioState();
            var native = CreateRetailEndingFixture(nativeAudio, hours, 0);
            var installed = CreateRetailEndingFixture(installedAudio, hours, 0);
            native.BindStaffCredits(credits);
            installed.BindStaffCredits(credits);
            installed.BindObjectArtwork(stock);
            installed.BindMode7Artwork(stockMode7);
            var checkedPhases = new HashSet<EndingCreditsPhase>();
            for (int frame = 0; frame < 60_000 &&
                native.Phase != EndingCreditsPhase.PostCreditsShot; frame++)
            {
                AssertEqual(native.Phase, installed.Phase,
                    $"post-credits installed/native phase for {hours}h at frame {frame}");
                if (native.Phase >= EndingCreditsPhase.Credits &&
                    checkedPhases.Add(native.Phase))
                {
                    LayeredRenderSnapshot expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                    LayeredRenderSnapshot actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                    AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                            actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                            SoftwareLayeredSnapshotRenderer.Render(actual).SequenceEqual(
                                SoftwareLayeredSnapshotRenderer.Render(expected)),
                        $"installed post-credits art preserves VRAM, palette, and pixels in {hours}h {native.Phase}");
                }
                native.Step();
                installed.Step();
                nativeAudio.AdvanceFrame(default);
                installedAudio.AdvanceFrame(default);
            }
            AssertEqual(EndingCreditsPhase.PostCreditsShot, native.Phase,
                $"native {hours}h reward reaches the post-credit shot");
            AssertEqual(EndingCreditsPhase.PostCreditsShot, installed.Phase,
                $"installed {hours}h reward reaches the post-credit shot");
            AssertTrue(checkedPhases.Contains(EndingCreditsPhase.Credits) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsWaitingSamus) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsReward) &&
                    checkedPhases.Contains(EndingCreditsPhase.PostCreditsJump),
                $"{hours}h reward exercised waiting, selected reward, and jump sheets");
            // The newly installed logo is not transferred until the Samus fade
            // admits six successive post-shot DMA calls. Compare the actual
            // native and installed VRAM/pixels across that handoff, not merely
            // the phase entry before the logo has appeared.
            for (int frame = 0; frame < 56; frame++)
            {
                native.Step();
                installed.Step();
                nativeAudio.AdvanceFrame(default);
                installedAudio.AdvanceFrame(default);
                if (frame < 47) continue;
                LayeredRenderSnapshot expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                LayeredRenderSnapshot actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
                    $"installed post-shot logo preserves native VRAM/pixels in {hours}h frame {frame}");
            }
            if (hours == 2)
            {
                EndingObjectArtworkCatalog editedLogo = LoadEditedEndingLogoArtwork(installation);
                bool visibleLogoEdit = false;
                // Continue the installed suitless route through all four logo
                // actors, not merely the initial logo upload after the shot.
                for (int frame = 0; frame < 2000 &&
                    native.Phase != EndingCreditsPhase.ItemPercentage; frame++)
                {
                    AssertEqual(native.Phase, installed.Phase,
                        $"installed logo phase at frame {frame}");
                    if (frame % 29 == 0 || native.Phase == EndingCreditsPhase.PostCreditsLogo)
                    {
                        LayeredRenderSnapshot expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                        LayeredRenderSnapshot actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                        AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                                actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                                SoftwareLayeredSnapshotRenderer.Render(actual).SequenceEqual(
                                    SoftwareLayeredSnapshotRenderer.Render(expected)),
                            $"installed logo pixels at frame {frame}");
                        if (native.Phase == EndingCreditsPhase.PostCreditsLogo)
                        {
                            installed.BindObjectArtwork(editedLogo);
                            LayeredRenderSnapshot changed = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                            visibleLogoEdit |= !actual.Memory.SpriteParts().SequenceEqual(changed.Memory.SpriteParts()) &&
                                !SoftwareLayeredSnapshotRenderer.Render(actual).SequenceEqual(
                                    SoftwareLayeredSnapshotRenderer.Render(changed));
                            installed.BindObjectArtwork(stock);
                        }
                    }
                    native.Step();
                    installed.Step();
                    nativeAudio.AdvanceFrame(default);
                    installedAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.ItemPercentage, native.Phase,
                    "native logo reaches item percentage");
                AssertEqual(EndingCreditsPhase.ItemPercentage, installed.Phase,
                    "compiled installed logo reaches item percentage");
                AssertTrue(visibleLogoEdit,
                    "edited right logo circle changes live OAM and visible ending pixels");
            }
            AssertEqual(0, guardedBus.ForbiddenReadAttempts,
                $"installed {hours}h reward never rereads post-credit art sources");
        }
        foreach ((string fileName, ushort hours, EndingCreditsPhase target) in new[]
        {
            (EndingObjectArtworkFormat.WaitingSamusFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsWaitingBackdrop),
            (EndingObjectArtworkFormat.ShootingScreenFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsShot),
            (EndingObjectArtworkFormat.SuitlessSamusFileName, (ushort)2,
                EndingCreditsPhase.PostCreditsReward),
            (EndingObjectArtworkFormat.WaitingTilemapFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsWaitingBackdrop),
            (EndingObjectArtworkFormat.PostCreditsFragmentAFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsReward),
            (EndingObjectArtworkFormat.PostCreditsFragmentBFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsReward),
            (EndingObjectArtworkFormat.PostShotLogoTileFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsShot),
            (EndingObjectArtworkFormat.PostShotLogoMapFileName, (ushort)3,
                EndingCreditsPhase.PostCreditsShot),
        })
        {
            bool fragment = fileName is EndingObjectArtworkFormat.PostCreditsFragmentAFileName or
                EndingObjectArtworkFormat.PostCreditsFragmentBFileName;
            bool postShotLogo = fileName is EndingObjectArtworkFormat.PostShotLogoTileFileName or
                EndingObjectArtworkFormat.PostShotLogoMapFileName;
            string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory,
                fileName);
            try
            {
                if (fileName is EndingObjectArtworkFormat.WaitingTilemapFileName or
                    EndingObjectArtworkFormat.PostShotLogoMapFileName)
                {
                    int mapBytes = fileName == EndingObjectArtworkFormat.PostShotLogoMapFileName
                        ? EndingObjectArtworkFormat.PostShotLogoMapByteCount
                        : EndingObjectArtworkFormat.WaitingTilemapByteCount;
                    File.WriteAllBytes(overridePath, RoomBackgroundTilemapExtractor.Encode(
                        new byte[mapBytes]));
                }
                else
                {
                    int byteCount = fileName switch
                    {
                        EndingObjectArtworkFormat.PostCreditsFragmentAFileName =>
                            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount,
                        EndingObjectArtworkFormat.PostCreditsFragmentBFileName =>
                            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount,
                        EndingObjectArtworkFormat.PostShotLogoTileFileName =>
                            EndingObjectArtworkFormat.PostShotLogoTileByteCount,
                        _ => EndingObjectArtworkFormat.RewardByteCount,
                    };
                    int tiles = byteCount / RoomCharacterAtlasFormat.BytesPerTile;
                    int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
                    int rows = (tiles + columns - 1) / columns;
                    IndexedPngImage image;
                    using (var input = File.OpenRead(Path.Combine(
                               installation.EndingObjectDirectory, fileName)))
                        image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
                    Array.Fill(image.Pixels, (byte)0);
                    using var output = File.Create(overridePath);
                    IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels,
                        image.Palette);
                }
                EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
                var stockBus = NewEndingObjectSourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
                var editedBus = NewEndingObjectSourceReadGuard(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom));
                var stockAudio = new CartridgeAudioState();
                var editedAudio = new CartridgeAudioState();
                var stockState = CreateRetailEndingFixture(stockAudio, hours, 0);
                var editedState = CreateRetailEndingFixture(editedAudio, hours, 0);
                stockState.BindStaffCredits(credits);
                editedState.BindStaffCredits(credits);
                stockState.BindObjectArtwork(stock);
                editedState.BindObjectArtwork(edited);
                stockState.BindMode7Artwork(stockMode7);
                editedState.BindMode7Artwork(stockMode7);
                for (int frame = 0; frame < 60_000 && stockState.Phase != target; frame++)
                {
                    AssertEqual(stockState.Phase, editedState.Phase,
                        $"edited {fileName} does not alter reward timing at frame {frame}");
                    stockState.Step();
                    editedState.Step();
                    stockAudio.AdvanceFrame(default);
                    editedAudio.AdvanceFrame(default);
                }
                AssertEqual(target, stockState.Phase,
                    $"edited {fileName} reaches the native reward scene");
                AssertEqual(target, editedState.Phase,
                    $"edited {fileName} retains reward scene timing");
                bool visible = false;
                // The logo map is BG2 only after the 216-frame post-shot hold,
                // 32-frame white flash, and the assembling S actors' crossfade.
                for (int frame = 0; frame < (postShotLogo ? 450 : 120) && !visible; frame++)
                {
                    if (frame % 8 == 0)
                    {
                        LayeredRenderSnapshot original = EndingCreditsPresentation.CaptureRenderSnapshot(stockState);
                        LayeredRenderSnapshot changed = EndingCreditsPresentation.CaptureRenderSnapshot(editedState);
                        bool changedVram = !original.Memory.Vram.SequenceEqual(changed.Memory.Vram);
                        if (!postShotLogo || changedVram)
                        {
                            AssertTrue(changedVram &&
                                    original.Memory.Cgram.SequenceEqual(changed.Memory.Cgram),
                                $"edited {fileName} changes only art, not palette state");
                            if (fragment)
                            {
                                int destination = fileName == EndingObjectArtworkFormat.PostCreditsFragmentAFileName
                                    ? (EndingVramLayout.PostCreditsFragmentA.Offset * 2)
                                    : (EndingVramLayout.PostCreditsFragmentB.Offset * 2);
                                int count = fileName == EndingObjectArtworkFormat.PostCreditsFragmentAFileName
                                    ? EndingObjectArtworkFormat.PostCreditsFragmentAByteCount
                                    : EndingObjectArtworkFormat.PostCreditsFragmentBByteCount;
                                AssertTrue(original.Memory.Vram[..destination].SequenceEqual(
                                        changed.Memory.Vram[..destination]) &&
                                        !original.Memory.Vram[destination..(destination + count)]
                                            .SequenceEqual(changed.Memory.Vram[destination..(destination + count)]) &&
                                        original.Memory.Vram[(destination + count)..].SequenceEqual(
                                            changed.Memory.Vram[(destination + count)..]),
                                    $"edited {fileName} changes only its native fragment transfer range");
                            }
                            visible = !SoftwareLayeredSnapshotRenderer.Render(original)
                                .SequenceEqual(
                                    SoftwareLayeredSnapshotRenderer.Render(changed));
                        }
                    }
                    stockState.Step();
                    editedState.Step();
                    stockAudio.AdvanceFrame(default);
                    editedAudio.AdvanceFrame(default);
                }
                if (!fragment)
                    AssertTrue(visible,
                        $"edited {fileName} changes visible post-credits pixels");
                if (postShotLogo)
                {
                    EndingCreditsPhase phaseBeforeRebind = stockState.Phase;
                    stockState.BindObjectArtwork(edited);
                    AssertEqual(phaseBeforeRebind, stockState.Phase,
                        $"rebind {fileName} preserves the live ending phase");
                    LayeredRenderSnapshot rebound = EndingCreditsPresentation.CaptureRenderSnapshot(stockState);
                    LayeredRenderSnapshot expectedRebind = EndingCreditsPresentation.CaptureRenderSnapshot(editedState);
                    AssertTrue(rebound.Memory.Vram.SequenceEqual(expectedRebind.Memory.Vram) &&
                            SoftwareLayeredSnapshotRenderer.Render(rebound).SequenceEqual(
                                SoftwareLayeredSnapshotRenderer.Render(expectedRebind)),
                        $"rebind {fileName} immediately replaces already-uploaded logo art");
                }
                AssertEqual(0, stockBus.ForbiddenReadAttempts,
                    $"stock {fileName} never rereads source ROM art");
                AssertEqual(0, editedBus.ForbiddenReadAttempts,
                    $"edited {fileName} never rereads source ROM art");
            }
            finally
            {
                File.Delete(overridePath);
            }
        }
        string invalidMap = Path.Combine(installation.EndingObjectOverrideDirectory,
            EndingObjectArtworkFormat.WaitingTilemapFileName);
        File.WriteAllBytes(invalidMap, [0]);
        try
        {
            AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
                "malformed waiting-Samus map override fails loudly");
        }
        finally
        {
            File.Delete(invalidMap);
        }
        foreach (string invalidName in new[]
        {
            EndingObjectArtworkFormat.PostShotLogoTileFileName,
            EndingObjectArtworkFormat.PostShotLogoMapFileName,
        })
        {
            string invalidPath = Path.Combine(installation.EndingObjectOverrideDirectory,
                invalidName);
            File.WriteAllBytes(invalidPath, [0]);
            try
            {
                AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
                    $"malformed post-shot logo override {invalidName} fails loudly");
            }
            finally
            {
                File.Delete(invalidPath);
            }
        }
        Console.WriteLine("Post-credits art: three reward variants, waiting BG2 map, two tile fragments, and both logo streams retain native phase, VRAM, palette and pixels without source reads; independent edits remain visible.");
    }
}
