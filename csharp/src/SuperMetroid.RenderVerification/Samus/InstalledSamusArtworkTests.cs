using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core;

/// <summary>
/// Finite acceptance checks for source-identified Samus drawing and upload owners.
/// This is artwork verification, never a gameplay search for remaining ROM reads.
/// </summary>
internal static class InstalledSamusArtworkTests
{
    /// <summary>Checks edited Samus body, cannon, and death art through software and GPU rendering without a cartridge.</summary>
    internal static void Run(InstalledSamusArtworkFixture fixture,
        D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var check = new ArtworkPixelCheck(device, renderer);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        int transfers = CheckBodyTransfers(fixture, check);
        int poses = CheckPoseDrawing(fixture, check);
        int queued = CheckQueuedTransfers(fixture, memory, check);
        Require(check.ChangedPixels > 0, "The artwork acceptance run changed no visible pixels.");
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {fixture.EditedPngCount} actual PNG overrides; " +
            $"{transfers} split body transfers, {poses} pose draws, {queued} cannon/death uploads; " +
            $"{check.Comparisons} exact software/GPU comparisons, {check.ChangedPixels} recolored pixels.");
    }

    /// <summary>Draws each Samus pose with stock and edited art and checks OAM, physical state, and visible pixels.</summary>
    private static int CheckPoseDrawing(InstalledSamusArtworkFixture fixture,
        ArtworkPixelCheck check)
    {
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            var stock = new SamusState { Pose = (SamusPoseId)pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), Health = 99 };
            var edited = new SamusState { Pose = stock.Pose, Position = FixedPosition<RoomPixels>.FromWhole(stock.Position.Whole), Health = stock.Health };
            stock.PoseMetadata.RefreshCollisionRadii(); edited.PoseMetadata.RefreshCollisionRadii();
            stock.Presentation.TileTransfers.BindArtwork(fixture.Stock);
            stock.Presentation.TileTransfers.BindStandardObjects(SpriteFrameTestArtwork.StandardObjects);
            edited.Presentation.TileTransfers.BindArtwork(fixture.Edited);
            edited.Presentation.TileTransfers.BindStandardObjects(SpriteFrameTestArtwork.StandardObjects);
            string before = PhysicalState(stock);
            var a = new SnesVram(); var b = new SnesVram();
            var oaFrame = new SpriteFrameBuffer(); var obFrame = new SpriteFrameBuffer();
            oaFrame.BeginFrame(); obFrame.BeginFrame();
            bool visible = stock.Presentation.Draw(oaFrame, new(0, 0));
            Require(visible == edited.Presentation.Draw(obFrame, new(0, 0)), $"pose {pose:X2}: visibility changed");
            stock.Presentation.TileTransfers.TransferToVram(a);
            edited.Presentation.TileTransfers.TransferToVram(b);
            OamBuffer oa = oaFrame.FinalizeAsOam(), ob = obFrame.FinalizeAsOam();
            Require(oa.ImageEquals(ob),
                $"pose {pose:X2}: pixel-only edit changed OAM placement");
            Require(before == PhysicalState(stock) && before == PhysicalState(edited),
                $"pose {pose:X2}: drawing changed physics, damage or animation state");
            check.Pair(a, b, oa, $"pose {pose:X2}");
        }
        return SamusBodyArtworkCatalog.PoseCount;
    }

    /// <summary>
    /// Captures pose timing, collision radii, and fixed-point motion for artwork parity.
    /// Derived spritemap indices and DMA flags are rendering outputs and are excluded.
    /// </summary>
    private static string PhysicalState(SamusState samus) => string.Join("|",
        samus.Pose, samus.Position.X.Whole, samus.Position.Y.Whole, samus.Health,
        samus.AnimationFrame, samus.AnimationFrameTimer, samus.AnimationFrameBuffer,
        samus.InvincibilityTimer, samus.KnockbackTimer, samus.EquippedItems,
        samus.EquippedBeams, samus.Kinematics.Position.X.Fraction, samus.Kinematics.Position.Y.Fraction,
        samus.HorizontalSpeed.BaseSpeed, samus.HorizontalSpeed.BaseSubspeed,
        samus.HorizontalSpeed.ExtraRunSpeed, samus.HorizontalSpeed.ExtraRunSubspeed,
        samus.Kinematics.YSpeed, samus.Kinematics.YSubspeed,
        samus.Kinematics.Radius.X, samus.Kinematics.Radius.Y);

    /// <summary>Raises a focused artwork fixture failure when an expected property changes.</summary>
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    /// <summary>Checks split body-tile DMA, pending definition identity, neighboring VRAM, and rendered pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static int CheckBodyTransfers(InstalledSamusArtworkFixture fixture,
        ArtworkPixelCheck check)
    {
        int count = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int sets = upper ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount;
            var destinations = upper ? SamusRenderingRomData.TileTransfers.TopDestinations :
                SamusRenderingRomData.TileTransfers.BottomDestinations;
            for (int set = 0; set < sets; set++)
            {
                var stock = upper ? fixture.Stock.TopSet(set) : fixture.Stock.BottomSet(set);
                var edited = upper ? fixture.Edited.TopSet(set) : fixture.Edited.BottomSet(set);
                Require(stock.Count == edited.Count, "PNG changed definition count");
                for (int position = 0; position < stock.Count; position++)
                {
                    SamusBodyTileDefinition a = stock[position], b = edited[position];
                    Require(a.SourceAddress == b.SourceAddress && a.FirstSize == b.FirstSize && a.SecondSize == b.SecondSize,
                        "PNG changed transfer identity or split size");
                    var va = new SnesVram(); var vb = new SnesVram();
                    // A pending NMI retains the selected definition pointer. Rebind only
                    // the installed catalog, exactly as a restored debugger state does.
                    var transfer = new SamusTileTransferState();
                    int pointer = fixture.Stock.DefinitionAddress(upper, (byte)set, (byte)position);
                    SetPendingDefinition(transfer, upper, pointer);
                    transfer.BindArtwork(fixture.Stock); transfer.TransferToVram(va);
                    transfer.BindArtwork(fixture.Edited); transfer.TransferToVram(vb);
                    Require(transfer.TopDefinitionAddress == (upper ? pointer : 0) &&
                        transfer.BottomDefinitionAddress == (upper ? 0 : pointer),
                        "Artwork rebind changed a pending DMA pointer");
                    var expected = new SnesVram();
                    expected.LoadBytes(destinations.First * 2, b.Planar.Span[..b.FirstSize]);
                    if (b.SecondSize != 0)
                        expected.LoadBytes(destinations.Second * 2, b.Planar.Span[b.FirstSize..]);
                    Require(expected.ImageEquals(vb), "Split DMA changed neighboring VRAM");
                    var oam = TileGallery(destinations.First, a.FirstSize, destinations.Second, a.SecondSize);
                    check.Pair(va, vb, oam, $"{(upper ? "top" : "bottom")}/{set:X2}/{position:X2}");
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>Restores a pending top or bottom transfer pointer as a saved runtime state would.</summary>
    private static void SetPendingDefinition(SamusTileTransferState transfer, bool upper, int pointer)
    {
        // Construct a faithful saved pending-transfer state without inventing a gameplay pose.
        if (upper)
        {
            transfer.TopDefinitionAddress = pointer;
            transfer.TopTransferEnabled = true;
        }
        else
        {
            transfer.BottomDefinitionAddress = pointer;
            transfer.BottomTransferEnabled = true;
        }
    }

    /// <summary>Checks queued arm-cannon and death-tile uploads across an installed-artwork rebind.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static int CheckQueuedTransfers(InstalledSamusArtworkFixture fixture,
        SuperMetroidAddressSpace memory, ArtworkPixelCheck check)
    {
        int count = 0;
        foreach (ushort source in SamusArmCannonArtworkFormat.TileSourcePointers)
        {
            Check(SamusRenderingRomData.Banks.CharacterData | source,
                SamusRenderingRomData.ArmCannon.TileUploadByteCount,
                SamusRenderingRomDataArmCannonConstants.TileVramDestination, $"cannon {source:X4}");
            count++;
        }
        foreach (SamusDeathTileSegment segment in SamusSpecialSequenceRomData.Death.TileSegments)
        {
            Check(segment.SourceAddress, SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
                segment.Destination.Encoded, $"death {segment.SourceAddress:X6}");
            count++;
        }
        return count;

        void Check(int source, ushort length, ushort destination, string name)
        {
            var a = new SnesVram(); var b = new SnesVram();
            var queue = new VramWriteQueue();
            queue.Enqueue(length, source, VramDmaDestination.FromNative(destination));
            VramWriteEntry pending = queue.Entries.Single();
            queue.DrainTo(a, new SamusQueuedArtwork(fixture.Stock));
            queue.Enqueue(checked((ushort)pending.SizeInBytes), pending.SourceAddress, pending.Destination);
            Require(queue.Entries.Single() == pending, $"{name}: replacement changed queued transfer");
            queue.DrainTo(b, new SamusQueuedArtwork(fixture.Edited));
            Require(queue.Entries.Count == 0 && queue.TailInBytes == 0, $"{name}: NMI did not clear queue");
            check.Pair(a, b, TileGallery(destination, length, 0, 0), name);
        }
    }

    /// <summary>Supplies installed cannon and death tiles to the queued VRAM transfer path.</summary>
    private sealed class SamusQueuedArtwork(SamusBodyArtworkCatalog artwork) :
        IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Resolves a queued transfer from the installed cannon or death-tile catalog.</summary>
        public bool TryResolve(int source, int length, out ReadOnlyMemory<byte> data) =>
            artwork.ArmCannon.TryResolveTile(source, length, out data) ||
            artwork.DeathTiles.TryResolve(source, length, out data);
        /// <summary>Rejects typed assets outside this fixture's address-based Samus uploads.</summary>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidDataException($"Unexpected typed transfer {asset} in Samus upload fixture.");
    }

    /// <summary>Arranges transferred tiles into a deterministic OAM gallery for pixel comparison.</summary>
    private static OamBuffer TileGallery(ushort first, int firstSize, ushort second, int secondSize)
    {
        var oam = new OamBuffer(); oam.BeginFrame();
        int slot = 0;
        Add(first, firstSize); Add(second, secondSize);
        oam.FinalizeFrame(); return oam;
        void Add(ushort start, int bytes)
        {
            for (int tile = 0; tile < bytes / RoomCharacterAtlasFormat.BytesPerTile; tile++, slot++)
            {
                int character = (start - SamusRenderingRomData.TileTransfers.TopDestinations.First) / 16 + tile;
                oam.AddOnScreenSpritePart(new SnesSpritemapXWord(0), new SnesSpritemapYByte(0),
                    SnesObjAttributeWord.Create(character, 4, 3),
                    new Position16<OamPixels>((ushort)(32 + slot % 8 * 16), (ushort)(32 + slot / 8 * 16)));
            }
        }
    }
}
