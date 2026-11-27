using Game.Compatibility;

namespace Game.Tests.Movies;

/// <summary>
/// The sound program a movie's host gives the game: it answers a library when the cartridge's own answered it in
/// the recording. When the real program took up what it was asked depended on the load of the console's processor,
/// update by update, which the game does not have; the recording shows the outcome, whether each library that was
/// waiting on the program was answered during an update, and this reports exactly that. On an update the recording
/// says nothing of (one with no record of its own, a message box's frames, a library that was not waiting) it
/// answers as the game's own program does, which runs beside it throughout.
/// </summary>
internal sealed class CapturedSoundProgram : ISoundProgram
{
    /// <summary>The libraries, in the order the cartridge keeps their states.</summary>
    private static readonly string[] Libraries = ["samus", "enemy", "room"];

    /// <summary>The states of the cartridge's handler in which a library waits on the program: for its sound, and for its withdrawal.</summary>
    private const int AwaitingSound = 1, AwaitingWithdrawal = 3;

    private readonly ISoundProgram _own;
    private readonly Dictionary<string, string?> _asked = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _lastSound = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _answered = new(StringComparer.Ordinal);

    /// <summary>A program that answers as the recording shows, and as <paramref name="own"/> where it shows nothing.</summary>
    public CapturedSoundProgram(ISoundProgram own) => _own = own;

    /// <summary>
    /// Takes what the recording shows of the update about to be played: each library's state in the cartridge's
    /// handler as the update began and as it ended, or nothing for an update it does not show. A library that was
    /// waiting on the program and has left that state was answered.
    /// </summary>
    public void Expect(IReadOnlyList<int>? before, IReadOnlyList<int>? after)
    {
        _answered.Clear();
        if (before is null || after is null)
            return;
        for (int index = 0; index < Libraries.Length; index++)
        {
            if (before[index] is AwaitingSound or AwaitingWithdrawal)
                _answered[Libraries[index]] = after[index] != before[index];
        }
    }

    /// <inheritdoc/>
    public string? Reported(string library)
    {
        if (!_answered.TryGetValue(library, out bool answered))
            return _own.Reported(library);
        string? asked = _asked.GetValueOrDefault(library);
        if (answered)
            return asked;

        // Not answered: the library reads something other than what it waits for. Waiting for a sound that is
        // nothing; waiting for its withdrawal, the sound it had asked for.
        return asked is null ? _lastSound.GetValueOrDefault(library, library) : null;
    }

    /// <inheritdoc/>
    public void Ask(string library, string? sound)
    {
        _asked[library] = sound;
        if (sound is not null)
            _lastSound[library] = sound;
        _own.Ask(library, sound);
    }

    /// <inheritdoc/>
    public void DropAsked()
    {
        foreach (string library in Libraries)
            _asked[library] = null;
        _own.DropAsked();
    }

    /// <summary>The update is over: what the recording showed of it is spent.</summary>
    public void Advance()
    {
        _answered.Clear();
        _own.Advance();
    }
}
