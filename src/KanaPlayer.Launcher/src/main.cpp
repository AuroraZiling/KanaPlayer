#include <filesystem>
#include <string>
#include <vector>
#include <windows.h>

void panic_then_pause(const std::wstring &msg)
{
    std::wstring command = L"cmd.exe /c echo " + msg + L" && pause";
    _wsystem(command.c_str());
    exit(1);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
    int argc = 0;
    LPWSTR *argv = CommandLineToArgvW(GetCommandLineW(), &argc);

    std::vector<std::wstring> args(argv, argv + argc);

    std::filesystem::path working_path = std::filesystem::current_path();
    std::filesystem::path root_directory = working_path / L"KanaApp";

    if (!std::filesystem::exists(root_directory))
    {
        panic_then_pause(root_directory.wstring() + L" does not exist");
    }
    else if (!std::filesystem::is_directory(root_directory))
    {
        panic_then_pause(root_directory.wstring() + L" is not a directory");
    }

    std::filesystem::path executable = root_directory / L"KanaPlayer.Windows.exe";
    if (!std::filesystem::exists(executable))
    {
        panic_then_pause(executable.wstring() + L" not found");
    }

    STARTUPINFOW si = {sizeof(si)};
    PROCESS_INFORMATION pi;
    std::wstring command = executable.wstring();

    if (argc > 1)
    {
        for (int i = 1; i < argc; ++i)
        {
            command += L" " + args[i];
        }
    }

    std::vector<wchar_t> command_buffer(command.begin(), command.end());
    command_buffer.push_back(L'\0');

    if (!CreateProcessW(NULL, command_buffer.data(), NULL, NULL, FALSE, 0, NULL, root_directory.wstring().c_str(), &si,
                        &pi))
    {
        panic_then_pause(L"Failed to start process");
    }

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);

    return 0;
}
