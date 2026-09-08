using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Room-local controller reproduction from the player's Halfie Climb slot.</summary>
    private static void VerifyAnchoredGrapple()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-376-anchored-grapple", 0);
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        Console.WriteLine($"room={game.GameplayActiveRoomPointer:X4} pos={samus.XPosition},{samus.YPosition} pose={samus.Pose:X2} grapple={samus.Grapple.Phase}");
        Directory.CreateDirectory("csharp/test-temp/issue-376-anchored-grapple");
        PngWriter.WriteRgba("csharp/test-temp/issue-376-anchored-grapple/start.png", 256, 224,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        int grabbedFrames = 0;
        for (int frame = 0; frame < 160; frame++)
        {
            SnesButton input = frame switch
            {
                0 => SnesButton.Select,
                < 20 => SnesButton.Right,
                < 40 => SnesButton.Right | SnesButton.A,
                _ => SnesButton.Right | SnesButton.A | SnesButton.X,
            };
            if (samus.Grapple.Phase == GrapplePhase.ConnectedSwinging)
                input = SnesButton.X | SnesButton.Up | SnesButton.Right;
            if (samus.Grapple.Phase == GrapplePhase.WallGrab || grabbedFrames > 0)
            {
                grabbedFrames++;
                // Jump while still holding Shoot must NOT detach: native C814 waits for
                // Shoot release. A fresh Jump on the following frame enters C832's check.
                input = grabbedFrames <= 5 ? SnesButton.X | SnesButton.A
                    : grabbedFrames == 6 ? SnesButton.None : SnesButton.A | SnesButton.Left;
            }
            game.Step((ushort)input);
            if (frame is 43 or 50 || grabbedFrames == 1)
            {
                Console.WriteLine($"GRAPHICS frame={frame} phase={samus.Grapple.Phase} pose={samus.Pose:X2} anim={samus.AnimationFrame} top={samus.TopSpritemapIndex:X4} bottom={samus.BottomSpritemapIndex:X4} topDMA={samus.TileTransfers.TopDefinitionAddress:X6} bottomDMA={samus.TileTransfers.BottomDefinitionAddress:X6}");
                PngWriter.WriteRgba($"csharp/test-temp/issue-376-anchored-grapple/graphics-{frame}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            }
            if (grabbedFrames is >= 1 and <= 7)
            {
                // Independently pinned bank-$92 records: verify both OAM selections and
                // actual NMI-uploaded bytes, not just the movement phase or cached pose.
                Check(samus.AnimationFrame == 0 &&
                    samus.TopSpritemapIndex == GrappleWallGrabGraphicsReference.TopSpritemap &&
                    samus.BottomSpritemapIndex == GrappleWallGrabGraphicsReference.BottomSpritemap,
                    "Wall-grab ready sprite differs from native B8 frame zero.");
                Check(samus.TileTransfers.TopDefinitionAddress == GrappleWallGrabGraphicsReference.TopDma &&
                    samus.TileTransfers.BottomDefinitionAddress == GrappleWallGrabGraphicsReference.BottomDma,
                    "Wall-grab tile definitions retain hanging/swing graphics.");
                VerifyGrappleWallGrabDma(loaded.AddressSpace, runtime.Vram,
                    GrappleWallGrabGraphicsReference.TopDma, SamusRenderingRomData.TileTransfers.TopDestinations);
                VerifyGrappleWallGrabDma(loaded.AddressSpace, runtime.Vram,
                    GrappleWallGrabGraphicsReference.BottomDma, SamusRenderingRomData.TileTransfers.BottomDestinations);
            }
            if (grabbedFrames is >= 1 and <= 5)
                Check(samus.Grapple.Phase == GrapplePhase.WallGrab && samus.XPosition == 175 && samus.YPosition == 552,
                    "Holding Shoot+Jump must retain the native wall-grab pose and position.");
            if (grabbedFrames == 6)
                Check(samus.Grapple.Phase == GrapplePhase.WallGrabRelease && samus.Grapple.WallJumpTimer == 30,
                    "Shoot release must open the native 30-frame grace window.");
            if (grabbedFrames == 7)
                Check(runtime.LastGrappleMovement is { WallJumpQueued: true, WallProbeCollided: true },
                    "Fresh Jump must accept the real wall contact in the player's room.");
            if (grabbedFrames == 8)
                Check(runtime.LastGrappleMovement is { WallJumpStarted: true } &&
                    samus.Pose == SamusPoseIds.WallJumpLeftPose && samus.Grapple.Phase == GrapplePhase.Inactive,
                    "Accepted grapple wall jump must detach and select the leftward wall-jump pose.");
            if (grabbedFrames is 1 or 8 or 21)
                PngWriter.WriteRgba($"csharp/test-temp/issue-376-anchored-grapple/grab-step-{grabbedFrames}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            if (grabbedFrames == 21)
            {
                Check(samus.XPosition == 158 && samus.YPosition == 489,
                    $"Wall jump did not propel Samus upward and away: ({samus.XPosition},{samus.YPosition}).");
                Console.WriteLine("PASS slot 0: retract into wall grab (175,552), retain while Shoot held, release then Jump, detach and rise to (158,489).");
                return;
            }
        }
        throw new InvalidDataException("Player-room sequence never reached a completed grapple wall jump.");
    }

    private static void VerifyGrappleWallGrabDma(SuperMetroid.Core.Hardware.SuperMetroidAddressSpace bus,
        SuperMetroid.Core.Hardware.SnesVram vram, int definition,
        SamusRenderingRomData.TileTransfers.SplitVramDestinations destinations)
    {
        int source = bus.ReadByte(definition) | bus.ReadByte(definition + 1) << 8 | bus.ReadByte(definition + 2) << 16;
        int firstSize = bus.ReadByte(definition + 3) | bus.ReadByte(definition + 4) << 8;
        int secondSize = bus.ReadByte(definition + 5) | bus.ReadByte(definition + 6) << 8;
        for (int i = 0; i < firstSize; i++)
            Check(vram.ReadByte(destinations.First * 2 + i) == bus.ReadByte(source + i), "Wall-grab first DMA bytes differ from ROM.");
        for (int i = 0; i < secondSize; i++)
            Check(vram.ReadByte(destinations.Second * 2 + i) == bus.ReadByte(source + firstSize + i), "Wall-grab second DMA bytes differ from ROM.");
    }
}

/// <summary>Independent reference records from pinned bank_92.asm for wall-grab pose B8.</summary>
internal static class GrappleWallGrabGraphicsReference
{
    /// <summary>$92:86EB, top spritemap index for B8 frame zero.</summary>
    internal const ushort TopSpritemap = 0x032f;
    /// <summary>$92:8BF7, bottom spritemap index for B8 frame zero.</summary>
    internal const ushort BottomSpritemap = 0x05b5;
    /// <summary>$92:CD22, B8 top graphics transfer, set 1 entry C.</summary>
    internal const int TopDma = 0x92cd22;
    /// <summary>$92:D254, B8 bottom graphics transfer, set 0 entry 1A.</summary>
    internal const int BottomDma = 0x92d254;
}
