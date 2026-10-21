namespace SuperMetroid.Diagnostics.Android;

/// <summary>A queued Android session action and where its result goes.</summary>
/// <param name="Action">The action.</param>
/// <param name="Result">The result.</param>
internal readonly record struct AndroidSessionRequest(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result);
