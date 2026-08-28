using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Real-cartridge projectile checks shared by the compact Ceres room capture entry point.
/// </summary>
/// <remarks>
/// These checks deliberately live outside the already-large top-level runner. They exercise
/// the same runtime as the desktop Playable tab, but they remain diagnostic assertions rather
/// than alternate gameplay logic: every tile, pose, collision, and bytecode record comes from
/// the mounted private ROM.
/// </remarks>
internal static class CeresProjectileAudit
{
    private const int PowerBeamTilePointerAddress = 0x90c3b1;
    private const int PowerBeamTileBank = 0x9a0000;
    private const int PowerBeamVramByteAddress = 0x6300 * 2;
    private const int PowerBeamTileByteCount = 0x100;

    /// <summary>
    /// Proves the live power-beam tile allocation still matches its ROM source after the
    /// full elevator arrival and more than one hundred Samus graphics transfers.
    /// </summary>
    internal static void AssertPowerBeamGraphics(
        SuperMetroidAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(runtime);

        // The standard $2E00-byte OBJ upload covers VRAM $6000-$76FF and therefore also
        // covers the beam's much smaller $6300-$637F allocation. Room setup must append
        // $90:AC8D's beam DMA after that broad upload. Comparing every physical byte here,
        // after 132 Samus animation/NMI passes, also catches any later body DMA that grows
        // past its intended range and turns legal tile-$30 art into Samus fragments.
        ushort tilePointer = RomDataReader.ReadWordFixedBank(
            bus,
            PowerBeamTilePointerAddress);
        for (int beamByte = 0; beamByte < PowerBeamTileByteCount; beamByte++)
        {
            byte expected = bus.ReadByte(
                PowerBeamTileBank | unchecked((ushort)(tilePointer + beamByte)));
            byte actual = runtime.Vram.ReadByte(PowerBeamVramByteAddress + beamByte);
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Power-beam VRAM diverged at byte ${beamByte:X2}: " +
                    $"expected ${expected:X2}, got ${actual:X2}.");
            }
        }
    }

    /// <summary>
    /// Fires one power beam into real Ceres terrain and requires its complete traveling,
    /// impact-animation, delete-opcode, and following-frame publication lifecycle.
    /// </summary>
    internal static void RunPowerBeamLifetime(SuperMetroidRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        // A successful allocation alone is not sufficient: the visible failure occurred
        // when a traveling record became an impact record. Moving briefly first installs
        // an ordinary right-facing pose without host-writing a shot-direction byte, so the
        // muzzle origin and collision route continue to come from cartridge tables.
        for (int movementFrame = 0; movementFrame < 12; movementFrame++)
            runtime.StepFrame((ushort)SnesButton.Right);
        for (int releaseFrame = 0; releaseFrame < 8; releaseFrame++)
            runtime.StepFrame(0);

        runtime.StepFrame((ushort)SnesButton.X);
        if (runtime.Projectiles.LastFrameResult.FiredSlot is not int slotIndex ||
            runtime.Projectiles.ProjectileCounter != 1)
        {
            throw new InvalidOperationException(
                "Ceres power-beam lifetime probe did not allocate exactly one projectile.");
        }

        bool impacted = false;
        bool deleted = false;
        for (int lifetimeFrame = 0; lifetimeFrame < 120; lifetimeFrame++)
        {
            runtime.StepFrame(0);
            SamusProjectileFrameResult frame = runtime.Projectiles.LastFrameResult;
            impacted |= frame.CollisionStartedExplosion;
            deleted |= frame.ProjectileDeleted;

            // All drawable projectile spritemaps live in the upper half of bank $93. A
            // lower pointer is strong evidence that data or code is being misread as art;
            // that was the exact signature of the bad `$83FF` indirection.
            SamusProjectileSlot slot = runtime.Projectiles.Slots[slotIndex];
            if (slot.IsActive && slot.SpritemapPointer != 0 &&
                (slot.SpritemapPointer & 0x8000) == 0)
            {
                throw new InvalidDataException(
                    $"Power-beam slot {slotIndex} selected non-spritemap " +
                    $"bank-$93 pointer ${slot.SpritemapPointer:X4}.");
            }

            if (runtime.Projectiles.ProjectileCounter == 0)
                break;
        }

        if (!impacted || !deleted ||
            runtime.Projectiles.ProjectileCounter != 0 ||
            runtime.Projectiles.Slots[slotIndex].IsActive)
        {
            throw new InvalidOperationException(
                $"Ceres power beam did not complete impact/deletion: " +
                $"impact={impacted}, delete={deleted}, " +
                $"count={runtime.Projectiles.ProjectileCounter}, " +
                $"active={runtime.Projectiles.Slots[slotIndex].IsActive}.");
        }

        // The software PPU displays OAM published at the following accepted NMI. Advance
        // one additional complete frame and prove the deleted logical slot cannot leak back
        // into a new main-loop OAM image.
        runtime.StepFrame(0);
        if (runtime.Projectiles.ProjectileCounter != 0 ||
            runtime.Projectiles.Slots[slotIndex].IsActive)
        {
            throw new InvalidOperationException(
                "Deleted Ceres power beam reappeared on the post-delete publication frame.");
        }

        Console.WriteLine(
            $"Ceres power-beam lifetime probe: slot {slotIndex}, " +
            $"impact={impacted}, delete={deleted}.");
    }
}
