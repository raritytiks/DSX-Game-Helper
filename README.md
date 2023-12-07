# DSX Game Helper

DSX Game Helper is a simple utility that makes managing and launching DSX (DualSenseX) for your specific games easier. With this tool, you can keep track of your games, select the version of DSX you're using, and specify the location of the DSX executable. DSX Game Helper will periodically check your running processes to detect when your games are running and automatically launch or kill DSX accordingly.

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 7 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.14-windows-x64-installer)

### Installation

1. Download the latest release of DSX Game Helper from the [Releases](https://github.com/raritytiks/dsx-game-helper/releases) page.
2. Extract the downloaded ZIP file to a directory of your choice.

### Usage

1. Run the `DSXGameHelper.exe` executable that you extracted.

2. Add your games to the list by clicking the "Add Game" button and selecting the game's executable file.

3. Choose the version of DSX you are using (DSX Free or DSX Steam) using the dropdown menu.

4. Click the "Browse" button to select the location of the DSX executable file on your system.

5. DSX Game Helper will periodically check your running processes for the added games.

6. If DSX detects a game running and you have selected DSX Free, it will automatically launch the DSX executable.

7. If you have selected DSX Steam, it will launch DSX via Steam when it detects a game running.

8. All your configurations and settings will be saved and loaded automatically on the next launch. Configuration and settings files will be created in the same folder as the `DSXGameHelper.exe` executable.

### Configuration

- The interval is 1 second.

### Updates

- DSX Game Helper may receive updates in the future. Check the [Releases](https://github.com/raritytiks/dsx-game-helper/releases) page for the latest versions.

### Issues and Feedback

- If you encounter any issues or have feedback, please [open an issue](https://github.com/raritytiks/dsx-game-helper/issues) on the GitHub repository.

---

**Note:** This tool is provided as-is and without warranty.
