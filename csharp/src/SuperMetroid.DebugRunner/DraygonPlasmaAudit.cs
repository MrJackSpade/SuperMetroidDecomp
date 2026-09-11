using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static class DraygonPlasmaAudit
{
    /// <summary>
    /// Full runtime at the native release boundary: the retained shot hits before
    /// hurt AI, but an entry invincibility of one still excludes this entire call.
    /// Values come from native-xplasma-draygon-probe.h, not another managed path.
    /// </summary>
    public static int Run(string rom)
    {
        foreach (ushort entryTimer in new ushort[] { 0, 1 })
        {
            var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(0xda60);
            runtime.InitializeDebugGroundedSamus(64, 166, 8);
            runtime.Samus!.InputLocked = false;
            var body = runtime.Enemies.Draygon!.Body;
            body.XPosition = body.YPosition = 128;
            body.Health = 6000;
            body.InvincibilityTimer = entryTimer;
            body.FlashTimer = 12;
            body.AiHandlerBits = 2;
            body.SpritemapPointer = 0xa3bb;
            body.Properties = 0;
            body.ExtraProperties = 4;
            runtime.Enemies.Draygon.SwoopYAcceleration = 0;
            var shot = runtime.Projectiles.Slots[0];
            shot.Type = SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, true);
            shot.Damage = 450;
            shot.XPosition = shot.YPosition = 128;
            shot.XRadius = shot.YRadius = 4;
            shot.InstructionPointer = 0x9000;
            shot.InstructionTimer = 100;
            runtime.StepFrame(0);
            ushort health = entryTimer == 0 ? (ushort)5550 : (ushort)6000;
            ushort timer = entryTimer == 0 ? (ushort)16 : (ushort)0;
            if (body.Health != health || body.InvincibilityTimer != timer || body.FlashTimer != 11 ||
                runtime.Enemies.Draygon.SwoopYAcceleration != (entryTimer == 0 ? 8 : 0) ||
                body.XPosition != 128 || body.YPosition != 128)
                throw new InvalidDataException($"Draygon release (entry {entryTimer}): {body.Health}/{body.InvincibilityTimer}/{body.FlashTimer}, native {health}/{timer}/11.");
        }
        Console.WriteLine("Draygon full-runtime release and entry-invincibility gate pass.");
        return 0;
    }
}
