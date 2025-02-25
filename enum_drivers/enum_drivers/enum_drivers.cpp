#include <windows.h>
#include <iostream>
#include <psapi.h>
#include <format>
#include <cstdint>
#include <optional>
#include <vector>
#include <string>

// To ensure correct resolution of symbols, add Psapi.lib to TARGETLIBS
// and compile with -DPSAPI_VERSION=1

std::optional<std::uint32_t> GetNumDrivers() {
	// EnumDeviceDrivers spews the load addresses of every driver into an array.  The nBytes out param
	// is the size of the array needed to hold all the load addresses, so nBytes = numDrivers*PointerSize.
	DWORD nBytes {};
	int result = EnumDeviceDrivers(nullptr, 0, &nBytes);
	if (result == 0) {
		return std::nullopt;
	}
	static_assert(sizeof(DWORD)==sizeof(std::uint32_t));
	std::uint32_t nDrivers = static_cast<std::uint32_t>(nBytes/sizeof(LPVOID));
	return nDrivers;
}

std::optional<std::vector<void*>> GetDriverLoadAddresses() {
	std::optional<std::uint32_t> nDrivers = GetNumDrivers();
	if (!nDrivers) {
		return std::nullopt;
	}
	std::vector<void*> loadAddrs(*nDrivers);

	DWORD nBytesWritten {};
	int result = EnumDeviceDrivers(loadAddrs.data(), loadAddrs.size()*sizeof(void*), &nBytesWritten);
	if (result==0 || loadAddrs.size()*sizeof(void*)!=nBytesWritten) {
		return std::nullopt;
	}

	return loadAddrs;
}

std::optional<std::wstring> GetDriverName(void* loadAddr) {
	std::wstring s(200, L'\0');

	DWORD nChars = GetDeviceDriverBaseNameW(reinterpret_cast<LPVOID>(loadAddr), s.data(), s.size());
	if (nChars == 0) {
		return std::nullopt;
	}
	s.resize(nChars);

	return s;
}

std::optional<std::wstring> GetDriverFileName(void* loadAddr) {
	std::wstring s(200, L'\0');

	DWORD nChars = GetDeviceDriverFileNameW(reinterpret_cast<LPVOID>(loadAddr), s.data(), s.size());
	if (nChars == 0) {
		return std::nullopt;
	}
	s.resize(nChars);

	return s;
}

int main(int argc, char* argv[]) {
	std::optional<std::uint32_t> nDrivers = GetNumDrivers();
	if (!nDrivers) {
		std::cout << "GetNumDrivers() failed!" << std::endl;
		return 1;
	}
	std::wcout << std::format(L"There are {} drivers:\n", *nDrivers);

	std::optional<std::vector<void*>> loadAddrs = GetDriverLoadAddresses();
	if (!loadAddrs) {
		std::cout << "GetDriverLoadAddresses() failed!" << std::endl;
		return 1;
	}

	for (int i {0}; void* currLoadAddr : *loadAddrs) {
		std::optional<std::wstring> currDriverName = GetDriverName(currLoadAddr);
		if (!currDriverName) {
			currDriverName = L"Error retrieving driver name";
		}
		std::optional<std::wstring> currDriverFileName = GetDriverFileName(currLoadAddr);
		if (!currDriverName) {
			currDriverName = L"Error retrieving driver file name";
		}
		std::wcout << std::format(L"{}: {}: {}\n", i++, *currDriverName, *currDriverFileName);
	}

	return 0;
}