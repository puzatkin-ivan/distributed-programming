if "%~1" == "" goto invalid_argument

if exist "%~1" goto build_exists

cd src/Frontend
dotnet publish --configuration Release
if errorlevel 1 goto build_error

cd ../Backend
dotnet publish --configuration Release
if errorlevel 1 goto build_error

cd ../..                         

mkdir "%~1"\Frontend
mkdir "%~1"\Backend

xcopy src\Frontend\bin\Release\netcoreapp2.2\publish "%~1"\Frontend\
xcopy src\Backend\bin\Release\netcoreapp2.2\publish "%~1"\Backend\
xcopy run.cmd "%~1"
xcopy stop.cmd "%~1"

echo BUILD SUCCESS
exit 0

:invalid_argument
    echo Incorrect number of arguments.
	echo Example: build.cmd <MAJOR.MINOR.PATCH>
    exit 1

:build_error
    echo Failed to build project.
    exit 1	

:build_exists
   echo Build "%~1" already exists
   exit 1