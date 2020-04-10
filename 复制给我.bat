@echo off
timeout /T 3
xcopy "Native.Csharp\\bin\\x86\\Debug" "dev" /Y /S /F
pause