$path = "f:\UnityProjects\FindBrainrots\Assets\BuildReport\Scripts\Editor\Window\BRT_BuildReportWindow.cs"
$content = [System.IO.File]::ReadAllText($path)
$content = $content -replace '(?<!BuildReportTool\.)TextureData\.DataId', 'BuildReportTool.TextureData.DataId'
[System.IO.File]::WriteAllText($path, $content)
Write-Host "Done"
