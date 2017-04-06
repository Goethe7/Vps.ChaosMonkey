param([string]$ServiceLocation, [string]$ServiceDescription)
$chaosUrl = "http://localhost:52356/api/Chaos/UnRegister?ServiceLocation=" + $ServiceLocation + "&ServiceDescription=" + $ServiceDescription
Invoke-RestMethod -Uri $chaosUrl -Method Post -UseDefaultCredential 