# League Build Tool (v1.0)

## Overview
A **.NET 8** application designed to assist League of Legends players in theory-crafting and optimizing item builds.
> **Dev Note:** This project is an exploration of **AI-Assisted Development**, utilizing GitHub Copilot to rapidly prototype a **Clean Architecture** solution.

## Project Structure
The solution follows a modular **N-Tier Architecture** to separate concerns:
* **`LeagueBuildTool.Core`**: Contains the business logic, champion models, and API data contracts.
* **`LeagueBuildTool.App`**: The main entry point and User Interface (Windows Forms).
* **`LeagueBuildTool.Tests`**: Unit tests to ensure calculation accuracy.

## Key Features
* **Real-Time Data:** Architecture designed to consume the Riot Games API for up-to-date item stats.
* **Build Optimization:** Logic to filter and recommend items based on statistical efficiency (Gold Value vs. Stats).
* **Modern Stack:** Built on the latest .NET 8 SDK.

## Tech Stack
* **Language:** C#
* **Framework:** .NET 8.0
* **Tools:** Visual Studio 2022, GitHub Copilot, xUnit (Testing).

## Getting Started
1. Clone the repository.
2. Open `LeagueBuildTool.sln` in Visual Studio.
3. Right-click the `LeagueBuildTool.App` project and select **"Set as Startup Project"**.
4. Press **F5** to run.
