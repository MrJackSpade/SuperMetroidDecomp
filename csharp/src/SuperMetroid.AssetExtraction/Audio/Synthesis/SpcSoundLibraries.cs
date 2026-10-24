using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The SPC driver's three sound-effect libraries: their requests, configuration and channels.</summary>
internal sealed class SpcSoundLibraries
{
    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    [SpcMemoryAccess]
    private readonly byte[] ram;
    /// <summary>Eight music sequencer states that continue advancing under borrowed DSP voices.</summary>
    private readonly ManagedSpcMusicChannel[] channels ;
    /// <summary>Priority, mode and voice-allocation state for the three SFX libraries.</summary>
    private readonly ManagedSpcSoundLibrary[] soundLibraries ;
    /// <summary>Per-library SFX instruction cursors and borrowed voice state.</summary>
    private readonly ManagedSpcSoundChannel[][] soundChannels ;
    /// <summary>DSP renderer reading BRR and echo data from this driver's APU RAM.</summary>
    private readonly ManagedSnesDsp dsp;
    /// <summary>What music and sound effects share on the DSP: instruments, pitch and volume writes, the key-on, key-off, noise and echo flags, and memory reads.</summary>
    private readonly SpcVoices _voices;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    [SpcMemoryBoundary("implementation: shares the APU RAM the player owns")]
    internal SpcSoundLibraries(byte[] ram, ManagedSpcMusicChannel[] channels, ManagedSpcSoundLibrary[] soundLibraries, ManagedSpcSoundChannel[][] soundChannels, ManagedSnesDsp dsp, SpcVoices voices)
    {
        this.ram = ram;
        this.channels = channels;
        this.soundLibraries = soundLibraries;
        this.soundChannels = soundChannels;
        this.dsp = dsp;
        _voices = voices;
    }

    /// <summary>The next byte of a sound-effect stream: the channel's stream pointer plus its index, wrapping at 16 bits.</summary>
    [SpcMemoryBoundary("driver-memory view: sound-effect instruction stream bytes")]
    private byte ReadSoundByte(ManagedSpcSoundChannel channel)
    {
        byte value = ram[unchecked((ushort)(channel.Pointer + channel.PointerIndex))];
        channel.PointerIndex = SpcDriverWrap.Byte(channel.PointerIndex + 1);
        return value;
    }

    /// <summary>
    /// One sound-effect service of library <paramref name="libraryIndex"/>: Handle CPU IO
    /// ($1EE7/$3157/$4706) followed by Write/read CPU IO ($1621).
    /// </summary>
    /// <remarks>
    /// The handler acts on the value latched by the previous service and starts a sound only
    /// when that value changed. $1621 then echoes the acted-on value and latches the CPU's
    /// current write, so a request is echoed one service after the service that reads it.
    /// </remarks>
    internal void HandleSoundLibraryCommand(int libraryIndex, SoundRequestCheck<SoundEffectId> poll) =>
        ApplySoundLibraryCommand(libraryIndex, poll.Changed,
            poll.Request is SoundEffectId sound ? checked((byte)SoundEffectCommands.Command(sound)) : (byte)0);

    /// <summary>Applies native library priority/cancellation gates, then selects a new sound stream or advances the active one.</summary>
    private void ApplySoundLibraryCommand(int libraryIndex, bool changed, byte command)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        if (!changed || command == 0 || RejectsRequest(libraryIndex, command, library.Priority, library.Mode))
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

    /// <summary>
    /// Whether a library rejects a changed nonzero request given its current priority and
    /// low-health mode: a high-priority sound blocks ordinary requests, library 3's low-health mode
    /// blocks everything except its clearing request, and each library has requests exempt from
    /// priority. The offline clip exporter (#1305) reads its policy from this rule.
    /// </summary>
    internal static bool RejectsRequest(int libraryIndex, int command, int priority, int mode) => libraryIndex switch
    {
        0 => command != 1 && command != 2 && priority != 0,
        1 => command != 0x71 && command != 0x7e && priority != 0, // cartridge-encoding: allow(HardwareValue) - retail SFX2 cancellation overrides
        2 => command != 1 && (mode == SpcDriverDefinitions.SoundEffects.LowHealthMode || command != 2 && priority != 0),
        _ => throw new ArgumentOutOfRangeException(nameof(libraryIndex)),
    };

    /// <summary>Applies native allocation-policy writes for a validated sound library.</summary>
    /// <remarks>#1165 original-handler proof covers all policy cases. Mode is unchanged
    /// except for library-three cancel/low-health operations; low-health preserves priority.
    /// Native handler identities are documented on the SpcLibraryNPolicy catalog members.</remarks>
    internal static void ConfigureSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        int configuration)
    {
        if (libraryIndex == 0)
        {
            (int voices, int priority) = (SpcLibrary1Policy)configuration switch
            {
                SpcLibrary1Policy.OneVoiceLowPriority => new SoundLibrarySetup(1, 0),
                SpcLibrary1Policy.OneVoiceHighPriority => new SoundLibrarySetup(1, 1),
                SpcLibrary1Policy.TwoVoicesLowPriority => new SoundLibrarySetup(2, 0),
                SpcLibrary1Policy.ThreeVoicesHighPriority => new SoundLibrarySetup(3, 1),
                SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => new SoundLibrarySetup(4, 0),
                _ => throw new InvalidDataException($"Unknown SPC SFX1 configuration {configuration}."),
            };
            library.VoicesToSetup = checked((byte)voices);
            library.Priority = checked((byte)priority);
            return;
        }
        if (libraryIndex == 1)
        {
            (int voices, int priority) = (SpcLibrary2Policy)configuration switch
            {
                SpcLibrary2Policy.OneVoiceLowPriority => new SoundLibrarySetup(1, 0),
                SpcLibrary2Policy.OneVoiceHighPriority => new SoundLibrarySetup(1, 1),
                SpcLibrary2Policy.TwoVoicesLowPriority => new SoundLibrarySetup(2, 0),
                SpcLibrary2Policy.TwoVoicesHighPriority => new SoundLibrarySetup(2, 1),
                _ => throw new InvalidDataException($"Unknown SPC SFX2 configuration {configuration}."),
            };
            library.VoicesToSetup = checked((byte)voices);
            library.Priority = checked((byte)priority);
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
                library.Mode = SpcDriverDefinitions.SoundEffects.LowHealthMode;
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

    /// <summary>Allocates borrowed voices on first service and then ticks each sound-effect stream.</summary>
    private void ProcessSoundLibrary(int libraryIndex)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.InitializationFlag != SpcDriverDefinitions.SoundEffects.Active)
        {
            InitializeSoundLibrary(libraryIndex, library, libraryChannels);
            for (int index = 0; index < libraryChannels.Length; index++)
            {
                ManagedSpcSoundChannel channel = libraryChannels[index];
                channel.Pointer = _voices.ReadPointerEntry(library.CurrentPointer, index);
                channel.VoiceIndexTimesEight = unchecked((byte)(channel.VoiceIndex * 8));
                channel.PointerIndex = 0;
                channel.InstructionTimer = 1;
            }
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            RunSoundChannel(library, channel);
    }

    /// <summary>Scans hardware voices high to low and captures music state before assigning them to a sound library.</summary>
    private void InitializeSoundLibrary(
        int libraryIndex,
        ManagedSpcSoundLibrary library,
        ManagedSpcSoundChannel[] libraryChannels)
    {
        library.VoiceId = SpcDriverDefinitions.SoundEffects.VoiceSearchStart;
        library.VoicesRemaining = _voices.channelOnMask;
        library.InitializationFlag = SpcDriverDefinitions.SoundEffects.Active;
        library.ChannelIndex = library.ChannelIndexTimesTwo = 0;
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
        {
            channel.VoiceBitset = 0;
            channel.VoiceIndex = 0;
            channel.ChannelMask = byte.MaxValue;
            channel.Disabled = SpcDriverDefinitions.SoundEffects.Active;
        }

        while ((library.VoiceId = SpcDriverWrap.Byte(library.VoiceId - 1)) != 0)
        {
            int occupied = library.VoicesRemaining;
            library.VoicesRemaining = SpcDriverWrap.Byte(library.VoicesRemaining << 1);
            if ((occupied & 0x80) != 0) // cartridge-encoding: allow(BitMask) - high-to-low hardware voice scan
                continue;
            if (library.VoicesToSetup == 0)
                return;

            library.VoicesToSetup = SpcDriverWrap.Byte(library.VoicesToSetup - 1);
            ManagedSpcSoundChannel soundChannel = libraryChannels[library.ChannelIndex];
            soundChannel.Disabled = 0;
            library.ChannelVoiceBitsetPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceBitset) + library.ChannelIndex));
            library.ChannelVoiceMaskPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.ChannelMask) + library.ChannelIndex));
            library.ChannelVoiceIndexPointer = unchecked((ushort)(
                SpcSoundEffectTables.AllocationStateAddress(libraryIndex, SpcAllocationField.VoiceIndex) + library.ChannelIndex));
            library.ChannelIndex = SpcDriverWrap.Byte(library.ChannelIndex + 1);
            library.ChannelIndexTimesTwo = SpcDriverWrap.Byte(library.ChannelIndexTimesTwo + 2);

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
            _voices.channelOnMask = _voices.channelOnMask | voiceBit;
            _voices.currentChannelBit = _voices.currentChannelBit & unchecked((byte)~voiceBit);
            _voices.echoEnable = _voices.echoEnable & unchecked((byte)~voiceBit);
            library.EnabledVoices = library.EnabledVoices | voiceBit;
            soundChannel.VoiceBitset = voiceBit;
            soundChannel.ChannelMask = unchecked((byte)~voiceBit);
        }
    }

    /// <summary>Releases a borrowed DSP voice and restores its music instrument, pan and volume ownership.</summary>
    private void ResetSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        soundChannel.Disabled = SpcDriverDefinitions.SoundEffects.Active;
        soundChannel.UpdateAdsr = 0;
        library.EnabledVoices = library.EnabledVoices & soundChannel.ChannelMask;
        _voices.channelOnMask = _voices.channelOnMask & soundChannel.ChannelMask;
        _voices.currentChannelBit = _voices.currentChannelBit | soundChannel.VoiceBitset;
        _voices.keyOff = _voices.keyOff | soundChannel.VoiceBitset;
        ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
        _voices.SetInstrumentWithoutSavingId(musicChannel, musicChannel.InstrumentId);
        musicChannel.FinalVolume = soundChannel.Volume;
        musicChannel.PanFlags = soundChannel.PhaseInvert;
        if (library.EnabledVoices == 0)
        {
            library.CurrentSound = 0;
            library.Priority = 0;
            library.InitializationFlag = 0;
        }
    }

    /// <summary>Interprets one sound stream's timed note, repeat, noise, ADSR and pitch-slide commands.</summary>
    private void RunSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        if (soundChannel.Disabled == SpcDriverDefinitions.SoundEffects.Active)
            return;
        soundChannel.InstructionTimer = SpcDriverWrap.Byte(soundChannel.InstructionTimer - 1);
        if (soundChannel.InstructionTimer == 0)
        {
            if (soundChannel.Legato == 0)
            {
                soundChannel.EnablePitchSlide = 0;
                soundChannel.SubnoteDelta = 0;
                soundChannel.TargetNote = 0;
                if (soundChannel.ReleaseFlag != SpcDriverDefinitions.SoundEffects.Active)
                {
                    _voices.keyOff = _voices.keyOff | soundChannel.VoiceBitset;
                    soundChannel.ReleaseTimer = 2;
                    soundChannel.InstructionTimer = 1;
                    soundChannel.ReleaseFlag = SpcDriverDefinitions.SoundEffects.Active;
                }
                soundChannel.ReleaseTimer = SpcDriverWrap.Byte(soundChannel.ReleaseTimer - 1);
                if (soundChannel.ReleaseTimer != 0)
                    return;
                soundChannel.ReleaseFlag = 0;
                _voices.currentChannelBit = _voices.currentChannelBit & soundChannel.ChannelMask;
                _voices.noiseEnable = _voices.noiseEnable & soundChannel.ChannelMask;
            }

            byte command;
        NextCommand:
            command = ReadSoundByte(soundChannel);
            if (command == 0xf9) // cartridge-encoding: allow(DomainDiscriminator) - SFX inline ADSR override
            {
                soundChannel.AdsrSettings = ReadSoundByte(soundChannel);
                soundChannel.AdsrSettings = soundChannel.AdsrSettings | SnesLittleEndian.Word(low: 0, high: ReadSoundByte(soundChannel));
                soundChannel.PointerIndex = SpcDriverWrap.Byte(soundChannel.PointerIndex + 2);
                soundChannel.UpdateAdsr = SpcDriverDefinitions.SoundEffects.Active;
                goto NextCommand;
            }
            if (command is 0xf5 or 0xf8) // cartridge-encoding: allow(DomainDiscriminator) - SFX pitch-slide forms
            {
                soundChannel.EnablePitchSlideLegato = command == 0xf5 // cartridge-encoding: allow(DomainDiscriminator) - legato slide
                    ? (byte)0xf5 // cartridge-encoding: allow(DomainDiscriminator) - retained cartridge marker
                    : (byte)0;
                soundChannel.SubnoteDelta = ReadSoundByte(soundChannel);
                soundChannel.TargetNote = ReadSoundByte(soundChannel);
                soundChannel.EnablePitchSlide = SpcDriverDefinitions.SoundEffects.Active;
                command = ReadSoundByte(soundChannel);
            }
            if (command == byte.MaxValue)
            {
                ResetSoundChannel(library, soundChannel);
                return;
            }
            if (command == 0xfe) // cartridge-encoding: allow(DomainDiscriminator) - SFX repeat-start opcode
            {
                soundChannel.RepeatCounter = ReadSoundByte(soundChannel);
                soundChannel.RepeatPoint = soundChannel.PointerIndex;
                command = ReadSoundByte(soundChannel);
            }
            if (command is 0xfb or 0xfd) // cartridge-encoding: allow(DomainDiscriminator) - SFX repeat opcodes
            {
                if (command == 0xfd) // cartridge-encoding: allow(DomainDiscriminator) - counted repeat opcode
                {
                    soundChannel.RepeatCounter = SpcDriverWrap.Byte(soundChannel.RepeatCounter - 1);
                    if (soundChannel.RepeatCounter == 0)
                        goto NextCommand;
                }
                soundChannel.PointerIndex = soundChannel.RepeatPoint;
                command = ReadSoundByte(soundChannel);
            }
            if (command == 0xfc) // cartridge-encoding: allow(DomainDiscriminator) - SFX noise-enable opcode
            {
                _voices.noiseEnable = _voices.noiseEnable | soundChannel.VoiceBitset;
                goto NextCommand;
            }

            ManagedSpcMusicChannel musicChannel = channels[soundChannel.VoiceIndex >> 1];
            _voices.SetInstrumentWithoutSavingId(musicChannel, command);
            musicChannel.FinalVolume = ReadSoundByte(soundChannel);
            musicChannel.PanFlags = 0;
            _voices.WriteVolume(musicChannel, SpcFixedPoint.FromWhole(ReadSoundByte(soundChannel)));
            command = ReadSoundByte(soundChannel);
            if (command != SpcDriverDefinitions.SoundEffects.RetainedNote)
            {
                soundChannel.Note = command;
                soundChannel.Subnote = 0;
            }
            _voices.WritePitchInner(musicChannel, SpcFixedPoint.Compose(soundChannel.Note, soundChannel.Subnote));
            soundChannel.InstructionTimer = ReadSoundByte(soundChannel);
            if (soundChannel.UpdateAdsr != 0)
            {
                WriteSoundEffectAdsr(soundChannel.VoiceIndexTimesEight, soundChannel.AdsrSettings);
            }
            if (soundChannel.Legato == 0)
                _voices.keyOn = _voices.keyOn | soundChannel.VoiceBitset;
        }

        if (soundChannel.EnablePitchSlide != SpcDriverDefinitions.SoundEffects.Active)
            return;
        if (soundChannel.EnablePitchSlideLegato != 0)
            soundChannel.Legato = SpcDriverDefinitions.SoundEffects.Active;

        if (soundChannel.Note >= soundChannel.TargetNote)
        {
            int next = soundChannel.Subnote - soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // cartridge-encoding: allow(BitMask) - subnote borrow
            {
                soundChannel.Note = SpcDriverWrap.Byte(soundChannel.Note - 1);
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        else
        {
            int next = soundChannel.Subnote + soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // cartridge-encoding: allow(BitMask) - subnote carry
            {
                soundChannel.Note = SpcDriverWrap.Byte(soundChannel.Note + 1);
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.EnablePitchSlide = soundChannel.Legato = 0;
            }
        }
        _voices.WritePitchInner(channels[soundChannel.VoiceIndex >> 1], SpcFixedPoint.Compose(soundChannel.Note, soundChannel.Subnote));
    }

    /// <summary>
    /// The sound-effect ADSR update as the cartridge driver performs it: the register address is
    /// formed from voice × 8 rather than the sixteen-byte voice stride, so for odd voices these two
    /// writes land on another register. The quirk is preserved exactly.
    /// </summary>
    [DspRegisterBoundary("native quirk: sound-effect ADSR writes address voice × 8 + ADSR1/ADSR2, as the cartridge driver does")]
    private void WriteSoundEffectAdsr(int voiceIndexTimesEight, int adsr)
    {
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterLayout.Voice.Adsr1)), unchecked((byte)adsr));
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterLayout.Voice.Adsr2)), SnesLittleEndian.HighByte(checked((ushort)adsr)));
    }
}
