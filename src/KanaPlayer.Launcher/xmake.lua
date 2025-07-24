add_rules("mode.release", "mode.debug")
add_rules("plugin.compile_commands.autoupdate", {outputdir = "./"})

target("KanaLauncher")
    set_kind("binary")
    set_languages("cxx20")
    add_files("src/main.cpp")
    add_files("KanaLauncher.rc")
    if is_os("windows") then 
        add_cxflags("/utf-8")
        add_rules("win.sdk.application")
    end
target_end()
