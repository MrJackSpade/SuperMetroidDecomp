namespace SuperMetroid.AssetExtraction;

/// <summary>Host content a restored debugger graph re-attaches after its saved fields are set.</summary>
public interface IRestoredSharedContent
{
    /// <summary>Reconnects deserialized state to process-owned immutable content after its serialized fields have been restored.</summary>
    void ReattachSharedContent();
}
