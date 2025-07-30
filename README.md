
```markdown
# Git Graph (In-App) Extension for Mendix Studio Pro

This extension adds a custom **"Git Graph (In-App)"** option to the Mendix Studio Pro menu. When selected, it opens a dockable pane within the IDE that visualizes the Git commit history through a graphical interface.

---

## Features

- Adds a new top-level menu item: `Git Graph (In-App)`
- Opens an in-IDE pane to display Git commit history
- Clean integration using the Mendix Studio Pro Extensions API
- Helps developers quickly understand the branching and commit flow without leaving the IDE

---


##  Prerequisites

- Mendix Studio Pro 11 
- .NET Framework (as required by your Studio Pro version)
- Mendix Extensions SDK access enabled

---

## Setup Instructions

### 1. Build the Extension

Use Visual Studio to build the project and generate a `.dll` file.

```sh
dotnet build
````

> Make sure all necessary references to the Mendix Extensions API are correctly set.

---

### 2. Deploy the Extension

Copy the generated `.dll` file to the Mendix Studio Pro extensions directory:

```sh
C:\Program Files\Mendix\Studio Pro 11.x.x\extensions
```
---

### 3. Enable Extension Development Mode

Use the following command to start Studio Pro in extension mode (macOS example):

```sh
"/Applications/Studio Pro 11.0.0.73100-Beta.app/Contents/MacOS/studiopro" --enable-extension-development
```

---

### 4. Launch Studio Pro

Open Mendix Studio Pro and load any Mendix project.

---

### 5. Use the Git Graph (In-App) Menu

You will now see a new menu item:
**`Git Graph (In-App)`**

Clicking it will:

* Show an informational message
* Open a dockable pane inside Studio Pro
* Load and render the Git commit history visually

---