set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
call %msBuildDir%\msbuild.exe src\Tempo.sln /p:Configuration=Release
if not exist "bin" mkdir bin
copy src\Tempo.Wpf\bin\Release\*.dll bin
set msBuildDir=