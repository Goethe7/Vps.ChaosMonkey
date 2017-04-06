$chaosUrl = "http://localhost:52356/api/Chaos/Registrants"
Invoke-RestMethod -Uri $chaosUrl -Method Get -UseDefaultCredential 