using SuperMetroid.Core.Runtime;

/// <summary>
/// Writes a local, private seed for the original-CPU room movement probe. This is
/// an investigation format, not a save state: enemies, PLM execution, and live FX
/// animation are not restored by the native consumer. Only use a collision-only
/// interval for comparison. Do not distribute generated cartridge-derived seeds.
/// </summary>
internal static class RoomMovementSeedExporter
{
    public static void Write(SuperMetroidRuntime runtime, string path, bool includeScrollOwners = false)
    {
        var samus = runtime.Samus ?? throw new InvalidDataException("Missing Samus seed.");
        var level = runtime.LevelData ?? throw new InvalidDataException("Missing room seed.");
        using var output = new BinaryWriter(File.Create(path));
        // Fixed ordered uint32 header shared with DiagnosticRoomRelease. Positions
        // and velocities retain all fractional bits; numeric words are little-endian.
        uint[] words = [includeScrollOwners ? 0x32564f4du : 0x31564f4du, runtime.ActiveRoom!.Pointer,
            (uint)level.WidthInBlocks, (uint)level.HeightInBlocks,
            samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
            samus.Kinematics.XRadius, samus.Kinematics.YRadius,
            samus.HorizontalSpeed.BaseFixed,
            ((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed,
            samus.HorizontalSpeed.AccelerationMode, samus.HorizontalSpeed.HasRunningMomentum ? 1u : 0u,
            samus.HorizontalSpeed.SpeedDivisor, samus.HorizontalSpeed.DecelerationMultiplier,
            samus.EquippedItems, samus.EquippedBeams, (uint)samus.LiquidPhysics.FxType,
            samus.LiquidPhysics.FxYPosition, samus.LiquidPhysics.LavaAcidYPosition,
            samus.LiquidPhysics.LiquidOptions, samus.LiquidPhysics.LiquidPhysicsType,
            samus.AnimationFrame, samus.AnimationFrameTimer, samus.AnimationFrameBuffer,
            samus.Kinematics.VerticalSpeedFixed, samus.Kinematics.YDirection,
            ((uint)samus.HorizontalSpeed.TotalSpeed << 16) | samus.HorizontalSpeed.TotalSubspeed,
            unchecked((uint)samus.Kinematics.ExtraXFixed), unchecked((uint)samus.Kinematics.ExtraYFixed),
            samus.Kinematics.YAcceleration, samus.Kinematics.YSubacceleration];
        foreach (uint word in words) output.Write(word);
        for (int block = 0; block < level.ForegroundEntries.Length; block++)
        {
            output.Write(level.ForegroundEntries.Span[block]);
            output.Write(level.BehaviorBytes.Span[block]);
        }
        if (includeScrollOwners)
        {
            // Collision-only owner projection. The native consumer does not execute
            // PLM programs or camera scroll updates, and must not claim that it does.
            var owners = runtime.Plms.ScrollPlms;
            output.Write(owners.Count);
            foreach (var owner in owners)
            {
                output.Write(owner.BlockIndex);
                output.Write(owner.Triggered ? 0x8000u : 0u);
            }
            output.Write((uint)samus.AutoJumpTimer);
            output.Write((uint)samus.PreviousDrawHeldInput);
            output.Write((uint)samus.PreviousDrawNewInput);
            output.Write(samus.AutoJumpInputPending ? 1u : 0u);
        }
    }
}
