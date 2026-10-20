using AndroidX.Activity.Result;

namespace SuperMetroid.Android;

/// <summary>Hands a document picker's chosen URI, or null when the picker was cancelled, to the activity.</summary>
/// <param name="chosen">Receives the chosen document.</param>
internal sealed class AndroidDocumentResult(Action<global::Android.Net.Uri?> chosen) : Java.Lang.Object, IActivityResultCallback
{
    /// <summary>Delivers the picker's result; anything other than a URI or nothing is a contract violation.</summary>
    public void OnActivityResult(Java.Lang.Object? result) => chosen(result switch
    {
        null => null,
        global::Android.Net.Uri uri => uri,
        _ => throw new InvalidOperationException($"A document picker returned {result.Class.Name} instead of a URI."),
    });
}
