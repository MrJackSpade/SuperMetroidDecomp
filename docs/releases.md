# Tagged application releases

Push a tag such as `v1.0.0` or `v1.1.0-rc.1` to run `.github/workflows/release.yml`.
The workflow builds the tagged source, stages both downloads in a draft GitHub
Release, and publishes it only after both builds and asset checks pass. Tags with a
suffix become prereleases. Ordinary branch pushes and pull requests do not publish.

Downloads are `SuperMetroid-windows-x64.zip` (self-contained .NET desktop app),
`SuperMetroid-android-arm64.apk` (Release AOT), and `SHA256SUMS.txt`. Neither app
contains a ROM or extracted game resources. Players supply their ROM at first launch.
Windows builds retain the existing pinned shader compiler/version/hash check.

The Actions **Run workflow** button performs build validation and uploads Actions
artifacts without creating a release or posting to Discord. Without signing secrets,
this manual validation uses the runner's temporary Android debug key; do not use that
APK to update an existing installation. Tag builds require the persistent signing secrets.

## One-time GitHub configuration

Repository Actions secrets:

- `ANDROID_KEYSTORE_BASE64`: base64 bytes of the existing compatible Android keystore.
- `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD`, `ANDROID_STORE_PASSWORD`: signing identity.
- `DISCORD_RELEASES_WEBHOOK_URL`: webhook created in the Discord **releases** channel.

Repository Actions variable `DISCORD_RELEASES_CHANNEL_ID` identifies that channel.
The notifier additionally verifies server `1547270817312809112` before sending.
Keep the keystore backed up privately. The workflow decodes it into a temporary file,
passes passwords using MSBuild's `env:` mechanism, and deletes its temporary copy in
`finally`. No secret belongs in a workflow file, Git commit, release asset, or log.

The existing Android testing package ID/signature are retained for update compatibility.
It remains a debuggable sideloading build; this workflow does not submit to an app store.
Android version codes use the workflow run number plus one (reruns retain the same code).
Keep using this workflow for monotonically increasing codes; recreating it with a reset
run counter would require choosing a higher code before shipping another update.

## Discord delivery and retries

The announcement runs in the publishing job after release publication. It uses a
separate webhook from the commit-update tracker, disables mentions/link embeds, and
waits for Discord's message receipt. It reports the release link and both target platforms.
Missing configuration or a failed send makes the job fail visibly while keeping the
already-published GitHub release available.

The release body keeps a hidden `discord-release` JSON comment recording the send intent
and then the confirmed message ID. Rerunning a completed release updates that message
instead of posting another one. Published binaries are not overwritten on a rerun.
If the receipt remains `pending`, delivery is uncertain and automatic retries stop:

1. Inspect the releases channel for the exact tagged release link.
2. If it arrived, edit the hidden receipt to `status: "sent"` and add its
   `message_id`, retaining the `webhook_id` and `run_id` fields.
3. Only after confirming no message arrived, remove that pending comment and rerun.

Do not delete a sent receipt or replace the webhook to bypass reconciliation. A webhook
cannot make sending to Discord and saving the GitHub receipt one atomic transaction.

Actions permissions are read-only for builds; only the publishing job receives
`contents: write`. Credentials are scoped to their signing/notification steps, and official
actions are pinned to commit SHAs. Anyone allowed to push release tags can execute release
code with these credentials; restrict release-tag creation to trusted maintainers.

References: [GitHub tag triggers](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows),
[Android signing properties](https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-10.0),
and [Discord webhook receipts](https://docs.discord.com/developers/resources/webhook).
