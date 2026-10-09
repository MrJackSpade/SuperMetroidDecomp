using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Projectile instruction bytecode, flare animation, draw helpers, and palette reads.
/// </summary>
public sealed partial class SamusProjectileSystem
{
    /// <summary>
    /// Advances one projectile's bytecode until it reaches a timed frame or deletes the slot.
    /// </summary>
    /// <param name="bus">The address space used for projectile instructions and frame metadata.</param>
    /// <param name="slot">The projectile slot whose timer, animation state, and instruction pointer are advanced.</param>
    /// <returns><see langword="true"/> if a delete instruction cleared the projectile; otherwise, <see langword="false"/>.</returns>
    internal bool RunProjectileInstructionHandler(
        ISnesAddressSpace bus,
        SamusProjectileSlot slot)
    {
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (slot.InstructionTimer != 0)
            return false;

        ushort pointer = slot.InstructionPointer;
        while (true)
        {
            ushort instructionOrTimer = SamusProjectileInstructionDefinitions.ReadWord(SamusProjectileRomData.Banks.Projectile | pointer);
            if ((instructionOrTimer & 0x8000) == 0)
            {
                slot.InstructionTimer = instructionOrTimer;
                slot.SpritemapPointer = FrameBindings?.Resolve(pointer) ??
                    SamusProjectileInstructionDefinitions.ReadWord(
                        SamusProjectileRomData.Banks.Projectile | AddWithinBank(pointer, 2));
                slot.XRadius = SamusProjectileRadiusDefinitions.ReadByte(
                    (int)new SnesAddress(
                        SamusProjectileRomData.Banks.ProjectileNumber,
                        unchecked((ushort)AddWithinBank(pointer, 4))));
                slot.YRadius = SamusProjectileRadiusDefinitions.ReadByte(
                    (int)new SnesAddress(
                        SamusProjectileRomData.Banks.ProjectileNumber,
                        unchecked((ushort)AddWithinBank(pointer, 5))));
                slot.AnimationFrame = SamusProjectileInstructionDefinitions.ReadWord(
                    SamusProjectileRomData.Banks.Projectile | AddWithinBank(pointer, 6));
                slot.InstructionPointer = unchecked((ushort)(pointer + 8));
                return false;
            }

            if (instructionOrTimer == SamusProjectileRomData.Instructions.Delete)
            {
                ClearProjectile(slot);
                return true;
            }

            if (instructionOrTimer == SamusProjectileRomData.Instructions.GoTo)
            {
                pointer = SamusProjectileInstructionDefinitions.ReadWord(
                    SamusProjectileRomData.Banks.Projectile | AddWithinBank(pointer, 2));
                continue;
            }

            throw new InvalidOperationException(
                $"Unsupported bank-$93 projectile instruction ${instructionOrTimer:X4} " +
                $"at $93:{pointer:X4}.");
        }
    }

    /// <summary>
    /// Clears a projectile slot and decrements the active projectile count without underflowing it.
    /// </summary>
    /// <param name="slot">The projectile slot being removed.</param>
    private void ClearProjectile(SamusProjectileSlot slot)
    {
        slot.ClearFields();
        ProjectileCounter = ProjectileCounter == 0
            ? (ushort)0
            : unchecked((ushort)(ProjectileCounter - 1));
    }

    /// <summary>
    /// Draws a projectile composition when its origin falls within the configured horizontal margin and visible Y range.
    /// </summary>
    /// <param name="bus">The address space associated with projectile rendering.</param>
    /// <param name="oam">The OAM buffer that receives visible projectile sprites.</param>
    /// <param name="slot">The projectile whose spritemap and world position are rendered.</param>
    /// <param name="layer1X">The layer-one camera X position subtracted from the projectile position.</param>
    /// <param name="layer1Y">The layer-one camera Y position subtracted from the projectile position.</param>
    /// <param name="horizontalMargin">Optional horizontal visibility margin around the 256-pixel screen.</param>
    /// <param name="compositions">The installed sprite catalog used to resolve and draw the projectile spritemap.</param>
    /// <exception cref="InvalidOperationException">The sprite composition catalog is unavailable.</exception>
    private static void DrawSlot(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y,
        int? horizontalMargin,
        Assets.ProjectileSpriteCatalog? compositions)
    {
        short screenX = unchecked((short)(slot.XPosition - layer1X));
        ushort screenY = unchecked((ushort)(slot.YPosition - layer1Y));
        if (horizontalMargin is int margin && (screenX < -margin || screenX >= 256 + margin) ||
            (screenY & 0xff00) != 0)
        {
            return;
        }

        (compositions ?? throw new InvalidOperationException(
            "Samus projectiles require installed sprite compositions."))
            .Draw(slot.SpritemapPointer, oam, unchecked((ushort)screenX), screenY);
    }

    /// <summary>
    /// Advances one charge-flare component according to its ROM-authored delay, restart, and rewind entries.
    /// </summary>
    /// <param name="component">The flare component whose frame and timer are updated.</param>
    private void AdvanceFlareComponent(int component)
    {
        // The assembly advances only when 16-bit DEC crosses zero into `$FFFF`. A timer
        // value of zero therefore survives one visible call; testing equality here would
        // make every ROM-authored delay one frame too short.
        _flareTimers[component] = unchecked((ushort)(_flareTimers[component] - 1));
        if ((_flareTimers[component] & 0x8000) == 0)
            return;

        ushort frame = unchecked((ushort)(_flareFrames[component] + 1));
        ushort delayList = ChargeFlareAnimationDefinitions.ReadWord(
            SamusProjectileRomData.Beams.ChargeFlareDelayListPointers + component * 2);
        byte delay = ChargeFlareAnimationDefinitions.ReadByte(
            (int)new SnesAddress(
                SamusProjectileRomData.Banks.MovementNumber,
                unchecked((ushort)(delayList + frame))));
        if (delay == ChargeFlareAnimationDefinitions.Restart)
        {
            frame = 0;
            delay = ChargeFlareAnimationDefinitions.ReadByte((int)new SnesAddress(
                SamusProjectileRomData.Banks.MovementNumber,
                delayList));
        }
        else if (delay == ChargeFlareAnimationDefinitions.Rewind)
        {
            byte rewind = ChargeFlareAnimationDefinitions.ReadByte(
                (int)new SnesAddress(
                    SamusProjectileRomData.Banks.MovementNumber,
                    unchecked((ushort)(delayList + frame + 1))));
            frame = unchecked((ushort)(frame - rewind));
            delay = ChargeFlareAnimationDefinitions.ReadByte(
                (int)new SnesAddress(
                    SamusProjectileRomData.Banks.MovementNumber,
                    unchecked((ushort)(delayList + frame))));
        }

        _flareFrames[component] = frame;
        _flareTimers[component] = delay;
    }

    /// <summary>
    /// Reads Samus's live shot and pose metadata, then draws a charge-flare component when the shot direction is valid.
    /// </summary>
    /// <param name="bus">The address space used to read Samus's shot, movement, pose, and facing state.</param>
    /// <param name="oam">The OAM buffer that receives the flare sprites.</param>
    /// <param name="samus">Samus's current gameplay state used to select direction and placement metadata.</param>
    /// <param name="layer1X">The camera X position used to convert the flare to screen coordinates.</param>
    /// <param name="layer1Y">The camera Y position used to convert the flare to screen coordinates.</param>
    /// <param name="component">The flare component whose current animation frame is drawn.</param>
    /// <param name="mode7Transform">Optional transform applied to Samus's center before pose offsets are added.</param>
    /// <param name="placement">The installed muzzle-placement catalog for running and direction variants.</param>
    /// <param name="compositions">The installed sprite catalog for charge-flare animation frames.</param>
    private void DrawFlareComponent(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y,
        int component,
        SamusMode7Transform? mode7Transform,
        Assets.ChargeFlarePlacementCatalog? placement,
        Assets.ChargeFlareSpriteCatalog? compositions)
    {
        byte direction = samus.ReadShotDirection(bus);
        if (direction is 0xff or 0x10 || (direction & 0xf0) != 0)
            return;

        DrawFlareComponentWithMetadata(oam, samus, layer1X, layer1Y, component,
            mode7Transform, placement, compositions, direction,
            samus.ReadMovementKind(bus) == SamusMovementType.Running,
            unchecked((byte)samus.ReadGraphicsYOffset(bus)), samus.IsFacingLeft(bus));
    }

    /// <summary>
    /// Resolves a charge-flare muzzle position and draws the selected frame using already-read pose and direction metadata.
    /// </summary>
    /// <param name="oam">The OAM buffer that receives the flare sprites.</param>
    /// <param name="samus">Samus's position, transformed for the current cinematic rendering mode when applicable.</param>
    /// <param name="layer1X">The camera X position subtracted from the muzzle location.</param>
    /// <param name="layer1Y">The camera Y position subtracted from the muzzle location.</param>
    /// <param name="component">The flare component whose animation frame is drawn.</param>
    /// <param name="mode7Transform">Optional transform applied to Samus's center before muzzle offsets.</param>
    /// <param name="placement">The installed catalog that resolves offsets for the movement and direction.</param>
    /// <param name="compositions">The installed catalog that resolves flare sprite compositions.</param>
    /// <param name="direction">The validated shot-direction byte read from Samus.</param>
    /// <param name="running">Whether Samus is in the running movement mode.</param>
    /// <param name="poseYOffset">The vertical adjustment selected by Samus's current pose.</param>
    /// <param name="facingLeft">Whether Samus faces left, selecting the corresponding frame-index range.</param>
    /// <exception cref="InvalidOperationException">A required placement or sprite-composition catalog is unavailable.</exception>
    private void DrawFlareComponentWithMetadata(
        OamBuffer oam, SamusState samus, ushort layer1X, ushort layer1Y, int component,
        SamusMode7Transform? mode7Transform, Assets.ChargeFlarePlacementCatalog? placement,
        Assets.ChargeFlareSpriteCatalog? compositions, byte direction,
        bool running, byte poseYOffset, bool facingLeft)
    {
        var visualOffset = (placement ?? throw new InvalidOperationException(
            "Charge flare requires installed placement definitions."))
            .Resolve(running, direction & 0x0f);
        short xOffset = visualOffset.X;
        short yOffset = visualOffset.Y;


        // `$90:BBE1` calls `$8B:8A52` under the same Ceres-status high bit used by the
        // body renderer. Only Samus's center is transformed; the pose-selected muzzle
        // offsets are added afterward. Retail then restores `$0AF6/$0AFA`, so this path
        // must never write the public physics coordinates merely to reuse the arithmetic.
        SamusMode7Point renderPoint = mode7Transform is { } transform
            ? transform.Transform(samus.XPosition, samus.YPosition)
            : new SamusMode7Point(samus.XPosition, samus.YPosition);
        ushort screenX = unchecked((ushort)(renderPoint.X + xOffset - layer1X));
        ushort screenY = unchecked((ushort)(renderPoint.Y + yOffset - poseYOffset - layer1Y));

        // `$90:BC98` clips only by the origin's Y high byte. The shared bank-$81 loader
        // deliberately allows individual entries to wrap, matching charge sparks near an
        // edge instead of applying the generic on-screen-origin parking rule.
        if ((screenY & 0xff00) != 0)
            return;


        ushort indexOffset = unchecked((ushort)(facingLeft
            ? component switch { 0 => 0, 1 => 0x2a, _ => 0x30 }
            : component switch { 0 => 0, 1 => 0x1e, _ => 0x24 }));
        ushort tableIndex = unchecked((ushort)(indexOffset + _flareFrames[component]));
        // A selector outside the installed catalog is not valid executable art.
        // The catalog rejects it loudly rather than reading adjacent ROM bytes.
        (compositions ?? throw new InvalidOperationException(
            "Charge flare requires installed sprite compositions."))
            .Draw(tableIndex, oam, screenX, screenY);
    }

    /// <summary>
    /// Resets all charge-flare component frame indices and delay timers to their initial values.
    /// </summary>
    private void ClearFlareAnimationState()
    {
        Array.Clear(_flareFrames);
        Array.Clear(_flareTimers);
    }

    /// <summary>
    /// Loads the normal suit palette selected by equipped items into CGRAM and returns the native table offset.
    /// </summary>
    /// <param name="bus">The address space associated with the current projectile-system update.</param>
    /// <param name="cgram">The color memory receiving the displayed BGR555 palette words.</param>
    /// <param name="samus">Samus's equipment and suit-color state used to select the palette.</param>
    /// <returns>The palette-table offset retained for diagnostics.</returns>
    private static ushort LoadNormalSuitPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus)
    {
        // Preserve the native pointer as the diagnostic result while the installed
        // color source replaces only the sixteen displayed BGR555 words.
        return SamusNormalSuitPalette.Load(cgram, samus.EquippedItems, samus.SuitColors);
    }

    /// <summary>
    /// Resolves the native suit-palette table offset for an equipment bitfield.
    /// </summary>
    /// <param name="equippedItems">The current Samus equipment flags.</param>
    /// <returns>The byte offset of the selected suit-palette entry.</returns>
    private static ushort GetSuitPaletteOffset(ushort equippedItems) =>
        equippedItems.GetSuitPaletteTableOffset();

}
