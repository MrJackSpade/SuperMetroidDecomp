using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification access to the native tables the import resolved an installed body's pictures from (#1465). Gameplay
/// reads only the pictures; verification compares these tables, and the records they select, with the cartridge.
/// </summary>
internal static class InstalledSamusBodyArtworkAccess
{
    /// <summary>The native tables behind an installed body.</summary>
    extension(InstalledSamusBodyArtwork self)
    {
        /// <summary>Each upper-body definition group's first record.</summary>
        internal ReadOnlySpan<int> TopSetStarts => self.Source.TopSetStarts;

        /// <summary>Each lower-body definition group's first record.</summary>
        internal ReadOnlySpan<int> BottomSetStarts => self.Source.BottomSetStarts;

        /// <summary>Each pose's first frame selector.</summary>
        internal ReadOnlySpan<int> PoseFirstFrames => self.Source.PoseFirstFrames;

        /// <summary>The contiguous frame-selector interval.</summary>
        internal ReadOnlySpan<SamusBodyFrameSelection> Frames => self.Source.Frames;

        /// <summary>The frame selector <paramref name="animationFrame"/> places after <paramref name="pose"/>'s first.</summary>
        /// <exception cref="InvalidDataException">The pose has no record, or the selector is outside the table.</exception>
        internal SamusBodyFrameSelection Selection(SamusPoseId pose, int animationFrame)
        {
            if (!SamusPoseNumbering.HasRecord(pose))
                throw new InvalidDataException($"Pose {pose} has no authored Samus body frame list.");
            int index = self.Source.PoseFirstFrames[SamusPoseNumbering.Number(pose)] + animationFrame;
            return (uint)index < SamusBodySources.FrameCount
                ? self.Source.Frames[index]
                : throw new InvalidDataException($"Samus frame {index} is outside extracted visual selectors.");
        }

        /// <summary>
        /// The record a native body set and position select: the group's first record plus the position, which may run
        /// into the next group, as the import resolves it.
        /// </summary>
        /// <exception cref="InvalidDataException">The set and position name no installed record.</exception>
        internal int DefinitionRecord(SamusBodyHalf half, int set, int position)
        {
            SamusBodySources source = self.Source;
            ReadOnlySpan<int> starts = half == SamusBodyHalf.Top ? source.TopSetStarts : source.BottomSetStarts;
            int first = SamusBodySources.GroupLengths(source.TopSetStarts.ToArray(), source.BottomSetStarts.ToArray()).Keys.Min();
            int record = set >= 0 && set < starts.Length && position is >= 0 and <= byte.MaxValue ? starts[set] + position : -1;
            return record >= first && record < SamusBodySources.RecordCount
                ? record
                : throw new InvalidDataException($"Samus {half} definition {set:X2}/{position:X2} is absent from installed art.");
        }
    }
}
