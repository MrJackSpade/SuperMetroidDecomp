using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Traces ordinary moving-jump input through the pose and grapple launch seams.</summary>
internal static class GrappleJumpAimAudit
{
    public static int Run(string rom)
    {
        foreach (bool left in new[] { false, true })
        foreach (bool aimUp in new[] { false, true })
        foreach (int fireDelay in new[] { 0, 1, 2, 4, 8 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.VramWrites.DrainTo(runtime.Vram, bus);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 672, 448);
            // A wide, flat synthetic floor leaves the jump trajectory independent
            // of retail obstacles. All pose and grapple tables remain retail data.
            var level = runtime.LevelData!;
            for (int i = 0; i < level.WidthInBlocks * level.HeightInBlocks; i++)
                level.SetForegroundEntry(i, (ushort)(i / level.WidthInBlocks == 40 ? (int)RoomCollisionType.SolidBlock << 12 : 0));
            var samus = runtime.Samus!;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.XPosition = 800;
            samus.YPosition = 619;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.InputLocked = false;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
            samus.SelectedHudItem = SamusHudRomData.GrappleSelectedItem;
            runtime.Camera!.SetPosition(672, 448);
            ushort move = (ushort)(left ? SnesButton.Left : SnesButton.Right);
            ushort aim = aimUp ? runtime.ControllerBindings.AimUp : (ushort)0;
            for (int frame = 0; frame < 10; frame++) runtime.StepFrame((ushort)(move | aim));
            Console.WriteLine($"CASE left={left} aimUp={aimUp} fireDelay={fireDelay}");
            for (int frame = 0; frame < 14; frame++)
            {
                ushort input = (ushort)(move | aim | runtime.ControllerBindings.Jump |
                    (frame >= fireDelay ? runtime.ControllerBindings.Shoot : 0));
                byte before = samus.Pose;
                runtime.StepFrame(input);
                Console.WriteLine($"{frame}: pose {before:X2}->{samus.Pose:X2} shot={samus.ReadShotDirection(bus)} grapple={samus.Grapple.Phase} dir={samus.Grapple.FireDirection} velocity=({samus.Grapple.ExtensionXVelocity},{samus.Grapple.ExtensionYVelocity})");
                if (fireDelay != 0 && frame == fireDelay + 2 &&
                    samus.Grapple.FireDirection != samus.ReadShotDirection(bus))
                    throw new InvalidDataException("Grapple retained horizontal launch after the aimed jump pose became current; native restart window should re-aim it.");
                if (fireDelay != 0 && frame == fireDelay + 3)
                {
                    var cannon = runtime.LastArmCannonDraw;
                    if (!cannon.SpriteWritten || cannon.DirectionSelector != samus.Grapple.FireDirection ||
                        (aimUp ? samus.Grapple.EndpointYOffsetFixed >= 0 : samus.Grapple.EndpointYOffsetFixed != 0) ||
                        (left ? samus.Grapple.EndpointXOffsetFixed >= 0 : samus.Grapple.EndpointXOffsetFixed <= 0))
                        throw new InvalidDataException("Rendered aimed cannon and moving grapple endpoint disagree.");
                }
            }
        }
        return 0;
    }
}
