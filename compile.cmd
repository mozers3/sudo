@echo off
pushd "%~dp0"

set compiler="%WINDIR%\Microsoft.NET\Framework\v2.0.50727\vbc.exe"
REM set compiler="%WINDIR%\Microsoft.NET\Framework\v3.5\vbc.exe"
REM set compiler="%WINDIR%\Microsoft.NET\Framework\v4.0.30319\vbc.exe"

%compiler% /out:sudo.exe sudo.vb

copy sudo.exe %WINDIR%

popd
