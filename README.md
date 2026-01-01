# SUDO for Windows

```
Usage: sudo [-n] <command> [args...]
 -n, --nowait       Starts the command and does not wait for it to complete. Use to launch GUI applications.

Examples:
 sudo whoami /groups
 sudo -n notepad %windir%\system32\drivers\etc\hosts
 sudo bcdedit
 sudo net session
 sudo net user hacker P@ss123 /add
 sudo cmd /c "exit /b %random%" (and then, "echo %errorlevel%")
```