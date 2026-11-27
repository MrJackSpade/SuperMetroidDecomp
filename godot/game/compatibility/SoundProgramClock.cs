namespace Game.Compatibility;

/// <summary>
/// When the cartridge's sound program takes up what the game asks of it. The program looks at what each library is
/// asked for on a clock of its own, every 512 of its samples, while an update lasts 534: so its look falls a little
/// earlier in each update, and now and then twice in one. At each look, library by library in a fixed order, it
/// reports back what it took at the look before and takes what is asked now. Something asked for and replaced
/// between two looks is never seen. It is a count for the waits play keeps: it starts no sound, since what is heard
/// is started as it is asked for.
/// </summary>
internal sealed class SoundProgramClock : ISoundProgram
{
    /// <summary>The program's samples in one update.</summary>
    private const int SamplesPerUpdate = 534;

    /// <summary>The samples in one step of the program's timer.</summary>
    private const int SamplesPerStep = 64;

    /// <summary>The timer's steps between two looks at what is asked.</summary>
    private const int StepsPerLook = 8;

    private readonly Dictionary<string, Library> _libraries;
    private int _samplesSinceStep;
    private int _stepsSinceLook;

    /// <summary>A clock at power-on, serving <paramref name="libraries"/>.</summary>
    public SoundProgramClock(IReadOnlyList<string> libraries)
    {
        _libraries = libraries.ToDictionary(name => name, _ => new Library(), StringComparer.Ordinal);
    }

    /// <summary>What <paramref name="library"/> reports back as taken up: a sound, or nothing.</summary>
    public string? Reported(string library) => _libraries[library].Reported;

    /// <summary>Sets what <paramref name="library"/> is asked for now; nothing withdraws what was asked.</summary>
    public void Ask(string library, string? sound) => _libraries[library].Asked = sound;

    /// <summary>Loading music stops the program: what every library is asked for is dropped. What it had taken is kept.</summary>
    public void DropAsked()
    {
        foreach (Library library in _libraries.Values)
            library.Asked = null;
    }

    /// <summary>Runs one update's samples.</summary>
    public void Advance()
    {
        int sample = 0;
        while (sample < SamplesPerUpdate)
        {
            if (_samplesSinceStep == SamplesPerStep)
            {
                _samplesSinceStep = 0;
                if (++_stepsSinceLook == StepsPerLook)
                {
                    _stepsSinceLook = 0;
                    Look();
                }
            }

            int taken = Math.Min(SamplesPerUpdate - sample, SamplesPerStep - _samplesSinceStep);
            _samplesSinceStep += taken;
            sample += taken;
        }
    }

    /// <summary>One look: each library reports what it took last and takes what is asked now.</summary>
    private void Look()
    {
        foreach (Library library in _libraries.Values)
        {
            library.Reported = library.Taken;
            library.Taken = library.Asked;
        }
    }

    private sealed class Library
    {
        /// <summary>What the game asks for now.</summary>
        public string? Asked;

        /// <summary>What the program took at its last look, which the next look reports.</summary>
        public string? Taken;

        /// <summary>What the program reports back.</summary>
        public string? Reported;
    }
}
