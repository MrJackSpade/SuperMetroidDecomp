using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Makes the assets of Super Metroid Rebirth, the Godot game, from the player's own ROM: the one door by which the
/// program beside the Windows game and the Android game itself have them made.
/// </summary>
public static partial class RebirthAssets
{
    /// <summary>
    /// Makes the assets from the ROM at <paramref name="rom"/> into the folder <paramref name="assets"/>, telling
    /// <paramref name="progress"/> how far it has come each time that changes. The ROM is read and never changed.
    /// </summary>
    /// <remarks>
    /// The folder holds all of the assets or is not touched: they are written into a folder beside it, named as it is
    /// with <c>.making</c> after, which takes its place only once every file is written. What is read out of the ROM on
    /// the way is kept in another, named with <c>.installation</c> after. Both are removed whatever happens, and any
    /// left by a run that was cut short are removed first; they lie beside the assets so that they are on the same
    /// storage, which on a handheld is the one place the game may write this much.
    /// </remarks>
    /// <param name="rom">The ROM file.</param>
    /// <param name="assets">The folder the game reads its assets from.</param>
    /// <param name="progress">Told how far the making has come, one report at a time, from whichever thread did the work.</param>
    public static void Make(string rom, string assets, IProgress<RebirthAssetsProgress> progress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rom);
        ArgumentException.ThrowIfNullOrWhiteSpace(assets);
        ArgumentNullException.ThrowIfNull(progress);
        assets = Path.TrimEndingDirectorySeparator(Path.GetFullPath(assets));
        string installation = assets + ".installation", making = assets + ".making";
        Remove(installation);
        Remove(making);
        var tally = new Tally(progress);
        try
        {
            InstallationWork.Watching(tally.Read, () => GameAssetInstaller.Install(rom, installation, progress: tally));
            tally.BeginWriting();
            EngineAssetWriter.Write(installation, making, tally, tally.Wrote);
            Remove(assets);
            Directory.Move(making, assets);
        }
        finally
        {
            Remove(installation);
            Remove(making);
        }
    }

    /// <summary>
    /// What tells the assets one build of the extractor makes from another's: the identities of the extractor's own
    /// code and of the old game's code it reads its definitions from, either of which changes what is written. Assets
    /// made under another are to be made again.
    /// </summary>
    public static string Version =>
        $"{typeof(RebirthAssets).Assembly.ManifestModule.ModuleVersionId} {typeof(Core.Input.ControllerButton).Assembly.ManifestModule.ModuleVersionId}";

    /// <summary>Removes a folder and all in it, if it is there.</summary>
    private static void Remove(string folder)
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    /// <summary>
    /// Keeps how far a making has come and tells its watcher each time that changes, one report at a time however
    /// many threads are at work. It is the listener the reading and the writing name their steps to.
    /// </summary>
    private sealed partial class Tally(IProgress<RebirthAssetsProgress> progress) : IProgress<string>
    {
        /// <summary>Taken around each change and its report.</summary>
        private readonly Lock _gate = new();
        private string _doing = "";
        private int _read;
        private int _toRead;
        private int _part;
        private int _files;
        private bool _writing;

        /// <summary>Takes the step now starting; of the writing, it is the next of its parts.</summary>
        public void Report(string value)
        {
            lock (_gate)
            {
                _doing = value;
                if (_writing)
                    _part++;
                Tell();
            }
        }

        /// <summary>Takes how many pieces of the reading are done, and of how many known so far.</summary>
        internal void Read(int done, int known)
        {
            lock (_gate)
            {
                // The counts come from many threads, and not in the order they were taken.
                _read = Math.Max(_read, done);
                _toRead = Math.Max(_toRead, known);
                Tell();
            }
        }

        /// <summary>Has the steps named from now on counted as parts of the writing.</summary>
        internal void BeginWriting()
        {
            lock (_gate)
                _writing = true;
        }

        /// <summary>Takes how many files have been written.</summary>
        internal void Wrote(int files)
        {
            lock (_gate)
            {
                _files = files;
                Tell();
            }
        }

        private void Tell() => progress.Report(new RebirthAssetsProgress(_doing, _read, _toRead, _part, EngineAssetWriter.PartCount, _files));
    }
}
