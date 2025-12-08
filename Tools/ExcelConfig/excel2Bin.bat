
set "WORKSPACE=../../"

set "LUBAN_DLL=%WORKSPACE%/Tools/ExcelConfig/Luban/Luban.dll"
set "CONF_ROOT=%WORKSPACE%/Tools/ExcelConfig"

dotnet "%LUBAN_DLL%" ^
    -t all ^
    -c cs-bin ^
    -d bin ^
    --conf "%CONF_ROOT%/luban.conf" ^
    -x "outputCodeDir=%WORKSPACE%/Project/Assets/Scripts/Configs/AutoGen" ^
    -x "outputDataDir=%WORKSPACE%/Project/Assets/Resources/ConfigsExcel" ^
    -x "pathValidator.rootDir=%WORKSPACE%/Project" ^
    -x "l10n.textProviderFile=%WORKSPACE%/Tools/ExcelConfig/l10n/texts.json"

pause
