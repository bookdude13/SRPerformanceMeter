
set BUILT_VERSION="1.2.0"

set RELEASE_BUILD_DIR=".\PerformanceMeter\PerformanceMeter\bin\Release"
set MAIN_DLL="%RELEASE_BUILD_DIR%\PerformanceMeter.dll"
set DB_DLL="%RELEASE_BUILD_DIR%\LiteDB.dll"
set LIB_DLL_DIR="%RELEASE_BUILD_DIR%\libs"

set OUTPUT_DIR=".\build\SRPerformanceMeter_v%BUILT_VERSION%"
mkdir %OUTPUT_DIR%

copy %MAIN_DLL% %OUTPUT_DIR%
copy %DB_DLL% %OUTPUT_DIR%
copy %LIB_DLL_DIR%\* %OUTPUT_DIR%
