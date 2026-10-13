using SuperMetroid.Core;
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

        /// <summary>Restricts native animation-list lookup to its eight physical table rows.</summary>
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

    /// <summary>Resolves a Chozo-room bit into the native 64-byte persistent table.</summary>
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
        /// <summary>Identifies either byte of a compiled Baby spritemap operand in bank $A6.</summary>
        internal static bool IsCompiledSpritemapByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
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

        /// <summary>Identifies either byte of a compiled Baby palette operand in bank $A6.</summary>
        internal static bool IsCompiledPaletteByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
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
        /// <summary>Reads the frame half of a Ceres door presentation pair.</summary>
        internal static ushort PresentationWordFrame(int index) => (((ushort Address, ushort Frame))(PrivateState.InvokeStatic(typeof(CeresDoorInstructionProgramDefinitions), "PresentationWord", (int)(index)))!).Frame;
    }
}

/// <summary>Verification access to <see cref="CeresEscapeVramTransferDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeVramTransferDefinitionsAccess
{
    extension(CeresEscapeVramTransferDefinitions)
    {
        /// <summary>Recognizes bytes in compiled seven-byte VRAM records or their terminator words.</summary>
        internal static bool IsDescriptorByteAddress(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
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
        /// <summary>Number of compiled Mode 7 tile transfers in this sequence.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsAccess
{
    extension(CeresSteamCollisionDefinitions)
    {
        /// <summary>Native roots of the compiled Ceres steam hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => PrivateState.StaticField<CollisionRecordRuns<CeresSteamCollisionHitbox>>(typeof(CeresSteamCollisionDefinitions), "Lists").Pointers;
    }
}

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsComponentSequenceAccess
{
    extension(CeresSteamCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Number of collision components in this steam frame.</summary>
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
        /// <summary>Requires a word in the compiled bank-$86 projectile mechanics layout.</summary>
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
        internal static CorpseRottingTableEntry ReadEntry(CorpseRotTable table, int entryIndex) =>
            new(unchecked((short)table.YOffset(entryIndex)), table.Delay(entryIndex));
    }
}

/// <summary>Verification access to <see cref="CrateriaLightningPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class CrateriaLightningPaletteFxProgramDefinitionAccess
{
    extension(CrateriaLightningPaletteFxProgramDefinition self)
    {
        /// <summary>$8D:F765 live surface-lightning definition or $F769 unused dark-lightning definition.</summary>
        internal ushort DefinitionPointer => PrivateState.Property<bool>(self, "IsSurface") ? (ushort)0xf765 : (ushort)0xf769;

        /// <summary>Full lightning cycle including neutral holds, two flashes, and recovery.</summary>
        internal int CycleFrames => 240 * (1 + PrivateState.Property<int>(self, "NeutralCount")) + 2 * 9 + 5;

        /// <summary>Displayed records across neutral, active, and recovery phases.</summary>
        internal int DisplayedRecordsPerCycle => 1 + 14 + PrivateState.Property<int>(self, "NeutralCount") + 4 + 1;
    }
}

/// <summary>Verification access to <see cref="CrocomireBg2ScrollDefinitions"/> members production does not use.</summary>
internal static class CrocomireBg2ScrollDefinitionsAccess
{
    extension(CrocomireBg2ScrollDefinitions)
    {
        /// <summary>Twelve charge/step-back frames followed by five claw-motion frames.</summary>
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
        /// <summary>Visits compiled melting uploads in their native index order.</summary>
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
        /// <summary>Count of the fifteen native Dachora entry programs above.</summary>
        internal static int ProgramCount => ProgramEntries.Length;

        /// <summary>Reads a program root, timed-frame count, and whether its terminator loops.</summary>
        internal static DachoraInstructionProgram Program(int index)
        {
            ushort entry = ProgramEntries[index];
            (int frames, ushort terminator) = PrivateState.StaticField<InstructionProgramLayout>(typeof(DachoraInstructionProgramDefinitions), "Layout").FramesFrom(entry);
            return new(entry, frames, terminator == (ushort)CommonEnemyInstruction.Goto);
        }
    }
}

/// <summary>Verification access to <see cref="DeadMonsterRottingDefinitions.TransferRows"/> members production does not use.</summary>
internal static class DeadMonsterRottingDefinitionsTransferRowsAccess
{
    extension(DeadMonsterRottingDefinitions.TransferRows self)
    {
        /// <summary>Number of native dead-monster transfer rows in this view.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="DeadTorizoVramTransferDefinitions.PhaseRows"/> members production does not use.</summary>
internal static class DeadTorizoVramTransferDefinitionsPhaseRowsAccess
{
    extension(DeadTorizoVramTransferDefinitions.PhaseRows self)
    {
        /// <summary>Number of VRAM transfers scheduled in this Dead Torizo phase.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="DeadTourianCorpseInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class DeadTourianCorpseInstructionProgramDefinitionsAccess
{
    extension(DeadTourianCorpseInstructionProgramDefinitions)
    {
        /// <summary>Locates the sleep command after a Dead Tourian corpse program's first frame.</summary>
        internal static ushort SleepWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+4);
    }
}

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsAccess
{
    extension(DraygonCollisionDefinitions)
    {
        /// <summary>Empty OAM frames across both mirrored Draygon body sequences.</summary>
        internal static int EmptyOamFrameCount => 2 * (6 + 4 + 7 + 7);
    }
}

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsComponentSequenceAccess
{
    extension(DraygonCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Zero or one collision component depending on this frame's hitbox presence.</summary>
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

        /// <summary>Recognizes presentation operands among the bank-$86 projectile instruction programs.</summary>
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
        ushort highByte = unchecked((ushort)(SnesLittleEndian.HighByteInPlace(word)));
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

    /// <summary>Tests whether any requested named high-byte property is set.</summary>
    internal static bool HasAny(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) != 0;

    /// <summary>Tests whether all requested named high-byte properties are set.</summary>
    internal static bool HasAll(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) == flags;

    /// <summary>Validates and tests the requested extra-property bits in a native word.</summary>
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
            if (
                value is (< (byte)GameplayMessageId.EnergyTank or > (byte)GameplayMessageId.GravitySuit) and
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
        /// <summary>Searches compiled awakening mechanics words by bank-relative address.</summary>
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
        /// <summary>Reads a mechanics word from the compiled eye-beam program layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoInitialInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoInitialInstructionProgramDefinitions)
    {
        /// <summary>Searches the initial Torizo instruction stream's compiled mechanics words.</summary>
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
        /// <summary>Searches the jump-landing stream's compiled mechanics words.</summary>
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
        /// <summary>Reads a mechanics word from the left-foot orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftFootOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoLeftOrbInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoLeftTurnInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoLeftTurnInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left-turn instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoLeftTurnInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightOrbInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the right orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightOrbInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightSonicInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the right sonic instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightSonicInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoRightwardInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the rightward instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoRightwardInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStunnedInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStunnedInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoStunnedInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the stunned instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoStunnedInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitionsAccess
{
    extension(GoldenTorizoWalkingInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the walking instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(GoldenTorizoWalkingInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="GrowingShutterInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GrowingShutterInstructionProgramDefinitionsAccess
{
    extension(GrowingShutterInstructionProgramDefinitions)
    {
        /// <summary>The shutter's four successive growth stages.</summary>
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
        /// <summary>Maps the even dust variant parameter to one of six native entry programs.</summary>
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
            if (SnesAddressMath.BankNumber(longAddress) != self.Bank) return false;
            int bankAddress = SnesAddressMath.OffsetInBank(longAddress);
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
        /// <summary>Materializes hitboxes from a Kraid arm list in native order.</summary>
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
        /// <summary>Derives a quarter-health threshold from Kraid's captured initial health.</summary>
        internal ushort HealthQuarterThreshold(int index) => KraidHealthThresholdDefinitions.Quarter(self.InitialHealth, index);
    }
}

/// <summary>Verification access to <see cref="KraidFootCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidFootCollisionDefinitionsAccess
{
    extension(KraidFootCollisionDefinitions)
    {
        /// <summary>Computes a foot collision frame root from the fixed native record stride.</summary>
        internal static ushort FramePointer(int index) =>
            checked((ushort)(KraidFootCollisionDefinitions.FirstFrame + index * PrivateState.StaticField<int>(typeof(KraidFootCollisionDefinitions), "FrameByteCount")));
    }
}

/// <summary>Verification access to <see cref="KraidFootHitboxSequence"/> members production does not use.</summary>
internal static class KraidFootHitboxSequenceAccess
{
    extension(KraidFootHitboxSequence self)
    {
        /// <summary>Materializes the foot's single collision hitbox.</summary>
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

/// <summary>Independent compiled Mama Turtle family headers that verification compares with the cartridge.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogAccess
{
    /// <summary>Compiled Mama Turtle enemy definition.</summary>
    private static readonly RoomEnemyDefinition Mama = new(
        TileDataSize: 0x0c00,
        PalettePointer: 0x8b60,
        Health: 0x4e20,
        Damage: 0x00c8,
        Radius: new(0x0014, 0x0010),
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d6c,
        PartCount: 0x0005,
        Unused16: 0x0000,
        MainAiPointer: 0x8dd2,
        GrappleAiPointer: GrappleAiRoutine.Cancel,
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
        Radius: new(0x0008, 0x0005),
        Bank: 0xa2,
        HurtAiTime: 0x00,
        HurtSoundEffect: 0x0000,
        BossId: 0x0000,
        InitializationAiPointer: 0x8d9d,
        PartCount: 0x0001,
        Unused16: 0x0000,
        MainAiPointer: 0x912e,
        GrappleAiPointer: GrappleAiRoutine.Cancel,
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

    /// <summary>Resolves a compiled family header by its native bank-$A0 pointer.</summary>
    internal static bool TryGet(EnemyDefinitionId pointer, out RoomEnemyDefinition definition)
    {
        definition = pointer == EnemyDefinitionId.MamaTurtle ? Mama
            : pointer == EnemyDefinitionId.BabyTurtle ? Baby
            : default;
        return pointer is EnemyDefinitionId.MamaTurtle or EnemyDefinitionId.BabyTurtle;
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
        /// <summary>Native roots for the Oum collision hitbox lists.</summary>
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
        internal static MotherBrainCorpseRotEntry ReadEntry(ISnesMemoryBus memory, int entryIndex)
        {
            CorpseRottingTableEntry entry = CorpseRottingTableProcessor.ReadEntry(
                new EnemyScratchMemory(memory).MotherBrainRotTable, entryIndex);
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
        /// <summary>One external-call command occurs after the first frame of each hand-beam stage.</summary>
        internal static int ExternalCallCount => PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "StageCount");

        /// <summary>Locates the external-call opcode after a stage's initial timed frame.</summary>
        internal static ushort ExternalCallInstruction(int index)
        {
            if ((uint)index >= MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallCount) throw new ArgumentOutOfRangeException(nameof(index));
            return (ushort)(((int)(PrivateState.InvokeStatic(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "StageStart", (int)(index)))!) + PrivateState.StaticField<int>(typeof(MotherBrainHandBeamInstructionProgramDefinitions), "FrameBytes"));
        }

        /// <summary>Identifies bank-$86 hand-beam frame artwork operands, excluding callback mechanics.</summary>
        internal static bool IsPresentationByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
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
            PrivateState.Invoke(self, "RequestWalkForwardReallySlow", (ushort)(unchecked((ushort)(self.Body.Position.X + 0x0010))));
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
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            if (self.NeckMovementEnabled != 0)
            {
                ushort lowerAngle = self.LowerNeckAngle;
                ushort upperAngle = self.UpperNeckAngle;
                MotherBrainNeckMovement lowerIndex = self.LowerNeckMovementIndex;
                MotherBrainNeckMovement upperIndex = self.UpperNeckMovementIndex;
                MotherBrainNeckKinematics.StepAngles(
                    ref lowerAngle,
                    ref upperAngle,
                    ref lowerIndex,
                    ref upperIndex,
                    self.NeckAngleDelta,
                    self.BrainPosition.Y,
                    samus.Position.Y.Whole);
                PrivateState.SetProperty(self, "LowerNeckAngle", lowerAngle);
                PrivateState.SetProperty(self, "UpperNeckAngle", upperAngle);
                PrivateState.SetProperty(self, "LowerNeckMovementIndex", lowerIndex);
                PrivateState.SetProperty(self, "UpperNeckMovementIndex", upperIndex);
            }

            MotherBrainNeckGeometry geometry = MotherBrainNeckKinematics.CalculateGeometry(self.Body.Position, self.LowerNeckAngle, self.UpperNeckAngle, new MotherBrainNeckSegmentDistances());
            self.BrainPosition = geometry.Segment4;
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
        /// <summary>Reconstructs a timed flash word or terminal loop command from native addresses.</summary>
        internal static InstructionMechanicsWord MechanicsWord(int index)
        {
            if ((uint)index >= MotherBrainRoomPaletteProgramDefinitions.MechanicsWordCount) throw new IndexOutOfRangeException();
            return index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount
                ? new((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart + index * 4), PrivateState.StaticField<ushort>(typeof(MotherBrainRoomPaletteProgramDefinitions), "FlashImageTicks"))
                : new((ushort)(PrivateState.StaticField<ushort>(typeof(MotherBrainRoomPaletteProgramDefinitions), "LoopInstruction") + (index - MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount) * 2),
                    index == MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount ? MotherBrainRoomPaletteProgramDefinitions.GotoInstruction : MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        }

        /// <summary>Recognizes timed flash controls and the final loop in bank $A9.</summary>
        internal static bool IsCompiledMechanicsByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa90000) return false;
            int offset = (ushort)address - MotherBrainRoomPaletteProgramDefinitions.FlashStart;
            return (uint)offset < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 && offset % 4 < 2 ||
                offset >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 && offset < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount * 4 + 4;
        }

        /// <summary>Finds a flash image operand word containing the requested bank-$A9 byte.</summary>
        internal static bool TryGetPresentationWord(int address, out ushort wordAddress)
        {
            wordAddress = 0;
            if ((SnesAddressMath.BankBase(address)) != 0xa90000) return false;
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
        /// <summary>Eight six-byte aiming programs surround the turret.</summary>
        internal static int DirectionCount => 8;

        /// <summary>Selects a turret aiming program from the leftmost native root.</summary>
        internal static ushort TurretProgram(MotherBrainTurretDirection direction) =>
            MotherBrainTurretInstructionProgramDefinitions.DirectionProgram(MotherBrainTurretInstructionProgramDefinitions.TurretLeft, direction);

        /// <summary>Selects a bullet program from its leftmost native root.</summary>
        internal static ushort BulletProgram(MotherBrainTurretDirection direction) =>
            MotherBrainTurretInstructionProgramDefinitions.DirectionProgram(PrivateState.StaticField<ushort>(typeof(MotherBrainTurretInstructionProgramDefinitions), "BulletLeft"), direction);

        /// <summary>Advances six bytes per aiming direction after validating the eight-way selector.</summary>
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
        /// <summary>Number of casual-flame spawn entries in this schedule.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.ComponentSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsComponentSequenceAccess
{
    extension(PhantoonCollisionDefinitions.ComponentSequence self)
    {
        /// <summary>Number of components in this Phantoon collision frame.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonCollisionDefinitions.HitboxSequence"/> members production does not use.</summary>
internal static class PhantoonCollisionDefinitionsHitboxSequenceAccess
{
    extension(PhantoonCollisionDefinitions.HitboxSequence self)
    {
        /// <summary>Number of hitboxes in this Phantoon collision list.</summary>
        internal int Length => self.Count;
    }
}

/// <summary>Verification access to <see cref="PhantoonWaveHdmaState"/> members production does not use.</summary>
internal static class PhantoonWaveHdmaStateAccess
{
    extension(PhantoonWaveHdmaState self)
    {
        /// <summary>Only the initialized prefix of the room's wave-scroll cycle.</summary>
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

        /// <summary>Native roots of Ridley's compiled contact hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => PrivateState.StaticField<CollisionRecordRuns<RidleyCollisionHitbox>>(typeof(RidleyCollisionDefinitions), "Lists").Pointers;
    }
}

/// <summary>Verification access to <see cref="RoomEnemyAuxiliaryDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyAuxiliaryDefinitionCatalogAccess
{
    extension(RoomEnemyAuxiliaryDefinitionCatalog)
    {
        /// <summary>The five auxiliary header identities missing from room lists.</summary>
        internal static IEnumerable<EnemyDefinitionId> Pointers => PrivateState.StaticField<IReadOnlyDictionary<EnemyDefinitionId, RoomEnemyDefinition>>(typeof(RoomEnemyAuxiliaryDefinitionCatalog), "Definitions").Keys;
    }
}

/// <summary>Verification access to <see cref="RoomEnemyDefinitionCatalog"/> members production does not use.</summary>
internal static class RoomEnemyDefinitionCatalogAccess
{
    extension(RoomEnemyDefinitionCatalog)
    {
        /// <summary>Enumerates every compiled native definition pointer for ROM-oracle tests.</summary>
        internal static IEnumerable<EnemyDefinitionId> Pointers => PrivateState.StaticField<(EnemyDefinitionId Pointer, RoomEnemyDefinition Definition)[]>(typeof(RoomEnemyDefinitionCatalog), "Definitions").Select(entry => entry.Pointer);
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
            Ensure.NotNull(bus);
            Ensure.NotNull(projectiles);
            Ensure.NotNull(sharedProjectiles);
            var kraid = self.KraidBehavior.Kraid;
            RoomEnemySlot body = self.SlotTable.Slots[0];
            if (kraid is null || body.EnemyDefinitionPointer != EnemyDefinitionId.Kraid ||
                body.Properties.HasAny(EnemyProperties.Deleted))
                return 0;
            var shots = new KraidBehavior.KraidShotSlots(projectiles, sharedProjectiles);
            return self.KraidBehavior.ResolveKraidMouthProjectileHits(body, kraid, shots) +
                self.KraidBehavior.ResolveKraidBodyProjectileHits(body, kraid, shots);
        }

        /// <summary>Botwoon's live state while its room is loaded.</summary>
        internal BotwoonEnemyState? Botwoon => self.BotwoonBehavior._botwoonState;

        /// <summary>Per-slot Evir states, indexed by enemy slot.</summary>
        internal IReadOnlyList<EvirEnemyState?> EvirStates => self.EvirBehavior._evirStates;

        /// <summary>Special drops a dying Metroid has requested this frame.</summary>
        internal IReadOnlyList<MetroidDropRequest> MetroidDropRequests =>
            self.MetroidBehavior._metroidDropRequests;


        /// <summary>Companion/corpse state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadSidehopperEnemyState?> DeadSidehoppers =>
            self.DeadSidehopperBehavior._deadSidehopperStates;

        /// <summary>The loaded Dead Torizo's typed extended-WRAM state.</summary>
        internal DeadTorizoEnemyState? DeadTorizo => self.DeadTorizoBehavior._deadTorizo;

        /// <summary>Alternating VRAM records authored by the most recent actor frame.</summary>
        internal IReadOnlyList<VramWriteEntry> LastDeadTorizoVramTransfers =>
            self.DeadTorizoBehavior._deadTorizoFrameVramTransfers;

        /// <summary>Typed state for every physical slot currently owned by a Skree.</summary>
        internal IReadOnlyList<SkreeEnemyState?> SkreeStates => self.SkreeBehavior._skreeStates;

        /// <summary>Typed Spark state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SparkEnemyState?> SparkStates => self.SparkBehavior._sparkStates;

        /// <summary>Typed state for every physical slot currently owned by a Zoa.</summary>
        internal IReadOnlyList<ZoaEnemyState?> ZoaStates => self.ZoaBehavior._zoaStates;

        /// <summary>Typed state for both graphics and hitbox records in all physical slots.</summary>
        internal IReadOnlyList<HibashiEnemyState?> HibashiStates => self.HibashiBehavior._hibashiStates;

        /// <summary>Typed actor extensions in fixed native slot order.</summary>
        internal IReadOnlyList<ElevatorEnemyState?> ElevatorStates => self.ElevatorBehavior._elevatorStates;

        /// <summary>Kago extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<KagoEnemyState?> KagoStates => self.KagoBehavior._kagoStates;

        /// <summary>Typed Waver state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<WaverEnemyState?> WaverStates => self.WaverBehavior._waverStates;

        /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
        internal IReadOnlyList<CrawlerEnemyState?> CrawlerStates => self.CrawlersBehavior._crawlerStates;

        /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
        internal IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates => self.WorkRobotBehavior._workRobotStates;

        /// <summary>Typed state for every physical enemy slot currently owned by a Rio.</summary>
        internal IReadOnlyList<RioEnemyState?> RioStates => self.RioBehavior._rioStates;

        /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
        internal IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates => self.ZebetiteBehavior._zebetiteStates;

        /// <summary>
        /// Per-physical-slot Shaktool state. A live retail group occupies seven consecutive
        /// non-null entries; every other room slot remains null.
        /// </summary>
        internal IReadOnlyList<ShaktoolSegmentState?> ShaktoolSegments => self.ShaktoolBehavior._shaktoolSegments;

        /// <summary>Typed state for every physical slot currently owned by a Mochtroid.</summary>
        internal IReadOnlyList<MochtroidEnemyState?> MochtroidStates => self.MochtroidBehavior._mochtroidStates;

        /// <summary>Typed state for each physical slot currently occupied by a Yard.</summary>
        internal IReadOnlyList<YardEnemyState?> YardStates => self.YardBehavior._yardStates;

        /// <summary>Typed Metaree state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MetareeEnemyState?> MetareeStates => self.MetareeBehavior._metareeStates;

        /// <summary>Typed state for every physical Owtch-capable enemy slot.</summary>
        internal IReadOnlyList<OwtchEnemyState?> OwtchStates => self.OwtchBehavior._owtchStates;

        /// <summary>Typed state for all physical slots occupied by Fune or Namihe.</summary>
        internal IReadOnlyList<FuneNamiheEnemyState?> FuneNamiheStates => self.FuneNamiheBehavior._funeNamiheStates;

        /// <summary>Typed Stoke state for each of the 32 physical enemy slots.</summary>
        internal IReadOnlyList<StokeEnemyState?> StokeStates => self.StokeBehavior._stokeStates;

        /// <summary>Fake Kraid extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<FakeKraidEnemyState?> FakeKraidStates => self.FakeKraidBehavior._fakeKraidStates;

        /// <summary>Typed state for every physical slot currently owned by this family.</summary>
        internal IReadOnlyList<NorfairLavaJumpingEnemyState?> NorfairLavaJumpingEnemyStates =>
            self.NorfairLavaJumpingEnemyBehavior._norfairLavaJumpingEnemyStates;

        /// <summary>Typed Fireflea state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<FirefleaEnemyState?> FirefleaStates => self.FirefleaBehavior._firefleaStates;

        /// <summary>All 32 physical bank-$B4 slots, including inactive ones.</summary>
        internal IReadOnlyList<RoomSpriteObjectSlot> RoomSpriteObjects => self.SpriteObjects.Slots;

        /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
        internal SporeSpawnEnemyState? SporeSpawn => self.SporeSpawnBehavior._sporeSpawn;

        /// <summary>Typed state for all 32 physical Boyon-capable enemy slots.</summary>
        internal IReadOnlyList<BoyonEnemyState?> BoyonStates => self.BoyonBehavior._boyonStates;

        /// <summary>Typed state for each physical escape-Etecoon slot.</summary>
        internal IReadOnlyList<EscapeEtecoonEnemyState?> EscapeEtecoonStates =>
            self.EscapeAnimalsBehavior._escapeEtecoonStates;

        /// <summary>Nuclear Waffle extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
            self.NuclearWaffleBehavior._nuclearWaffleStates;

        /// <summary>Typed state for every physical slot currently owned by the hopper family.</summary>
        internal IReadOnlyList<HopperEnemyState?> HopperStates => self.HoppersBehavior._hopperStates;

        /// <summary>Typed face-block state in fixed physical enemy-slot order.</summary>
        internal IReadOnlyList<BlueBrinstarFaceBlockEnemyState?> BlueBrinstarFaceBlockStates =>
            self.BlueBrinstarFaceBlockBehavior._blueBrinstarFaceBlockStates;

        /// <summary>Frame-local drops requested by destroyed free-floating spores.</summary>
        internal IReadOnlyList<SporeSpawnDropRequest> SporeSpawnDropRequests =>
            self.SporeSpawnBehavior._sporeSpawnDropRequests;

        /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
            self.DeadTourianBehavior._deadTourianCorpseStates;

        /// <summary>Typed Atomic state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AtomicEnemyState?> AtomicStates => self.AtomicBehavior._atomicStates;

        /// <summary>Typed state for every physical Cacatac-capable enemy slot.</summary>
        internal IReadOnlyList<CacatacEnemyState?> CacatacStates => self.CacatacBehavior._cacatacStates;

        /// <summary>Typed state for every physical enemy slot currently owned by a Rinka.</summary>
        internal IReadOnlyList<RinkaEnemyState?> RinkaStates => self.RinkaBehavior._rinkaStates;

        /// <summary>Walking Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WalkingSpacePirateEnemyState?> WalkingSpacePirateStates =>
            self.WalkingSpacePirateBehavior._walkingSpacePirateStates;

        /// <summary>Wall Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WallSpacePirateEnemyState?> WallSpacePirateStates =>
            self.WallSpacePirateBehavior._wallSpacePirateStates;

        /// <summary>Magdollite extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<MagdolliteEnemyState?> MagdolliteStates => self.MagdolliteBehavior._magdolliteStates;

        /// <summary>Typed state for each physical enemy slot currently owned by a Metroid.</summary>
        internal IReadOnlyList<MetroidEnemyState?> MetroidStates => self.MetroidBehavior._metroidStates;

        /// <summary>Baby Metroid actor state during the Mother Brain encounter.</summary>
        internal ShitroidEnemyState? Shitroid => self.ShitroidBehavior._shitroid;

        /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
        internal IReadOnlyList<ushort> ActiveEnemyIndexes => self.ProcessingWindow._activeEnemyIndexes;

        /// <summary>Typed native state for every physical Etecoon slot.</summary>
        internal IReadOnlyList<EtecoonEnemyState?> EtecoonStates => self.EtecoonBehavior._etecoonStates;

        /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
        internal IReadOnlyList<ushort> InteractiveEnemyIndexes => self.ProcessingWindow._interactiveEnemyIndexes;

        /// <summary>Typed Alcoon state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AlcoonEnemyState?> AlcoonStates => self.AlcoonBehavior._alcoonStates;

        /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
        internal IReadOnlyList<DragonEnemyState?> DragonStates => self.DragonBehavior._dragonStates;

        /// <summary>The room's terminated bank-$B4 graphics-set records.</summary>
        internal IReadOnlyList<RoomEnemyGraphicsSetEntry> GraphicsSet => self.PopulationLoader._graphicsSet;

        /// <summary>Typed state for all 32 physical Puyo-capable enemy slots.</summary>
        internal IReadOnlyList<PuyoEnemyState?> PuyoStates => self.PuyoBehavior._puyoStates;

        /// <summary>Typed Bull state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BullEnemyState?> BullStates => self.BullBehavior._bullStates;

        /// <summary>Typed Skultera state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SkulteraEnemyState?> SkulteraStates => self.SkulteraBehavior._skulteraStates;

        /// <summary>Typed state for every physical slot currently owned by Sbug/Sbug2.</summary>
        internal IReadOnlyList<SbugEnemyState?> SbugStates => self.SbugBehavior._sbugStates;

        /// <summary>Typed state for every physical Pipe Bug enemy slot.</summary>
        internal IReadOnlyList<PipeBugEnemyState?> PipeBugStates => self.PipeBugsBehavior._pipeBugStates;

        /// <summary>Typed Ki-Hunter state for every body and wing physical enemy slot.</summary>
        internal IReadOnlyList<KiHunterEnemyState?> KiHunterStates => self.KiHunterBehavior._kiHunterStates;

        /// <summary>Typed Choot state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<ChootEnemyState?> ChootStates => self.ChootBehavior._chootStates;

        /// <summary>Pickup requests emitted by shot destroyable flames in this room load.</summary>
        internal IReadOnlyList<PhantoonFlameDropRequest> PhantoonFlameDropRequests =>
            self.PhantoonBehavior._phantoonFlameDropRequests;

        /// <summary>Typed native state for every loaded Yapping Maw slot.</summary>
        internal IReadOnlyList<YappingMawEnemyState?> YappingMawStates => self.YappingMawBehavior._yappingMawStates;

        /// <summary>Typed Oum state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MaridiaLargeSnailEnemyState?> MaridiaLargeSnailStates =>
            self.MaridiaLargeSnailBehavior._maridiaLargeSnailStates;

        /// <summary>Typed Beetom state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BeetomEnemyState?> BeetomStates => self.BeetomBehavior._beetomStates;

        /// <summary>Typed Tripper/Kamer state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<PlatformEnemyState?> PlatformStates => self.PlatformsBehavior._platformStates;

        /// <summary>Bomb Torizo's state while definition $EEFF owns slot zero.</summary>
        internal TorizoEnemyState? BombTorizo =>
            self.BombTorizoBehavior._torizoState is { IsGolden: false } ? self.BombTorizoBehavior._torizoState : null;

        /// <summary>Growing shutter extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<GrowingShutterEnemyState?> GrowingShutterStates => self.ShuttersBehavior._growingShutterStates;

        /// <summary>Pickup requests emitted by shot Chozo orbs during this room load.</summary>
        internal IReadOnlyList<TorizoOrbDropRequest> TorizoOrbDropRequests =>
            self.BombTorizoBehavior._torizoOrbDropRequests;

        /// <summary>Number of live actors in the shared bank-$86 enemy-projectile pool.</summary>
        internal int ActiveEnemyProjectileCount =>
            self.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);

        /// <summary>
        /// Executes the projectile portion of the gameplay frame after enemy instructions have
        /// had their opportunity to spawn a fireball. This is the same producer/consumer order
        /// as EnemyMain followed by bank $86's enemy-projectile handler.
        /// </summary>
        internal void StepEnemyProjectiles(
            RoomLevelData level,
            SamusState? samus,
            Position16<RoomPixels> camera = default,
            byte? nmiFrameCounter8 = null,
            SamusBombProjectileSystem? samusBombs = null)
        {
            self.StepEnemyProjectileInstructions(level, samus, camera, nmiFrameCounter8, samusBombs);
            self.Projectiles.ResolveEnemyProjectileSamusHits(samus);
        }

        /// <summary>
        /// Emits both native enemy-projectile priority passes for focused diagnostics which do
        /// not draw Samus between them. Gameplay must call the two phase-specific methods so
        /// property bit <c>$1000</c> determines which side of Samus owns each OAM record.
        /// </summary>
        internal void DrawEnemyProjectiles(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false)
        {
            Ensure.NotNull(oam);
            self.Room.EnsureLoaded();

            self.SpriteObjects.DrawRoomSpriteObjects(oam, camera);
            self.Projectiles.DrawEnemyProjectilePass(oam, camera, EnemyProjectileDrawPriority.High, timeIsFrozen);
            self.Projectiles.DrawEnemyProjectilePass(oam, camera, EnemyProjectileDrawPriority.Low, timeIsFrozen);
        }

        /// <summary>Standalone contact probe; runtime dispatches body before AI and tail during tail update.</summary>
        internal bool ResolveRidleySamusContact(SamusState samus) =>
            (bool)self.CeresRidleyBehavior.ResolveRidleyBodySamusContact(samus)! ||
            (bool)self.RidleyTail.ResolveRidleyTailSamusContact(samus)!;
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
            1 => SnesLittleEndian.HighByte(self.DoorPointer),
            2 => (byte)self.BaseYPosition,
            3 => SnesLittleEndian.HighByte(self.BaseYPosition),
            4 => (byte)self.TargetYPosition,
            5 => SnesLittleEndian.HighByte(self.TargetYPosition),
            6 => (byte)self.PackedYVelocity,
            7 => (byte)(SnesEightEight.WholeOf(self.PackedYVelocity)),
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

        /// <summary>Combines two adjacent native FX-record bytes in little-endian order.</summary>
        internal ushort ReadWord(int offset)
        {
            if ((uint)offset > RoomFxRomData.Record.ByteCount - sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(offset));
            return SnesLittleEndian.Word(self.ReadByte(offset), self.ReadByte(offset + 1));
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
            Ensure.NotNull(definition);
            PrivateState.Invoke(self, "LoadCore", (ISnesAddressSpace)(bus), (SnesVram)(vram), (SnesCgram)(cgram), new RoomFxLoadSource
            {
                FxPointer = definition.Pointer,
                DoorPointer = definition.DoorPointer,
                RandomNumber = randomNumber,
                RoomHeaderPointer = 0,
                SuppliedDefinition = definition,
            });
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
            Ensure.NotNull(definition);
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
        /// <summary>Finds a presentation operand word containing the requested sprite-object byte.</summary>
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
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);

            (samus.Kinematics.YSpeed, samus.Kinematics.YSubspeed) =
                SamusVerticalMotionDefinitions.Launch(SamusLiquidMedium.Air, highJump: false, wallJump: false);
            SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(samus);
            SamusAerialMovement.ConfigureDryAirGravity(bus, samus);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        }

        /// <summary>
        /// Publishes the normal-air gravity pair selected by <c>$90:9C5B</c>. The normal frame
        /// pipeline refreshes these environment-dependent words before movement even when the
        /// pose change was a walk-off rather than <c>Make_Samus_Jump</c>.
        /// </summary>
        internal static void ConfigureDryAirGravity(ISnesAddressSpace bus, SamusState samus)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            (samus.Kinematics.YAcceleration, samus.Kinematics.YSubacceleration) =
                SamusVerticalMotionDefinitions.Gravity(SamusLiquidMedium.Air);
        }
    }
}

/// <summary>Verification access to <see cref="SamusAnimationDelayPrograms"/> members production does not use.</summary>
internal static class SamusAnimationDelayProgramsAccess
{
    extension(SamusAnimationDelayPrograms)
    {
        /// <summary>Compiled pose-animation delay segments in their native order.</summary>
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
    /// <summary>Tests that every requested equipment bit is present in the native word.</summary>
    internal static bool HasAll(this ushort word, SamusEquipmentFlags flags) =>
        ((SamusEquipmentFlags)word & flags) == flags;

    /// <summary>Tests that every requested beam bit is present in the native word.</summary>
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

    /// <summary>Preserves the selected equipment bits as a WRAM-compatible unsigned word.</summary>
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
            Position16<RoomPixels> anchor,
            byte ropeLength,
            SnesAngle angle,
            short angularVelocity,
            bool faceRight)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            if (ropeLength is < 8 or > 63)
                throw new ArgumentOutOfRangeException(nameof(ropeLength), "Retail connected rope length is 8..63 pixels.");
            if (angularVelocity is < -SamusGrappleRomData.Physics.MaximumAngularVelocity or
                > SamusGrappleRomData.Physics.MaximumAngularVelocity)
                throw new ArgumentOutOfRangeException(nameof(angularVelocity));
            if (samus.Grapple.Phase != GrapplePhase.Inactive)
                throw new InvalidOperationException("A grapple state is already active.");

            SamusGrappleState grapple = samus.Grapple;
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            grapple.Anchor = anchor;
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
                ? SamusPoseId.GrappleSwingRightPose
                : SamusPoseId.GrappleSwingLeftPose;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus, initialFrame: 0);

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
            Ensure.NotNull(samus);
            ushort bottom = samus.Kinematics.BottomBoundary;
            PrivateState.SetProperty(self, "LiquidPhysicsType", self.FxType switch
            {
                RoomFxType.Lava or RoomFxType.Acid
                    when ((bool)(PrivateState.InvokeStatic(typeof(SamusLiquidPhysicsState), "IsBelowSurface", (ushort)(self.LavaAcidYPosition), (ushort)(bottom)))!) => SamusLiquidMedium.LavaOrAcid,
                RoomFxType.Water or RoomFxType.TourianEntranceStatue when ((bool)(PrivateState.Invoke(self, "WaterAffectsBoundary", (ushort)(bottom)))!) => SamusLiquidMedium.Water,
                _ => SamusLiquidMedium.Air,
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
            SamusPoseId currentPose,
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
        internal IReadOnlyList<SamusProjectileTrailSlot> TrailSlots => self.SlotTable._trailSlots;

        /// <summary>Debugger-friendly count of slots whose left stream still owns the slot.</summary>
        internal int ActiveTrailCount => self.SlotTable._trailSlots.Count(slot => slot.IsActive);
    }
}

/// <summary>Verification access to <see cref="SamusProjectileTrailSlot"/> members production does not use.</summary>
internal static class SamusProjectileTrailSlotAccess
{
    extension(SamusProjectileTrailSlot self)
    {
        /// <summary>Converts the trail slot index to its two-byte native slot offset.</summary>
        internal int NativeByteIndex => self.SlotIndex * 2;

        /// <summary>The left trail stream owns the slot while its instruction timer is active.</summary>
        internal bool IsActive => self.Left.InstructionTimer != 0;
    }
}

/// <summary>Verification access to <see cref="SamusProjectileTypeWord"/> members production does not use.</summary>
internal static class SamusProjectileTypeWordAccess
{
    extension(SamusProjectileTypeWord self)
    {
        /// <summary>Tests the projectile word's native live marker.</summary>
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
            Position16<RoomPixels> layer1)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            PrivateState.Invoke(self, "StepReleasedCrashEcho", (SamusState)(samus), layer1, PrivateState.Field<object>(self, "_firstReleasedCrashEcho"));
            PrivateState.Invoke(self, "StepReleasedCrashEcho", (SamusState)(samus), layer1, PrivateState.Field<object>(self, "_secondReleasedCrashEcho"));
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
            set => self.Pose = value;
        }

        /// <summary>
        /// Compatibility name for the `$13/$14` Fire subset. Keeping this narrow wrapper makes
        /// existing focused tests readable while all input-table exits share one native handler.
        /// </summary>
        internal bool TryApplySpinToNormalJumpFireTransition(
            ISnesAddressSpace bus,
            RoomLevelData level,
            SamusPoseId targetPose,
            ushort nmiFrameCounter,
            ushort controllerNewInput,
            RoomPlmSystem? plms = null)
        {
            if (targetPose is not (SamusPoseId.NormalJumpGunExtendedRightPose or SamusPoseId.NormalJumpGunExtendedLeftPose))
            {
                throw new InvalidOperationException(
                    $"Spin-fire compatibility route requires pose $13/$14, not ${(int)targetPose:X2}.");
            }
            return self.PoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(
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
        /// side of movement through <see cref="SamusPoseMetadata.PublishBombJumpDirection"/> and
        /// <see cref="SamusPoseMetadata.TrySetupPublishedBombJump"/>.
        /// </summary>
        internal void RequestMorphedBombJump(byte direction)
        {
            if (!SamusPoseMetadata.IsStableBallPose(self.Pose))
            {
                throw new InvalidOperationException(
                    $"The morphed bomb-jump fixture requires a stable ball pose, not ${(int)self.Pose:X2}.");
            }

            self.PoseMetadata.PublishBombJumpDirection(direction);
            PrivateState.Invoke(self.PoseMetadata, "ArmPublishedBombJump");
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
        /// <summary>Accepts nonzero forward distances below the signed-wrap boundary.</summary>
        internal static void RequireDistance(ushort pixelDistance)
        {
            if (pixelDistance is 0 or >= 0x8000)
                throw new ArgumentOutOfRangeException(nameof(pixelDistance));
        }
    }

    extension(ScrollBoundaryCamera self)
    {
        /// <summary>
        /// Replaces the integer X speed word, then moves the ideal and integer layer-one X words
        /// to <paramref name="target"/>; fractions and the Y axis are untouched.
        /// </summary>
        private void ApplyDebugStimulusX(ushort pixelDistance, ushort target)
        {
            PrivateState.SetProperty(self, "Speed", self.Speed.WithX(SnesSignedSixteenSixteen.FromParts(unchecked((short)pixelDistance), self.Speed.X.Fraction)));
            PrivateState.SetProperty(self, "IdealPosition", self.IdealPosition.WithX(target));
            PrivateState.SetProperty(self, "Position", self.Position.WithX(self.Position.X.WithWhole(target)));
        }

        /// <summary>The vertical counterpart of <see cref="ApplyDebugStimulusX"/>.</summary>
        private void ApplyDebugStimulusY(ushort pixelDistance, ushort target)
        {
            PrivateState.SetProperty(self, "Speed", self.Speed.WithY(SnesSignedSixteenSixteen.FromParts(unchecked((short)pixelDistance), self.Speed.Y.Fraction)));
            PrivateState.SetProperty(self, "IdealPosition", self.IdealPosition.WithY(target));
            PrivateState.SetProperty(self, "Position", self.Position.WithY(self.Position.Y.WithWhole(target)));
        }

        /// <summary>Applies a debug rightward stimulus, then runs <c>$80:A641</c>.</summary>
        internal void MoveRight(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusX(pixelDistance, unchecked((ushort)(self.PixelPosition.X + pixelDistance)));
            PrivateState.Invoke(self, "HandleScrollingRight");
        }

        /// <summary>Applies a debug leftward stimulus, then runs <c>$80:A6BB</c>.</summary>
        internal void MoveLeft(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusX(pixelDistance, unchecked((ushort)(self.PixelPosition.X - pixelDistance)));
            PrivateState.Invoke(self, "HandleScrollingLeft");
        }

        /// <summary>Applies a debug downward stimulus, then runs <c>$80:A893</c>.</summary>
        internal void MoveDown(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusY(pixelDistance, unchecked((ushort)(self.PixelPosition.Y + pixelDistance)));
            PrivateState.Invoke(self, "HandleScrollingDown");
        }

        /// <summary>Applies a debug upward stimulus, then runs <c>$80:A936</c>.</summary>
        internal void MoveUp(ushort pixelDistance)
        {
            ScrollBoundaryCamera.RequireDistance(pixelDistance);
            self.ApplyDebugStimulusY(pixelDistance, unchecked((ushort)(self.PixelPosition.Y - pixelDistance)));
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
        /// <summary>Recognizes either byte of the compiled Skree or Metaree mechanics words.</summary>
        internal static bool IsCompiledMechanicsByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
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
        /// <summary>Number of compiled Space Pirate collision component frames.</summary>
        internal static int FrameCount => PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").Count;

        /// <summary>Number of compiled Space Pirate hitbox lists.</summary>
        internal static int ListCount => PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionHitbox>>(typeof(SpacePirateCollisionDefinitions), "Lists").Count;

        /// <summary>Pairs a native frame pointer with its compiled component record.</summary>
        internal static SpacePirateCollisionFrame Frame(int index) =>
            new(PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").PointerAt(index), PrivateState.StaticField<CollisionRecordRuns<SpacePirateCollisionComponent>>(typeof(SpacePirateCollisionDefinitions), "Frames").RecordAt(index));

        /// <summary>Pairs a native list pointer with its compiled hitbox record.</summary>
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
        (ushort)SporeSpawnHitboxList.ClosedHead,
        (ushort)SporeSpawnHitboxList.OpenHead,
        (ushort)SporeSpawnHitboxList.ExtendedHead,
        (ushort)SporeSpawnHitboxList.MovingHead0,
        (ushort)SporeSpawnHitboxList.MovingHead1,
        (ushort)SporeSpawnHitboxList.MovingHead2,
        (ushort)SporeSpawnHitboxList.MovingHead3,
        (ushort)SporeSpawnHitboxList.TrailingShotPoint,
        (ushort)SporeSpawnHitboxList.MirroredTrailingShotPoint,
        (ushort)SporeSpawnHitboxList.TrailingDudPoint,
        (ushort)SporeSpawnHitboxList.MovingHead4,
        (ushort)SporeSpawnHitboxList.MovingHead5,
    ];

    extension(SporeSpawnCollisionDefinitions)
    {
        /// <summary>Twelve compiled head and movement collision frames.</summary>
        internal static int FrameCount => 12;

        /// <summary>Number of named Spore Spawn hitbox roots in the native roster.</summary>
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
        /// <summary>Empty, mirrored facing, and jump-back frame totals.</summary>
        internal static int FrameCount => 2 + PrivateState.StaticProperty<int>(typeof(TorizoCollisionDefinitions), "MirroredCount") * 2 + PrivateState.StaticField<GoldenTorizoCollisionComponent[][]>(typeof(TorizoCollisionDefinitions), "JumpBackFrames").Length;

        /// <summary>Every frame pointer, derived from the four runs.</summary>
        internal static IEnumerable<ushort> FramePointers
        {
            get
            {
                foreach (object emptyFrame in Enum.GetValues(PrivateState.Nested(typeof(TorizoCollisionDefinitions), "EmptyFrame")))
                    yield return Convert.ToUInt16(emptyFrame, System.Globalization.CultureInfo.InvariantCulture);
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

        /// <summary>Native roots of the fixed Torizo hitbox-list roster.</summary>
        internal static IEnumerable<ushort> HitboxPointers => ListPointers;

        /// <summary>Checks membership in the compiled Torizo component-frame set.</summary>
        internal static bool HasFrame(ushort frame) => TorizoCollisionDefinitions.TryGetComponents(frame, out _);

        /// <summary>Finds components only after a pointer matches a compiled native frame root.</summary>
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
        /// <summary>Reads a mechanics word from the falling-left instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoFallingLeftInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionProgramDefinitionsAccess
{
    extension(TorizoInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the common Torizo instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoInstructionVramTransferDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionVramTransferDefinitionsAccess
{
    extension(TorizoInstructionVramTransferDefinitions)
    {
        /// <summary>Compiled native VRAM transfer records in source order.</summary>
        internal static ReadOnlySpan<TorizoInstructionVramTransferDefinition> All => PrivateState.StaticField<TorizoInstructionVramTransferDefinition[]>(typeof(TorizoInstructionVramTransferDefinitions), "Entries");
    }
}

/// <summary>Verification access to <see cref="TorizoJumpBackInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackInstructionProgramDefinitionsAccess
{
    extension(TorizoJumpBackInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the jump-back instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            PrivateState.StaticField<InstructionProgramLayout>(typeof(TorizoJumpBackInstructionProgramDefinitions), "Layout").TryReadMechanicsWord(address, out value);
    }
}

/// <summary>Verification access to <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitionsAccess
{
    extension(TorizoJumpBackLeftInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left jump-back instruction layout.</summary>
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
