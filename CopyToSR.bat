set MOD_NAME="PerformanceMeter"

set BUILT_DLL=".\%MOD_NAME%\%MOD_NAME%\bin\Debug\%MOD_NAME%.dll"
set DB_DLL=".\%MOD_NAME%\%MOD_NAME%\bin\Debug\LiteDB.dll"
set LIB_DLL_DIR=".\%MOD_NAME%\%MOD_NAME%\bin\Debug\libs"
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"

copy %BUILT_DLL% %SYNTHRIDERS_MODS_DIR%
copy %DB_DLL% %SYNTHRIDERS_MODS_DIR%
copy %LIB_DLL_DIR%\* %SYNTHRIDERS_MODS_DIR%
