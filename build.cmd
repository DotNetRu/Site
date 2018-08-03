@echo off
powershell -NoProfile -ExecutionPolicy Unrestricted ./build/build.ps1 %CAKE_ARGS% %*