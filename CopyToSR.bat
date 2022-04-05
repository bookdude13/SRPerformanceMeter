
set BUILT_DLL=".\PerformanceMeter\PerformanceMeter\bin\Debug\PerformanceMeter.dll"
set DB_DLL=".\PerformanceMeter\PerformanceMeter\bin\Debug\LiteDB.dll"
set LIB_DLL_DIR=".\PerformanceMeter\PerformanceMeter\bin\Debug\libs"
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"

copy %BUILT_DLL% %SYNTHRIDERS_MODS_DIR%
copy %DB_DLL% %SYNTHRIDERS_MODS_DIR%
copy %LIB_DLL_DIR%\* %SYNTHRIDERS_MODS_DIR%
