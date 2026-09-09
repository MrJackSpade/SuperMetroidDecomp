$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../tools/discord-updates.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) "discord-queue-$([guid]::NewGuid())"
$null = New-Item -ItemType Directory $testRoot
$script:Repository = Join-Path $testRoot 'repository'
$script:StateDirectory = Join-Path $testRoot 'state'
$script:Remote = 'origin'
$script:Branch = 'main'
$script:sent = @()
$script:failSend = $false
function Assert($Condition, $Description) { if (-not $Condition) { throw "FAIL: $Description" }; Write-Host "PASS: $Description" }
function Expect-Failure([scriptblock]$Action, [string]$Pattern) {
    $failure = $null
    try { & $Action | Out-Null } catch { $failure = $_.Exception.Message }
    Assert ($failure -and $failure -like "*$Pattern*") "reject $Pattern"
}
function Get-Webhook { 'https://discord.com/api/webhooks/123/test' }
function Invoke-DiscordRequest($Method, $Url, $Body) {
    if ($Method -eq 'Get' -and $Url -notmatch '/messages/') { return @{channel_id='1547271407476408411';guild_id='1547270817312809112'} }
    if ($Method -eq 'Get') { return $script:sent[-1] }
    Assert ($Url.EndsWith('?wait=true')) 'wait for receipt'
    Assert ($Body.allowed_mentions.parse.Count -eq 0) 'disable mentions'
    $receipt = @{ id=[string](1000 + $script:sent.Count); channel_id='1547271407476408411'; content=$Body.content }
    $script:sent += $receipt
    if ($script:failSend) { throw 'simulated lost receipt' }
    return $receipt
}
try {
    $null = & git.exe init --quiet --initial-branch=main $Repository
    $null = Git @('config','user.email','queue-test@example.invalid')
    $null = Git @('config','user.name','Queue Test')
    $null = Git @('remote','add','origin','https://github.com/example/queue.git')
    # Redirect fetch locally without changing the GitHub identity used by init.
    $null = Git @('config',('url.' + $Repository.Replace('\','/') + '.insteadOf'),'https://github.com/example/queue.git')
    $shas = @()
    foreach ($number in 1..3) {
        Set-Content (Join-Path $Repository 'change.txt') "Change $number"
        $null = Git @('add','change.txt')
        $null = Git @('commit','--quiet','-m',"Change $number")
        $shas += Git @('rev-parse','HEAD')
    }
    # get-url expands insteadOf; override only this read for initialization.
    $originalGit = ${function:Git}
    function Git([string[]]$Arguments) {
        if (($Arguments -join ' ') -eq 'remote get-url origin') { return 'https://github.com/example/queue.git' }
        & $originalGit $Arguments
    }
    $script:Command='init'; $script:After='ALL'; $null=Invoke-Queue
    Expect-Failure { Invoke-Queue } 'already initialized'
    $script:Command='next'
    $first=Invoke-Queue
    Assert ($first.commit -eq $shas[0]) 'expose oldest commit only'
    Assert ((Invoke-Queue).commit -eq $shas[0]) 'next does not advance'
    $script:SummaryFile=Join-Path $testRoot 'summary.txt'; Set-Content $SummaryFile 'Plain language update.'
    $script:Command='post'; $script:Commit=$shas[1]
    Expect-Failure { Invoke-Queue } 'only that commit'
    $script:Commit=$shas[0]; $script:Command='preview'
    Assert ((Invoke-Queue).content.Contains('Plain language update.')) 'preview renders summary'
    $preview = Invoke-Queue
    Assert ($preview.content.StartsWith('**Version ') -and $preview.content.Contains(('```text' + "`nPlain language update.`n" + '```'))) 'updates have a bold heading and boxed body'
    Assert ($script:sent.Count -eq 0) 'preview does not send'
    $script:Command='post'; $null=Invoke-Queue
    $script:Command='next'; Assert ((Invoke-Queue).commit -eq $shas[1]) 'confirmed post advances exactly once'
    $script:Command='post'; Expect-Failure { Invoke-Queue } 'only that commit'
    $script:Commit=$shas[1]; $script:failSend=$true
    Expect-Failure { Invoke-Queue } 'simulated lost receipt'
    $script:Command='next'; Expect-Failure { Invoke-Queue } 'Delivery uncertain'
    $script:Command='resolve'; $script:MessageId='1001'; $null=Invoke-Queue
    $script:Command='next'; Assert ((Invoke-Queue).commit -eq $shas[2]) 'resolve delivered message without reposting'
    $script:Command='post'; $script:Commit=$shas[2]; $script:failSend=$false
    Set-Content $SummaryFile ('x' * 2000)
    Expect-Failure { Invoke-Queue } '2,000-character'
    Set-Content $SummaryFile 'Third change.'
    $heldLock=[IO.File]::Open((Join-Path $StateDirectory 'queue.lock'),'OpenOrCreate','ReadWrite','None')
    try { Expect-Failure { Invoke-Queue } 'queue lock' } finally { $heldLock.Dispose() }
    $null=Invoke-Queue
    $script:Command='next'; Assert ((Invoke-Queue).status -eq 'caught-up') 'queue finishes'
    $state=Get-Content (Join-Path $StateDirectory 'state.json') -Raw | ConvertFrom-Json
    Assert ($state.history.Count -eq 3 -and $script:sent.Count -eq 3) 'persist one receipt per commit'
    $null = Git @('checkout','--orphan','replacement')
    $null = Git @('commit','--quiet','-m','Replacement')
    $null = Git @('branch','-f','main','HEAD')
    Expect-Failure { Invoke-Queue } 'no longer on published'
    Write-Host 'All Discord queue tests passed (offline; no messages sent).'
} finally {
    $resolved=[IO.Path]::GetFullPath($testRoot)
    $tempPrefix=[IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
    if ($resolved.StartsWith($tempPrefix) -and (Split-Path $resolved -Leaf).StartsWith('discord-queue-')) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
