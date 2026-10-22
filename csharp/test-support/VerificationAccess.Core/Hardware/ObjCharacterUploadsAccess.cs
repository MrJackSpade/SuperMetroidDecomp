namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="ObjCharacterUploads"/> members production does not use.</summary>
internal static class ObjCharacterUploadsAccess
{
    /// <summary>Exposes <see cref="ObjCharacterUploads"/> state that only verification reads.</summary>
    extension(ObjCharacterUploads self)
    {
        /// <summary>The uploads in order.</summary>
        public IReadOnlyList<ObjCharacterUpload> Uploads => self.uploads;
    }
}
