namespace SuperMetroid.Core.Audio;

public sealed partial class ManagedSpcPlayer
{
    /// <summary>
    /// One sound-effect service of library <paramref name="libraryIndex"/>: Handle CPU IO
    /// ($1EE7/$3157/$4706) followed by Write/read CPU IO ($1621).
    /// </summary>
    /// <remarks>
    /// The handler acts on the value latched by the previous service and starts a sound only
    /// when that value changed. $1621 then echoes the acted-on value and latches the CPU's
    /// current write, so a request is echoed one service after the service that reads it.
    /// </remarks>
    private void HandleSoundLibraryCommand(int libraryIndex)
    {
        int port = libraryIndex + (byte)ApuPort.SoundLibrary1;
        byte previous = previousSoundCommandReads[libraryIndex];
        byte command = soundCommandReads[libraryIndex];
        previousSoundCommandReads[libraryIndex] = command;
        ApplySoundLibraryCommand(libraryIndex, changed: command != previous, command);
        portsToSnes[port] = command;
        soundCommandReads[libraryIndex] = inputPorts[port];
    }

    private void ApplySoundLibraryCommand(int libraryIndex, bool changed, byte command)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        bool reject = !changed || command == 0 || libraryIndex switch
        {
            0 => command != 1 && command != 2 && library.Priority != 0,
            1 => command != 0x71 && command != 0x7e && library.Priority != 0, // allow(HardwareValue): retail SFX2 cancellation overrides
            2 => command != 1 && (library.Mode == 2 || command != 2 && library.Priority != 0),
            _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
        };
        if (reject)
        {
            if (library.CurrentSound != 0)
                ProcessSoundLibrary(libraryIndex);
            return;
        }

        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.CurrentSound != 0)
        {
            library.EnabledVoices = 0;
            foreach (ManagedSpcSoundChannel channel in libraryChannels)
                ResetSoundChannel(library, channel);
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            channel.Legato = 0;

        int tableIndex = command - 1;
        if ((uint)tableIndex >= SpcSoundEffectTables.CommandCount(libraryIndex))
        {
            throw new InvalidDataException(
                $"SPC sound library {libraryIndex + 1} has no command ${command:X2}.");
        }
        library.CurrentSoundIndex = unchecked((byte)(tableIndex * 2));
        library.CurrentPointer = SpcSoundEffectTables.StreamPointer(libraryIndex, command);
        library.CurrentSound = command;

        byte configuration = SpcSoundEffectTables.Configuration(libraryIndex, command);
        ConfigureSoundLibrary(libraryIndex, library, configuration);
    }

    /// <summary>Applies native allocation-policy writes for a validated sound library.</summary>
    /// <remarks>#1165 original-handler proof covers all policy cases. Mode is unchanged
    /// except for library-three cancel/low-health operations; low-health preserves priority.
    /// Native handler identities are documented on the SpcLibraryNPolicy catalog members.</remarks>
    private static void ConfigureSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        byte configuration)
    {
        if (libraryIndex == 0)
        {
            (byte voices, byte priority) = (SpcLibrary1Policy)configuration switch
            {
                SpcLibrary1Policy.OneVoiceLowPriority => ((byte)1, (byte)0),
                SpcLibrary1Policy.OneVoiceHighPriority => ((byte)1, (byte)1),
                SpcLibrary1Policy.TwoVoicesLowPriority => ((byte)2, (byte)0),
                SpcLibrary1Policy.ThreeVoicesHighPriority => ((byte)3, (byte)1),
                SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => ((byte)4, (byte)0),
                _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }
        if (libraryIndex == 1)
        {
            (byte voices, byte priority) = (SpcLibrary2Policy)configuration switch
            {
                SpcLibrary2Policy.OneVoiceLowPriority => ((byte)1, (byte)0),
                SpcLibrary2Policy.OneVoiceHighPriority => ((byte)1, (byte)1),
                SpcLibrary2Policy.TwoVoicesLowPriority => ((byte)2, (byte)0),
                SpcLibrary2Policy.TwoVoicesHighPriority => ((byte)2, (byte)1),
                _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }

        switch ((SpcLibrary3Policy)configuration)
        {
            case SpcLibrary3Policy.CancelAndClearLowHealthMode:
                library.VoicesToSetup = 1;
                library.Mode = 0;
                library.Priority = 1;
                break;
            case SpcLibrary3Policy.LowHealthModePreservePriority:
                library.VoicesToSetup = 1;
                library.Mode = 2;
                break;
            case SpcLibrary3Policy.OneVoiceLowPriority:
                library.VoicesToSetup = 1;
                library.Priority = 0;
                break;
            case SpcLibrary3Policy.TwoVoicesLowPriority:
                library.VoicesToSetup = 2;
                library.Priority = 0;
                break;
            case SpcLibrary3Policy.TwoVoicesHighPriority:
                library.VoicesToSetup = 2;
                library.Priority = 1;
                break;
            case SpcLibrary3Policy.OneVoiceHighPriority:
                library.VoicesToSetup = 1;
                library.Priority = 1;
                break;
            default:
                throw new InvalidDataException($"Unknown SPC SFX3 configuration {configuration}.");
        }
    }

    private void ProcessSoundLibrary(int libraryIndex)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.InitializationFlag != SpcDriverData.SoundEffects.Active)
        {
            InitializeSoundLibrary(libraryIndex, library, libraryChannels);
            for (int index = 0; index < libraryChannels.Length; index++)
            {
                ManagedSpcSoundChannel channel = libraryChannels[index];
                channel.Pointer = ReadWord(library.CurrentPointer + index * 2);
                channel.VoiceIndexTimesEight = unchecked((byte)(channel.VoiceIndex * 8));
                channel.PointerIndex = 0;
                channel.InstructionTimer = 1;
            }
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            RunSoundChannel(library, channel);
    }

    private void InitializeSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        ManagedSpcSoundChannel[] libraryChannels)
    {
        library.VoiceId = SpcDriverData.SoundEffects.VoiceSearchStart;
        library.VoicesRemaining = channelOnMask;
        library.InitializationFlag = SpcDriverData.SoundEffects.Active;
        library.ChannelIndex = library.ChannelIndexTimesTwo = 0;
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
        {
            channel.VoiceBitset = 0;
            channel.VoiceIndex = 0;
            channel.ChannelMask = byte.MaxValue;
            channel.Disabled = SpcDriverData.SoundEffects.Active;
        }

        while (--library.VoiceId != 0)
        {
            int occupied = library.VoicesRemaining;
            library.VoicesRemaining <<= 1;
            if ((occupied & 0x80) != 0) // allow(BitMask): high-to-low hardware voice scan
                continue;
            if (library.VoicesToSetup == 0)
                return;

            library.VoicesToSetup--;
            ManagedSpcSoundChannel soundChannel = libraryChannels[library.ChannelIndex];
            soundChannel.Disabled = 0;
            library.ChannelVoiceBitsetPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceBitset) + library.ChannelIndex));
            library.ChannelVoiceMaskPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.ChannelMask) + library.ChannelIndex));
            library.ChannelVoiceIndexPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceIndex) + library.ChannelIndex));
            library.ChannelIndex++;
            library.ChannelIndexTimesTwo += 2;

            library.VoiceIndex = unchecked((byte)((library.VoiceId - 1) * 2));
            soundChannel.VoiceIndex = library.VoiceIndex;
            int musicChannelIndex = library.VoiceId - 1;
            // The native field is one byte; the apparent phase byte in its source expression
            // is intentionally split into the adjacent managed field. Both halves must be
            // captured before the SFX overwrites the shared music-channel registers: the
            // cancellation stream later restores them as one packed native word.
            ManagedSpcSoundOwnership.CaptureBorrowedMusicState(
                channels[musicChannelIndex],
                soundChannel);
            byte voiceBit = unchecked((byte)(1 << musicChannelIndex));
            channelOnMask |= voiceBit;
            currentChannelBit &= unchecked((byte)~voiceBit);
            echoEnable &= unchecked((byte)~voiceBit);
            library.EnabledVoices |= voiceBit;
            soundChannel.VoiceBitset = voiceBit;
            soundChannel.ChannelMask = unchecked((byte)~voiceBit);
        }
    }

    private void ResetSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        soundChannel.Disabled = SpcDriverData.SoundEffects.Active;
        soundChannel.UpdateAdsr = 0;
        library.EnabledVoices &= soundChannel.ChannelMask;
        channelOnMask &= soundChannel.ChannelMask;
        currentChannelBit |= soundChannel.VoiceBitset;
        keyOff |= soundChannel.VoiceBitset;
        ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
        SetInstrumentWithoutSavingId(musicChannel, musicChannel.InstrumentId);
        musicChannel.FinalVolume = soundChannel.Volume;
        musicChannel.PanFlags = soundChannel.PhaseInvert;
        if (library.EnabledVoices == 0)
        {
            library.CurrentSound = 0;
            library.Priority = 0;
            library.InitializationFlag = 0;
        }
    }

    private byte ReadSoundByte(ManagedSpcSoundChannel channel)
    {
        byte value = ram[unchecked((ushort)(channel.Pointer + channel.PointerIndex))];
        channel.PointerIndex++;
        return value;
    }

    private void RunSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        if (soundChannel.Disabled == SpcDriverData.SoundEffects.Active)
            return;
        soundChannel.InstructionTimer--;
        if (soundChannel.InstructionTimer == 0)
        {
            if (soundChannel.Legato == 0)
            {
                soundChannel.EnablePitchSlide = 0;
                soundChannel.SubnoteDelta = 0;
                soundChannel.TargetNote = 0;
                if (soundChannel.ReleaseFlag != SpcDriverData.SoundEffects.Active)
                {
                    keyOff |= soundChannel.VoiceBitset;
                    soundChannel.ReleaseTimer = 2;
                    soundChannel.InstructionTimer = 1;
                    soundChannel.ReleaseFlag = SpcDriverData.SoundEffects.Active;
                }
                soundChannel.ReleaseTimer--;
                if (soundChannel.ReleaseTimer != 0)
                    return;
                soundChannel.ReleaseFlag = 0;
                currentChannelBit &= soundChannel.ChannelMask;
                noiseEnable &= soundChannel.ChannelMask;
            }

            byte command;
        NextCommand:
            command = ReadSoundByte(soundChannel);
            if (command == 0xf9) // allow(DomainDiscriminator): SFX inline ADSR override
            {
                soundChannel.AdsrSettings = ReadSoundByte(soundChannel);
                soundChannel.AdsrSettings |= unchecked((ushort)(ReadSoundByte(soundChannel) << 8));
                soundChannel.PointerIndex += 2;
                soundChannel.UpdateAdsr = SpcDriverData.SoundEffects.Active;
                goto NextCommand;
            }
            if (command is 0xf5 or 0xf8) // allow(DomainDiscriminator): SFX pitch-slide forms
            {
                soundChannel.EnablePitchSlideLegato = command == 0xf5 // allow(DomainDiscriminator): legato slide
                    ? (byte)0xf5 // allow(DomainDiscriminator): retained cartridge marker
                    : (byte)0;
                soundChannel.SubnoteDelta = ReadSoundByte(soundChannel);
                soundChannel.TargetNote = ReadSoundByte(soundChannel);
                soundChannel.EnablePitchSlide = SpcDriverData.SoundEffects.Active;
                command = ReadSoundByte(soundChannel);
            }
            if (command == byte.MaxValue)
            {
                ResetSoundChannel(library, soundChannel);
                return;
            }
            if (command == 0xfe) // allow(DomainDiscriminator): SFX repeat-start opcode
            {
                soundChannel.RepeatCounter = ReadSoundByte(soundChannel);
                soundChannel.RepeatPoint = soundChannel.PointerIndex;
                command = ReadSoundByte(soundChannel);
            }
            if (command is 0xfb or 0xfd) // allow(DomainDiscriminator): SFX repeat opcodes
            {
                if (command == 0xfd) // allow(DomainDiscriminator): counted repeat opcode
                {
                    soundChannel.RepeatCounter--;
                    if (soundChannel.RepeatCounter == 0)
                        goto NextCommand;
                }
                soundChannel.PointerIndex = soundChannel.RepeatPoint;
                command = ReadSoundByte(soundChannel);
            }
            if (command == 0xfc) // allow(DomainDiscriminator): SFX noise-enable opcode
            {
                noiseEnable |= soundChannel.VoiceBitset;
                goto NextCommand;
            }

            ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
            SetInstrumentWithoutSavingId(musicChannel, command);
            musicChannel.FinalVolume = ReadSoundByte(soundChannel);
            musicChannel.PanFlags = 0;
            WriteVolume(musicChannel, unchecked((ushort)(ReadSoundByte(soundChannel) << 8)));
            command = ReadSoundByte(soundChannel);
            if (command != 0xf6) // allow(DomainDiscriminator): retain prior SFX note opcode
            {
                soundChannel.Note = command;
                soundChannel.Subnote = 0;
            }
            WritePitchInner(musicChannel, unchecked((ushort)((soundChannel.Note << 8) |
                soundChannel.Subnote)));
            soundChannel.InstructionTimer = ReadSoundByte(soundChannel);
            if (soundChannel.UpdateAdsr != 0)
            {
                WriteDsp(unchecked((byte)(soundChannel.VoiceIndexTimesEight +
                    SnesDspRegisterMap.Voice.Adsr1)), soundChannel.AdsrSettings);
                WriteDsp(unchecked((byte)(soundChannel.VoiceIndexTimesEight +
                    SnesDspRegisterMap.Voice.Adsr2)), soundChannel.AdsrSettings >> 8);
            }
            if (soundChannel.Legato == 0)
                keyOn |= soundChannel.VoiceBitset;
        }

        if (soundChannel.EnablePitchSlide != SpcDriverData.SoundEffects.Active)
            return;
        if (soundChannel.EnablePitchSlideLegato != 0)
            soundChannel.Legato = SpcDriverData.SoundEffects.Active;

        if (soundChannel.Note >= soundChannel.TargetNote)
        {
            int next = soundChannel.Subnote - soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // allow(BitMask): subnote borrow
            {
                soundChannel.Note--;
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        else
        {
            int next = soundChannel.Subnote + soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // allow(BitMask): subnote carry
            {
                soundChannel.Note++;
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        WritePitchInner(channels[soundChannel.VoiceIndex >> 1], unchecked((ushort)(
            (soundChannel.Note << 8) | soundChannel.Subnote)));
    }
}
