using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The #1275 Mother Brain glitch movie's own check: it counts her library-two cries ($6F low, $7E high: the stretching,
/// recoil, drool, onion-ring, bomb and Baby-attack head lists). Every cry the cartridge starts, by the APU's current
/// library-two sound, must be one the game publishes (#1391): the port's counterpart is the request the enemies' library
/// handler has asked its library for and not yet withdrawn, read at the same checkpoints. One instance follows one replay.
/// </summary>
internal sealed class MotherBrainCryChecks : IConvertedMovieChecks
{
    /// <summary>Cries the port has published so far.</summary>
    private int cries;

    /// <summary>Cries the cartridge has started so far.</summary>
    private int nativeCries;

    /// <summary>The enemies' library request the port's handler held at the previous checkpoint, or none.</summary>
    private SoundEffectId? lastRequested;

    /// <summary>The library-two sound the cartridge last sent to the APU at the previous checkpoint.</summary>
    private byte nativeCurrent;

    /// <summary>The audio queues of the game the previous checkpoint read; a console reset replaces them.</summary>
    private AudioQueues? audio;

    /// <inheritdoc/>
    public IEnumerable<string> Compare(SuperMetroidGame game, int update, NativeCheckpointMemory native)
    {
        // $064E is the library-two sound the cartridge last sent to the APU.
        byte current = native[0x064e];
        if (current != nativeCurrent && current is 0x6f or 0x7e)
            nativeCries++;
        nativeCurrent = current;
        if (!ReferenceEquals(audio, game.Session.audio))
        {
            audio = game.Session.audio;
            lastRequested = audio.SoundHandshakes.Awaited(SoundEffectLibrary.Enemies);
        }
        SoundEffectId? requested = audio.SoundHandshakes.Awaited(SoundEffectLibrary.Enemies);
        if (requested != lastRequested && requested is SoundEffectId cry &&
            (cry == EnemySound.MotherBrainLowCry || cry == EnemySound.MotherBrainHighCry))
            cries++;
        lastRequested = requested;
        return [];
    }

    /// <inheritdoc/>
    public IEnumerable<string> Finish()
    {
        string report = $"Mother Brain cries: the cartridge starts {nativeCries}, the port publishes {cries}.";
        VerificationAssertions.AssertTrue(nativeCries > 0, "the replayed fight has cartridge cries to compare");
        VerificationAssertions.AssertTrue(cries >= nativeCries, "every cry the cartridge starts during the fight is published");
        return [report];
    }
}
