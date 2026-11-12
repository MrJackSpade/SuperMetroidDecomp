# Shows a Windows toast for the asynchronous cleanup (tools/cleanup/README.md). A toast appears in the corner and the
# notification centre and never takes focus, so it is the cleanup's visible signal without a modal dialog.
# Run by Windows PowerShell 5.1 (powershell.exe), which projects the Windows Runtime toast API; PowerShell 7 does not.
# Usage: powershell.exe -NoProfile -File notify.ps1 -Title <title> -Text <text>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$Text
)
$ErrorActionPreference = 'Stop'
try {
    . (Join-Path $PSScriptRoot 'common.ps1')
    Disable-ErrorDialogs
    $null = [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime]
    $null = [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime]
    $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
    $xml.LoadXml('<toast><visual><binding template="ToastGeneric"><text>' + [Security.SecurityElement]::Escape($Title) +
        '</text><text>' + [Security.SecurityElement]::Escape($Text) + '</text></binding></visual></toast>')
    # Windows PowerShell's own registered application identity, so the toast needs no registration of its own.
    $application = '{1AC14E77-02E7-4E5D-B744-2EB1AE5198B7}\WindowsPowerShell\v1.0\powershell.exe'
    [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($application).Show(
        (New-Object Windows.UI.Notifications.ToastNotification $xml))
    exit 0
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    [Console]::Error.WriteLine($_.ScriptStackTrace)
    exit 1
}
