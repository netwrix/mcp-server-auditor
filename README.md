# Using the Netwrix Auditor MCP Server for Activity Record Investigation

## 1. Introduction

This document provides instructions on how to install and utilize the Netwrix Auditor Model Context Protocol (MCP) server. This server allows Large Language Models (LLMs), such as Claude via compatible clients like Claude Desktop, to query and analyze historical Activity Records collected by your Netwrix Auditor instance. This facilitates streamlined investigations and complex data retrieval using natural language queries.

## 2. Prerequisites

Before proceeding with the installation, please ensure you have the following:

* A functioning Netwrix Auditor 10.6 or later installation actively collecting audit data.
* An installed MCP-compatible client application (e.g., Claude Desktop).
* Credentials for a Netwrix Auditor user account with sufficient permissions to read audit data (Activity Records) via Netwrix Auditor's underlying mechanisms (e.g., API or database access, depending on server implementation).

## 3. Installation

This section describes how to set up the Netwrix Auditor MCP Server to run locally from source code for use with Claude Desktop.

### Local Installation from Source

This method involves downloading (or cloning) the server's source code and configuring Claude Desktop to run it directly using the `dotnet` command, with connection settings provided via environment variables.

**Prerequisites:**

* .NET SDK (ensure the version meets the server's requirements, e.g., 9.0 or later) installed. Download from <https://dotnet.microsoft.com/download>.
* Git installed (if you need to clone the repository). Download from <https://git-scm.com/>.
* Source code for the MCP server (either downloaded as a ZIP or cloned using Git).

**Steps:**


1. **Get the Source Code:**
   * **If using Git:** Open your terminal, navigate to where you want to store the code, and clone the repository:

     ```bash
     # Replace with the actual URL of the C# MCP server repository
     git clone https://github.com/netwrix/mcp-server-auditor.git
     # Navigate into the root directory of the cloned repository
     cd mcp-server-auditor
     ```
   * **If downloaded as ZIP:** Extract the ZIP file to a known location on your computer. Open your terminal and navigate into the extracted folder (the one containing the solution `.sln` or project `.csproj` file).
2. **Identify Project Path:**
   * Locate the main server project file within the source code. This file usually has a `.csproj` extension (e.g., `NetwrixAuditorMCPServer.csproj`).
   * Note the **full, absolute path** to this `.csproj` file. You will need it later.
   * *(Note: No manual configuration files need editing here; settings will be provided via Claude Desktop config).*
3. **Build (Optional but Recommended First Time):**
   * While `dotnet run` can build automatically, it's good practice to run a build initially to download dependencies and check for errors. Navigate to the directory containing the `.csproj` file if you aren't already there.

     ```bash
     dotnet build
     ```
4. **⚙️ Configure Claude Desktop:**
   * Locate and open the Claude Desktop configuration file (`claude_desktop_config.json`) in a text editor.
     * **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`
     * **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`
   * If the file or the `mcpServers` section doesn't exist, create the basic structure: `{"mcpServers": {}}`.
   * Add or modify the `mcpServers` section to include the configuration for running your server directly from source:

     ```json
     {
       "mcpServers": {
         "netwrix-auditor-local": {
           "command": "dotnet",
           "args": [
             "run",
             "--project",
             "C:\\path\\to\\your\\cloned\\repo\\YourServerProject\\YourServerProject.csproj",
             "--no-build"
           ],
           "cwd": "C:\\path\\to\\your\\cloned\\repo\\YourServerProject",
           "env": {
             "NETWRIX_API_URL": "https://your-netwrix-server:9699",
             "NETWRIX_API_USERNAME": "domain\\user",
             "NETWRIX_API_PASSWORD": "YOUR_NETWRIX_PASSWORD"
           }
         }
       }
     }
     
     ```

   \
   **Recommendations for Configuring the MCP Server**

   
   1. **Server Name**
      * Use a descriptive key name for the MCP server, e.g., "netwrix-auditor-local".
   2. **Command**
      * Set "dotnet" as the command if you're running a .NET Core or .NET project.
   3. **Arguments (args)**
      * Include "run" and "--project" followed by the **absolute path** to your server's .csproj file.
      * Replace the example paths in `"args"` (the path to the `.csproj` file) and `"cwd"` (the path to the directory containing the `.csproj` file) with the correct **absolute paths** on your system.
      * Example paths:
        * **Windows**:\nC:\\\\path\\\\to\\\\your\\\\cloned\\\\repo\\\\YourServerProject\\\\YourServerProject.csproj
        * **macOS/Linux**:\n/Users/yourname/path/to/cloned/repo/YourServerProject/YourServerProject.csproj
   4. **Working Directory (cwd)**
      * This should point to the **directory containing** the .csproj file.
   5. **Environment Variables (env)**
      * Provide Netwrix API connection details:
        * NETWRIX_API_URL: Your Netwrix API server URL (e.g., https://your-netwrix-server:9699)
        * NETWRIX_API_USERNAME: Your Netwrix username (e.g., domain\\\\user)
        * NETWRIX_API_PASSWORD: Your Netwrix password
      * Add any additional environment variables required by your server logic.
      * Ensure the environment variable names (`NETWRIX_API_URL`, etc.) exactly match what the server application expects to read.
      * Replace the placeholder values (`YOUR_NETWRIX_...`) in the `"env"` section with your actual Netwrix Auditor API endpoint and credentials. **Be careful with storing passwords directly in configuration files.** Consider security implications.
   6. **Additional Servers (Optional)**
      * You can define multiple MCP servers by adding more entries under the mcpServers object.

   \
5. **🔄 Restart Claude Desktop:**
   * Completely close the Claude Desktop application.
   * Reopen Claude Desktop.
6. **✅ Verify Server Operation:**
   * Claude Desktop should now launch the server using `dotnet run` with the specified project and environment variables.
   * Check for the tools icon (🔨) in Claude Desktop, click it, and verify that the server's tools are listed.
   * Test a tool by asking a relevant question.


---

### Option 2: Using Docker

This method runs the MCP server inside a Docker container, providing environment isolation. This is suitable if a Docker image for the server is available and you prefer containerization.

**Prerequisites:**

* Docker installed and running. Get Docker from <https://www.docker.com/products/docker-desktop/>.
* The name of the official Docker image for the Netwrix Auditor MCP server (e.g., `netwrix/auditor-mcp-server:latest` - **Note:** You must replace this with the actual, correct image name if provided by the server's developers).

**Steps:**


1. **Pull the Docker Image (Optional but Recommended):**

   ```bash
   # Replace with the correct image name and tag
   docker pull netwrix/auditor-mcp-server:latest
   ```
2. **Configure Claude Desktop:** Edit the `claude_desktop_config.json` file (see paths in Option 1 - Step 4). Add or modify the `mcpServers` section to tell Claude Desktop how to start the container:

   ```json
   {
     "mcpServers": {
       "netwrix-auditor-docker": {
         "command": "docker",
         "args": [
           "run",
           "--rm",
           "-i",
           "-p", "127.0.0.1:50051:50051",
           "-e", "NETWRIX_ENDPOINT=YOUR_API_SERVER",
           "-e", "NETWRIX_USER=YOUR_USERNAME",
           "-e", "NETWRIX_PASSWORD=YOUR_PASSWORD",
           "netwrix/auditor-mcp-server:latest"
         ]
       }
     }
   }
   
   ```

   \
   **Recommendations for Configuring the Docker-based MCP Server**

   
   1. **Server Key**
      * Use a descriptive key (e.g., "netwrix-auditor-docker") to identify the server configuration.
      * Replace `netwrix/auditor-mcp-server:latest` with the correct Docker image name.
   2. **Command**
      * Set "docker" as the command to run a containerized MCP server.
   3. **Arguments (args)**
      * "run": Start a new container.
      * "--rm": Automatically remove the container when it exits (cleans up resources).
      * "-i": Keeps STDIN open, which is required for interactive services like MCP.
   4. **Port Mapping**
      * Use "-p", "127.0.0.1:50051:50051" to:
        * Expose the MCP port only on the local machine.
        * Adjust the port if your MCP server uses a different one.
   5. **Environment Variables**
      * Use "-e" to pass configuration values into the container.
      * Replace `YOUR_API_ENDPOINT`, `YOUR_USERNAME`, `YOUR_PASSWORD` with your actual Netwrix Auditor connection details:
        * NETWRIX_ENDPOINT: The URL or hostname of your Netwrix API server.
        * NETWRIX_USER: The username for authentication (e.g., domain\\\\user).
        * NETWRIX_PASSWORD: The user's password.
      * Add other environment variables as needed (e.g., "ASPNETCORE_ENVIRONMENT=Production").
      * Verify the expected environment variable names (`NETWRIX_ENDPOINT`, etc.) and the container port (`50051` in the example) from the server's documentation. Adjust the `-p` mapping if needed.
   6. **Docker Image**
      * Specify the Docker image name and tag to run (e.g., "netwrix/auditor-mcp-server:latest").
      * Make sure this matches the image available in your registry or environment.
   7. **Additional Servers (Optional)**
      * Add more server configurations inside the mcpServers object as needed.

      \
3. **Restart Claude Desktop:** Close and reopen the application. Claude Desktop will now execute the `docker run` command to start the server in a container. You can verify its operation as described in Step 6 of Option 1.

## 4. Core Functionality: Querying Activity Records

The primary and sole function of this `MCP` server is to provide access to Netwrix Auditor Activity Records. It allows you to query the historical log of actions ("who did what, where, and when") that have occurred within your monitored IT environment.

You can filter searches based on standard Netwrix Auditor fields, including:

* **Who:** The user or entity that performed the action.
* **Object type:** The type of object affected (e.g., File, Folder, User, Group, Mailbox, Registry Key).
* **Action:** The specific operation performed (e.g., Modified, Added, Removed, Read, Failed Logon, Renamed).
* **What:** Details about the object involved or the specifics of the change.
* **Where:** The location, system, or target where the action occurred.
* **When:** A specific date or time range.
* **Data source:** The origin of the audit data (e.g., Active Directory, File Servers, Exchange Online, Azure AD, SQL Server, VMware).
* **Monitoring plan:** The specific Netwrix Auditor monitoring plan that collected the data.

## 5. Use Cases and Benefits

This `MCP` server is particularly useful for:

* **Streamlined Investigations:** Quickly search for specific events using natural language without needing deep expertise in the Netwrix Auditor search interface. Ask questions like:

  > "Show all file deletions on the finance share yesterday." "List failed logon attempts for `'admin_user'` last week." "Who modified the `'Domain Admins'` group membership on March 26th, 2020?" "Find activities related to the document `'ProjectPhoenix.docx'` in SharePoint Online."
* **Complex Queries:** Leverage the `LLM`'s capabilities to formulate searches combining multiple criteria across different fields.

  > "Show permission changes on SQL server `'SQL01'` performed by users outside the IT department in the last month." "Find all activities by `'contractor_X'` involving file servers between 9 PM and 6 AM last week."
* **Correlating Activity Records:** Identify patterns by linking different activity events.

  > "Show successful logons for users who were recently added to the `'Remote Access'` AD group." "List file access attempts on server `'FS-HR'` immediately following failed logon events for the same user."

## 6. Key Limitations

It is crucial to understand what this `MCP` server cannot do:

* **No State-in-Time (SIT) Data Access:** The server cannot retrieve or analyze point-in-time snapshots of system state. It cannot answer questions like "Who currently has access to this folder?" or "What were the members of the `'Admin'` group last Tuesday?". For this information, you must use the Netwrix Auditor user interface to generate the appropriate State-in-Time reports.
* **No Configuration Capabilities:** This server cannot be used to configure Netwrix Auditor. This includes creating or modifying monitoring plans, setting up alerts or subscriptions, or changing any product settings. All configuration must be done through the Netwrix Auditor user interface.
* **Read-Only Access:** The server provides read-only access to historical Activity Records. It cannot perform any administrative actions within Netwrix Auditor or the monitored systems.
* **Historical Data Only:** The server queries existing audit data. It does not provide real-time monitoring or alerting (use Netwrix Auditor's built-in alerting for that).

## 7. Example Interaction

While you interact using natural language, the `LLM` and `MCP` client translate your request into a structured query for the server.

**Your Query:**

> "Show me who removed the file `'confidential_report.docx'` from the `'Finance'` share on server `'FS01'` yesterday."

**Conceptual MCP Request:** The system would generate a request similar to:

```json
{
  "tool_name": "netwrix_auditor.search_activity_records",
  "parameters": {
    "action": "Removed",
    "object_type": "File",
    "what_contains": "confidential_report.docx",
    "where_contains": "FS01", // And potentially path info like 'Finance'
    "start_date": "YYYY-MM-DD", // Yesterday's date
    "end_date": "YYYY-MM-DD"   // Yesterday's date
  }
}
```

**Result:** The server returns the relevant Activity Record(s) matching these criteria.

## 8. Questions

If you need help using this MCP server or understanding your results, just visit the [Netwrix Community](https://community.netwrix.com/) - we’re here to help!