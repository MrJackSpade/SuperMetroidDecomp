using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;
using System.Collections;

/// <summary>Verification access to <see cref="AreaAnimatedTileObjectDefinitions"/> members production does not use.</summary>
internal static class AreaAnimatedTileObjectDefinitionsAccess
{
    extension(AreaAnimatedTileObjectDefinitions)
    {
        /// <summary>Calculates a native list identity as AC76+20h*area, for indices0..7.</summary>
        /// <remarks>Each eight-word animation list follows an eight-word palette list,
        /// producing the native32-byte area stride. All eight original pointer words
        /// are independently verified; invalid indices reject before arithmetic.</remarks>
        internal static ushort NativeListPointer(int nativeAreaIndex)
        {
            AreaAnimatedTileObjectDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            return (ushort)(0xac76 + 0x20 * nativeAreaIndex);
        }

        /// <summary>
        /// Returns one object pointer from any native row, including the non-retail eighth
        /// row retained solely for a complete immutable-source audit.
        /// </summary>
        internal static ushort NativeObjectPointer(int nativeAreaIndex, int bit)
        {
            AreaAnimatedTileObjectDefinitions.ValidateNativeAreaIndex(nativeAreaIndex);
            if ((uint)bit >= AreaAnimatedTileObjectDefinitions.ObjectsPerArea)
            {
                throw new ArgumentOutOfRangeException(
                    "bit", bit, $"Animated-tile bit must be 0..{AreaAnimatedTileObjectDefinitions.ObjectsPerArea - 1}.");
            }

            return ((ushort)(PrivateState.InvokeStatic(typeof(AreaAnimatedTileObjectDefinitions), "SelectObject", (int)(nativeAreaIndex), (int)(bit)))!);
        }

        internal static void ValidateNativeAreaIndex(int nativeAreaIndex)
        {
            if ((uint)nativeAreaIndex >= AreaAnimatedTileObjectDefinitions.NativeAreaCount)
            {
                throw new ArgumentOutOfRangeException(
                    "nativeAreaIndex", nativeAreaIndex,
                    $"Native animated-tile area index must be 0..{AreaAnimatedTileObjectDefinitions.NativeAreaCount - 1}.");
            }
        }
    }
}

/// <summary>Verification access to <see cref="BabyMetroidCutsceneState"/> members production does not use.</summary>
internal static class BabyMetroidCutsceneStateAccess
{
    extension(BabyMetroidCutsceneState self)
    {
        /// <summary>True when enemy property `$0100` suppresses the Baby's spritemap.</summary>
        internal bool IsInvisible => self.Properties.HasAny(EnemyProperties.Invisible);
    }
}

/// <summary>Verification access to <see cref="Bank80SystemState"/> members production does not use.</summary>
internal static class Bank80SystemStateAccess
{
    extension(Bank80SystemState)
    {
        /// <summary>
        /// Unsigned <c>16 x 16 -> 32</c> multiplication from <c>$80:82D6</c>.
        /// The widening casts must happen before multiplication or C# would discard the high
        /// word before returning it.
        /// </summary>
        internal static uint Multiply16By16(ushort left, ushort right) => (uint)left * right;
    }

    private static (int ByteIndex, byte BitMask) RoomChozoBit(int bitIndex) =>
        ((int, byte))PrivateState.InvokeStatic(typeof(Bank80SystemState), "ResolvePersistentRoomBit", bitIndex,
            Bank80SystemState.RoomChozoBitByteCount, "Chozo-room bit index must fit the native 64-byte table.")!;

    extension(Bank80SystemState self)
    {
        /// <summary>Whether the room's Chozo-statue bit is set in the native 64-byte table.</summary>
        internal bool HasRoomChozoBit(int bitIndex)
        {
            (int byteIndex, byte bitMask) = RoomChozoBit(bitIndex);
            return (PrivateState.Field<byte[]>(self, "_roomChozoBits")[byteIndex] & bitMask) != 0;
        }

        /// <summary>Sets the room's Chozo-statue bit in the native 64-byte table.</summary>
        internal void SetRoomChozoBit(int bitIndex)
        {
            (int byteIndex, byte bitMask) = RoomChozoBit(bitIndex);
            PrivateState.Field<byte[]>(self, "_roomChozoBits")[byteIndex] |= bitMask;
        }

        /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
        internal byte GetExploredMapByteRaw(AreaId areaIndex, int byteIndex) =>
            self.GetExploredMapByteRaw(AreaIds.ToIndex(areaIndex), byteIndex);
    }
}

/// <summary>Verification access to <see cref="BrinstarBlueSporePaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class BrinstarBlueSporePaletteFxProgramDefinitionAccess
{
    extension(BrinstarBlueSporePaletteFxProgramDefinition self)
    {
        /// <summary>Native palette-FX definitions $8D:F775 (standard) and $8D:F779 (Spore Spawn).</summary>
        internal ushort DefinitionPointer => (ushort)(0xf775 + 4 * (int)self.Owner);
    }
}

/// <summary>Verification access to <see cref="CeresBabyInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CeresBabyInstructionProgramDefinitionsAccess
{
    extension(CeresBabyInstructionProgramDefinitions)
    {
        internal static bool IsCompiledSpritemapByte(int address)
        {
            if ((address & 0xff0000) != 0xa60000)
                return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount; index++)
            {
                ushort operand = CeresBabyInstructionProgramDefinitions.SpritemapOperandAddress(index);
                if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                    return true;
            }
            return false;
        }

        internal static bool IsCompiledPaletteByte(int address)
        {
            if ((address & 0xff0000) != 0xa60000)
                return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < CeresBabyInstructionProgramDefinitions.PaletteOperandCount; index++)
            {
                ushort operand = CeresBabyInstructionProgramDefinitions.PaletteOperandAddress(index);
                if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                    return true;
            }
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="CeresDoorInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CeresDoorInstructionProgramDefinitionsAccess
{
    extension(CeresDoorInstructionProgramDefinitions)
    {
        internal static ushort PresentationWordFrame(int index) => (((ushort Address, ushort Frame))(PrivateState.InvokeStatic(typeof(CeresDoorInstructionProgramDefinitions), "PresentationWord", (int)(index)))!).Frame;
    }
}

/// <summary>Verification access to <see cref="CeresEscapeVramTransferDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeVramTransferDefinitionsAccess
{
    extension(CeresEscapeVramTransferDefinitions)
    {
        internal static bool IsDescriptorByteAddress(int address)
        {
            if ((address & 0xff0000) != 0xa60000)
                return false;
            ushort offset = unchecked((ushort)address);
            foreach (CeresEscapeVramTransferDefinition record in PrivateState.StaticField<IReadOnlyList<CeresEscapeVramTransferDefinition>>(typeof(CeresEscapeVramTransferDefinitions), "Records"))
                if (offset >= record.Pointer && offset < record.Pointer + 7)
                    return true;
            return CeresEscapeVramTransferDefinitions.IsTerminator(offset) ||
                CeresEscapeVramTransferDefinitions.IsTerminator(unchecked((ushort)(offset - 1)));
        }
    }
}

/// <summary>Verification access to <see cref="CeresMode7TransferDefinitions.TileSequence"/> members production does not use.</summary>
internal static class CeresMode7TransferDefinitionsTileSequenceAccess
{
    extension(CeresMode7TransferDefinitions.TileSequence self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsAccess
{
    extension(CeresSteamCollisionDefinitions)
    {
        internal static IEnumerable<ushort> HitboxPointers => PrivateState.StaticField<CollisionRecordRuns<CeresSteamCollisionHitbox>>(typeof(CeresSteamCollisionDefinitions), "Lists").Pointers;
    }
}

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsComponentSequenceAccess
{
    extension(CeresSteamCollisionDefinitions.ComponentSequence self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="CinematicGlowPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class CinematicGlowPaletteFxProgramDefinitionAccess
{
    extension(CinematicGlowPaletteFxProgramDefinition self)
    {
        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames =>
            CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount * self.FrameDuration;
    }
}

/// <summary>Verification access to <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CommonEnemyProjectileInstructionProgramDefinitionsAccess
{
    extension(CommonEnemyProjectileInstructionProgramDefinitions)
    {
        internal static ushort ReadMechanicsWord(ushort address)
        {
            if (CommonEnemyProjectileInstructionProgramDefinitions.TryReadMechanicsWord(address, out ushort value))
                return value;

            throw new InvalidDataException(
                $"Shared enemy-projectile instruction mechanics pointer $86:{address:X4} " +
                "is not compiled.");
        }
    }
}

/// <summary>Verification access to <see cref="CorpseRottingTableProcessor"/> members production does not use.</summary>
internal static class CorpseRottingTableProcessorAccess
{
    extension(CorpseRottingTableProcessor)
    {
        /// <summary>Reads one native table record without inventing a parallel host state.</summary>
        internal static CorpseRottingTableEntry ReadEntry(
            ISnesMutableMemory memory,
            int tableAddress,
            ushort entryCount,
            int entryIndex)
        {
            ArgumentNullException.ThrowIfNull(memory);
            if ((uint)entryIndex >= entryCount)
                throw new ArgumentOutOfRangeException(nameof(entryIndex));

            int entryAddress = checked(tableAddress + entryIndex * PrivateState.StaticField<int>(typeof(CorpseRottingTableProcessor), "EntryByteCount"));
            return new CorpseRottingTableEntry(
                unchecked((short)SnesWorkRam.ReadWord(memory, entryAddress)),
                SnesWorkRam.ReadWord(memory, entryAddress + 2));
        }
    }
}

/// <summary>Verification access to <see cref="CrateriaLightningPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class CrateriaLightningPaletteFxProgramDefinitionAccess
{
    extension(CrateriaLightningPaletteFxProgramDefinition self)
    {
        /// <summary>$8D:F765 live surface-lightning definition or $F769 unused dark-lightning definition.</summary>
        internal ushort DefinitionPointer => PrivateState.Property<bool>(self, "IsSurface") ? (ushort)0xf765 : (ushort)0xf769;

        internal int CycleFrames => 240 * (1 + PrivateState.Property<int>(self, "NeutralCount")) + 2 * 9 + 5;

        internal int DisplayedRecordsPerCycle => 1 + 14 + PrivateState.Property<int>(self, "NeutralCount") + 4 + 1;
    }
}

/// <summary>Verification access to <see cref="CrocomireBg2ScrollDefinitions"/> members production does not use.</summary>
internal static class CrocomireBg2ScrollDefinitionsAccess
{
    extension(CrocomireBg2ScrollDefinitions)
    {
        internal static int EntryCount => 17;

        /// <summary>Native pointer order: twelve charge/step-back frames followed by
        /// five moving-claw frames. Each six-component extended frame occupies $32 bytes.</summary>
        internal static CrocomireBg2VerticalCorrection Entry(int index)
        {
            if ((uint)index >= CrocomireBg2ScrollDefinitions.EntryCount) throw new IndexOutOfRangeException();
            ushort frame = (ushort)(index < 12 ? PrivateState.StaticField<ushort>(typeof(CrocomireBg2ScrollDefinitions), "ChargeStepFrameStart") + 0x32 * index
                : PrivateState.StaticField<ushort>(typeof(CrocomireBg2ScrollDefinitions), "MovingClawsFrameStart") + 0x32 * (index - 12));
            return new(frame, unchecked((ushort)((int)(PrivateState.InvokeStatic(typeof(CrocomireBg2ScrollDefinitions), "Correction", (ushort)(frame)))!)));
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireDeathState"/> members production does not use.</summary>
internal static class CrocomireDeathStateAccess
{
    extension(CrocomireDeathState self)
    {
        /// <summary>Resultant per-column erase heights, exposed read-only to the debugger.</summary>
        internal IReadOnlyList<byte> MeltingColumnHeights => PrivateState.Field<byte[]>(self, "_meltingColumnHeights");
    }
}

/// <summary>Verification access to <see cref="CrocomireMeltingPassSequence"/> members production does not use.</summary>
internal static class CrocomireMeltingPassSequenceAccess
{
    extension(CrocomireMeltingPassSequence self)
    {
        /// <summary>The two native melt passes, in header order; the indexer rejects any other index.</summary>
        internal IEnumerator<CrocomireMeltingPass> GetEnumerator()
        {
            yield return self[0];
            yield return self[1];
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireMeltingUploadSequence"/> members production does not use.</summary>
internal static class CrocomireMeltingUploadSequenceAccess
{
    extension(CrocomireMeltingUploadSequence self)
    {
        internal IEnumerator<CrocomireMeltingUpload> GetEnumerator()
        {
            for (int index = 0; index < self.Length; index++)
                yield return self[index];
        }
    }
}

/// <summary>Verification access to <see cref="DachoraColorRomData"/> members production does not use.</summary>
internal static class DachoraColorRomDataAccess
{
    extension(DachoraColorRomData)
    {
        /// <summary>Returns the authored color source for a cartridge-selected phase/frame.</summary>
        internal static int Source(DachoraPalettePhase phase, int frame) => phase switch
        {
            DachoraPalettePhase.Default when frame == 0 => DachoraColorRomData.DefaultSource,
            DachoraPalettePhase.Speed when (uint)frame < DachoraColorRomData.AnimatedFrameCount =>
                DachoraColorRomData.SpeedSource + frame * DachoraColorRomData.FrameByteCount,
            DachoraPalettePhase.Shine when (uint)frame < DachoraColorRomData.AnimatedFrameCount =>
                DachoraColorRomData.ShineSource + frame * DachoraColorRomData.FrameByteCount,
            _ => throw new ArgumentOutOfRangeException(nameof(frame),
                $"Dachora phase {phase} has no frame {frame}."),
        };
    }
}

/// <summary>Verification access to <see cref="DachoraInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class DachoraInstructionProgramDefinitionsAccess
{
    /// <summary>The fifteen production entry programs, in native order.</summary>
    private static readonly ushort[] ProgramEntries =
    [
        DachoraInstructionProgramDefinitions.RunningLeft, DachoraInstructionProgramDefinitions.RunningLeftFast,
        DachoraInstructionProgramDefinitions.RunningLeftVeryFast, DachoraInstructionProgramDefinitions.IdleLeft,
        DachoraInstructionProgramDefinitions.BlinkLeft, DachoraInstructionProgramDefinitions.EchoLeft,
        DachoraInstructionProgramDefinitions.FallingLeft, DachoraInstructionProgramDefinitions.RunningRight,
        DachoraInstructionProgramDefinitions.RunningRightFast, DachoraInstructionProgramDefinitions.RunningRightVeryFast,
        DachoraInstructionProgramDefinitions.IdleRight, DachoraInstructionProgramDefinitions.BlinkRight,
        DachoraInstructionProgramDefinitions.ChargeRight, DachoraInstructionProgramDefinitions.EchoRight,
        DachoraInstructionProgramDefinitions.FallingRight,
    ];

    extension(DachoraInstructionProgramDefinitions)
    {
        internal static int ProgramCount => ProgramEntries.Length;

        internal static DachoraInstructionProgram Program(int index)
        {
            ushort entry = ProgramEntries[index];
            (int frames, ushort terminator) = PrivateState.StaticField<InstructionProgramLayout>(typeof(DachoraInstructionProgramDefinitions), "Layout").FramesFrom(entry);
            return new(entry, frames, terminator == CommonEnemyInstructionCodes.Goto);
        }
    }
}

/// <summary>Verification access to <see cref="DeadMonsterRottingDefinitions.TransferRows"/> members production does not use.</summary>
internal static class DeadMonsterRottingDefinitionsTransferRowsAccess
{
    extension(DeadMonsterRottingDefinitions.TransferRows self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="DeadTorizoVramTransferDefinitions.PhaseRows"/> members production does not use.</summary>
internal static class DeadTorizoVramTransferDefinitionsPhaseRowsAccess
{
    extension(DeadTorizoVramTransferDefinitions.PhaseRows self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="DeadTourianCorpseInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class DeadTourianCorpseInstructionProgramDefinitionsAccess
{
    extension(DeadTourianCorpseInstructionProgramDefinitions)
    {
        internal static ushort SleepWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+4);
    }
}

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsAccess
{
    extension(DraygonCollisionDefinitions)
    {
        internal static int EmptyOamFrameCount => 2 * (6 + 4 + 7 + 7);
    }
}

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsComponentSequenceAccess
{
    extension(DraygonCollisionDefinitions.ComponentSequence self)
    {
        internal int Length => self.HasComponent ? 1 : 0;
    }
}

/// <summary>Verification access to <see cref="EnemyProjectileInstructionMechanicsDefinitions"/> members production does not use.</summary>
internal static class EnemyProjectileInstructionMechanicsDefinitionsAccess
{
    extension(EnemyProjectileInstructionMechanicsDefinitions)
    {
        /// <summary>Native $86:E42C selector domain, including its final two out-of-order roots.</summary>
        internal static int MiscDustProgramCount => 30;

        internal static bool IsVisualOperand(ushort address)
        {
            int offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial;
            if (offset is >= 6 and < 48 && offset % 8 == 6) return true;
            offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch;
            if (offset is >= 6 and < 28 && offset % 4 == 2) return true;
            offset = address - EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial;
            if (offset is >= 2 and < 20 && offset % 4 == 2 || offset == 28) return true;
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(EnemyProjectileInstructionMechanicsDefinitions), "TimedProgramCount"); index++)
            {
                var program = ((EnemyProjectileTimedProgramDefinition)(PrivateState.InvokeStatic(typeof(EnemyProjectileInstructionMechanicsDefinitions), "TimedProgram", (int)(index)))!);
                offset = address - program.InitialPointer - (program.PrefixInstruction.HasValue ? 2 : 0);
                if (offset is >= 2 && offset < program.FrameCount * 4 && offset % 4 == 2) return true;
            }
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="EnemyPropertyFlagExtensions"/> members production does not use.</summary>
internal static class EnemyPropertyFlagExtensionsAccess
{
    /// <summary>
    /// Reads the typed high-byte flags while retaining the family-specific low byte in the
    /// caller's raw word. This is the explicit escape hatch for lossless cartridge records.
    /// </summary>
    internal static EnemyProperties ReadFlagsChecked(this ushort word)
    {
        ushort highByte = unchecked((ushort)(word & EnemyPropertyFlagExtensions.KnownPropertyFlagMask));
        EnemyProperties flags = (EnemyProperties)highByte;
        if ((highByte & ~(ushort)(EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.BlocksPlasmaBeam |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.RespawnIfKilled |
                EnemyProperties.SolidToSamus)) != 0)
        {
            throw new InvalidDataException(
                $"Enemy property word ${word:X4} contains an unnamed high-byte flag.");
        }

        return flags;
    }

    internal static bool HasAny(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) != 0;

    internal static bool HasAll(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) == flags;

    internal static bool HasAll(this ushort word, EnemyExtraProperties flags)
    {
        PrivateState.InvokeStatic(typeof(EnemyPropertyFlagExtensions), "ValidateKnown", (EnemyExtraProperties)(flags));
        return ((EnemyExtraProperties)word & flags) == flags;
    }
}

/// <summary>Verification access to <see cref="GameplayMessageIds"/> members production does not use.</summary>
internal static class GameplayMessageIdsAccess
{
    extension(GameplayMessageIds)
    {
        /// <summary>
        /// Converts a byte read from a translated cartridge owner into the closed retail
        /// message catalog. The diagnostic identifies both the raw ID and the owner that read
        /// it, rather than failing later at an unrelated definition-table access.
        /// </summary>
        internal static GameplayMessageId FromCartridge(byte value, string sourceContext)
        {
            if (                value is (< (byte)GameplayMessageId.EnergyTank or > (byte)GameplayMessageId.GravitySuit) and
                not ((byte)GameplayMessageId.GunshipSaveConfirmation))
            {
                throw new NotSupportedException(
                    $"Gameplay message ${value:X2} from {sourceContext} is not translated.");
            }

            return (GameplayMessageId)value;
        }
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoAwakeningInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoAwakeningInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
            {
                var word = GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWord(index);
                if (word.Address != address) continue;
                value = word.Value;
                return true;
            }
            value = 0;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoEyeBeamAttackInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoEyeBeamAttackInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoInitialInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoInitialInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWordCount; index++)
            {
                InstructionMechanicsWord word = GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(index);
                if (word.Address == address)
                {
                    value = word.Value;
                    return true;
                }
            }
            value = 0;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoJumpLandingInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoJumpLandingInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoJumpLandingInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWordCount; index++)
            {
                InstructionMechanicsWord word = GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWord(index);
                if (word.Address != address) continue;
                value = word.Value;
                return true;
            }
            value = 0;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoLeftFootOrbInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftFootOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoLeftOrbInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoLeftTurnInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoLeftTurnInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftTurnInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightOrbInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightSonicInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightSonicInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightwardInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightwardInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStunnedInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStunnedInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoStunnedInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoStunnedInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoWalkingInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoWalkingInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GrowingShutterInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GrowingShutterInstructionProgramDefinitionsAccess
{
    extension(GrowingShutterInstructionProgramDefinitions)
    {
        internal static int ProgramCount => 4;

        /// <summary>Each growth stage owns a six-byte duration/visual/sleep program.</summary>
        internal static ushort ProgramEntryPoint(int index)
        {
            if ((uint)index >= GrowingShutterInstructionProgramDefinitions.ProgramCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(GrowingShutterInstructionProgramDefinitions.TenPixels + 6 * index);
        }
    }
}

/// <summary>Verification access to <see cref="GunshipDustInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GunshipDustInstructionProgramDefinitionsAccess
{
    extension(GunshipDustInstructionProgramDefinitions)
    {
        internal static ushort InitialForParameter(ushort parameter) => parameter switch
        {
            0 => GunshipDustInstructionProgramDefinitions.Index0,
            2 => GunshipDustInstructionProgramDefinitions.Index2,
            4 => GunshipDustInstructionProgramDefinitions.Index4,
            6 => GunshipDustInstructionProgramDefinitions.Index6,
            8 => GunshipDustInstructionProgramDefinitions.Index8,
            10 => GunshipDustInstructionProgramDefinitions.IndexA,
            _ => throw new ArgumentOutOfRangeException(
                "parameter", parameter, "Gunship dust parameter must be 0,2,4,6,8,A."),
        };
    }
}

/// <summary>Verification access to <see cref="HyperBeamPaletteFxState"/> members production does not use.</summary>
internal static class HyperBeamPaletteFxStateAccess
{
    extension(HyperBeamPaletteFxState self)
    {
        /// <summary>Current bank-$8D instruction pointer, exposed for exact-step debugging.</summary>
        internal ushort InstructionPointer => PrivateState.Field<ushort>(self, "_instructionPointer");
    }
}

/// <summary>Verification access to <see cref="InstructionProgramLayout"/> members production does not use.</summary>
internal static class InstructionProgramLayoutAccess
{
    extension(InstructionProgramLayout self)
    {
        /// <summary>
        /// Counts the timed frames that run from <paramref name="entry"/> and returns the opcode of
        /// the first control instruction after them.
        /// </summary>
        internal (int Frames, ushort Terminator) FramesFrom(ushort entry)
        {
            int index = Array.FindIndex(PrivateState.Field<InstructionItem[]>(self, "items"), item => item.Kind == InstructionItemKind.Entry && item.Value == entry);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(entry), entry, "Entry is not part of this layout.");
            int frames = 0;
            for (index++; index < PrivateState.Field<InstructionItem[]>(self, "items").Length; index++)
            {
                InstructionItem item = PrivateState.Field<InstructionItem[]>(self, "items")[index];
                if (item.Kind != InstructionItemKind.Words) continue;
                if (item.Words is [{ IsPresentation: false }, { IsPresentation: true }]) { frames++; continue; }
                return (frames, item.Words[0].Value);
            }
            throw new InvalidDataException($"Instruction entry ${self.Bank:X2}:{entry:X4} has no terminating instruction.");
        }

        /// <summary>Finds the presentation slot containing either byte at <paramref name="longAddress"/>.</summary>
        internal bool TryGetPresentationWord(int longAddress, out ushort wordAddress)
        {
            wordAddress = 0;
            if (longAddress >> 16 != self.Bank) return false;
            int bankAddress = longAddress & ushort.MaxValue;
            // Words never overlap, so the byte belongs to the word starting one byte before it or at it.
            object?[] containing = [bankAddress, null, null];
            if ((bool)PrivateState.InvokeWithOut(self, "TryWordContaining", containing)! &&
                ((InstructionWord)containing[2]!).IsPresentation)
            {
                wordAddress = (ushort)containing[1]!;
                return true;
            }
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="KraidArmCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidArmCollisionDefinitionsAccess
{
    /// <summary>A single-component frame: a two-byte count and one eight-byte hitbox.</summary>
    private const int SingleComponentFrameBytes = 2 + 8;

    extension(KraidArmCollisionDefinitions)
    {
        /// <summary>Calculates the ordered physical frame roots from native component-record sizes.</summary>
        /// <remarks>Independently reviewed for #1165 against bank_A7.asm and every original
        /// frame count. General and rising/sinking groups have ten five-component frames
        /// each; the final two frames have one component. No pointer roster is stored.</remarks>
        internal static ushort FramePointer(int index)
        {
            if ((uint)index >= KraidArmCollisionDefinitions.FrameCount) throw new IndexOutOfRangeException();
            return (ushort)(index < 20 ? PrivateState.StaticField<ushort>(typeof(KraidArmCollisionDefinitions), "FirstGeneralFrame") + PrivateState.StaticField<int>(typeof(KraidArmCollisionDefinitions), "GeneralFrameBytes") * index
                : PrivateState.StaticField<ushort>(typeof(KraidArmCollisionDefinitions), "FirstSingleComponentFrame") + SingleComponentFrameBytes * (index - 20));
        }
    }
}

/// <summary>Verification access to <see cref="KraidArmHitboxSequence"/> members production does not use.</summary>
internal static class KraidArmHitboxSequenceAccess
{
    extension(KraidArmHitboxSequence self)
    {
        internal KraidArmCollisionHitbox[] ToArray()
        {
            var result = new KraidArmCollisionHitbox[self.Length];
            for (int index = 0; index < result.Length; index++) result[index] = self[index];
            return result;
        }
    }
}

/// <summary>Verification access to <see cref="KraidEnemyState"/> members production does not use.</summary>
internal static class KraidEnemyStateAccess
{
    extension(KraidEnemyState self)
    {
        internal ushort HealthQuarterThreshold(int index) => KraidHealthThresholdDefinitions.Quarter(self.InitialHealth, index);
    }
}

/// <summary>Verification access to <see cref="KraidFootCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidFootCollisionDefinitionsAccess
{
    extension(KraidFootCollisionDefinitions)
    {
        internal static ushort FramePointer(int index) =>
            checked((ushort)(KraidFootCollisionDefinitions.FirstFrame + index * PrivateState.StaticField<int>(typeof(KraidFootCollisionDefinitions), "FrameByteCount")));
    }
}

/// <summary>Verification access to <see cref="KraidFootHitboxSequence"/> members production does not use.</summary>
internal static class KraidFootHitboxSequenceAccess
{
    extension(KraidFootHitboxSequence self)
    {
        internal KraidFootCollisionHitbox[] ToArray() => [self[0]];
    }
}

/// <summary>Verification access to <see cref="KraidHealthThresholdDefinitions"/> members production does not use.</summary>
internal static class KraidHealthThresholdDefinitionsAccess
{
    extension(KraidHealthThresholdDefinitions)
    {
        /// <summary>$A7:AA23-AA43: truncate health/4, then accumulate four multiples.</summary>
        internal static ushort Quarter(ushort initialHealth, int index)
        {
            if ((uint)index >= 4) throw new IndexOutOfRangeException();
            return (ushort)((initialHealth >> 2) * (index + 1));
        }
    }
}

/// <summary>Verification access to <see cref="LayerBlendingConfigurations"/> members production does not use.</summary>
internal static class LayerBlendingConfigurationsAccess
{
    extension(LayerBlendingConfigurations)
    {
        /// <summary>Rejects undefined configuration values supplied by host-side callers.</summary>
        internal static void Validate(
            LayerBlendingConfiguration configuration,
            string parameterName) =>
            PrivateState.InvokeStatic(typeof(LayerBlendingConfigurations), "ValidateDefined", (LayerBlendingConfiguration)(configuration), (string)(parameterName), (bool)(false));
    }
}

/// <summary>Verification access to <see cref="MamaTurtleEnemyDefinitionCatalog"/> members production does not use.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogAccess
{
    /// <summary>Compiled Mama Turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Mama = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x00c8,
        XRadius: 0x0014,
        YRadius: 0x0010,
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d6c,
        PartCount: 0x0005,
        Unused16: 0x0000,
        MainAiPointer: 0x8dd2,
        GrappleAiPointer: 0x800f,
        HurtAiPointer: 0x804c,
        FrozenAiPointer: 0x8041,
        TimeFrozenAiPointer: 0x0000,
        DeathAnimation: 0x0004,
        Unused24: 0x0000,
        Unused26: 0x0000,
        PowerBombReactionPointer: 0x0000,
        VariantIndex: 0x0000,
        Unused2C: 0x0000,
        Unused2E: 0x0000,
        TouchAiPointer: 0x9281,
        ShotAiPointer: 0x802d,
        InitialSpritemapPointer: 0x0000,
        TileDataAddress: 0xacd400,
        Layer: 0x05,
        ItemDropChancesPointer: 0xf3bc,
        VulnerabilityPointer: 0xeec6,
        NamePointer: 0xdf11);

    /// <summary>Compiled baby turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Baby = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x0000,
        XRadius: 0x0008,
        YRadius: 0x0005,
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d9d,
        PartCount: 0x0001,
        Unused16: 0x0000,
        MainAiPointer: 0x912e,
        GrappleAiPointer: 0x800f,
        HurtAiPointer: 0x804c,
        FrozenAiPointer: 0x8041,
        TimeFrozenAiPointer: 0x0000,
        DeathAnimation: 0x0000,
        Unused24: 0x0000,
        Unused26: 0x0000,
        PowerBombReactionPointer: 0x0000,
        VariantIndex: 0x0000,
        Unused2C: 0x0000,
        Unused2E: 0x0000,
        TouchAiPointer: 0x929f,
        ShotAiPointer: 0x930f,
        InitialSpritemapPointer: 0x0000,
        TileDataAddress: 0xacd400,
        Layer: 0x05,
        ItemDropChancesPointer: 0xf3bc,
        VulnerabilityPointer: 0xeec6,
        NamePointer: 0x0000);

    extension(MamaTurtleEnemyDefinitionCatalog)
    {
        /// <summary>Resolves a compiled family header by its native bank-$A0 pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomEnemyDefinition definition)
        {
            switch (pointer)
            {
                case MamaTurtleEnemyDefinitionCatalog.MamaPointer:
                    definition = Mama;
                    return true;
                case MamaTurtleEnemyDefinitionCatalog.BabyPointer:
                    definition = Baby;
                    return true;
                default:
                    definition = default;
                    return false;
            }
        }
    }
}

/// <summary>Verification access to <see cref="MamaTurtleShellContourDefinitions"/> members production does not use.</summary>
internal static class MamaTurtleShellContourDefinitionsAccess
{
    extension(MamaTurtleShellContourDefinitions)
    {
        /// <summary>Returns one raw word by native table index for cartridge parity checks.</summary>
        internal static short GetRawOffset(int index)
        {
            if ((uint)index >= MamaTurtleShellContourDefinitions.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return PrivateState.StaticSpanProperty<short>(typeof(MamaTurtleShellContourDefinitions), "Offsets")[index];
        }
    }
}

/// <summary>Verification access to <see cref="MaridiaEnvironmentalPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class MaridiaEnvironmentalPaletteFxProgramDefinitionAccess
{
    extension(MaridiaEnvironmentalPaletteFxProgramDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by environmental owner.</summary>
        internal ushort DefinitionPointer => (ushort)(MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPitDefinition + (int)self.Owner * 4);

        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames => self.FrameCount * self.Duration;
    }
}

/// <summary>Verification access to <see cref="MaridiaLargeSnailCollisionDefinitions"/> members production does not use.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsAccess
{
    extension(MaridiaLargeSnailCollisionDefinitions)
    {
        internal static IEnumerable<ushort> HitboxPointers => PrivateState.StaticField<CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox>>(typeof(MaridiaLargeSnailCollisionDefinitions), "Lists").Pointers;
    }
}

/// <summary>Verification access to <see cref="MotherBrainContactHitboxDefinitions"/> members production does not use.</summary>
internal static class MotherBrainContactHitboxDefinitionsAccess
{
    extension(MotherBrainContactHitboxDefinitions)
    {
        /// <summary>Returns the pinned list address used only for parity diagnostics.</summary>
        internal static int GetSourceAddress(MotherBrainContactPart part) =>
            part switch
            {
                MotherBrainContactPart.Body => MotherBrainContactHitboxDefinitions.BodySourceAddress,
                MotherBrainContactPart.Brain => MotherBrainContactHitboxDefinitions.BrainSourceAddress,
                MotherBrainContactPart.Neck => MotherBrainContactHitboxDefinitions.NeckSourceAddress,
                _ => throw new ArgumentOutOfRangeException(nameof(part)),
            };
    }
}

/// <summary>Verification access to <see cref="MotherBrainCorpseRottingState"/> members production does not use.</summary>
internal static class MotherBrainCorpseRottingStateAccess
{
    extension(MotherBrainCorpseRottingState)
    {
        /// <summary>Reads one native four-byte table entry for debugger and verification use.</summary>
        internal static MotherBrainCorpseRotEntry ReadEntry(ISnesMutableMemory memory, int entryIndex)
        {
            ArgumentNullException.ThrowIfNull(memory);
            if ((uint)entryIndex >= MotherBrainCorpseRottingState.EntryCount)
                throw new ArgumentOutOfRangeException(nameof(entryIndex));

            CorpseRottingTableEntry entry = CorpseRottingTableProcessor.ReadEntry(
                memory,
                MotherBrainCorpseRottingState.RotTableAddress,
                MotherBrainCorpseRottingState.EntryCount,
                entryIndex);
            return new MotherBrainCorpseRotEntry(entry.YOffset, entry.Timer);
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainFallingTubePopulationDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFallingTubePopulationDefinitionsAccess
{
    /// <summary>The five eight-word population records following <c>BottomLeft</c>.</summary>
    private static readonly ushort[] RecordPointers =
        [.. Enumerable.Range(0, 5).Select(index => (ushort)(MotherBrainFallingTubePopulationDefinitions.BottomLeft + index * 8 * sizeof(ushort)))];

    extension(MotherBrainFallingTubePopulationDefinitions)
    {
        /// <summary>Ordered native population identities, calculated from the eight-word record format.</summary>
        internal static IReadOnlyList<ushort> Pointers => RecordPointers;
    }
}

/// <summary>Verification access to <see cref="MotherBrainHandBeamInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainHandBeamInstructionProgramDefinitionsAccess
{
    extension(MotherBrainHandBeamInstructionProgramDefinitions)
    {
        internal static int ExternalCallCount => PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "StageCount");

        internal static ushort ExternalCallInstruction(int index)
        {
            if ((uint)index >= MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(((int)(PrivateState.InvokeStatic(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "StageStart", (int)(index)))!) + PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "FrameBytes"));
        }

        internal static bool IsPresentationByte(int address)
        {
            if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
            int bankAddress = (ushort)address;
            if (bankAddress < MotherBrainHandBeamInstructionProgramDefinitions.Initial || bankAddress >= PrivateState.StaticField<ushort>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "TerminalDelete")) return false;
            int offset = (bankAddress - MotherBrainHandBeamInstructionProgramDefinitions.Initial) % PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "StageBytes");
            if (offset < PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "FrameBytes")) return offset >= sizeof(ushort);
            return offset >= PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "FrameBytes") + PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "CallbackBytes") && (offset - PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "CallbackBytes")) % PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "FrameBytes") >= sizeof(ushort);
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainHeadInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainHeadInstructionProgramDefinitionsAccess
{
    extension(MotherBrainHeadInstructionProgramDefinitions)
    {
        /// <summary>The precise pointer windows executed by the dedicated head interpreter.</summary>
        internal static bool IsActivePointer(ushort pointer) =>
            pointer is >= MotherBrainHeadInstructionProgramDefinitionsTooling.NeutralStart and <= MotherBrainHeadInstructionProgramDefinitionsConstants.NeutralActiveEnd or
                >= MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackStart and <= MotherBrainHeadInstructionProgramDefinitionsConstants.BabyAttackActiveEnd or
                >= MotherBrainHeadInstructionProgramDefinitionsConstants.BombStart and <= MotherBrainHeadInstructionProgramDefinitionsConstants.BombActiveEnd;
    }
}

/// <summary>Verification access to <see cref="MotherBrainRainbowBeamAttackSequence"/> members production does not use.</summary>
internal static class MotherBrainRainbowBeamAttackSequenceAccess
{
    extension(MotherBrainRainbowBeamAttackSequence self)
    {
        /// <summary>
        /// Shared row-by-row corpse graphics processor initialized by the brain enemy slot.
        /// Its WRAM table and graphics buffer remain public debugger evidence rather than being
        /// hidden behind a host-only opacity value.
        /// </summary>
        internal MotherBrainCorpseRottingState CorpseRotting => PrivateState.Field<MotherBrainCorpseRottingState>(self, "_corpseRotting");

        /// <summary>
        /// Starts at the low-health handoff in `$A9:BB1A`, including its immediate really-slow
        /// forward-walk request. This is the exact debugger entry point reached after the `$BB06`
        /// decision timer; it does not skip the later `$BD45` health-selection loop.
        /// </summary>
        internal void StartFinishOffSequence()
        {
            PrivateState.Invoke(self, "RequestWalkForwardReallySlow", (ushort)(unchecked((ushort)(self.Body.XPosition + 0x0010))));
            PrivateState.SetProperty(self, "BabyMetroidTileTransferIndex", 0);
            PrivateState.SetProperty(self, "BabyMetroidSpawned", false);
            PrivateState.SetProperty(self, "Phase", MotherBrainRainbowBeamAttackPhase.FinishSamusOff);
        }

        /// <summary>
        /// Applies damage already calculated by the ordinary enemy-shot engine. Bank `$A0` owns
        /// beam/item damage multipliers, invulnerability, projectile deletion, and hit flashing;
        /// this actor owns only the resulting health word and the phase-three zero-health branch.
        /// Keeping that boundary explicit lets a live projectile producer call the real actor
        /// without duplicating the room enemy system's translated generic damage routine.
        /// </summary>
        internal void ApplyCalculatedBrainDamage(ushort damage)
        {
            // Generic enemy damage saturates at zero. A host subtraction with ushort wrapping
            // would resurrect a nearly dead boss, so perform the borrow test before the write.
            PrivateState.SetProperty(self, "BrainHealth", damage >= self.BrainHealth
                ? (ushort)0
                : unchecked((ushort)(self.BrainHealth - damage)));
        }

        /// <summary>
        /// Executes the movement half of <c>$A9:9072-$91B7</c> on Mother Brain's later brain
        /// enemy slot. Call this after the body AI/body instruction stage and before the still
        /// later Baby slot, matching the retail increasing-slot enemy loop.
        /// </summary>
        internal void StepNeckMovement(ISnesAddressSpace bus, SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(samus);
            if (self.NeckMovementEnabled != 0)
            {
                ushort lowerAngle = self.LowerNeckAngle;
                ushort upperAngle = self.UpperNeckAngle;
                ushort lowerIndex = self.LowerNeckMovementIndex;
                ushort upperIndex = self.UpperNeckMovementIndex;
                MotherBrainNeckKinematics.StepAngles(
                    ref lowerAngle,
                    ref upperAngle,
                    ref lowerIndex,
                    ref upperIndex,
                    self.NeckAngleDelta,
                    self.BrainYPosition,
                    samus.YPosition);
                PrivateState.SetProperty(self, "LowerNeckAngle", lowerAngle);
                PrivateState.SetProperty(self, "UpperNeckAngle", upperAngle);
                PrivateState.SetProperty(self, "LowerNeckMovementIndex", lowerIndex);
                PrivateState.SetProperty(self, "UpperNeckMovementIndex", upperIndex);
            }

            MotherBrainNeckGeometry geometry = MotherBrainNeckKinematics.CalculateGeometry(
                self.Body.XPosition,
                self.Body.YPosition,
                self.LowerNeckAngle,
                self.UpperNeckAngle);
            self.BrainXPosition = geometry.Segment4.X;
            self.BrainYPosition = geometry.Segment4.Y;
        }

        /// <summary>
        /// Executes the body-owned shake countdown consumed while the later graphics hook draws
        /// Mother Brain's brain at <c>$A9:9382-$939A</c>. Call after all enemy slots, matching the
        /// renderer: `$BE96` can then observe zero and reseed fifty on the following frame.
        /// </summary>
        internal ushort StepBrainShakeForDraw()
        {
            if (self.BrainMainShakeTimer != 0)
                PrivateState.SetProperty(self, "BrainMainShakeTimer", unchecked((ushort)(self.BrainMainShakeTimer - 1)));
            return unchecked((ushort)(self.BrainMainShakeTimer & 6));
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainRoomPaletteProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainRoomPaletteProgramDefinitionsAccess
{
    extension(MotherBrainRoomPaletteProgramDefinitions)
    {
        internal static InstructionMechanicsWord MechanicsWord(int index)
        {
            if ((uint)index >= MotherBrainRoomPaletteProgramDefinitions.MechanicsWordCount) throw new IndexOutOfRangeException();
            return index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount
                ? new((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart + index * 4), PrivateState.StaticField<ushort>(typeof(MotherBrainRoomPaletteProgramDefinitions), "FlashImageTicks"))
                : new((ushort)(PrivateState.StaticField<ushort>(typeof(MotherBrainRoomPaletteProgramDefinitions), "LoopInstruction") + (index - MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount) * 2),
                    index == MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount ? MotherBrainRoomPaletteProgramDefinitions.GotoInstruction : MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        }

        internal static bool IsCompiledMechanicsByte(int address)
        {
            if ((address & 0xff0000) != 0xa90000) return false;
            int offset = (ushort)address - MotherBrainRoomPaletteProgramDefinitions.FlashStart;
            return (uint)offset < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 && offset % 4 < 2 ||
                offset >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 && offset < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 + 4;
        }

        internal static bool TryGetPresentationWord(int address, out ushort wordAddress)
        {
            wordAddress = 0;
            if ((address & 0xff0000) != 0xa90000) return false;
            int offset = (ushort)address - MotherBrainRoomPaletteProgramDefinitions.FlashStart;
            if ((uint)offset >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 || offset % 4 < 2) return false;
            wordAddress = (ushort)(address & 0xfffe);
            return true;
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainTurretInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainTurretInstructionProgramDefinitionsAccess
{
    extension(MotherBrainTurretInstructionProgramDefinitions)
    {
        internal static int DirectionCount => 8;

        internal static ushort TurretProgram(MotherBrainTurretDirection direction) =>
            MotherBrainTurretInstructionProgramDefinitions.DirectionProgram(MotherBrainTurretInstructionProgramDefinitions.TurretLeft, direction);

        internal static ushort BulletProgram(MotherBrainTurretDirection direction) =>
            MotherBrainTurretInstructionProgramDefinitions.DirectionProgram(PrivateState.StaticField<ushort>(typeof(MotherBrainTurretInstructionProgramDefinitions), "BulletLeft"), direction);

        internal static ushort DirectionProgram(ushort first, MotherBrainTurretDirection direction)
        {
            int index = (byte)direction;
            if ((uint)index >= MotherBrainTurretInstructionProgramDefinitions.DirectionCount)
                throw new ArgumentOutOfRangeException(nameof(direction), direction,
                    "Mother Brain turret direction must be zero through seven.");
            return (ushort)(first + 6 * index);
        }
    }
}

/// <summary>Verification access to <see cref="PaletteFxHeatInstructionListDefinitions"/> members production does not use.</summary>
internal static class PaletteFxHeatInstructionListDefinitionsAccess
{
    extension(PaletteFxHeatInstructionListDefinitions)
    {
        /// <summary>Returns the native bank-$8D selector-word address for parity verification.</summary>
        internal static ushort NativeSourceAddress(PaletteFxHeatSuit suit, int phase)
        {
            if ((uint)phase >= PaletteFxHeatInstructionListDefinitions.PhaseCount)
                throw new ArgumentOutOfRangeException(nameof(phase));

            ushort table = suit switch
            {
                PaletteFxHeatSuit.Power => PaletteFxHeatInstructionListDefinitions.PowerSourceTable,
                PaletteFxHeatSuit.Varia => PaletteFxHeatInstructionListDefinitions.VariaSourceTable,
                PaletteFxHeatSuit.Gravity => PaletteFxHeatInstructionListDefinitions.GravitySourceTable,
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
            };
            return unchecked((ushort)(table + phase * sizeof(ushort)));
        }
    }
}

/// <summary>Verification access to <see cref="PaletteFxHeatProgramFrameDefinition"/> members production does not use.</summary>
internal static class PaletteFxHeatProgramFrameDefinitionAccess
{
    extension(PaletteFxHeatProgramFrameDefinition self)
    {
        /// <summary>The terminal wait command after fifteen live BGR555 colors.</summary>
        internal ushort WaitInstructionPointer => unchecked((ushort)(
            self.FirstColorPointer + PaletteFxHeatProgramDefinition.ColorsPerFrame * sizeof(ushort)));
    }
}

/// <summary>Verification access to <see cref="PhantoonCasualFlameDefinitions.Schedule"/> members production does not use.</summary>
internal static class PhantoonCasualFlameDefinitionsScheduleAccess
{
    extension(PhantoonCasualFlameDefinitions.Schedule self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsComponentSequenceAccess
{
    extension(PhantoonCollisionDefinitions.ComponentSequence self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.HitboxSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsHitboxSequenceAccess
{
    extension(PhantoonCollisionDefinitions.HitboxSequence self)
    {
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonWaveHdmaState"/> members production does not use.</summary>
internal static class PhantoonWaveHdmaStateAccess
{
    extension(PhantoonWaveHdmaState self)
    {
        internal ReadOnlySpan<ushort> ScrollCycle => PrivateState.Field<ushort[]>(self, "_cycle").AsSpan(0, PrivateState.Field<int>(self, "_cycleLength"));
    }
}

/// <summary>Verification access to <see cref="PlanetZebesTextPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class PlanetZebesTextPaletteFxProgramDefinitionAccess
{
    extension(PlanetZebesTextPaletteFxProgramDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by fade direction.</summary>
        internal ushort DefinitionPointer => (ushort)(PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FadeInDefinition + (int)self.Owner * 4);
    }
}

/// <summary>Verification access to <see cref="RidleyCollisionDefinitions"/> members production does not use.</summary>
internal static class RidleyCollisionDefinitionsAccess
{
    extension(RidleyCollisionDefinitions)
    {
        /// <summary>The eleven side-facing body frames, `$32` bytes apart from the leftmost.</summary>
        internal static ushort[] FramePointers
        {
            get
            {
                Type bodyFrame = PrivateState.Nested(typeof(RidleyCollisionDefinitions), "BodyFrame");
                ushort left = Convert.ToUInt16(Enum.Parse(bodyFrame, "Left"));
                int stride = PrivateState.StaticField<int>(typeof(RidleyCollisionDefinitions), "SideFrameBytes");
                return [.. Enumerable.Range(0, 11).Select(index => (ushort)(left + index * stride))];
            }
        }

        internal static IEnumerable<ushort> HitboxPointers => PrivateState.StaticField<CollisionRecordRuns<RidleyCollisionHitbox>>(typeof(RidleyCollisionDefinitions), "Lists").Pointers;
    }
}

/// <summary>Verification access to <see cref="RoomEnemyAuxiliaryDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalogAccess
{
    extension(RoomEnemyAuxiliaryDefinitionCatalog)
    {
        /// <summary>The five auxiliary header identities missing from room lists.</summary>
        internal static IEnumerable<ushort> Pointers => PrivateState.StaticField<IReadOnlyDictionary<ushort, RoomEnemyDefinition>>(typeof(RoomEnemyAuxiliaryDefinitionCatalog), "Definitions").Keys;
    }
}

/// <summary>Verification access to <see cref="RoomEnemyDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyDefinitionCatalogAccess
{
    extension(RoomEnemyDefinitionCatalog)
    {
        /// <summary>Enumerates every compiled native definition pointer for ROM-oracle tests.</summary>
        internal static IEnumerable<ushort> Pointers => PrivateState.StaticField<(ushort Pointer, RoomEnemyDefinition Definition)[]>(typeof(RoomEnemyDefinitionCatalog), "Definitions").Select(entry => entry.Pointer);
    }
}

/// <summary>Verification access to <see cref="RoomEnemyPopulationDefinitions"/> members production does not use.</summary>
internal static class RoomEnemyPopulationDefinitionsAccess
{
    extension(RoomEnemyPopulationDefinitions)
    {
        /// <summary>Enumerates every compiled population identity for ROM-oracle tests.</summary>
        internal static IEnumerable<ushort> Pointers => PrivateState.StaticField<RoomEnemyPopulationDefinition[]>(typeof(RoomEnemyPopulationDefinitions), "Definitions").Select(list => list.Pointer);
    }
}

/// <summary>Verification access to <see cref="RoomEnemySystem"/> members production does not use.</summary>
internal static class RoomEnemySystemAccess
{
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// Runs the native mouth pass followed by the outer mouth/body pass outside Kraid's AI.
        /// <c>MainAI_Kraid</c> itself interleaves palette handling between the two passes.
        /// </summary>
        internal int ResolveKraidProjectileHits(
            ISnesAddressSpace bus,
            SamusProjectileSystem projectiles,
            SamusBombProjectileSystem sharedProjectiles)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(projectiles);
            ArgumentNullException.ThrowIfNull(sharedProjectiles);
            var kraid = PrivateState.Field<KraidEnemyState?>(self, "<Kraid>k__BackingField");
            RoomEnemySlot body = self.Slots[0];
            if (kraid is null || body.EnemyDefinitionPointer != RoomEnemySystem.KraidDefinition ||
                body.Properties.HasAny(EnemyProperties.Deleted))
                return 0;
            object shots = PrivateState.Construct(PrivateState.Nested(typeof(RoomEnemySystem), "KraidShotSlots"),
                projectiles, sharedProjectiles);
            return (int)PrivateState.Invoke(self, "ResolveKraidMouthProjectileHits", body, kraid, shots)! +
                (int)PrivateState.Invoke(self, "ResolveKraidBodyProjectileHits", body, kraid, shots)!;
        }

        /// <summary>Botwoon's live state while its room is loaded.</summary>
        internal BotwoonEnemyState? Botwoon => PrivateState.Field<BotwoonEnemyState?>(self, "_botwoonState");

        /// <summary>Per-slot Evir states, indexed by enemy slot.</summary>
        internal IReadOnlyList<EvirEnemyState?> EvirStates => PrivateState.Field<EvirEnemyState?[]>(self, "_evirStates");

        /// <summary>Special drops a dying Metroid has requested this frame.</summary>
        internal IReadOnlyList<MetroidDropRequest> MetroidDropRequests =>
            PrivateState.Field<List<MetroidDropRequest>>(self, "_metroidDropRequests");


        /// <summary>Companion/corpse state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadSidehopperEnemyState?> DeadSidehoppers =>
            PrivateState.Field<DeadSidehopperEnemyState?[]>(self, "_deadSidehopperStates");

        /// <summary>The loaded Dead Torizo's typed extended-WRAM state.</summary>
        internal DeadTorizoEnemyState? DeadTorizo => PrivateState.Field<DeadTorizoEnemyState?>(self, "_deadTorizo");

        /// <summary>Alternating VRAM records authored by the most recent actor frame.</summary>
        internal IReadOnlyList<VramWriteEntry> LastDeadTorizoVramTransfers =>
            PrivateState.Field<List<VramWriteEntry>>(self, "_deadTorizoFrameVramTransfers");

        /// <summary>Typed state for every physical slot currently owned by a Skree.</summary>
        internal IReadOnlyList<SkreeEnemyState?> SkreeStates => PrivateState.Field<SkreeEnemyState?[]>(self, "_skreeStates");

        /// <summary>Typed Spark state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SparkEnemyState?> SparkStates => PrivateState.Field<SparkEnemyState?[]>(self, "_sparkStates");

        /// <summary>Typed state for every physical slot currently owned by a Zoa.</summary>
        internal IReadOnlyList<ZoaEnemyState?> ZoaStates => PrivateState.Field<ZoaEnemyState?[]>(self, "_zoaStates");

        /// <summary>Typed state for both graphics and hitbox records in all physical slots.</summary>
        internal IReadOnlyList<HibashiEnemyState?> HibashiStates => PrivateState.Field<HibashiEnemyState?[]>(self, "_hibashiStates");

        /// <summary>Typed actor extensions in fixed native slot order.</summary>
        internal IReadOnlyList<ElevatorEnemyState?> ElevatorStates => PrivateState.Field<ElevatorEnemyState?[]>(self, "_elevatorStates");

        internal IReadOnlyList<KagoEnemyState?> KagoStates => PrivateState.Field<KagoEnemyState?[]>(self, "_kagoStates");

        /// <summary>Typed Waver state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<WaverEnemyState?> WaverStates => PrivateState.Field<WaverEnemyState?[]>(self, "_waverStates");

        /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
        internal IReadOnlyList<CrawlerEnemyState?> CrawlerStates => PrivateState.Field<CrawlerEnemyState?[]>(self, "_crawlerStates");

        /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
        internal IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates => PrivateState.Field<WorkRobotEnemyState?[]>(self, "_workRobotStates");

        /// <summary>Typed state for every physical enemy slot currently owned by a Rio.</summary>
        internal IReadOnlyList<RioEnemyState?> RioStates => PrivateState.Field<RioEnemyState?[]>(self, "_rioStates");

        /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
        internal IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates => PrivateState.Field<ZebetiteEnemyState?[]>(self, "_zebetiteStates");

        /// <summary>
        /// Per-physical-slot Shaktool state. A live retail group occupies seven consecutive
        /// non-null entries; every other room slot remains null.
        /// </summary>
        internal IReadOnlyList<ShaktoolSegmentState?> ShaktoolSegments => PrivateState.Field<ShaktoolSegmentState?[]>(self, "_shaktoolSegments");

        /// <summary>Typed state for every physical slot currently owned by a Mochtroid.</summary>
        internal IReadOnlyList<MochtroidEnemyState?> MochtroidStates => PrivateState.Field<MochtroidEnemyState?[]>(self, "_mochtroidStates");

        /// <summary>Typed state for each physical slot currently occupied by a Yard.</summary>
        internal IReadOnlyList<YardEnemyState?> YardStates => PrivateState.Field<YardEnemyState?[]>(self, "_yardStates");

        /// <summary>Typed Metaree state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MetareeEnemyState?> MetareeStates => PrivateState.Field<MetareeEnemyState?[]>(self, "_metareeStates");

        /// <summary>Typed state for every physical Owtch-capable enemy slot.</summary>
        internal IReadOnlyList<OwtchEnemyState?> OwtchStates => PrivateState.Field<OwtchEnemyState?[]>(self, "_owtchStates");

        /// <summary>Typed state for all physical slots occupied by Fune or Namihe.</summary>
        internal IReadOnlyList<FuneNamiheEnemyState?> FuneNamiheStates => PrivateState.Field<FuneNamiheEnemyState?[]>(self, "_funeNamiheStates");

        /// <summary>Typed Stoke state for each of the 32 physical enemy slots.</summary>
        internal IReadOnlyList<StokeEnemyState?> StokeStates => PrivateState.Field<StokeEnemyState?[]>(self, "_stokeStates");

        internal IReadOnlyList<FakeKraidEnemyState?> FakeKraidStates => PrivateState.Field<FakeKraidEnemyState?[]>(self, "_fakeKraidStates");

        /// <summary>Typed state for every physical slot currently owned by this family.</summary>
        internal IReadOnlyList<NorfairLavaJumpingEnemyState?> NorfairLavaJumpingEnemyStates =>
            PrivateState.Field<NorfairLavaJumpingEnemyState?[]>(self, "_norfairLavaJumpingEnemyStates");

        /// <summary>Typed Fireflea state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<FirefleaEnemyState?> FirefleaStates => PrivateState.Field<FirefleaEnemyState?[]>(self, "_firefleaStates");

        /// <summary>All 32 physical bank-$B4 slots, including inactive ones.</summary>
        internal IReadOnlyList<RoomSpriteObjectSlot> RoomSpriteObjects => PrivateState.Field<RoomSpriteObjectSlot[]>(self, "_roomSpriteObjects");

        /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
        internal SporeSpawnEnemyState? SporeSpawn => PrivateState.Field<SporeSpawnEnemyState?>(self, "_sporeSpawn");

        /// <summary>Typed state for all 32 physical Boyon-capable enemy slots.</summary>
        internal IReadOnlyList<BoyonEnemyState?> BoyonStates => PrivateState.Field<BoyonEnemyState?[]>(self, "_boyonStates");

        /// <summary>Typed state for each physical escape-Etecoon slot.</summary>
        internal IReadOnlyList<EscapeEtecoonEnemyState?> EscapeEtecoonStates =>
            PrivateState.Field<EscapeEtecoonEnemyState?[]>(self, "_escapeEtecoonStates");

        internal IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
            PrivateState.Field<NuclearWaffleEnemyState?[]>(self, "_nuclearWaffleStates");

        /// <summary>Typed state for every physical slot currently owned by the hopper family.</summary>
        internal IReadOnlyList<HopperEnemyState?> HopperStates => PrivateState.Field<HopperEnemyState?[]>(self, "_hopperStates");

        /// <summary>Typed face-block state in fixed physical enemy-slot order.</summary>
        internal IReadOnlyList<BlueBrinstarFaceBlockEnemyState?> BlueBrinstarFaceBlockStates =>
            PrivateState.Field<BlueBrinstarFaceBlockEnemyState?[]>(self, "_blueBrinstarFaceBlockStates");

        /// <summary>Frame-local drops requested by destroyed free-floating spores.</summary>
        internal IReadOnlyList<SporeSpawnDropRequest> SporeSpawnDropRequests =>
            PrivateState.Field<List<SporeSpawnDropRequest>>(self, "_sporeSpawnDropRequests");

        /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
            PrivateState.Field<DeadTourianCorpseEnemyState?[]>(self, "_deadTourianCorpseStates");

        /// <summary>Typed Atomic state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AtomicEnemyState?> AtomicStates => PrivateState.Field<AtomicEnemyState?[]>(self, "_atomicStates");

        /// <summary>Typed state for every physical Cacatac-capable enemy slot.</summary>
        internal IReadOnlyList<CacatacEnemyState?> CacatacStates => PrivateState.Field<CacatacEnemyState?[]>(self, "_cacatacStates");

        /// <summary>Typed state for every physical enemy slot currently owned by a Rinka.</summary>
        internal IReadOnlyList<RinkaEnemyState?> RinkaStates => PrivateState.Field<RinkaEnemyState?[]>(self, "_rinkaStates");

        internal IReadOnlyList<WalkingSpacePirateEnemyState?> WalkingSpacePirateStates =>
            PrivateState.Field<WalkingSpacePirateEnemyState?[]>(self, "_walkingSpacePirateStates");

        internal IReadOnlyList<WallSpacePirateEnemyState?> WallSpacePirateStates =>
            PrivateState.Field<WallSpacePirateEnemyState?[]>(self, "_wallSpacePirateStates");

        internal IReadOnlyList<MagdolliteEnemyState?> MagdolliteStates => PrivateState.Field<MagdolliteEnemyState?[]>(self, "_magdolliteStates");

        /// <summary>Typed state for each physical enemy slot currently owned by a Metroid.</summary>
        internal IReadOnlyList<MetroidEnemyState?> MetroidStates => PrivateState.Field<MetroidEnemyState?[]>(self, "_metroidStates");

        internal ShitroidEnemyState? Shitroid => PrivateState.Field<ShitroidEnemyState?>(self, "_shitroid");

        /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
        internal IReadOnlyList<ushort> ActiveEnemyIndexes => PrivateState.Field<List<ushort>>(self, "_activeEnemyIndexes");

        /// <summary>Typed native state for every physical Etecoon slot.</summary>
        internal IReadOnlyList<EtecoonEnemyState?> EtecoonStates => PrivateState.Field<EtecoonEnemyState?[]>(self, "_etecoonStates");

        /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
        internal IReadOnlyList<ushort> InteractiveEnemyIndexes => PrivateState.Field<List<ushort>>(self, "_interactiveEnemyIndexes");

        /// <summary>Typed Alcoon state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AlcoonEnemyState?> AlcoonStates => PrivateState.Field<AlcoonEnemyState?[]>(self, "_alcoonStates");

        /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
        internal IReadOnlyList<DragonEnemyState?> DragonStates => PrivateState.Field<DragonEnemyState?[]>(self, "_dragonStates");

        /// <summary>The room's terminated bank-$B4 graphics-set records.</summary>
        internal IReadOnlyList<RoomEnemyGraphicsSetEntry> GraphicsSet => PrivateState.Field<List<RoomEnemyGraphicsSetEntry>>(self, "_graphicsSet");

        /// <summary>Typed state for all 32 physical Puyo-capable enemy slots.</summary>
        internal IReadOnlyList<PuyoEnemyState?> PuyoStates => PrivateState.Field<PuyoEnemyState?[]>(self, "_puyoStates");

        /// <summary>Typed Bull state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BullEnemyState?> BullStates => PrivateState.Field<BullEnemyState?[]>(self, "_bullStates");

        /// <summary>Typed Skultera state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SkulteraEnemyState?> SkulteraStates => PrivateState.Field<SkulteraEnemyState?[]>(self, "_skulteraStates");

        /// <summary>Typed state for every physical slot currently owned by Sbug/Sbug2.</summary>
        internal IReadOnlyList<SbugEnemyState?> SbugStates => PrivateState.Field<SbugEnemyState?[]>(self, "_sbugStates");

        /// <summary>Typed state for every physical Pipe Bug enemy slot.</summary>
        internal IReadOnlyList<PipeBugEnemyState?> PipeBugStates => PrivateState.Field<PipeBugEnemyState?[]>(self, "_pipeBugStates");

        /// <summary>Typed Ki-Hunter state for every body and wing physical enemy slot.</summary>
        internal IReadOnlyList<KiHunterEnemyState?> KiHunterStates => PrivateState.Field<KiHunterEnemyState?[]>(self, "_kiHunterStates");

        /// <summary>Typed Choot state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<ChootEnemyState?> ChootStates => PrivateState.Field<ChootEnemyState?[]>(self, "_chootStates");

        /// <summary>Pickup requests emitted by shot destroyable flames in this room load.</summary>
        internal IReadOnlyList<PhantoonFlameDropRequest> PhantoonFlameDropRequests =>
            PrivateState.Field<List<PhantoonFlameDropRequest>>(self, "_phantoonFlameDropRequests");

        /// <summary>Typed native state for every loaded Yapping Maw slot.</summary>
        internal IReadOnlyList<YappingMawEnemyState?> YappingMawStates => PrivateState.Field<YappingMawEnemyState?[]>(self, "_yappingMawStates");

        /// <summary>Typed Oum state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MaridiaLargeSnailEnemyState?> MaridiaLargeSnailStates =>
            PrivateState.Field<MaridiaLargeSnailEnemyState?[]>(self, "_maridiaLargeSnailStates");

        /// <summary>Typed Beetom state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BeetomEnemyState?> BeetomStates => PrivateState.Field<BeetomEnemyState?[]>(self, "_beetomStates");

        /// <summary>Typed Tripper/Kamer state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<PlatformEnemyState?> PlatformStates => PrivateState.Field<PlatformEnemyState?[]>(self, "_platformStates");

        /// <summary>Bomb Torizo's state while definition $EEFF owns slot zero.</summary>
        internal TorizoEnemyState? BombTorizo =>
            PrivateState.Field<TorizoEnemyState?>(self, "_torizoState") is { IsGolden: false } ? PrivateState.Field<TorizoEnemyState?>(self, "_torizoState") : null;

        internal IReadOnlyList<GrowingShutterEnemyState?> GrowingShutterStates => PrivateState.Field<GrowingShutterEnemyState?[]>(self, "_growingShutterStates");

        /// <summary>Pickup requests emitted by shot Chozo orbs during this room load.</summary>
        internal IReadOnlyList<TorizoOrbDropRequest> TorizoOrbDropRequests =>
            PrivateState.Field<List<TorizoOrbDropRequest>>(self, "_torizoOrbDropRequests");

        /// <summary>Number of live actors in the shared bank-$86 enemy-projectile pool.</summary>
        internal int ActiveEnemyProjectileCount =>
            PrivateState.Field<RoomEnemyProjectileSlot[]>(self, "_enemyProjectiles").Count(projectile => projectile.IsActive);

        /// <summary>
        /// Executes the projectile portion of the gameplay frame after enemy instructions have
        /// had their opportunity to spawn a fireball. This is the same producer/consumer order
        /// as EnemyMain followed by bank $86's enemy-projectile handler.
        /// </summary>
        internal void StepEnemyProjectiles(
            RoomLevelData level,
            SamusState? samus,
            ushort cameraX = 0,
            ushort cameraY = 0,
            byte? nmiFrameCounter8 = null,
            SamusBombProjectileSystem? samusBombs = null)
        {
            self.StepEnemyProjectileInstructions(level, samus, cameraX, cameraY, nmiFrameCounter8, samusBombs);
            self.ResolveEnemyProjectileSamusHits(samus);
        }

        /// <summary>
        /// Emits both native enemy-projectile priority passes for focused diagnostics which do
        /// not draw Samus between them. Gameplay must call the two phase-specific methods so
        /// property bit <c>$1000</c> determines which side of Samus owns each OAM record.
        /// </summary>
        internal void DrawEnemyProjectiles(OamBuffer oam, ushort cameraX, ushort cameraY, bool timeIsFrozen = false)
        {
            ArgumentNullException.ThrowIfNull(oam);
            PrivateState.Invoke(self, "EnsureLoaded");

            PrivateState.Invoke(self, "DrawRoomSpriteObjects", (OamBuffer)(oam), (ushort)(cameraX), (ushort)(cameraY));
            PrivateState.Invoke(self, "DrawEnemyProjectilePass", (OamBuffer)(oam), (ushort)(cameraX), (ushort)(cameraY), (EnemyProjectileDrawPriority)(EnemyProjectileDrawPriority.High), (bool)(timeIsFrozen));
            PrivateState.Invoke(self, "DrawEnemyProjectilePass", (OamBuffer)(oam), (ushort)(cameraX), (ushort)(cameraY), (EnemyProjectileDrawPriority)(EnemyProjectileDrawPriority.Low), (bool)(timeIsFrozen));
        }

        /// <summary>Standalone contact probe; runtime dispatches body before AI and tail during tail update.</summary>
        internal bool ResolveRidleySamusContact(SamusState samus) =>
            (bool)PrivateState.Invoke(self, "ResolveRidleyBodySamusContact", samus)! ||
            (bool)PrivateState.Invoke(self, "ResolveRidleyTailSamusContact", samus)!;
    }
}

/// <summary>Verification access to <see cref="RoomFxAnimatedTileFrameDefinition"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileFrameDefinitionAccess
{
    extension(RoomFxAnimatedTileFrameDefinition self)
    {
        /// <summary>Native artwork-operand identity immediately after this compiled control word.</summary>
        internal ushort SourceOperandPointer => unchecked((ushort)(self.InstructionPointer + 2));
    }
}

/// <summary>Verification access to <see cref="RoomFxRecordDefinition"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionAccess
{
    extension(RoomFxRecordDefinition self)
    {
        /// <summary>Exposes one native byte for consumers that use offset-based FX fields.</summary>
        internal byte ReadByte(int offset) => offset switch
        {
            0 => (byte)self.DoorPointer,
            1 => (byte)(self.DoorPointer >> 8),
            2 => (byte)self.BaseYPosition,
            3 => (byte)(self.BaseYPosition >> 8),
            4 => (byte)self.TargetYPosition,
            5 => (byte)(self.TargetYPosition >> 8),
            6 => (byte)self.PackedYVelocity,
            7 => (byte)(self.PackedYVelocity >> 8),
            8 => self.Timer,
            9 => self.Type,
            10 => self.DefaultLayerBlend,
            11 => self.Layer3LayerBlend,
            12 => self.LiquidOptions,
            13 => self.PaletteFxBitset,
            14 => self.AnimatedTileBitset,
            15 => self.PaletteBlend,
            _ => throw new ArgumentOutOfRangeException(nameof(offset)),
        };

        internal ushort ReadWord(int offset)
        {
            if ((uint)offset > RoomFxRomData.Record.ByteCount - sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(offset));
            return (ushort)(self.ReadByte(offset) | self.ReadByte(offset + 1) << 8);
        }
    }
}

/// <summary>Verification access to <see cref="RoomFxRecordDefinitions"/> members production does not use.</summary>
internal static class RoomFxRecordDefinitionsAccess
{
    extension(RoomFxRecordDefinitions)
    {
        /// <summary>Enumerates every selected record in ascending native identity order without a stored cache.</summary>
        internal static IEnumerable<RoomFxRecordDefinition> All
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (((RoomFxRecordDefinition?)(PrivateState.InvokeStatic(typeof(RoomFxRecordDefinitions), "SelectRecord", (ushort)((ushort)pointer)))!) is { } record)
                        yield return record;
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomLayer3FxState"/> members production does not use.</summary>
internal static class RoomLayer3FxStateAccess
{
    extension(RoomLayer3FxState self)
    {
        /// <summary>Loads an explicit immutable FX definition through the normal room-load path.</summary>
        internal void LoadDefinition(ISnesAddressSpace bus, SnesVram vram, SnesCgram cgram,
            RoomFxRecordDefinition definition, ushort randomNumber)
        {
            ArgumentNullException.ThrowIfNull(definition);
            PrivateState.Invoke(self, "LoadCore", (ISnesAddressSpace)(bus), (SnesVram)(vram), (SnesCgram)(cgram), (ushort)(definition.Pointer), (ushort)(definition.DoorPointer), (ushort)(randomNumber), (ushort)(0), (RoomFxRecordDefinition?)(definition));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPaletteFxDefinitions"/> members production does not use.</summary>
internal static class RoomPaletteFxDefinitionsAccess
{
    extension(RoomPaletteFxDefinitions)
    {
        /// <summary>Calculates a native palette-FX area-list identity for index0..7.</summary>
        /// <remarks>$83:AC46 has eight pointers to AC66+20h*area. Each area owns an
        /// eight-word palette list followed by an eight-word animated-tile list.
        /// Includes the debug area; invalid indices retain the former span rejection.
        /// Independently verified against NTSC J/U v1.0 and pinned bank_83.asm
        /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
        internal static ushort NativeAreaListPointer(int areaIndex)
        {
            if ((uint)areaIndex >= RoomPaletteFxDefinitions.AreaCount) throw new IndexOutOfRangeException();
            return (ushort)(0xac66 + areaIndex * 0x20);
        }
    }
}

/// <summary>Verification access to <see cref="RoomPaletteFxSystem"/> members production does not use.</summary>
internal static class RoomPaletteFxSystemAccess
{
    extension(RoomPaletteFxSystem self)
    {
        /// <summary>
        /// Heat-animation phase at WRAM $1EED, published by Norfair palette streams and
        /// consumed one descending-slot pass later by the shared Samus-in-heat object.
        /// </summary>
        internal ushort SamusInHeatPaletteIndex => PrivateState.Field<ushort>(self, "samusInHeatPaletteIndex");

        /// <summary>
        /// Last heat-animation phase consumed by the shared Samus-in-heat object, corresponding
        /// to WRAM $1EEF. Exposing both words makes native one-frame handoff timing inspectable.
        /// </summary>
        internal ushort PreviousSamusInHeatPaletteIndex => PrivateState.Field<ushort>(self, "previousSamusInHeatPaletteIndex");

        /// <summary>Loads an explicit FX record through the normal room palette-object path.</summary>
        internal void LoadDefinition(ISnesAddressSpace bus, RoomFxRecordDefinition definition,
            AreaId area, ushort equippedItems, bool areaMiniBossDefeated)
        {
            ArgumentNullException.ThrowIfNull(definition);
            PrivateState.Invoke(self, "LoadRoomCore", (ISnesAddressSpace)(bus), (ushort)(definition.Pointer), (ushort)(definition.DoorPointer), (AreaId)(area), (ushort)(equippedItems), (bool)(areaMiniBossDefeated), (RoomFxRecordDefinition?)(definition));
        }
    }
}

/// <summary>Verification access to <see cref="RoomSandAnimatedTilesState"/> members production does not use.</summary>
internal static class RoomSandAnimatedTilesStateAccess
{
    extension(RoomSandAnimatedTilesState self)
    {
        /// <summary>Number of sand objects selected by the current room/door FX bitset.</summary>
        internal int Count => PrivateState.Field<List<RoomFxAnimatedTilesState>>(self, "objects").Count;
    }
}

/// <summary>Verification access to <see cref="RoomScrollGrid"/> members production does not use.</summary>
internal static class RoomScrollGridAccess
{
    extension(RoomScrollGrid self)
    {
        /// <summary>
        /// All 50 bytes, including bytes beyond the room dimensions. The original explicit
        /// loader at <c>$82:E878-$82:E889</c> copies 25 words unconditionally, so edge reads can
        /// observe data following a shorter ROM table rather than an invented zero padding.
        /// </summary>
        internal ReadOnlySpan<byte> Storage => PrivateState.Field<byte[]>(self, "_cells");
    }
}

/// <summary>Verification access to <see cref="RoomSpriteObjectInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class RoomSpriteObjectInstructionProgramDefinitionsAccess
{
    extension(RoomSpriteObjectInstructionProgramDefinitions)
    {
        internal static bool TryGetPresentationWord(int address, out ushort wordAddress) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(RoomSpriteObjectInstructionProgramDefinitions), "Layout").TryGetPresentationWord(address, out wordAddress);
    }
}

/// <summary>Verification access to <see cref="RoomTreadmillAnimatedTilesState"/> members production does not use.</summary>
internal static class RoomTreadmillAnimatedTilesStateAccess
{
    extension(RoomTreadmillAnimatedTilesState self)
    {
        /// <summary>Number of treadmill objects selected by the current room/door FX bitset.</summary>
        internal int Count => PrivateState.Field<List<WreckedShipTreadmillAnimatedTilesState>>(self, "objects").Count;
    }
}

/// <summary>Verification access to <see cref="SamusAerialMovement"/> members production does not use.</summary>
internal static class SamusAerialMovementAccess
{
    extension(SamusAerialMovement)
    {
        /// <summary>
        /// Ports the dry-air, no-hi-jump path through
        /// <c>Make_Samus_Jump</c> at <c>$90:98BC</c> and the normal-air branch of
        /// <c>Determine_Samus_YAcceleration</c> at <c>$90:9C5B</c>.
        /// </summary>
        internal static void InitializeDryAirJump(ISnesAddressSpace bus, SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(samus);

            (samus.Kinematics.YSpeed, samus.Kinematics.YSubspeed) =
                SamusVerticalMotionDefinitions.Launch(SamusLiquidPhysicsState.Air, highJump: false, wallJump: false);
            SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(samus);
            SamusAerialMovement.ConfigureDryAirGravity(bus, samus);
            samus.Kinematics.YDirection = 1;
        }

        /// <summary>
        /// Publishes the normal-air gravity pair selected by <c>$90:9C5B</c>. The normal frame
        /// pipeline refreshes these environment-dependent words before movement even when the
        /// pose change was a walk-off rather than <c>Make_Samus_Jump</c>.
        /// </summary>
        internal static void ConfigureDryAirGravity(ISnesAddressSpace bus, SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(samus);
            (samus.Kinematics.YAcceleration, samus.Kinematics.YSubacceleration) =
                SamusVerticalMotionDefinitions.Gravity(SamusLiquidPhysicsState.Air);
        }
    }
}

/// <summary>Verification access to <see cref="SamusAnimationDelayPrograms"/> members production does not use.</summary>
internal static class SamusAnimationDelayProgramsAccess
{
    extension(SamusAnimationDelayPrograms)
    {
        internal static ReadOnlySpan<SamusAnimationSegment> Segments => PrivateState.StaticField<SamusAnimationSegment[]>(typeof(SamusAnimationDelayPrograms), "segments");
    }
}

/// <summary>Verification access to <see cref="SamusBeamLoadoutWord"/> members production does not use.</summary>
internal static class SamusBeamLoadoutWordAccess
{
    /// <summary>Low four bits of the beam word select the combination.</summary>
    private const ushort CombinationMask = 0x000f;

    extension(SamusBeamLoadoutWord self)
    {
        /// <summary>
        /// Replaces the four-bit retail combination index while preserving Charge and every
        /// other raw bit. Debug menus use this to emulate changing only the selected beams.
        /// </summary>
        internal ushort WithCombinationIndex(int combinationIndex)
        {
            if ((uint)combinationIndex > CombinationMask)
                throw new ArgumentOutOfRangeException(nameof(combinationIndex));
            return (ushort)((self.Raw & ~CombinationMask) | combinationIndex);
        }
    }
}

/// <summary>Verification access to <see cref="SamusBombProjectileSlot"/> members production does not use.</summary>
internal static class SamusBombProjectileSlotAccess
{
    extension(SamusBombProjectileSlot self)
    {
        /// <summary>
        /// True after a normal bomb selects `$93:A06B`, or while a Power Bomb's timer-zero
        /// slot owns the expanding bank-$88 terrain scan, and before native-style deletion.
        /// </summary>
        internal bool IsExploding => self.IsActive && self.BombTimer == 0;
    }
}

/// <summary>Verification access to <see cref="SamusDrainedState"/> members production does not use.</summary>
internal static class SamusDrainedStateAccess
{
    extension(SamusDrainedState self)
    {
        /// <summary>
        /// True after controller function three writes equipped beams <c>$1009</c>, writes
        /// hyper-beam flag <c>$8000</c>, and spawns the persistent bank-$8D palette object.
        /// </summary>
        internal bool HyperBeamPaletteFxRequested => self.HyperBeamPaletteFx.IsActive;
    }
}

/// <summary>Verification access to <see cref="SamusEquipmentFlagExtensions"/> members production does not use.</summary>
internal static class SamusEquipmentFlagExtensionsAccess
{
    internal static bool HasAll(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) == flags;

    internal static bool HasAll(this ushort word, SamusBeamFlags flags) =>
        ((SamusBeamFlags)word & flags) == flags;

    /// <summary>
    /// Returns a native equipment word with the requested, independently combinable bits
    /// enabled. Keeping this operation here prevents call sites from reintroducing casts and
    /// hexadecimal masks merely because the WRAM-facing property remains a <see cref="ushort"/>.
    /// </summary>
    internal static ushort With(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native equipment word with the requested bits disabled.</summary>
    internal static ushort Without(this ushort word, SamusEquipmentFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits enabled.</summary>
    internal static ushort With(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word | (ushort)flags);

    /// <summary>Returns a native beam word with the requested equipment bits disabled.</summary>
    internal static ushort Without(this ushort word, SamusBeamFlags flags) =>
        (ushort)(word & ~(ushort)flags);

    internal static ushort ToNativeWord(this SamusEquipmentFlags flags) => (ushort)flags;
}

/// <summary>Verification access to <see cref="SamusGrappleMovement"/> members production does not use.</summary>
internal static class SamusGrappleMovementAccess
{
    extension(SamusGrappleMovement)
    {
        /// <summary>
        /// Installs an already-connected swinging state, equivalent to the airborne branch of
        /// <c>HandleConnectingGrapple</c> at $9B:B97C followed by $9B:BA61.
        /// </summary>
        /// <remarks>
        /// Anchor acquisition is intentionally outside this method: the caller must supply the
        /// exact grapple point selected by the beam/block collision pass. This is the same kind
        /// of narrow published-state seam used by the translated knockback and bomb-overlap code.
        /// </remarks>
        internal static void ConnectUnobstructedSwing(
            ISnesAddressSpace bus,
            SamusState samus,
            ushort anchorX,
            ushort anchorY,
            byte ropeLength,
            SnesAngle angle,
            short angularVelocity,
            bool faceRight)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(samus);
            if (ropeLength is < 8 or > 63)
                throw new ArgumentOutOfRangeException(nameof(ropeLength), "Retail connected rope length is 8..63 pixels.");
            if (angularVelocity is < -SamusGrappleRomData.Physics.MaximumAngularVelocity or
                > SamusGrappleRomData.Physics.MaximumAngularVelocity)
                throw new ArgumentOutOfRangeException(nameof(angularVelocity));
            if (samus.Grapple.Phase != GrapplePhase.Inactive)
                throw new InvalidOperationException("A grapple state is already active.");

            SamusGrappleState grapple = samus.Grapple;
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            grapple.AnchorX = anchorX;
            grapple.AnchorY = anchorY;
            grapple.RopeLength = ropeLength;
            grapple.RopeLengthDelta = 0;
            grapple.Angle = angle;
            grapple.MirroredAngle = angle;
            grapple.AngularVelocity = angularVelocity;
            grapple.DirectionInputAcceleration = 0;
            grapple.GravityAcceleration = 0;
            grapple.JumpImpulse = 0;
            grapple.CollisionBounceTimer = 0;
            grapple.Submerged = false;
            // This public helper is the debugger's already-accepted-anchor seam. Its caller may
            // deliberately place an anchor where the current room has no grapple block (the
            // Landing Site regression does exactly that), so only a beam acquired through the
            // block dispatcher opts into bank-$9B's per-frame connection revalidation.
            grapple.ValidateAnchorBlock = false;
            grapple.ValidateAnchorEnemy = false;
            grapple.SpecialAngleHandling = false;
            grapple.WallJumpTimer = 0;
            grapple.CancelFromConnectedPose = false;
            PrivateState.InvokeStatic(typeof(SamusGrappleMovement), "InitializeBeamAnimation", (SamusGrappleState)(grapple));

            samus.Pose = faceRight
                ? SamusPoseIds.GrappleSwingRightPose
                : SamusPoseIds.GrappleSwingLeftPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus, initialFrame: 0);

            // $94:AC11 and $9B:BD95 run as soon as the connection is accepted. Publishing the
            // first pendulum position now prevents one frame of stale pre-grapple coordinates.
            PrivateState.InvokeStatic(typeof(SamusGrappleMovement), "PositionSamusFromPendulum", (ISnesAddressSpace)(bus), (SamusState)(samus), (SamusGrappleState)(grapple));

            // This helper represents a connection that has already passed `$9B:C79D`; publish
            // the handler-tail flag that retail would leave for the following swing frame.
            SamusGrappleMovement.RefreshLiquidPhysicsFlag(samus);
        }
    }
}

/// <summary>Verification access to <see cref="SamusKinematicsState"/> members production does not use.</summary>
internal static class SamusKinematicsStateAccess
{
    extension(SamusKinematicsState self)
    {
        /// <summary>
        /// Native left/right/up/down collision-index words retained for debugger inspection.
        /// <c>$FFFF</c> means that direction did not encounter a solid enemy.
        /// </summary>
        internal IReadOnlyList<ushort> SolidEnemyCollisionIndexes => PrivateState.Field<ushort[]>(self, "_solidEnemyCollisionIndexes");
    }
}

/// <summary>Verification access to <see cref="SamusLiquidPhysicsState"/> members production does not use.</summary>
internal static class SamusLiquidPhysicsStateAccess
{
    extension(SamusLiquidPhysicsState self)
    {
        /// <summary>
        /// Reproduces <c>SetLiquidPhysicsType</c> at <c>$90:8E0F</c>. Unlike movement-table
        /// selection, this room-load helper dispatches on FX type and does not exempt Gravity
        /// Suit; animation later suppresses the suit's delay while retaining the medium word.
        /// </summary>
        internal void InitializeRememberedMedium(SamusState samus)
        {
            ArgumentNullException.ThrowIfNull(samus);
            ushort bottom = samus.Kinematics.BottomBoundary;
            PrivateState.SetProperty(self, "LiquidPhysicsType", self.FxType switch
            {
                RoomFxType.Lava or RoomFxType.Acid
                    when ((bool)(PrivateState.InvokeStatic(typeof(SamusLiquidPhysicsState), "IsBelowSurface", (ushort)(self.LavaAcidYPosition), (ushort)(bottom)))!) => SamusLiquidPhysicsState.LavaAcid,
                RoomFxType.Water or RoomFxType.TourianEntranceStatue when ((bool)(PrivateState.Invoke(self, "WaterAffectsBoundary", (ushort)(bottom)))!) => SamusLiquidPhysicsState.Water,
                _ => SamusLiquidPhysicsState.Air,
            });
        }
    }
}

/// <summary>Verification access to <see cref="SamusPoseTransitionTable"/> members production does not use.</summary>
internal static class SamusPoseTransitionTableAccess
{
    extension(SamusPoseTransitionTable)
    {
        /// <summary>
        /// Finds the first matching transition for canonical Super Metroid input bits.
        /// </summary>
        /// <param name="bus">CPU address space containing bank-$91 transition data.</param>
        /// <param name="currentPose">Current pose whose pointer-table entry is selected.</param>
        /// <param name="canonicalHeldInput">
        /// Held input after custom bindings have been translated to the table's canonical bits.
        /// Directional bits already equal their SNES hardware values; canonical action bits are
        /// jump=$0080, shoot=$0040, aim-down=$0020, aim-up=$0010.
        /// </param>
        /// <param name="canonicalNewInput">Rising-edge form of the same canonical input.</param>
        internal static SamusPoseTransition? Find(
            ISnesAddressSpace bus,
            byte currentPose,
            ushort canonicalHeldInput,
            ushort canonicalNewInput) =>
            SamusPoseTransitionTable.Lookup(bus, currentPose, canonicalHeldInput, canonicalNewInput).Transition;
    }
}

/// <summary>Verification access to <see cref="SamusProjectileSystem"/> members production does not use.</summary>
internal static class SamusProjectileSystemAccess
{
    extension(SamusProjectileSystem self)
    {
        /// <summary>
        /// The independent trail pool in native low-to-high byte-index order. Allocation scans
        /// this collection backward, matching <c>$90:B679-$B683</c> rather than using a queue.
        /// </summary>
        internal IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => PrivateState.Field<SamusProjectileTrailSlot[]>(self, "_trailSlots");

        /// <summary>Debugger-friendly count of slots whose left stream still owns the slot.</summary>
        internal int ActiveTrailCount => PrivateState.Field<SamusProjectileTrailSlot[]>(self, "_trailSlots").Count(slot => slot.IsActive);
    }
}

/// <summary>Verification access to <see cref="SamusProjectileTrailSlot"/> members production does not use.</summary>
internal static class SamusProjectileTrailSlotAccess
{
    extension(SamusProjectileTrailSlot self)
    {
        internal int NativeByteIndex => self.SlotIndex * 2;

        internal bool IsActive => self.Left.InstructionTimer != 0;
    }
}

/// <summary>Verification access to <see cref="SamusProjectileTypeWord"/> members production does not use.</summary>
internal static class SamusProjectileTypeWordAccess
{
    extension(SamusProjectileTypeWord self)
    {
        internal bool IsLive => (self.Raw & PrivateState.StaticField<ushort>(typeof(SamusProjectileTypeWord), "LiveMarker")) != 0;
    }
}

/// <summary>Verification access to <see cref="SamusRunningCadenceDefinitions"/> members production does not use.</summary>
internal static class SamusRunningCadenceDefinitionsAccess
{
    extension(SamusRunningCadenceDefinitions)
    {
        /// <summary>Returns one byte from the complete bounded native cadence catalog.</summary>
        internal static byte ReadCompiledByte(int address)
        {
            object?[] read = [address, null];
            if ((bool)PrivateState.InvokeStaticWithOut(typeof(SamusRunningCadenceDefinitions), "TryReadCompiledByte", read)!)
                return (byte)read[1]!;
            throw new ArgumentOutOfRangeException(
                "address", address, "Address is outside the compiled Samus running-cadence catalog.");
        }
    }
}

/// <summary>Verification access to <see cref="SamusShinesparkState"/> members production does not use.</summary>
internal static class SamusShinesparkStateAccess
{
    extension(SamusShinesparkState self)
    {
        /// <summary>Number of enabled departing echo drawings, independent of slot ownership.</summary>
        internal int ReleasedCrashEchoCount =>
            (PrivateState.Property<bool>(PrivateState.Field<object>(self, "_firstReleasedCrashEcho"), "Active") ? 1 : 0) +
            (PrivateState.Property<bool>(PrivateState.Field<object>(self, "_secondReleasedCrashEcho"), "Active") ? 1 : 0);

        /// <summary>
        /// Runs <c>ProjPreInstr_SpeedEcho</c> at <c>$90:D4D2</c> for the two fixed crash-echo
        /// projectile slots. Call this during alpha projectile processing, before Samus moves.
        /// </summary>
        internal void StepReleasedCrashEchoProjectiles(
            ISnesAddressSpace bus,
            SamusState samus,
            ushort layer1X,
            ushort layer1Y)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(samus);
            PrivateState.Invoke(self, "StepReleasedCrashEcho", (SamusState)(samus), (ushort)(layer1X), (ushort)(layer1Y), PrivateState.Field<object>(self, "_firstReleasedCrashEcho"));
            PrivateState.Invoke(self, "StepReleasedCrashEcho", (SamusState)(samus), (ushort)(layer1X), (ushort)(layer1Y), PrivateState.Field<object>(self, "_secondReleasedCrashEcho"));
        }
    }
}

/// <summary>Verification access to <see cref="SamusSlopePhysics"/> members production does not use.</summary>
internal static class SamusSlopePhysicsAccess
{
    extension(SamusSlopePhysics)
    {
        /// <summary>
        /// Returns the five-bit height sample selected by BTS shape/mirroring and Samus X.
        /// Values may be 0–20; masking with $1F is part of every native consumer.
        /// </summary>
        internal static byte ReadAlignmentHeight(
            ISnesAddressSpace bus,
            byte behavior,
            ushort xPosition)
            => SamusSlopePhysics.ReadAlignmentHeight(bus, new RoomBlockBehavior(behavior), xPosition);
    }
}

/// <summary>Verification access to <see cref="SamusState"/> members production does not use.</summary>
internal static class SamusStateAccess
{
    extension(SamusState self)
    {
        /// <summary>Typed view of <see cref="SamusState.Pose"/>; casting preserves undefined cartridge bytes.</summary>
        internal SamusPoseId PoseId
        {
            get => (SamusPoseId)self.Pose;
            set => self.Pose = (byte)value;
        }

        /// <summary>
        /// Compatibility name for the `$13/$14` Fire subset. Keeping this narrow wrapper makes
        /// existing focused tests readable while all input-table exits share one native handler.
        /// </summary>
        internal bool TryApplySpinToNormalJumpFireTransition(
            ISnesAddressSpace bus,
            RoomLevelData level,
            byte targetPose,
            ushort nmiFrameCounter,
            ushort controllerNewInput,
            RoomPlmSystem? plms = null)
        {
            if (targetPose is not (SamusPoseIds.NormalJumpGunExtendedRightPose or SamusPoseIds.NormalJumpGunExtendedLeftPose))
            {
                throw new InvalidOperationException(
                    $"Spin-fire compatibility route requires pose $13/$14, not ${targetPose:X2}.");
            }
            return self.TryApplySpinOrWallJumpToNormalJumpTransition(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                controllerNewInput,
                plms);
        }

        /// <summary>
        /// Convenience debugger/test entry that performs both native phases: publishing the
        /// bank-$A0 timer-eight overlap direction, then consuming it through $90:DF99 and
        /// special command three $91:EE80. Live runtime code calls those phases on either
        /// side of movement through <see cref="SamusState.PublishBombJumpDirection"/> and
        /// <see cref="SamusState.TrySetupPublishedBombJump"/>.
        /// </summary>
        internal void RequestMorphedBombJump(byte direction)
        {
            if (!SamusState.IsStableBallPose(self.Pose))
            {
                throw new InvalidOperationException(
                    $"The morphed bomb-jump fixture requires a stable ball pose, not ${self.Pose:X2}.");
            }

            self.PublishBombJumpDirection(direction);
            PrivateState.Invoke(self, "ArmPublishedBombJump");
        }
    }
}

/// <summary>Verification access to <see cref="SamusTileTransferState"/> members production does not use.</summary>
internal static class SamusTileTransferStateAccess
{
    extension(SamusTileTransferState self)
    {
        /// <summary>Clears both transfer flags as the game-state reset routines do.</summary>
        internal void ClearTransferFlags()
        {
            PrivateState.SetProperty(self, "TopTransferEnabled", false);
            PrivateState.SetProperty(self, "BottomTransferEnabled", false);
        }
    }
}

/// <summary>Verification access to <see cref="SamusXrayState"/> members production does not use.</summary>
internal static class SamusXrayStateAccess
{
    extension(SamusXrayState self)
    {
        /// <summary>True after Reserve recovery has stranded X-Ray's subsystem disables.</summary>
        internal bool IsGMode => !self.TimeIsFrozen &&
            self.SuspendedSubsystems == XraySuspendedSubsystems.All;
    }
}

/// <summary>Verification access to <see cref="ScrollBoundaryCamera"/> members production does not use.</summary>
internal static class ScrollBoundaryCameraAccess
{
    extension(ScrollBoundaryCamera)
    {
        internal static void RequireDistance(ushort pixelDistance)
        {
            if (pixelDistance is 0 or >= 0x8000)
                throw new ArgumentOutOfRangeException(nameof(pixelDistance));
        }
    }

    extension(ScrollBoundaryCamera self)
    {
        /// <summary>Applies a debug rightward stimulus, then runs <c>$80:A641</c>.</summary>
        internal void MoveRight(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            PrivateState.SetProperty(self, "CameraXSpeed", pixelDistance);
            PrivateState.SetProperty(self, "IdealXPosition", unchecked((ushort)(self.XPosition + pixelDistance)));
            PrivateState.SetProperty(self, "XPosition", self.IdealXPosition);
            PrivateState.Invoke(self, "HandleScrollingRight");
        }

        /// <summary>Applies a debug leftward stimulus, then runs <c>$80:A6BB</c>.</summary>
        internal void MoveLeft(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            PrivateState.SetProperty(self, "CameraXSpeed", pixelDistance);
            PrivateState.SetProperty(self, "IdealXPosition", unchecked((ushort)(self.XPosition - pixelDistance)));
            PrivateState.SetProperty(self, "XPosition", self.IdealXPosition);
            PrivateState.Invoke(self, "HandleScrollingLeft");
        }

        /// <summary>Applies a debug downward stimulus, then runs <c>$80:A893</c>.</summary>
        internal void MoveDown(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            PrivateState.SetProperty(self, "CameraYSpeed", pixelDistance);
            PrivateState.SetProperty(self, "IdealYPosition", unchecked((ushort)(self.YPosition + pixelDistance)));
            PrivateState.SetProperty(self, "YPosition", self.IdealYPosition);
            PrivateState.Invoke(self, "HandleScrollingDown");
        }

        /// <summary>Applies a debug upward stimulus, then runs <c>$80:A936</c>.</summary>
        internal void MoveUp(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            PrivateState.SetProperty(self, "CameraYSpeed", pixelDistance);
            PrivateState.SetProperty(self, "IdealYPosition", unchecked((ushort)(self.YPosition - pixelDistance)));
            PrivateState.SetProperty(self, "YPosition", self.IdealYPosition);
            PrivateState.Invoke(self, "HandleScrollingUp");
        }
    }
}


/// <summary>Verification access to <see cref="ShitroidEnemyState"/> members production does not use.</summary>
internal static class ShitroidEnemyStateAccess
{
    extension(ShitroidEnemyState self)
    {
        /// <summary>The independent 256-color target palette written during initialization.</summary>
        internal ReadOnlyMemory<Bgr555> TargetPalette => PrivateState.Field<Bgr555[]>(self, "_targetPalette");
    }
}

/// <summary>Verification access to <see cref="SkreeMetareeInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class SkreeMetareeInstructionProgramDefinitionsAccess
{
    extension(SkreeMetareeInstructionProgramDefinitions)
    {
        internal static bool IsCompiledMechanicsByte(int address)
        {
            if ((address & 0xff0000) != 0xa30000) return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int species = 0; species < 2; species++)
            for (int index = 0; index < 20; index++)
            {
                ushort word = SkreeMetareeInstructionProgramDefinitions.MechanicsWord(species == 0, index).Address;
                if (bankAddress == word || bankAddress == word + 1) return true;
            }
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="SpacePirateCollisionDefinitions"/> members production does not use.</summary>
internal static class SpacePirateCollisionDefinitionsAccess
{
    extension(SpacePirateCollisionDefinitions)
    {
        internal static int FrameCount => PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").Count;

        internal static int ListCount => PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionHitbox>>(typeof(SpacePirateCollisionDefinitions), "Lists").Count;

        internal static SpacePirateCollisionFrame Frame(int index) =>
            new(PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").PointerAt(index), PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").RecordAt(index));

        internal static SpacePirateCollisionList List(int index) =>
            new(PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionHitbox>>(typeof(SpacePirateCollisionDefinitions), "Lists").PointerAt(index), PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionHitbox>>(typeof(SpacePirateCollisionDefinitions), "Lists").RecordAt(index));
    }
}

/// <summary>Verification access to <see cref="SporeSpawnCollisionDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCollisionDefinitionsAccess
{
    /// <summary>The twelve native hitbox list identities.</summary>
    private static readonly ushort[] ListPointers =
    [
        SporeSpawnCollisionDefinitions.ClosedHead,
        SporeSpawnCollisionDefinitions.OpenHead,
        SporeSpawnCollisionDefinitions.ExtendedHead,
        SporeSpawnCollisionDefinitions.MovingHead0,
        SporeSpawnCollisionDefinitions.MovingHead1,
        SporeSpawnCollisionDefinitions.MovingHead2,
        SporeSpawnCollisionDefinitions.MovingHead3,
        SporeSpawnCollisionDefinitions.TrailingShotPoint,
        SporeSpawnCollisionDefinitions.MirroredTrailingShotPoint,
        SporeSpawnCollisionDefinitions.TrailingDudPoint,
        SporeSpawnCollisionDefinitions.MovingHead4,
        SporeSpawnCollisionDefinitions.MovingHead5,
    ];

    extension(SporeSpawnCollisionDefinitions)
    {
        internal static int FrameCount => 12;

        internal static int ListCount => ListPointers.Length;
    }
}

/// <summary>Verification access to <see cref="SporeSpawnEnemyState"/> members production does not use.</summary>
internal static class SporeSpawnEnemyStateAccess
{
    extension(SporeSpawnEnemyState self)
    {
        /// <summary>
        /// The global target-palette words written by $A5:EA2A/$E91C. The runtime currently
        /// presents completed room fades directly in CGRAM, but retaining this buffer makes the
        /// cartridge's separate target/current ownership inspectable and testable.
        /// </summary>
        internal ReadOnlyMemory<Bgr555> TargetPalette => PrivateState.Field<Bgr555[]>(self, "_targetPalette");
    }
}

/// <summary>Verification access to <see cref="SuperMetroidSaveSlot"/> members production does not use.</summary>
internal static class SuperMetroidSaveSlotAccess
{
    extension(SuperMetroidSaveSlot self)
    {
        /// <summary>Creates the complete translated snapshot accepted by the SRAM encoder.</summary>
        internal SuperMetroidSaveSnapshot ToSnapshot() => new()
        {
            ControllerBindings = self.ControllerBindings,
            MoonwalkEnabled = self.MoonwalkEnabled,
            DebugFlag = self.DebugFlag,
            NewFileMarker = self.NewFileMarker,
            IconCancelEnabled = self.IconCancelEnabled,
            ReserveMissiles = self.ReserveMissiles,
            JapaneseText = self.JapaneseText,
            LoadedItemCount = self.LoadedItemCount,
            EquippedItems = self.EquippedItems,
            CollectedItems = self.CollectedItems,
            EquippedBeams = self.EquippedBeams,
            CollectedBeams = self.CollectedBeams,
            ReserveMode = self.ReserveMode,
            Health = self.Health,
            MaxHealth = self.MaxHealth,
            Missiles = self.Missiles,
            MaxMissiles = self.MaxMissiles,
            SuperMissiles = self.SuperMissiles,
            MaxSuperMissiles = self.MaxSuperMissiles,
            PowerBombs = self.PowerBombs,
            MaxPowerBombs = self.MaxPowerBombs,
            HudItem = self.HudItem,
            MaxReserveEnergy = self.MaxReserveEnergy,
            ReserveEnergy = self.ReserveEnergy,
            GameTimeFrames = self.GameTimeFrames,
            GameTimeSeconds = self.GameTimeSeconds,
            GameTimeMinutes = self.GameTimeMinutes,
            GameTimeHours = self.GameTimeHours,
            SaveStation = self.SaveStation,
            Area = self.Area,
            EventBytes = self.EventBytes.ToArray(),
            BossBytes = self.BossBytes.ToArray(),
            RoomChozoBytes = self.RoomChozoBytes.ToArray(),
            CollectedItemBytes = self.CollectedItemBytes.ToArray(),
            OpenedDoorBytes = self.OpenedDoorBytes.ToArray(),
            UsedSaveStationBytes = self.UsedSaveStationBytes.ToArray(),
            MapStationBytes = self.MapStationBytes.ToArray(),
            ExploredMapBytes = self.ExploredMapBytes.ToArray(),
            LoadingGameState = self.LoadingGameState,
        };
    }
}

/// <summary>Verification access to <see cref="TitleScreenAmbientPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class TitleScreenAmbientPaletteFxProgramDefinitionAccess
{
    extension(TitleScreenAmbientPaletteFxProgramDefinition self)
    {
        /// <summary>Native palette definitions $8D:E1A0 (tube) / $E1A4 (displays).</summary>
        internal ushort DefinitionPointer => PrivateState.Property<bool>(self, "IsTubeLight") ? TitleSequenceRomData.ConsolePaletteFx.SlowLights : TitleSequenceRomData.ConsolePaletteFx.FastLights;

        /// <summary>The complete loop duration in frames.</summary>
        internal int CycleFrames => self.FrameCount * self.FrameDuration;
    }
}

/// <summary>Verification access to <see cref="TorizoCollisionDefinitions"/> members production does not use.</summary>
internal static class TorizoCollisionDefinitionsAccess
{
    /// <summary>The native Torizo hitbox list identities.</summary>
    private static readonly ushort[] ListPointers =
    [
        0x87c7, 0x87e8, 0x87f6, 0x8804, 0x8812, 0x8820, 0x882e, 0x883c, 0x884a, 0x8858, 0x885a, 0x886a, 0x887a, 0x888a, 0x889a, 0x88aa, 0x88ba, 0x88bc, 0x88cc, 0x88dc, 0x88ec, 0x88fc, 0x890c, 0x891c, 0x892a, 0x8946, 0x8954, 0x8962, 0x8970, 0x897e, 0x898c, 0x899a, 0x89a8, 0x89b6, 0x89b8, 0x89c8, 0x89d8, 0x89e8, 0x89f8, 0x8a08, 0x8a18, 0x8a1a, 0x8a2a, 0x8a3a, 0x8a4a, 0x8a5a, 0x8a6a, 0x8a7a, 0x8a88,
    ];

    extension(TorizoCollisionDefinitions)
    {
        internal static int FrameCount => 2 + PrivateState.StaticProperty<int>(typeof(TorizoCollisionDefinitions), "MirroredCount") * 2 + PrivateState.StaticField<GoldenTorizoCollisionComponent[][]>(typeof(TorizoCollisionDefinitions), "JumpBackFrames").Length;

        /// <summary>Every frame pointer, derived from the four runs.</summary>
        internal static IEnumerable<ushort> FramePointers
        {
            get
            {
                yield return PrivateState.StaticField<ushort>(typeof(TorizoCollisionDefinitions), "BlankFrame");
                yield return PrivateState.StaticField<ushort>(typeof(TorizoCollisionDefinitions), "TurningFrame");
                ushort cursor = PrivateState.StaticField<ushort>(typeof(TorizoCollisionDefinitions), "LeftStart");
                foreach (GoldenTorizoCollisionComponent[] frame in PrivateState.StaticField<GoldenTorizoCollisionComponent[][]>(typeof(TorizoCollisionDefinitions), "LeftFrames"))
                {
                    yield return cursor;
                    cursor = ((ushort)(PrivateState.InvokeStatic(typeof(TorizoCollisionDefinitions), "Next", (ushort)(cursor), (int)(frame.Length)))!);
                }
                cursor = PrivateState.StaticField<ushort>(typeof(TorizoCollisionDefinitions), "AwakeningStart");
                foreach (GoldenTorizoCollisionComponent[] frame in PrivateState.StaticField<GoldenTorizoCollisionComponent[][]>(typeof(TorizoCollisionDefinitions), "AwakeningFrames"))
                {
                    yield return cursor;
                    cursor = ((ushort)(PrivateState.InvokeStatic(typeof(TorizoCollisionDefinitions), "Next", (ushort)(cursor), (int)(frame.Length)))!);
                }
                cursor = PrivateState.StaticField<ushort>(typeof(TorizoCollisionDefinitions), "RightStart");
                for (int index = 0; index < PrivateState.StaticProperty<int>(typeof(TorizoCollisionDefinitions), "MirroredCount"); index++)
                {
                    yield return cursor;
                    cursor = ((ushort)(PrivateState.InvokeStatic(typeof(TorizoCollisionDefinitions), "Next", (ushort)(cursor), (int)(((GoldenTorizoCollisionComponent[])(PrivateState.InvokeStatic(typeof(TorizoCollisionDefinitions), "LeftFacing", (int)(index)))!).Length)))!);
                }
                foreach (GoldenTorizoCollisionComponent[] frame in PrivateState.StaticField<GoldenTorizoCollisionComponent[][]>(typeof(TorizoCollisionDefinitions), "JumpBackFrames"))
                {
                    yield return cursor;
                    cursor = ((ushort)(PrivateState.InvokeStatic(typeof(TorizoCollisionDefinitions), "Next", (ushort)(cursor), (int)(frame.Length)))!);
                }
            }
        }

        internal static IEnumerable<ushort> HitboxPointers => ListPointers;

        internal static bool HasFrame(ushort frame) => TorizoCollisionDefinitions.TryGetComponents(frame, out _);

        internal static bool TryGetComponents(ushort frame, out TorizoCollisionComponents components)
        {
            foreach (ushort pointer in TorizoCollisionDefinitions.FramePointers)
            {
                if (pointer != frame) continue;
                components = TorizoCollisionDefinitions.ComponentsAt(frame);
                return true;
            }
            components = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="TorizoFallingLeftInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoFallingLeftInstructionProgramDefinitionsAccess
{
    extension(TorizoFallingLeftInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoFallingLeftInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionProgramDefinitionsAccess
{
    extension(TorizoInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoInstructionVramTransferDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionVramTransferDefinitionsAccess
{
    extension(TorizoInstructionVramTransferDefinitions)
    {
        internal static ReadOnlySpan<TorizoInstructionVramTransferDefinition> All => PrivateState.StaticField<TorizoInstructionVramTransferDefinition[]>(typeof(TorizoInstructionVramTransferDefinitions), "Entries");
    }
}

/// <summary>Verification access to <see cref="TorizoJumpBackInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackInstructionProgramDefinitionsAccess
{
    extension(TorizoJumpBackInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoJumpBackInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitionsAccess
{
    extension(TorizoJumpBackLeftInstructionProgramDefinitions)
    {
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoJumpBackLeftInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TourianEscapeRedFlashPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class TourianEscapeRedFlashPaletteFxProgramDefinitionAccess
{
    extension(TourianEscapeRedFlashPaletteFxProgramDefinition self)
    {
        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames =>
            TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount * self.Duration;
    }
}

/// <summary>Verification access to <see cref="WreckedShipTreadmillMechanicsDefinitions"/> members production does not use.</summary>
internal static class WreckedShipTreadmillMechanicsDefinitionsAccess
{
    extension(WreckedShipTreadmillMechanicsDefinitions)
    {
        /// <summary>Both named directions in native header order, without a stored roster.</summary>
        internal static IEnumerable<WreckedShipTreadmillObjectDefinition> All
        {
            get
            {
                yield return WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Rightwards);
                yield return WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Leftwards);
            }
        }
    }
}

/// <summary>Verification access to <see cref="WreckedShipTreadmillObjectDefinition"/> members production does not use.</summary>
internal static class WreckedShipTreadmillObjectDefinitionAccess
{
    extension(WreckedShipTreadmillObjectDefinition self)
    {
        /// <summary>The four timed frame-control addresses in execution order.</summary>
        internal IReadOnlyList<ushort> FrameInstructionPointers =>
            PrivateState.Field<IReadOnlyList<ushort>>(self, "frameInstructionPointers");
    }
}

/// <summary>Verification access to <see cref="ZebesExplosionLayerFadePaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionLayerFadePaletteFxProgramDefinitionAccess
{
    extension(ZebesExplosionLayerFadePaletteFxProgramDefinition self)
    {
        /// <summary>The complete one-shot fade duration in frames.</summary>
        internal int CycleFrames =>
            ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
            self.FrameDuration;
    }
}

/// <summary>Verification access to <see cref="MagdollitePhaseDefinitions"/> members production does not use.</summary>
internal static class MagdollitePhaseDefinitionsAccess
{
    extension(MagdollitePhaseDefinitions)
    {
        /// <summary>Number of enemy slots covered by <see cref="MagdollitePhaseDefinitions.ApexThreshold"/>.</summary>
        internal static int ApexThresholdSlotCount =>
            PrivateState.StaticField<Array>(typeof(MagdollitePhaseDefinitions), "s_apexThresholdsByEnemySlot").Length;

        /// <summary>$A8:AF55, <c>MagdolliteArmHeightThreshold</c>.</summary>
        internal static ushort ArmHeightThresholdTable => 0xaf55;
    }
}
