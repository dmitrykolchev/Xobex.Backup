#ifndef NOMINMAX
#define NOMINMAX
#endif

#include <windows.h>
#include <winioctl.h>
#include <iostream>
#include <vector>
#include <string>
#include <cstdint>
#include <memory>
#include <algorithm>
#include <format>

#include "BackupUtils.h"
#include "SecurityManager.h"
#include "VolumeManager.h"
#include "VolumeBackupEngine.h"
#include "VolumeRestoreEngine.h"

#pragma comment(lib, "Advapi32.lib")

int main(int argc, char* argv[]) {
    if (argc < 4) {
        std::cout << "Usage:\n"
            << "  imager.exe backup \\\\.\\D: C:\\backup.img\n"
            << "  imager.exe restore C:\\backup.img \\\\.\\D:\n";
        return 1;
    }

    // Инициализация всех привилегий токена до работы с подсистемой хранения
    if (!SecurityManager::EnableRequiredPrivileges()) {
        std::cerr << "[!] Warning: Some storage privileges could not be enabled. "
            << "Execution might fail on restricted/locked volumes.\n" << std::endl;
    }

    std::string mode = argv[1];
    std::wstring arg2(argv[2], argv[2] + strlen(argv[2]));
    std::wstring arg3(argv[3], argv[3] + strlen(argv[3]));

    if (mode == "backup") {
        return VolumeBackupEngine::CreateBackup(arg2, arg3) ? 0 : 1;
    }
    else if (mode == "restore") {
        return VolumeRestoreEngine::RestoreBackup(arg2, arg3) ? 0 : 1;
    }

    return 1;
}