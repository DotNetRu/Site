@echo off

if [%1]==[] goto usage

powershell -NoProfile -ExecutionPolicy Unrestricted ./build/build.ps1 -Target News -ScriptArgs '-url="%1"'
goto :eof

:usage
@echo Usage: %0 ^<news-url^>