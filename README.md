# Pokémon Minesweeper

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Language](https://img.shields.io/badge/Language-C%23-green)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)
![Status](https://img.shields.io/badge/Project-Active-brightgreen)

A Pokémon-themed twist on the classic **Minesweeper** game.

This project is a fork of  
**Pokemon Sweeper by switch87**  
https://github.com/switch87/pokemon-sweeper

The original project replaces mines with Pokémon. 
This fork expands the concept into a **persistent Pokémon-collecting strategy game** with team building and battles.

---

# Screenshots

### Main Menu
![Main Menu](docs/screenshots/main_menu.png)

### Gameplay Board
![Gameplay](docs/screenshots/gameplay.png)

### Pokémon Box / Team Builder
![Pokemon Box](docs/screenshots/pokemon_box.png)

---

# Features

## Classic Minesweeper Gameplay
- Clear tiles while avoiding hidden Pokémon
- Numbers indicate how many Pokémon are adjacent to a tile
- Flag suspected Pokémon locations

## Pokémon Encounters
When uncovering a tile containing a Pokémon:

- Your **team battles the wild Pokémon**
- The game only ends if **your entire team faints**

## Pokémon Collection
After clearing a board, you may **keep Pokémon discovered during the run**.

| Difficulty | Pokémon You Can Keep |
|------------|----------------------|
| Easy | 2 |
| Medium | 3 |
| Hard | 4 |

Captured Pokémon are stored in your **Pokémon Box**.

---

## Pokémon Box & Team Building

The **Pokémon Box** allows you to:

- View all Pokémon you have captured
- Organize your collection
- Assemble a **battle team**

When starting a game with a team assembled, your Pokémon will automatically battle wild encounters.

---

## Battle System

The battle system implements several mechanics from the Pokémon games:

- Pokémon **leveling**
- **Individual Values (IVs)**
- **Effort Values (EVs)**
- **Natures**

Evolution is **not currently implemented**.

---

## Dynamic Pokémon Support

This project integrates with **PokeAPI**:

https://pokeapi.co/

Because of this integration:

- All **existing Pokémon** are supported
- **Future generations will automatically work**
- No code updates are required when new Pokémon are added

---

# Controls

The entire game is controlled using the **mouse**.

| Action | Control |
|------|------|
| Reveal Tile | Left Click |
| Flag Tile | Right Click |
| Menus / UI | Mouse + Buttons |

---

# Tech Stack

| Component | Technology |
|--------|--------|
| Language | C# |
| Framework | .NET 9 |
| Original Codebase | .NET Framework 4.5 |
| Data Storage | MongoDB or Local JSON |
| Pokémon Data | PokeAPI |

---

# Architecture Overview

The project expands a classic Minesweeper implementation into a small game system with persistent player data, battle mechanics, and dynamic Pokémon support. The architecture is designed to keep gameplay logic, data persistence, and external integrations loosely coupled.

---

## High-Level Architecture

The game is composed of several core systems:


+----------------------+
| UI Layer |
| (Menus, Board, Box) |
+----------+-----------+
|
v
+----------------------+

Game Logic Layer
Board Generation
Tile State Handling
Battle System
Team Management
+----------+-----------+
       |
       v

+----------------------+

Data Layer
MongoDB Persistence
JSON File Storage
Storage Abstraction
+----------+-----------+
       |
       v

+----------------------+

External API Layer
PokeAPI Integration
Pokémon Data Loader
+----------------------+

---

## Game Board System

The board system manages core Minesweeper gameplay.

Responsibilities include:

- Generating a board based on difficulty
- Randomly generating and placing hidden Pokémon
- Calculating adjacent Pokémon counts
- Tracking tile states

Each tile can exist in one of several states:

- Hidden
- Revealed
- Flagged
- Pokémon Encounter

When a tile is revealed, the system determines whether it contains:

- Empty space
- A numbered hint
- A Pokémon encounter

---

## Battle System

When a player reveals a tile containing a Pokémon, the battle system is triggered.

### Battle Flow

1. The wild Pokémon is revealed.
2. The player's active team enters battle.
3. Pokémon fight sequentially until one faints.
4. If all player Pokémon faint, the game ends.
5. If the wild Pokémon faints, gameplay resumes.

### Implemented Mechanics

The battle system includes several core Pokémon mechanics:

- Pokémon leveling
- Individual Values (IVs)
- Effort Values (EVs)
- Natures

Evolution is **not currently implemented**.

---

## Pokémon Data Integration

Pokémon data is retrieved dynamically using **PokeAPI**.

https://pokeapi.co/

This allows the game to support:

- All existing Pokémon
- Newly released Pokémon automatically
- Consistent stat and species data

Pokémon information such as species data, base stats, and identifiers are fetched from the API and used to generate battle-ready Pokémon objects within the game.

---

## Data Persistence Layer

Game data persistence is handled through an abstraction layer that supports two storage implementations:

- **MongoDB database storage**
- **Local JSON file storage**

The storage method is selected automatically based on application configuration.

### MongoDB Mode

If a `MongoConnectionString` is provided in configuration, the application connects to a MongoDB cluster and stores persistent data there.

This includes:

- Player Pokémon collection
- Player teams
- Game progression data

### Local JSON Mode

If no connection string is configured, the application falls back to local file storage.

Data is stored inside the project directory:


/save_data


These JSON files persist:

- Captured Pokémon
- Team configuration
- Player progress

This allows the game to run without requiring any external services.

---

## Storage Abstraction

The persistence system uses a storage abstraction that separates game logic from the storage implementation.

Benefits include:

- Switching between MongoDB and JSON storage without modifying game logic
- Easier future expansion (for example, cloud storage or other database systems)
- Simplified testing and debugging

The game logic interacts only with the abstraction layer, while the concrete storage implementations handle serialization and database operations.

---

## System Design Goals

The architecture was designed with several goals in mind:

- **Modularity** – gameplay, storage, and API integration are separate systems
- **Extensibility** – additional Pokémon mechanics can be added without major rewrites
- **Data portability** – save data works both locally and in a database
- **Future-proofing** – PokeAPI integration allows new Pokémon to work automatically

---

# Key Engineering Challenges

This project required solving several design challenges in order to extend a simple Minesweeper clone into a more complex system involving persistent game data, external APIs, and RPG-style battle mechanics.

---

## Integrating Pokémon Battles into a Minesweeper Game

One of the core challenges was integrating a **battle system** into the traditional Minesweeper gameplay loop without disrupting the pacing of the game.

Instead of ending the game immediately when encountering a Pokémon, the system introduces a battle phase:

1. Revealing a tile containing a Pokémon triggers a battle.
2. The player's assembled team fights the wild Pokémon.
3. Gameplay resumes if the wild Pokémon is defeated.
4. The game only ends if the entire team faints.

This approach preserves the core Minesweeper experience while adding strategic depth through team composition and Pokémon stats.

---

## Supporting All Pokémon Dynamically

Rather than hardcoding Pokémon data, the project integrates with **PokeAPI** to retrieve species information.

Advantages of this approach include:

- Automatic support for **all existing Pokémon**
- Compatibility with **future Pokémon generations**
- Reduced maintenance requirements for the project

The game dynamically generates Pokémon instances using API-provided species data combined with internally generated battle attributes such as IVs, EVs, and natures.

---

## Implementing a Simplified Pokémon Stat System

The battle system incorporates several mechanics inspired by the official Pokémon games while remaining lightweight enough for a small project.

Implemented mechanics include:

- Pokémon **leveling**
- **Individual Values (IVs)**
- **Effort Values (EVs)**
- **Natures**

These mechanics provide meaningful stat variation between Pokémon while avoiding the complexity of a full Pokémon battle simulator.

---

## Dual Persistence System

Another design challenge was implementing **flexible data persistence**.

The solution was to support two storage methods:

- **MongoDB database storage**
- **Local JSON file storage**

This allows the game to run in multiple environments:

| Environment | Storage Method |
|-------------|---------------|
| Local play | JSON files |
| Cloud / multi-device setups | MongoDB |

The persistence layer is abstracted so that gameplay systems do not depend on a specific storage implementation.

---

## Designing for Extensibility

The project structure was designed to make future improvements easier to implement.

Examples of potential extensions include:

- Pokémon evolution
- Additional battle mechanics
- Expanded board generation
- Additional storage backends

By separating gameplay logic, storage systems, and external integrations, the project can evolve without requiring major architectural changes.

---

# Data Persistence

Game data can be stored in **two different ways**.

The application automatically chooses the method depending on whether a MongoDB connection string is configured.

## Option 1 — MongoDB (Recommended)

If a MongoDB connection string is provided, the game will store all persistent data in a **MongoDB cluster**.

### Setup

Add a connection string using **User Secrets** or **appsettings**.

Example:

```json
{
  "DefaultConnectionString": "mongodb+srv://username:password@cluster.mongodb.net"
}
```
Or with User Secrets:
```bash
dotnet user-secrets set "DefaultConnectionString" "your_connection_string"
```
Once present, the game will automatically connect to MongoDB and store:

- Pokémon collection
- Player team
- Save data

## Option 2 — Local JSON Files (Default)

If no MongoDB connection string is provided, the game will instead store data locally.

Save files will be created in:

/save_data

These JSON files store:

- Captured Pokémon
- Team configuration
- Player progress

This option requires no configuration and works automatically.

# Gameplay Loop

1. Select a difficulty from the main menu
2. Reveal tiles on the board
3. Encounter and battle Pokémon
4. Clear the board
5. Choose 2–4 Pokémon to keep (Depends on Difficulty)
6. Manage your team in the Pokémon Box
7. Start another run

# Building the Project
## Requirements
- .NET 9 SDK
- Internet connection (for PokeAPI)

## Build
```bash
git clone https://github.com/asanders005/pokemon-minesweeper
cd pokemon-minesweeper
dotnet build
```
## Run
```bash
dotnet run
```
# Credits

Original project:

switch87 — Pokemon Sweeper
https://github.com/switch87/pokemon-sweeper

# Future Improvements

Possible future features:
- Pokémon evolution
- Expanded battle mechanics
- Additional board types
- UI improvements
- Expanded save features

# License

This project inherits licensing from the original repository unless otherwise specified.

See the original repository for license information.
