using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>
/// Finite acceptance checks for source-identified Samus drawing and upload owners.
/// This is artwork verification, never a gameplay search for remaining ROM reads.
/// </summary>
internal static partial class InstalledSamusArtworkTests
{
    internal static void Run(InstalledSamusArtworkFixture fixture,
        D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var check = new ArtworkPixelCheck(device, renderer);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        int transfers = CheckBodyTransfers(fixture, memory, check);
        int poses = CheckPoseDrawing(fixture, memory, check);
        int queued = CheckQueuedTransfers(fixture, memory, check);
        Require(check.ChangedPixels > 0, "The artwork acceptance run changed no visible pixels.");
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {fixture.EditedPngCount} actual PNG overrides; " +
            $"{transfers} split body transfers, {poses} pose draws, {queued} cannon/death uploads; " +
            $"{check.Comparisons} exact software/GPU comparisons, {check.ChangedPixels} recolored pixels.");
    }

    private static int CheckPoseDrawing(InstalledSamusArtworkFixture fixture,
        SuperMetroidAddressSpace memory, ArtworkPixelCheck check)
    {
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            var stock = new SamusState { Pose = (byte)pose, XPosition = 128, YPosition = 128, Health = 99 };
            var edited = new SamusState { Pose = stock.Pose, XPosition = stock.XPosition,
                YPosition = stock.YPosition, Health = stock.Health };
            stock.RefreshCollisionRadii(memory); edited.RefreshCollisionRadii(memory);
            stock.TileTransfers.BindArtwork(fixture.Stock);
            edited.TileTransfers.BindArtwork(fixture.Edited);
            string before = PhysicalState(stock);
            var a = new SnesVram(); var b = new SnesVram();
            var oa = new OamBuffer(); var ob = new OamBuffer();
            oa.BeginFrame(); ob.BeginFrame();
            bool visible = stock.Draw(memory, oa, 0, 0);
            Require(visible == edited.Draw(memory, ob, 0, 0), $"pose {pose:X2}: visibility changed");
            stock.TileTransfers.TransferToVram(memory, a);
            edited.TileTransfers.TransferToVram(memory, b);
            oa.FinalizeFrame(); ob.FinalizeFrame();
            Require(oa.LowTable.SequenceEqual(ob.LowTable) && oa.HighTable.SequenceEqual(ob.HighTable),
                $"pose {pose:X2}: pixel-only edit changed OAM placement");
            Require(before == PhysicalState(stock) && before == PhysicalState(edited),
                $"pose {pose:X2}: drawing changed physics, damage or animation state");
            check.Pair(a, b, oa, $"pose {pose:X2}");
        }
        return SamusBodyArtworkCatalog.PoseCount;
    }

    // Deliberately excludes derived spritemap indices and DMA flags. Those are the
    // rendering outputs; pose timing, collision radii and fixed-point motion are not.
    private static string PhysicalState(SamusState samus) => string.Join("|",
        samus.Pose, samus.XPosition, samus.YPosition, samus.Health,
        samus.AnimationFrame, samus.AnimationFrameTimer, samus.AnimationFrameBuffer,
        samus.InvincibilityTimer, samus.KnockbackTimer, samus.EquippedItems,
        samus.EquippedBeams, samus.Kinematics.XSubposition, samus.Kinematics.YSubposition,
        samus.HorizontalSpeed.BaseSpeed, samus.HorizontalSpeed.BaseSubspeed,
        samus.HorizontalSpeed.ExtraRunSpeed, samus.HorizontalSpeed.ExtraRunSubspeed,
        samus.Kinematics.YSpeed, samus.Kinematics.YSubspeed,
        samus.Kinematics.XRadius, samus.Kinematics.YRadius);

    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
