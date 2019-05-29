if "%~1" == "" (
    goto IncorrectNumberOfArguments
)

cd src/Core
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../Frontend
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../BackendApi
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../TextListener
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../TextRankCalc
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../VowelConsCounter
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../VowelConsRater
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../TextStatistics
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../TextProcessingLimiter
dotnet publish --configuration Release
if %ERRORLEVEL% NEQ 0 (
    goto BuildError
)

cd ../..
if exist "%~1" (
    rd /s /q "%~1"
)

mkdir "%~1"\Frontend
mkdir "%~1"\BackendApi
mkdir "%~1"\TextListener
mkdir "%~1"\TextRankCalc
mkdir "%~1"\VowelConsCounter
mkdir "%~1"\VowelConsRater
mkdir "%~1"\TextStatistics
mkdir "%~1"\TextProcessingLimiter
mkdir "%~1"\~conf
             
xcopy src\Frontend\bin\Release\netcoreapp2.2 "%~1"\Frontend\
xcopy src\BackendApi\bin\Release\netcoreapp2.2\publish "%~1"\BackendApi\
xcopy src\TextListener\bin\Release\netcoreapp2.2\publish "%~1"\TextListener\
xcopy src\TextRankCalc\bin\Release\netcoreapp2.2\publish "%~1"\TextRankCalc\
xcopy src\VowelConsCounter\bin\Release\netcoreapp2.2\publish  "%~1"\VowelConsCounter\
xcopy src\VowelConsRater\bin\Release\netcoreapp2.2\publish  "%~1"\VowelConsRater\
xcopy src\TextStatistics\bin\Release\netcoreapp2.2\publish  "%~1"\TextStatistics\
xcopy src\TextProcessingLimiter\bin\Release\netcoreapp2.2\publish  "%~1"\TextProcessingLimiter\

xcopy ~conf\properties.json "%~1"\Frontend\
xcopy ~conf\properties.json "%~1"\BackendApi\
xcopy ~conf\properties.json "%~1"\TextListener\
xcopy ~conf\properties.json "%~1"\TextRankCalc\
xcopy ~conf\properties.json  "%~1"\VowelConsCounter\
xcopy ~conf\properties.json  "%~1"\VowelConsRater\
xcopy ~conf\properties.json  "%~1"\TextStatistics\
xcopy ~conf\properties.json  "%~1"\TextProcessingLimiter\
xcopy ~conf\properties.json  "%~1"\~conf\
xcopy ~conf\count_instance.txt  "%~1"\~conf\
xcopy run.cmd "%~1"
xcopy stop.cmd "%~1"

echo BUILD SUCCESS
exit /b 0

:IncorrectNumberOfArguments
    echo Incorrect number of arguments.
	echo Example: build.cmd <SemVer>(MAJOR,MINOR,PATCH)
    exit /b 1

:BuildError
    echo Failed to build project.
    exit /b 1	
	