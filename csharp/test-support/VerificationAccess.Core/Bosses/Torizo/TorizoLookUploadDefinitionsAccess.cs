namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoLookUpload"/> members production does not use.</summary>
internal static class TorizoLookUploadDefinitionsAccess
{
    /// <summary>The cartridge source a look's characters were extracted from: its page's source plus its page byte.</summary>
    extension(TorizoLookUpload upload)
    {
        /// <summary>The look's cartridge source address.</summary>
        internal int SourceAddress => SuperMetroid.AssetExtraction.CartridgeTables.TorizoSourceDefinitions.PageSources[upload.Page] + upload.PageByte;
    }
}
