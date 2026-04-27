# 🎴 RikikiApp

A modern, cross-platform mobile companion app for the classic **Rikiki card game** — built with **.NET MAUI** and a clean **MVVM architecture**.

---

## ✨ Features

- 🎮 Create and manage Rikiki games  
- 👥 Add players (local or guest)  
- 🔄 Drag & drop player ordering  
- 🃏 Round-based gameplay system  
- 📊 Automatic score calculation  
- 📈 Game results & statistics popup  
- ⚡ Smooth navigation with custom NavigationService  
- 💾 Local persistence using SQLite  

---

## 🏗️ Architecture

The app follows a **feature-based MVVM architecture**, ensuring separation of concerns and scalability.

```text
Features/
 ├── Game/
 ├── Players/
 ├── Profile/
 ├── Stats/
 └── Shared/
