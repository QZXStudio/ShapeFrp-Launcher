param(
    [Parameter(Mandatory)][string]$UpgradeCodeGuid,
    [Parameter(Mandatory)][string]$Version,
    [string]$OutputFile
)

$ns   = [guid]::Parse($UpgradeCodeGuid).ToByteArray()
$name = [text.encoding]::UTF8.GetBytes($Version)
$sha = [security.cryptography.SHA256]::Create()
$hash = $sha.ComputeHash($ns + $name)
$b    = [byte[]]$hash[0..15]
$b[7] = ($b[7] -band 0x0f) -bor 0x50
$b[8] = ($b[8] -band 0x3f) -bor 0x80
$guid = [guid]::new($b).ToString('D').ToUpper()

if ($OutputFile) {
    $guid | Out-File -Encoding utf8 -NoNewline -FilePath $OutputFile
} else {
    $guid
}
