# Project Guide: Port for Command Palette

This document provides an overview of the project's architecture, focusing on its design principles and key components.

## Architecture Overview

The extension is designed following SOLID principles to ensure it is maintainable, scalable, and testable.

### Key Principles

*   **Single Responsibility Principle (SRP):** Each class has a single, well-defined responsibility. For instance, `PortService` is solely responsible for interacting with the Windows network stack to retrieve connection data, while `PortItemFactory` is responsible for transforming that data into UI components.
*   **Dependency Inversion Principle (DIP):** High-level modules do not depend on low-level modules; both depend on abstractions. This is achieved through the use of interfaces and the dependency injection setup in `PortForCommandPalette.cs`.

## Project Structure

The project is organized into the following key directories within the `PortForCommandPalette` project:

- **`PortForCommandPalette/`**:
  - [`PortForCommandPalette.cs`](./PortForCommandPalette/PortForCommandPalette.cs) - The main extension implementation and Dependency Injection container setup.
  - [`PortForCommandPaletteCommandsProvider.cs`](./PortForCommandPalette/PortForCommandPaletteCommandsProvider.cs) - The entry point for providing commands to the Command Palette.
- **`/Classes`**: Contains core data models and helper classes.
  - [`PortInfo.cs`](./PortForCommandPalette/Classes/PortInfo.cs) - The core data model representing a network connection (Protocol, Addresses, Ports, Process ID).
  - [`SettingsManager.cs`](./PortForCommandPalette/Classes/SettingsManager.cs) - Manages user-configurable settings, including search preferences and polling intervals.
  - `Constant.cs` - Centralized constants for the project.
- **`/Helpers`**: Contains utility classes.
  - `IdGenerator.cs` - Utility for generating stable IDs for ports based on Protocol and Local endpoint. This ensures pinning persistence.
  - `NativeMethods.cs` - P/Invoke signatures for the Windows IP Helper API (`iphlpapi.dll`).
- **`/Pages`**: Contains UI components.
  - [`PortsPage.cs`](./PortForCommandPalette/Pages/PortsPage.cs) - The main dynamic list page that displays active ports, handles search filtering, and manages the polling lifecycle.
- **`/Services`**: Primary service implementations.
  - [`PortService.cs`](./PortForCommandPalette/Services/PortService.cs) - Orchestrates the retrieval of TCP and UDP tables and resolves process names using an internal cache.
- **`/Workspaces`**:
    - [`PortItemFactory.cs`](./PortForCommandPalette/Workspaces/PortItemFactory.cs) - Constructs UI `ListItem` objects from `PortInfo` models, including contextual commands like "Kill Process".

## Core Components and Features

### Performance and Caching

To ensure the extension remains responsive:
- **ListItem Caching**: `PortsPage` maintains a cache of `ListItem` objects to avoid re-constructing them during every search update.
- **Intelligent Polling**: The extension monitors page visibility. Polling for updates starts when the page is loaded/focus and automatically stops when the page is hidden or unfocused, minimizing background CPU usage.
- **Process Info Caching**: `PortService` caches process names and paths. During search or rapid refreshes, the service avoids redundant system lookups for the same PID, significantly reducing overhead.
- **Asynchronous Loading**: Port discovery is performed on a background thread using `Task.Run` to avoid blocking the Command Palette UI.
- **Native Interop**: Direct usage of `GetExtendedTcpTable` and `GetExtendedUdpTable` provides the most efficient way to access network state on Windows.

### Advanced Search and Filtering

- **Hybrid Search Engine**: The search implementation uses a dual-strategy approach:
    - **Fuzzy Matching**: Applied to Process Names to allow for typos and flexible matching.
    - **Substring Matching**: Applied to Addresses and Ports for precise technical lookups.
- **Customizable Search Scope**: Users can toggle which properties (Local Address, Remote Port, etc.) are included in the search via the extension settings.

### Pinning and Stability

A key feature is the ability to pin specific network ports:
- **Stable IDs**: Unlike PIDs which change every time a process restarts, the IDs used for pinning are derived from the network endpoint itself. If you pin "TCP 127.0.0.1:3000", that pin will stay on your dashboard even if you stop and restart your development server.
- **Dynamic Updates**: When the port list refreshes, the pinned item automatically reflects the current process name and status associated with that port.

### Contextual Actions

Each port entry provides immediate access to management tools:
- **Kill Process**: Uses `System.Diagnostics.Process.Kill()` to terminate the owning process.
- **Open File Location**: Uses shell execution to highlight the executable in File Explorer.
- **Copy Local Address**: Quickly grab the address for use in a browser or other tool.

This architecture ensures a clean separation of concerns, making the codebase easier to understand, extend, and debug.