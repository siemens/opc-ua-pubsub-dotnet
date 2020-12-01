nuget restore %~dp0_ALL.sln

IF %ERRORLEVEL% NEQ 0 (
  pause
  EXIT %ERRORLEVEL%
)

msbuild /maxcpucount:1 /t:Rebuild /p:Configuration=Release /bl %~dp0_ALL.sln
set MSBUILDEXITCODE=%ERRORLEVEL%

IF %MSBUILDEXITCODE% NEQ 0 (
  pause
  EXIT %MSBUILDEXITCODE%
)