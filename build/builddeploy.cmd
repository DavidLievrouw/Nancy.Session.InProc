cls
echo off
SET DIR=%~dp0%
IF NOT EXIST "%DIR%log" MKDIR "%DIR%log"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /m /v:n "%DIR%Nancy.Session.InProc.proj" /target:BuildDeploy /logger:FileLogger,Microsoft.Build.Engine;LogFile=%DIR%log/builddeploy.log
pause