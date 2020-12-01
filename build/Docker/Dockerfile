# escape=`
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.403-nanoserver-1809 as core

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-20201013-windowsservercore-ltsc2019

ENV `
    # Unset ASPNETCORE_URLS from aspnet base image
    ASPNETCORE_URLS= `
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true `
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip `
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINERS=true

RUN $env:PATH = 'C:\Program Files\powershell;C:\Program Files\dotnet' + ';' + $env:PATH; `
    [Environment]::SetEnvironmentVariable('PATH', $env:PATH, [EnvironmentVariableTarget]::Machine)

COPY --from=core ["/Program Files/dotnet", "/Program Files/dotnet"]
COPY --from=core ["/Program Files/powershell", "/Program Files/powershell"]

# User PowerShell Core as default shell
SHELL ["pwsh","-command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Trigger first run experience by running arbitrary cmd
RUN dotnet help