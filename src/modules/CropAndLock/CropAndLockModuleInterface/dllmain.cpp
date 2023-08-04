#include "pch.h"

#include <interface/powertoy_module_interface.h>
#include <common/SettingsAPI/settings_objects.h>

#include <common/logger/logger.h>
#include <common/utils/logger_helper.h>
#include <common/utils/resources.h>
#include <common/utils/winapi_error.h>

#include <CropAndLock/trace.h>
#include <CropAndLock/ModuleConstants.h>

#include <shellapi.h>
#include <common/interop/shared_constants.h>

namespace NonLocalizable
{
    const wchar_t ModulePath[] = L"PowerToys.CropAndLock.exe";
}

namespace
{
    const wchar_t JSON_KEY_PROPERTIES[] = L"properties";
    const wchar_t JSON_KEY_WIN[] = L"win";
    const wchar_t JSON_KEY_ALT[] = L"alt";
    const wchar_t JSON_KEY_CTRL[] = L"ctrl";
    const wchar_t JSON_KEY_SHIFT[] = L"shift";
    const wchar_t JSON_KEY_CODE[] = L"code";
    const wchar_t JSON_KEY_REPARENT_HOTKEY[] = L"reparent-hotkey";
    const wchar_t JSON_KEY_THUMBNAIL_HOTKEY[] = L"thumbnail-hotkey";
    const wchar_t JSON_KEY_VALUE[] = L"value";
}

BOOL APIENTRY DllMain( HMODULE /*hModule*/,
                       DWORD  ul_reason_for_call,
                       LPVOID /*lpReserved*/
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        Trace::RegisterProvider();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        Trace::UnregisterProvider();
        break;
    }
    return TRUE;

}

class CropAndLockModuleInterface : public PowertoyModuleIface
{
public:
    // Return the localized display name of the powertoy
    virtual PCWSTR get_name() override
    {
        return app_name.c_str();
    }

    // Return the non localized key of the powertoy, this will be cached by the runner
    virtual const wchar_t* get_key() override
    {
        return app_key.c_str();
    }


    // Return JSON with the configuration options.
    // These are the settings shown on the settings page along with their current values.
    virtual bool get_config(wchar_t* buffer, int* buffer_size) override
    {
        HINSTANCE hinstance = reinterpret_cast<HINSTANCE>(&__ImageBase);

        // Create a Settings object.
        PowerToysSettings::Settings settings(hinstance, get_name());

        return settings.serialize_to_buffer(buffer, buffer_size);
    }

    // Passes JSON with the configuration settings for the powertoy.
    // This is called when the user hits Save on the settings page.
    virtual void set_config(const wchar_t* config) override
    {
        try
        {
            // Parse the input JSON string.
            PowerToysSettings::PowerToyValues values =
                PowerToysSettings::PowerToyValues::from_json_string(config, get_key());

            parse_hotkey(values);

            values.save_to_settings_file();
        }
        catch (std::exception&)
        {
            // Improper JSON.
        }
    }

    virtual bool on_hotkey(size_t hotkeyId) override
    {
        if (m_enabled)
        {
            Logger::trace(L"CropAndLock hotkey pressed");
            if (!is_process_running())
            {
                Enable();
            }

            if (hotkeyId == 0) { // Same order as set by get_hotkeys
                SetEvent(m_reparent_event_handle);
            }
            if (hotkeyId == 1) { // Same order as set by get_hotkeys
                SetEvent(m_thumbnail_event_handle);
            }

            return true;
        }

        return false;
    }

    virtual size_t get_hotkeys(Hotkey* hotkeys, size_t buffer_size) override
    {
        if (hotkeys && buffer_size >= 2)
        {
            hotkeys[0] = m_reparent_hotkey;
            hotkeys[1] = m_thumbnail_hotkey;
        }
        return 2;
    }

    // Enable the powertoy
    virtual void enable()
    {
        Logger::info("CropAndLock enabling");
        Enable();
    }

    // Disable the powertoy
    virtual void disable()
    {
        Logger::info("CropAndLock disabling");
        Disable(true);
    }

    // Returns if the powertoy is enabled
    virtual bool is_enabled() override
    {
        return m_enabled;
    }

    // Destroy the powertoy and free memory
    virtual void destroy() override
    {
        Disable(false);
        delete this;
    }

    CropAndLockModuleInterface()
    {
        app_name = L"CropAndLock";
        app_key = NonLocalizable::ModuleKey;
        LoggerHelpers::init_logger(app_key, L"ModuleInterface", LogSettings::cropAndLockLoggerName);

        m_reparent_event_handle = CreateDefaultEvent(CommonSharedConstants::CROP_AND_LOCK_REPARENT_EVENT);
        m_thumbnail_event_handle = CreateDefaultEvent(CommonSharedConstants::CROP_AND_LOCK_THUMBNAIL_EVENT);

        init_settings();
    }

private:
    void Enable()
    {
        m_enabled = true;

        // TODO: Log telemetry

        // TODO: Actually pass the PID.
        unsigned long powertoys_pid = GetCurrentProcessId();
        std::wstring executable_args = L"";
        executable_args.append(std::to_wstring(powertoys_pid));
        
        ResetEvent(m_reparent_event_handle);
        ResetEvent(m_thumbnail_event_handle);

        SHELLEXECUTEINFOW sei{ sizeof(sei) };
        sei.fMask = { SEE_MASK_NOCLOSEPROCESS | SEE_MASK_FLAG_NO_UI };
        sei.lpFile = NonLocalizable::ModulePath;
        sei.nShow = SW_SHOWNORMAL;
        sei.lpParameters = executable_args.data();
        if (ShellExecuteExW(&sei) == false)
        {
            Logger::error(L"Failed to start CropAndLock");
            auto message = get_last_error_message(GetLastError());
            if (message.has_value())
            {
                Logger::error(message.value());
            }
        }
        else
        {
            m_hProcess = sei.hProcess;
        }

    }

    void Disable(bool const /*traceEvent*/)
    {
        m_enabled = false;

        // TODO: Figure out a better disable... Crop And Lock might need to run clean up to free the reparented windows.

        ResetEvent(m_reparent_event_handle);
        ResetEvent(m_thumbnail_event_handle);

        // TODO: Log telemetry
        /*if (traceEvent)
        {
            
        }*/

        if (m_hProcess)
        {
            TerminateProcess(m_hProcess, 0);
            m_hProcess = nullptr;
        }

    }

    void parse_hotkey(PowerToysSettings::PowerToyValues& settings)
    {
        auto settingsObject = settings.get_raw_json();
        if (settingsObject.GetView().Size())
        {
            try
            {
                Hotkey _temp_reparent;
                auto jsonHotkeyObject = settingsObject.GetNamedObject(JSON_KEY_PROPERTIES).GetNamedObject(JSON_KEY_REPARENT_HOTKEY).GetNamedObject(JSON_KEY_VALUE);
                _temp_reparent.win = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_WIN);
                _temp_reparent.alt = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_ALT);
                _temp_reparent.shift = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_SHIFT);
                _temp_reparent.ctrl = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_CTRL);
                _temp_reparent.key = static_cast<unsigned char>(jsonHotkeyObject.GetNamedNumber(JSON_KEY_CODE));
                m_reparent_hotkey = _temp_reparent;
            }
            catch (...)
            {
                Logger::error("Failed to initialize CropAndLock reparent shortcut from settings. Value will keep unchanged.");
            }
            try
            {
                Hotkey _temp_thumbnail;
                auto jsonHotkeyObject = settingsObject.GetNamedObject(JSON_KEY_PROPERTIES).GetNamedObject(JSON_KEY_THUMBNAIL_HOTKEY).GetNamedObject(JSON_KEY_VALUE);
                _temp_thumbnail.win = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_WIN);
                _temp_thumbnail.alt = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_ALT);
                _temp_thumbnail.shift = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_SHIFT);
                _temp_thumbnail.ctrl = jsonHotkeyObject.GetNamedBoolean(JSON_KEY_CTRL);
                _temp_thumbnail.key = static_cast<unsigned char>(jsonHotkeyObject.GetNamedNumber(JSON_KEY_CODE));
                m_thumbnail_hotkey = _temp_thumbnail;
            }
            catch (...)
            {
                Logger::error("Failed to initialize CropAndLock thumbnail shortcut from settings. Value will keep unchanged.");
            }
        }
        else
        {
            Logger::info("CropAndLock settings are empty");
        }
    }

    bool is_process_running()
    {
        return WaitForSingleObject(m_hProcess, 0) == WAIT_TIMEOUT;
    }

    void init_settings()
    {
        try
        {
            // Load and parse the settings file for this PowerToy.
            PowerToysSettings::PowerToyValues settings =
                PowerToysSettings::PowerToyValues::load_from_settings_file(get_key());

            parse_hotkey(settings);
        }
        catch (std::exception&)
        {
            Logger::warn(L"An exception occurred while loading the settings file");
            // Error while loading from the settings file. Let default values stay as they are.
        }
    }

    std::wstring app_name;
    std::wstring app_key; //contains the non localized key of the powertoy

    bool m_enabled = false;
    HANDLE m_hProcess = nullptr;

    // TODO: actual default hotkey setting in line with other PowerToys.
    Hotkey m_reparent_hotkey = { .win = true, .ctrl = true, .shift = true, .alt = false, .key = 'R' };
    Hotkey m_thumbnail_hotkey = { .win = true, .ctrl = true, .shift = true, .alt = false, .key = 'T' };

    HANDLE m_reparent_event_handle;
    HANDLE m_thumbnail_event_handle;

};

extern "C" __declspec(dllexport) PowertoyModuleIface* __cdecl powertoy_create()
{
    return new CropAndLockModuleInterface();
}
