# SUDO for Windows

```
Usage: sudo [-n] <command> [args...]
 -n, --nowait       Not wait for the application to exit (use for launching GUI applications)

Example:
 sudo whoami /groups
 sudo -n notepad %windir%\system32\drivers\etc\hosts
 sudo net session
 sudo cmd /c dir %windir%
 sudo cmd /c "exit /b %RANDOM%" (and then, "echo %errorlevel%")
```