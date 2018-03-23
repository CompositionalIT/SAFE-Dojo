@echo off
cls

IF EXIST "paket.lock" (
  .paket\paket.exe restore
) ELSE (
  .paket\paket.exe install
)

if errorlevel 1 (
  exit /b %errorlevel%
)

packages\build\FAKE\tools\FAKE.exe build.fsx %*
