[CmdletBinding()]
param(
    [ValidateSet('init','next','preview','post','resolve','status')][string]$Command,
    [string]$After,
    [string]$Commit,
    [string]$SummaryFile,
    [string]$MessageId,
    [switch]$ConfirmedNotSent,
    [string]$Repository = (Split-Path $PSScriptRoot -Parent),
    [string]$StateDirectory,
    [string]$Remote = 'origin',
    [string]$Branch = 'main'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Git([string[]]$Arguments) {
    $result = & git.exe -C $Repository @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Git failed: $result" }
    return ($result -join "`n").Trim()
}
function Save-State($State, $Path) {
    $temporary = "$Path.$([guid]::NewGuid()).tmp"
    try {
        [IO.File]::WriteAllText($temporary, ($State | ConvertTo-Json -Depth 20), [Text.UTF8Encoding]::new($false))
        [IO.File]::Move($temporary, $Path, $true)
    } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary } }
}
function Get-HeadCommit($State) {
    $null = Git @('fetch', '--quiet', '--no-tags', $State.remote, "+refs/heads/$($State.branch):refs/remotes/$($State.remote)/$($State.branch)")
    $tip = "$($State.remote)/$($State.branch)"
    $chain = @( (Git @('rev-list','--first-parent','--reverse',$tip)) -split "`n" | Where-Object { $_ })
    if ($State.cursor) {
        $index = [Array]::IndexOf($chain, [string]$State.cursor)
        if ($index -lt 0) { throw 'Tracked commit is no longer on published first-parent history. Restore history or investigate; queue will not reset automatically.' }
        if ($index + 1 -ge $chain.Count) { return $null }
        return $chain[$index + 1]
    }
    if ($chain.Count) { return $chain[0] }
    return $null
}
function Get-Webhook {
    $url = [Environment]::GetEnvironmentVariable('DISCORD_WEBHOOK_URL')
    if (-not $url) { $url = [Environment]::GetEnvironmentVariable('DISCORD_WEBHOOK_URL','User') }
    if ($url -notmatch '^https://discord\.com/api(?:/v10)?/webhooks/[0-9]+/[A-Za-z0-9._-]+$') {
        throw 'Set DISCORD_WEBHOOK_URL to a Discord text-channel webhook URL (without query parameters).'
    }
    return $url
}
function Invoke-DiscordRequest($Method, $Url, $Body) {
    $options = @{ Method=$Method; Uri=$Url; TimeoutSec=30; MaximumRedirection=0 }
    if ($null -ne $Body) {
        $options.ContentType = 'application/json; charset=utf-8'
        $options.Body = [Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Depth 10 -Compress))
    }
    try { Invoke-RestMethod @options }
    catch { throw 'Discord request failed. No automatic retry was made. Check connectivity, webhook permissions and rate limits; reconcile pending delivery before retrying.' }
}
function Complete-Delivery($State, $Receipt, $Path) {
    if (-not $Receipt.id -or $Receipt.content -cne $State.pending.content) { throw 'Discord receipt does not match the pending update.' }
    $State.history = @($State.history) + @(@{ commit=$State.pending.commit; messageId=[string]$Receipt.id; channelId=[string]$Receipt.channel_id; postedAt=[DateTime]::UtcNow.ToString('o'); content=$State.pending.content })
    $State.cursor = $State.pending.commit
    $State.pending = $null
    Save-State $State $Path
    return @{ status='posted'; commit=$State.cursor; messageId=[string]$Receipt.id }
}
function Invoke-Queue {
    if (-not $StateDirectory) { $script:StateDirectory = Join-Path (Git @('rev-parse','--path-format=absolute','--git-common-dir')) 'discord-updates' }
    $null = New-Item -ItemType Directory -Force -Path $StateDirectory
    $path = Join-Path $StateDirectory 'state.json'
    $lock = $null
    try {
        try { $lock = [IO.File]::Open((Join-Path $StateDirectory 'queue.lock'), 'OpenOrCreate', 'ReadWrite', 'None') }
        catch { throw 'Another update command holds the queue lock. Try again when it finishes.' }
        if ($Command -eq 'init') {
            if (Test-Path -LiteralPath $path) { throw 'Queue already initialized; refusing to overwrite its tracker.' }
            if (-not $After) { throw 'Specify -After HEAD (future commits), a last-announced SHA, or ALL.' }
            $origin = Git @('remote','get-url',$Remote)
            if ($origin -notmatch '^(?:https://github\.com/|git@github\.com:)([^/]+/[^/]+?)(?:\.git)?$') { throw 'Remote must be a GitHub repository URL.' }
            $repositoryUrl = "https://github.com/$($Matches[1])"
            $state = @{ version=1; remote=$Remote; branch=$Branch; repositoryUrl=$repositoryUrl; channelId='1547271407476408411'; serverId='1547270817312809112'; cursor=$null; pending=$null; history=@() }
            $null = Get-HeadCommit $state
            if ($After -ne 'ALL') {
                $state.cursor = Git @('rev-parse', '--verify', "$After^{commit}")
                $null = Get-HeadCommit $state
            }
            Save-State $state $path
            return @{ status='initialized'; after=$state.cursor; stateFile=$path }
        }
        if (-not (Test-Path -LiteralPath $path)) { throw 'Run init first. See tools/discord-updates.md.' }
        $state = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -AsHashtable
        if ($state.version -ne 1) { throw 'Unsupported tracker version.' }
        if ($Command -eq 'status') { return @{ status=$(if ($state.pending) {'delivery-uncertain'} else {'ready'}); lastPosted=$state.cursor; postedCount=@($state.history).Count; pending=$state.pending; stateFile=$path } }
        if ($Command -eq 'resolve') {
            if (-not $state.pending) { throw 'No pending delivery to reconcile.' }
            if ($ConfirmedNotSent -and -not $MessageId) {
                $state.pending = $null
                Save-State $state $path
                return @{ status='retry-enabled'; instruction='The same commit remains next.' }
            }
            if ($ConfirmedNotSent -or $MessageId -notmatch '^[0-9]+$') { throw 'Provide -MessageId or -ConfirmedNotSent after checking the channel.' }
            $url = Get-Webhook
            if ($url.Split('/')[-2] -ne $state.pending.webhookId) { throw 'Use the webhook from the pending delivery.' }
            $receipt = Invoke-DiscordRequest 'Get' "$url/messages/$MessageId" $null
            return Complete-Delivery $state $receipt $path
        }
        if ($state.pending) { throw 'Delivery uncertain. Use status and resolve; no later commit can be exposed or posted.' }
        $next = Get-HeadCommit $state
        if (-not $next) { return @{ status='caught-up' } }
        if ($Command -eq 'next') {
            return @{ status='next'; commit=$next; url="$($state.repositoryUrl)/commit/$next"; message=(Git @('show','-s','--format=%B',$next)); changes=(Git @('diff-tree','--root','--first-parent','-m','--no-commit-id','--stat=80,50,20','-r',$next)); inspect="git show --first-parent $next" }
        }
        if ($Commit -cne $next) { throw 'Provide the full SHA exposed by next; only that commit can be posted.' }
        if (-not $SummaryFile) { throw 'Provide a UTF-8 plain-text -SummaryFile explaining the changes.' }
        $summary = (Get-Content -LiteralPath $SummaryFile -Raw -Encoding utf8).Trim()
        if (-not $summary) { throw 'Summary cannot be empty.' }
        $content = "Version $($next.Substring(0,7))`n`n$summary`n`n$($state.repositoryUrl)/commit/$next"
        if ($content.Length -gt 2000) { throw 'Message exceeds the Discord 2,000-character limit. Shorten the summary.' }
        if ($Command -eq 'preview') { return @{ status='preview'; commit=$next; content=$content } }
        $url = Get-Webhook
        $webhook = Invoke-DiscordRequest 'Get' $url $null
        if ([string]$webhook.channel_id -ne $state.channelId -or [string]$webhook.guild_id -ne $state.serverId) { throw 'Webhook does not belong to the configured Discord server and channel.' }
        $state.pending = @{ commit=$next; content=$content; webhookId=$url.Split('/')[-2]; attemptedAt=[DateTime]::UtcNow.ToString('o') }
        Save-State $state $path
        $receipt = Invoke-DiscordRequest 'Post' "$($url)?wait=true" @{ content=$content; allowed_mentions=@{parse=@()}; flags=4 }
        return Complete-Delivery $state $receipt $path
    } finally { if ($lock) { $lock.Dispose() } }
}
if ($Command) {
    try { Invoke-Queue | ConvertTo-Json -Depth 20 }
    catch { Write-Error $_.Exception.Message; exit 1 }
}

