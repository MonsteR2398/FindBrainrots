@echo off
setlocal enabledelayedexpansion
set "file=f:\UnityProjects\FindBrainrots\Assets\BuildReport\Scripts\Editor\Window\BRT_BuildReportWindow.cs"
set "tempfile=%temp%\brt_fix.txt"

if exist "%tempfile%" del "%tempfile%"

for /f "usebackq tokens=*" %%a in (`type "%file%"`) do (
    set "line=%%a"
    set "line=!line:TextureData.DataId=BuildReportTool.TextureData.DataId!"
    echo !line!>>"%tempfile%"
)

copy /y "%tempfile%" "%file%" >nul
if exist "%tempfile%" del "%tempfile%"
echo Done
