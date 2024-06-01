param([xml]$Configuration,$Context)

Write-Information "hello"
Write-Information "SUBSCRIPTION=$((Get-AzContext).Subscription.Id)"
Write-Information $Configuration.OuterXml

$count = 0
foreach ($x in $Configuration.SelectNodes("/job/print")) {
    Write-Information "MESSAGE: $($x.message)"
    $count += 1
}

return @{
    printed = $count
}