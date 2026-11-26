#if GODOT_ANDROID
using Engine.Content;
using Made = global::SuperMetroid.AssetExtraction.Engine.RebirthAssets;
using MadeProgress = global::SuperMetroid.AssetExtraction.Engine.RebirthAssetsProgress;

namespace Game;

/// <summary>
/// Makes this game's assets from the player's own ROM, on the device. A handheld has no computer beside it to make
/// them on, so the Android game carries the asset extractor and has it make them. This is the one place the game
/// touches the extractor, and it is built into the Android game alone.
/// </summary>
public sealed class RomAssets : AssetsMaker
{
    /// <summary>
    /// The share of the making's time that reading the ROM takes, the writing of the assets taking the rest: the bar's
    /// two lengths. Measured on a Retroid Pocket 6 on 2026-11-25, where the reading took 104 seconds of 150.
    /// </summary>
    private const float ReadingShare = 0.7f;

    /// <inheritdoc/>
    public override string Asks => "Choose your Super Metroid ROM.";

    /// <inheritdoc/>
    public override IReadOnlyList<string> Filters => ["*.smc", "*.sfc"];

    /// <inheritdoc/>
    public override string Version => Made.Version;

    /// <inheritdoc/>
    public override void Make(string source, string folder, IProgress<MakingProgress> steps) => Made.Make(source, folder, new Told(steps));

    /// <summary>Tells the setup screen the extractor's progress as one length and a line of counts. The extractor reports one at a time.</summary>
    private sealed class Told(IProgress<MakingProgress> steps) : IProgress<MadeProgress>
    {
        /// <summary>
        /// The furthest the reading has come, which the bar never goes back from: the reading learns of more work as
        /// its loops start, and its share of what it knows of can fall when it does.
        /// </summary>
        private float _read;

        /// <inheritdoc/>
        public void Report(MadeProgress value)
        {
            if (value.Part > 0)
            {
                // A part of the writing is counted done as the next starts.
                float written = (value.Part - 1) / (float)value.Parts;
                steps.Report(new MakingProgress(value.Doing, ReadingShare + (1 - ReadingShare) * written, $"{value.Files:N0} files written"));
                return;
            }

            if (value.ToRead > 0)
                _read = Math.Max(_read, value.Read / (float)value.ToRead);
            steps.Report(new MakingProgress(value.Doing, ReadingShare * _read, $"{value.Read:N0} of {value.ToRead:N0} pieces read"));
        }
    }
}
#endif
