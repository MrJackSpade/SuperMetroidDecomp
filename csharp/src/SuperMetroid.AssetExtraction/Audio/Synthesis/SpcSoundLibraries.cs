using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Compatibility;

using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The SPC driver's three sound-effect libraries: their requests, configuration and channels.</summary>
internal sealed class SpcSoundLibraries
{
    /// <summary>Owned 64-KiB APU RAM shared by the driver views and DSP sample decoder.</summary>
    private readonly SpcRam ram;
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
    internal SpcSoundLibraries(SpcRam ram, ManagedSpcMusicChannel[] channels, ManagedSpcSoundLibrary[] soundLibraries, ManagedSpcSoundChannel[][] soundChannels, ManagedSnesDsp dsp, SpcVoices voices)
    {
        this.ram = ram;
        this.channels = channels;
        this.soundLibraries = soundLibraries;
        this.soundChannels = soundChannels;
        this.dsp = dsp;
        _voices = voices;
    }

    /// <summary>The next byte of a sound-effect stream: the channel's stream pointer plus its index, wrapping at 16 bits.</summary>
    private byte ReadSoundByte(ManagedSpcSoundChannel channel)
    {
        int address = unchecked((ushort)(channel.Pointer + channel.PointerIndex));
        channel.PointerIndex = SpcDriverWrap.Byte(channel.PointerIndex + 1);
        return ram[address];
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
    internal void HandleSoundLibraryCommand(int libraryIndex, SoundRequestCheck<byte> poll)
    {
        // The native library priority/cancellation gates pass a changed request that names a sound, which selects its
        // stream; otherwise the library advances the sound it plays, if any.
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        int? changed = poll.Changed ? poll.Request : null;
        if (changed is { } command && command != ManagedSpcSoundLibrary.SilentCommand &&
            !RejectsRequest(libraryIndex, command, library.Priority, library.Mode))
            StartSound(libraryIndex, command);
        else if (library.IsPlaying)
            ProcessSoundLibrary(libraryIndex);
    }

    /// <summary>Selects the stream of sound <paramref name="command"/> in library <paramref name="libraryIndex"/>, silencing the sound it played.</summary>
    private void StartSound(int libraryIndex, int command)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (library.IsPlaying)
        {
            library.EnabledVoices = 0;
            foreach (ManagedSpcSoundChannel channel in libraryChannels)
                ResetSoundChannel(library, channel);
        }
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
            channel.Slide = channel.Slide == SoundEffectSlide.Off ? SoundEffectSlide.Off : SoundEffectSlide.Sliding;

        int tableIndex = command - 1;
        SoundEffectLibrary soundLibrary = SoundLibraryNumbers.FromDriverIndex(libraryIndex);
        if ((uint)tableIndex >= SpcSoundEffectTables.CommandCount(soundLibrary))
        {
            throw new InvalidDataException(
                $"SPC sound library {libraryIndex + 1} has no command ${command:X2}.");
        }
        library.CurrentSoundIndex = unchecked((byte)(tableIndex * 2));
        library.CurrentPointer = SpcSoundEffectTables.StreamPointer(soundLibrary, command);
        library.CurrentSound = command;

        ConfigureSoundLibrary(soundLibrary, library, SpcSoundEffectTables.PolicyKey(soundLibrary, command));
    }

    /// <summary>
    /// Whether a library rejects a changed nonzero request given its current priority and
    /// low-health mode: a high-priority sound blocks ordinary requests, library 3's low-health mode
    /// blocks everything except its clearing request, and each library has requests exempt from
    /// priority. The offline clip exporter (#1305) reads its policy from this rule.
    /// </summary>
    internal static bool RejectsRequest(int libraryIndex, int command, int priority, SoundLibraryMode mode)
    {
        bool lowHealth = SpcSoundEffectTables.HasLowHealthMode(libraryIndex) && mode == SoundLibraryMode.LowHealth;
        return SpcSoundEffectTables.Admission(libraryIndex, command) switch
        {
            SpcSoundRequestAdmission.Clearing => false,
            SpcSoundRequestAdmission.PriorityExempt => lowHealth,
            SpcSoundRequestAdmission.ByPriority => lowHealth || priority != 0,
            _ => throw new ArgumentOutOfRangeException(nameof(command)),
        };
    }

    /// <summary>Applies native allocation-policy writes for a validated sound library.</summary>
    /// <remarks>#1165 original-handler proof covers all policy cases. Mode is unchanged
    /// except for library-three cancel/low-health operations; low-health preserves priority.
    /// Native handler identities are documented on the SpcLibraryNPolicy catalog members.</remarks>
    internal static void ConfigureSoundLibrary(
        SoundEffectLibrary soundLibrary,
        ManagedSpcSoundLibrary library,
        string policyKey)
    {
        switch (soundLibrary)
        {
            case SoundEffectLibrary.Samus:
                Configure(library, SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library1Keys, policyKey, soundLibrary));
                break;
            case SoundEffectLibrary.Enemies:
                Configure(library, SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library2Keys, policyKey, soundLibrary));
                break;
            case SoundEffectLibrary.Room:
                Configure(library, SpcSoundPolicyCodec.Parse(SpcSoundPolicyCodec.Library3Keys, policyKey, soundLibrary));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(soundLibrary), soundLibrary, "Undefined sound library.");
        }
    }

    /// <summary>Applies a library-one policy's writes: its voices and priority.</summary>
    private static void Configure(ManagedSpcSoundLibrary library, SpcLibrary1Policy policy)
    {
        (int voices, int priority) = policy switch
        {
            SpcLibrary1Policy.OneVoiceLowPriority => new SoundLibrarySetup(1, 0),
            SpcLibrary1Policy.OneVoiceHighPriority => new SoundLibrarySetup(1, 1),
            SpcLibrary1Policy.TwoVoicesLowPriority => new SoundLibrarySetup(2, 0),
            SpcLibrary1Policy.ThreeVoicesHighPriority => new SoundLibrarySetup(3, 1),
            SpcLibrary1Policy.FourVoicesLowPriority or SpcLibrary1Policy.PowerBombFourVoices => new SoundLibrarySetup(4, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-one policy."),
        };
        library.VoicesToSetup = checked((byte)voices);
        library.Priority = checked((byte)priority);
    }

    /// <summary>Applies a library-two policy's writes: its voices and priority.</summary>
    private static void Configure(ManagedSpcSoundLibrary library, SpcLibrary2Policy policy)
    {
        (int voices, int priority) = policy switch
        {
            SpcLibrary2Policy.OneVoiceLowPriority => new SoundLibrarySetup(1, 0),
            SpcLibrary2Policy.OneVoiceHighPriority => new SoundLibrarySetup(1, 1),
            SpcLibrary2Policy.TwoVoicesLowPriority => new SoundLibrarySetup(2, 0),
            SpcLibrary2Policy.TwoVoicesHighPriority => new SoundLibrarySetup(2, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-two policy."),
        };
        library.VoicesToSetup = checked((byte)voices);
        library.Priority = checked((byte)priority);
    }

    /// <summary>Applies a library-three policy's writes: its voices, priority and low-health mode.</summary>
    private static void Configure(ManagedSpcSoundLibrary library, SpcLibrary3Policy policy)
    {
        switch (policy)
        {
            case SpcLibrary3Policy.CancelAndClearLowHealthMode:
                library.VoicesToSetup = 1;
                library.Mode = SoundLibraryMode.Normal;
                library.Priority = 1;
                break;
            case SpcLibrary3Policy.LowHealthModePreservePriority:
                library.VoicesToSetup = 1;
                library.Mode = SoundLibraryMode.LowHealth;
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
                throw new ArgumentOutOfRangeException(nameof(policy), policy, "Undefined library-three policy.");
        }
    }

    /// <summary>Allocates borrowed voices on first service and then ticks each sound-effect stream.</summary>
    private void ProcessSoundLibrary(int libraryIndex)
    {
        ManagedSpcSoundLibrary library = soundLibraries[libraryIndex];
        ManagedSpcSoundChannel[] libraryChannels = soundChannels[libraryIndex];
        if (!library.Initialized)
        {
            InitializeSoundLibrary(libraryIndex, library, libraryChannels);
            for (int index = 0; index < libraryChannels.Length; index++)
            {
                ManagedSpcSoundChannel channel = libraryChannels[index];
                channel.Pointer = _voices.ReadPointerEntry(library.CurrentPointer, index);
                channel.VoiceIndexTimesEight = unchecked((byte)(channel.VoiceIndex * 8));
                channel.PointerIndex = 0;
                channel.FrameHold = 1;
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
        library.Initialized = true;
        library.ChannelIndex = library.ChannelIndexTimesTwo = 0;
        foreach (ManagedSpcSoundChannel channel in libraryChannels)
        {
            channel.VoiceBitset = 0;
            channel.VoiceIndex = 0;
            channel.ChannelMask = byte.MaxValue;
            channel.Disabled = true;
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
            soundChannel.Disabled = false;
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
            int voiceBit = unchecked((byte)(1 << musicChannelIndex));
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
        soundChannel.Disabled = true;
        soundChannel.UpdateAdsr = false;
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
            library.CurrentSound = ManagedSpcSoundLibrary.SilentCommand;
            library.Priority = 0;
            library.Initialized = false;
        }
    }

    /// <summary>Interprets one sound stream's timed note, repeat, noise, ADSR and pitch-slide commands.</summary>
    private void RunSoundChannel(ManagedSpcSoundLibrary library, ManagedSpcSoundChannel soundChannel)
    {
        if (soundChannel.Disabled)
            return;
        soundChannel.FrameHold = SpcDriverWrap.Byte(soundChannel.FrameHold - 1);
        if (soundChannel.FrameHold == 0)
        {
            if (soundChannel.Slide != SoundEffectSlide.SlidingLegato)
            {
                soundChannel.Slide = SoundEffectSlide.Off;
                soundChannel.SubnoteDelta = 0;
                soundChannel.TargetNote = 0;
                if (!soundChannel.Releasing)
                {
                    _voices.keyOff = _voices.keyOff | soundChannel.VoiceBitset;
                    soundChannel.ReleaseTimer = 2;
                    soundChannel.FrameHold = 1;
                    soundChannel.Releasing = true;
                }
                soundChannel.ReleaseTimer = SpcDriverWrap.Byte(soundChannel.ReleaseTimer - 1);
                if (soundChannel.ReleaseTimer != 0)
                    return;
                soundChannel.Releasing = false;
                _voices.currentChannelBit = _voices.currentChannelBit & soundChannel.ChannelMask;
                _voices.noiseEnable = _voices.noiseEnable & soundChannel.ChannelMask;
            }

            int command;
        NextCommand:
            command = ReadSoundByte(soundChannel);
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.SetAdsr))
            {
                soundChannel.AdsrSettings = ReadSoundByte(soundChannel);
                soundChannel.AdsrSettings = soundChannel.AdsrSettings | LittleEndianBytes.Word(low: 0, high: ReadSoundByte(soundChannel));
                soundChannel.PointerIndex = SpcDriverWrap.Byte(soundChannel.PointerIndex + 2);
                soundChannel.UpdateAdsr = true;
                goto NextCommand;
            }
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.PitchSlideLegato) || SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.PitchSlide))
            {
                soundChannel.SlideForm = SpcDriverSnapshot.SlideFormOf(command);
                soundChannel.SubnoteDelta = ReadSoundByte(soundChannel);
                soundChannel.TargetNote = ReadSoundByte(soundChannel);
                if (soundChannel.Slide == SoundEffectSlide.Off)
                    soundChannel.Slide = SoundEffectSlide.Sliding;
                command = ReadSoundByte(soundChannel);
            }
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.End))
            {
                ResetSoundChannel(library, soundChannel);
                return;
            }
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.BeginRepeat))
            {
                soundChannel.RepeatCounter = ReadSoundByte(soundChannel);
                soundChannel.RepeatPoint = soundChannel.PointerIndex;
                command = ReadSoundByte(soundChannel);
            }
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.RepeatForever) || SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.EndRepeat))
            {
                if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.EndRepeat))
                {
                    soundChannel.RepeatCounter = SpcDriverWrap.Byte(soundChannel.RepeatCounter - 1);
                    if (soundChannel.RepeatCounter == 0)
                        goto NextCommand;
                }
                soundChannel.PointerIndex = soundChannel.RepeatPoint;
                command = ReadSoundByte(soundChannel);
            }
            if (SpcSoundEffectProgramCodec.Is(command, SpcSoundEffectOpcode.EnableNoise))
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
            soundChannel.FrameHold = ReadSoundByte(soundChannel);
            if (soundChannel.UpdateAdsr)
            {
                WriteSoundEffectAdsr(soundChannel.VoiceIndexTimesEight, soundChannel.AdsrSettings);
            }
            if (soundChannel.Slide != SoundEffectSlide.SlidingLegato)
                _voices.keyOn = _voices.keyOn | soundChannel.VoiceBitset;
        }

        if (soundChannel.Slide == SoundEffectSlide.Off)
            return;
        if (soundChannel.SlideForm == SoundEffectSlideForm.Legato)
            soundChannel.Slide = SoundEffectSlide.SlidingLegato;

        if (soundChannel.Note >= soundChannel.TargetNote)
        {
            int next = soundChannel.Subnote - soundChannel.SubnoteDelta;
            soundChannel.Subnote = unchecked((byte)next);
            if ((next & 0x100) != 0) // cartridge-encoding: allow(BitMask) - subnote borrow
            {
                soundChannel.Note = SpcDriverWrap.Byte(soundChannel.Note - 1);
                if (soundChannel.Note == soundChannel.TargetNote)
                    soundChannel.Slide = SoundEffectSlide.Off;
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
                    soundChannel.Slide = SoundEffectSlide.Off;
            }
        }
        _voices.WritePitchInner(channels[soundChannel.VoiceIndex >> 1], SpcFixedPoint.Compose(soundChannel.Note, soundChannel.Subnote));
    }

    /// <summary>
    /// The sound-effect ADSR update as the cartridge driver performs it: the register address is
    /// formed from voice × 8 rather than the sixteen-byte voice stride, so for odd voices these two
    /// writes land on another register. The quirk is preserved exactly.
    /// </summary>
    private void WriteSoundEffectAdsr(int voiceIndexTimesEight, int adsr)
    {
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterLayout.Voice.Adsr1)), unchecked((byte)adsr));
        dsp.WriteRegister(unchecked((byte)(voiceIndexTimesEight + SnesDspRegisterLayout.Voice.Adsr2)), LittleEndianBytes.HighByte(checked((ushort)adsr)));
    }
}
