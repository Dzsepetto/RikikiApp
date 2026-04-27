🎴 RikikiApp

A modern, cross-platform mobile companion app for the classic Rikiki card game — built with .NET MAUI and a clean MVVM architecture.

✨ Features
🎮 Create and manage Rikiki games
👥 Add players (local or guest)
🔄 Drag & drop player ordering
🃏 Round-based gameplay system
📊 Automatic score calculation
📈 Game results & statistics popup
⚡ Smooth navigation with custom NavigationService
💾 Local persistence using SQLite
🏗️ Architecture

The app follows a feature-based MVVM architecture, ensuring separation of concerns and scalability.

Features/
 ├── Game/
 ├── Players/
 ├── Profile/
 ├── Stats/
 └── Shared/
Core Concepts
MVVM (CommunityToolkit.Mvvm)
ContentView-first UI approach
Custom Navigation Service
Popup-driven interactions
Local database (SQLite)
🔄 Game Flow
Create a game
Add players
Start game
Players make calls
Play round
Enter results
Repeat until finished

Scoring logic:

✅ Exact call → 10 + called
❌ Miss → negative penalty based on difference
🧩 Tech Stack
.NET MAUI
C#
CommunityToolkit.Mvvm
MAUI Community Toolkit
SQLite (sqlite-net-pcl)
📱 UI Highlights
Custom bottom navigation bar
Popup-based workflows
Dynamic round header (Round X (Y cards))
Toggleable game/result views
Drag & drop player list
