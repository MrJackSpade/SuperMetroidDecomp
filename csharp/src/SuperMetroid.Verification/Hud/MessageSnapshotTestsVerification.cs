using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hud;

/// <summary>Checks gameplay message snapshots, reveal geometry and frame ownership.</summary>
internal static class MessageSnapshotTestsVerification
{
    /// <summary>Captures every retail message's changing overlay and checks the image shown, retained packets and malformed layers.</summary>
    internal static void VerifyMessageSnapshots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var vram = new SnesVram();
        byte[] graphics = new byte[SnesVram.ByteCount];
        new Random(32107).NextBytes(graphics);
        vram.LoadBytes(0, graphics);
        MessageBoxSheet messages = RepositoryInstallation.Installation.LoadRoomBlockSheets().MessageBoxes.Values.First();
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, RgbColor.FromWord((ushort)(i * 101 & 32767)));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureFrontend(vram, cgram, oam, 3);
        int samples = 0;
        foreach (GameplayMessageId id in Enum.GetValues<GameplayMessageId>())
        {
            if (id == GameplayMessageId.None) continue;
            var state = CreateGameplayMessageFixture();
            AssertTrue(MessageBoxLayer.Capture(state, messages) is null, "inactive message has no overlay");
            state.Begin(id, 0);
            int previousRadius = -1;
            bool previousYes = true;
            bool toggledSave = false, releasedSave = false;
            DisplayPacket? held = null;
            RgbaPixel[]? heldPixels = null;
            for (int tick = 0; tick < 600 && state.IsActive; tick++)
            {
                if (previousRadius != state.RadiusPixels || previousYes != state.ConfirmationSelectionYes)
                {
                    previousRadius = state.RadiusPixels;
                    previousYes = state.ConfirmationSelectionYes;
                    MessageBoxLayer? layer = MessageBoxLayer.Capture(state, messages);
                    DisplayLayer[] layers = layer is null ? [] : [layer];
                    var packet = new DisplayPacket(new(++samples, 1, (ushort)tick),
                        new LayeredScene(memory, layers, 15));
                    if (layer is not null)
                    {
                        AssertTrue(ReferenceEquals(layer.Picture, messages.Picture(state.Shown ?? throw new InvalidOperationException($"Message {id} shows nothing."))),
                            $"captured message {id} at radius {state.RadiusPixels} shows the image of the form shown");
                        if (held is null) { held = packet; heldPixels = DisplayRendering.Render(packet); }
                    }
                }
                ControllerButton input = state.Phase == GameplayMessageBoxPhase.AwaitingInput
                    ? tick % 3 == 0 ? ControllerButton.Right : tick % 3 == 2 ? ControllerButton.A : 0
                    : 0;
                if (id == GameplayMessageIds.SaveConfirmation && state.Phase == GameplayMessageBoxPhase.AwaitingInput)
                {
                    if (!toggledSave) { input = ControllerButton.Right; toggledSave = true; }
                    else if (!releasedSave) { input = 0; releasedSave = true; }
                    else input = ControllerButton.A;
                }
                state.Step(input);
            }
            AssertTrue(!state.IsActive, $"message {id} completes reveal/close fixture");
            // Reuse the same live owner after closing, replacing its message and radius.
            state.Begin(GameplayMessageIds.SaveCompleted, 0);
            AssertTrue(held is not null && heldPixels.SequenceEqual(DisplayRendering.Render(held)),
                "retained message survives closing and reuse of the source owner");
        }

        // An image the sheet holds of each height takes every reveal radius.
        foreach (IGrouping<ushort, ShownMessage> height in GameplayMessageArtwork.Forms().GroupBy(form => messages.Picture(form).Size.Y))
        {
            for (int radius = 0; radius <= GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels; radius++)
            {
                var layer = MessageBoxLayer.Of(height.First(), radius, messages);
                _ = new DisplayPacket(new(++samples, 1, 0), new LayeredScene(memory, [layer], 15));
            }
        }
        SpriteSheetFrame wrongSize = RepositoryInstallation.Installation.LoadRoomBlockSheets().Bg3Pictures.Values.First()
            .Sheet.Frame(SuperMetroid.Core.Fx.Layer3Plane.ClearedPicture);
        AssertThrows<ArgumentException>(() => new MessageBoxLayer(wrongSize, 1), "an image of no whole message rows rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => MessageBoxLayer.Of(GameplayMessageArtwork.Forms().First(), 25, messages),
            "invalid reveal radius rejected");
        Console.WriteLine($"  Message snapshots: {samples} retail/glyph-window captures and ownership checks agree.");
    }
}
