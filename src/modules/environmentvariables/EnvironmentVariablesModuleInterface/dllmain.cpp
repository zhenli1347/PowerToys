// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

#include "Generated Files/resource.h"
#include "trace.h"

#include <common/logger/logger.h>
#include <common/utils/logger_helper.h>
#include <common/utils/resources.h>
#include <common/utils/winapi_error.h>
#include <interface/powertoy_module_interface.h>

#include <string>

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        Trace::RegisterProvider();
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        Trace::UnregisterProvider();
        break;
    }
    return TRUE;
}

namespace
{
    // Name of the powertoy module.
    inline const std::wstring ModuleKey = L"EnvironmentVariables";
}

class EnvironmentVariablesModuleInterface : public PowertoyModuleIface
{
private:
    bool m_enabled = false;

    std::wstring app_name;

    //contains the non localized key of the powertoy
    std::wstring app_key;

    HANDLE m_hProcess;

    bool is_process_running()
    {
        return WaitForSingleObject(m_hProcess, 0) == WAIT_TIMEOUT;
    }

    void launch_process()
    {
        Logger::trace(L"Starting ColorPicker process");
        unsigned long powertoys_pid = GetCurrentProcessId();

        std::wstring executable_args = L"";
        executable_args.append(std::to_wstring(powertoys_pid));

        SHELLEXECUTEINFOW sei{ sizeof(sei) };
        sei.fMask = { SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI };
        sei.lpFile = L"modules\\EnvironmentVariables\\PowerToys.EnvironmentVariables.exe";
        sei.nShow = SW_SHOWNORMAL;
        sei.lpParameters = executable_args.data();
        if (ShellExecuteExW(&sei))
        {
            Logger::trace("Successfully started the EnvironmentVariables process");
        }
        else
        {
            Logger::error(L"EnvironmentVariables failed to start. {}", get_last_error_or_default(GetLastError()));
        }

        m_hProcess = sei.hProcess;
    }

public:
    EnvironmentVariablesModuleInterface()
    {
        app_name = GET_RESOURCE_STRING(IDS_ENVIRONMENTVARIABLESMODULEINTERFACE_NAME);
        app_key = ModuleKey;
        LoggerHelpers::init_logger(app_key, L"ModuleInterface", "EnvironmentVariables");
    }

    ~EnvironmentVariablesModuleInterface()
    {
        m_enabled = false;
    }

    // Destroy the powertoy and free memory
    virtual void destroy() override
    {
        Logger::trace("EnvironmentVariablesModuleInterface::destroy()");
        delete this;
    }

    // Return the localized display name of the powertoy
    virtual const wchar_t* get_name() override
    {
        return app_name.c_str();
    }

    // Return the non localized key of the powertoy, this will be cached by the runner
    virtual const wchar_t* get_key() override
    {
        return app_key.c_str();
    }

    virtual bool get_config(wchar_t* buffer, int* buffer_size) override
    {
        return false;
    }

    virtual void call_custom_action(const wchar_t* action) override
    {
        if (is_process_running())
        {
        }
        else
        {
            launch_process();
        }
    }

    virtual void set_config(const wchar_t* config) override
    {
    }

    virtual bool is_enabled() override
    {
        return m_enabled;
    }

    virtual void enable()
    {
        Logger::trace("EnvironmentVariablesModuleInterface::enable()");
        m_enabled = true;
    };

    virtual void disable()
    {
        Logger::trace("EnvironmentVariablesModuleInterface::disable()");
        if (m_enabled)
        {
            TerminateProcess(m_hProcess, 1);
        }

        m_enabled = false;
    }
};

extern "C" __declspec(dllexport) PowertoyModuleIface* __cdecl powertoy_create()
{
    return new EnvironmentVariablesModuleInterface();
}
