namespace SuperMetroid.Core.Audio;

public sealed partial class ManagedSpcPlayer
{
    private void HandleSoundLibraryCommand(int libraryIndex)
    {
        int port = libraryIndex + AudioRomData.Apu.FirstSoundPort;
        byte command = inputPorts[port];
        inputPorts[port] = SpcDriverData.NoPortCommand;
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];

        // Each SFX port is bidirectional. Echo every actual write, including zero, so the
        // 65816 request queue can observe both halves of its command/clear handshake.
        if (command != SpcDriverData.NoPortCommand)
            portsToSnes[port] = command;

        bool reject = libraryIndex switch
        {
            0 => command == SpcDriverData.NoPortCommand || command == 0 ||
                (command != 1 && command != 2 && library.Priority != 0),
            1 => command == SpcDriverData.NoPortCommand || command == 0 ||
                (command != 0x71 && command != 0x7e && library.Priority != 0), // allow(HardwareValue): retail SFX2 cancellation overrides
            2 => command == SpcDriverData.NoPortCommand || command == 0 ||
                (command != 1 && (library.Mode == 2 || command != 2 && library.Priority != 0)),
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
        if ((uint)tableIndex >= SpcSoundEffectTables.StreamPointerTables[libraryIndex].Length)
        {
            throw new InvalidDataException(
                $"SPC sound library {libraryIndex + 1} has no command ${command:X2}.");
        }
        library.CurrentSoundIndex = unchecked((byte)(tableIndex * 2));
        library.CurrentPointer = SpcSoundEffectTables.StreamPointerTables[libraryIndex][tableIndex];
        library.CurrentSound = command;

        byte configuration = SpcSoundEffectTables.Configurations[libraryIndex][tableIndex];
        ConfigureSoundLibrary(libraryIndex, library, configuration);
    }

    private static void ConfigureSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        byte configuration)
    {
        if (libraryIndex == 0)
        {
            (byte voices, byte priority) = configuration switch
            {
                0 => ((byte)1, (byte)0),
                1 => ((byte)1, (byte)1),
                2 => ((byte)2, (byte)0),
                3 => ((byte)3, (byte)1),
                4 or 5 => ((byte)4, (byte)0),
                _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }
        if (libraryIndex == 1)
        {
            (byte voices, byte priority) = configuration switch
            {
                0 => ((byte)1, (byte)0),
                1 => ((byte)1, (byte)1),
                2 => ((byte)2, (byte)0),
                3 => ((byte)2, (byte)1),
                _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
            };
            library.VoicesToSetup = voices;
            library.Priority = priority;
            return;
        }

        switch (configuration)
        {
            case 0:
                library.VoicesToSetup = 1;
                library.Mode = 0;
                library.Priority = 1;
                break;
            case 1:
                library.VoicesToSetup = 1;
                library.Mode = 2;
                break;
            case 2:
                library.VoicesToSetup = 1;
                library.Priority = 0;
                break;
            case 3:
                library.VoicesToSetup = 2;
                library.Priority = 0;
                break;
            case 4:
                library.VoicesToSetup = 2;
                library.Priority = 1;
                break;
            case 5:
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
                SpcSoundEffectTables.AllocationStateAddresses[libraryIndex, 0] + library.ChannelIndex));
            library.ChannelVoiceMaskPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddresses[libraryIndex, 1] + library.ChannelIndex));
            library.ChannelVoiceIndexPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddresses[libraryIndex, 2] + library.ChannelIndex));
            library.ChannelIndex++;
            library.ChannelIndexTimesTwo += 2;

            library.VoiceIndex = unchecked((byte)((library.VoiceId - 1) * 2));
            soundChannel.VoiceIndex = library.VoiceIndex;
            int musicChannelIndex = library.VoiceId - 1;
            // The native field is one byte; the apparent phase byte in its source expression
            // is intentionally truncated and restored separately from PhaseInvert.
            soundChannel.Volume = channels[musicChannelIndex].FinalVolume;
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
