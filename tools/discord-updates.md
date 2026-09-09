# Discord commit updates

Run with PowerShell 7 (`pwsh`). Git is the only other dependency. This repository's
destination is server `1547270817312809112`, channel `1547271407476408411`.
The webhook is checked against both IDs before each post.

## Connect Discord once

In the destination text channel, open **Edit Channel → Integrations → Webhooks**,
create a webhook, give it the desired bot name/avatar, and copy its URL. You need
Manage Webhooks permission. Store the URL locally, not in chat or the repository:

```powershell
$secret = Read-Host 'Discord webhook URL' -MaskInput
[Environment]::SetEnvironmentVariable('DISCORD_WEBHOOK_URL', $secret, 'User')
Remove-Variable secret
```

The CLI reads the process or user environment. No running bot service, bot token,
Discord developer application, or paid API is required.

## Queue and posting

Initialize once, including the very first published commit:

```powershell
pwsh -File tools/discord-updates.ps1 init -After ALL
pwsh -File tools/discord-updates.ps1 next
```

`next` fetches `origin/main` and returns exactly one commit: the oldest unposted
commit in first-parent order. Repeated calls return that same commit. Merge commits
represent the merged version, so their side-branch commits are not announced again.
There is no list-pending, skip, arbitrary-commit posting, or post-all command.

Inspect the returned commit with `git show --first-parent <SHA>`. Write a short,
plain-language description of what changed and what players will notice into a
UTF-8 file (for example `.git/discord-updates/summary.txt`). Describe only that
commit, distinguish tooling changes from gameplay changes, and do not claim tests
or player confirmation that the commit does not establish. The assistant writes
the summary; the CLI does not automatically treat a commit subject as release notes.

```powershell
pwsh -File tools/discord-updates.ps1 preview -Commit <full-SHA> -SummaryFile .git/discord-updates/summary.txt
pwsh -File tools/discord-updates.ps1 post -Commit <full-SHA> -SummaryFile .git/discord-updates/summary.txt
pwsh -File tools/discord-updates.ps1 next
```

Posts contain the version SHA, the summary, and the GitHub commit link. Mentions
and link embeds are disabled. The entire message must fit 2,000 characters.
The CLI waits for Discord's message receipt before advancing. It serializes all
commands with an OS file lock and writes state by atomic replacement.

## Tracker and recovery

`status` shows the last processed commit, count, and any uncertain delivery. The
tracker is `<git-common-directory>/discord-updates/state.json`, shared by linked
worktrees. It records message IDs, exact text, and timestamps. Back up this folder;
it is deliberately outside version control. One machine/tracker must own posting.
Do not initialize independent clones against the same channel. A missing tracker
requires initialization, and initialization refuses to overwrite an existing one.
Force-pushed history that loses the cursor blocks progress.

If a request fails, times out, or the process crashes after sending, the tracker
blocks all further posts. Check the channel for the exact version and summary:

```powershell
# If it arrived, copy its message ID (Discord Developer Mode enables Copy Message ID).
pwsh -File tools/discord-updates.ps1 resolve -MessageId <message-ID>
# Only if you have checked that it did NOT arrive:
pwsh -File tools/discord-updates.ps1 resolve -ConfirmedNotSent
```

Resolution with an ID retrieves the message from the original webhook and checks
its exact content. Clearing a pending attempt leaves the same commit next. There
are no automatic retries, including rate-limit responses; wait as required before
retrying. A crash between delivery and recording cannot be made exactly-once with
a webhook, so explicit reconciliation prevents blind duplicate sends.

Run the offline regression suite:

```powershell
pwsh -File tests/discord-updates.Tests.ps1
```

Discord API contract: https://docs.discord.com/developers/resources/webhook
