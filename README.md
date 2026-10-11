# Dziennik Plecakowy 🎒🏔️

[🇵🇱 Wersja polska](#wersja-polska) | [🇬🇧 English version](#english-version)

---

<a name="wersja-polska"></a>
## 🇵🇱 O projekcie

**Dziennik Plecakowy** to aplikacja mobilna stworzona z myślą o miłośnikach górskich wędrówek i aktywnego spędzania czasu na szlaku. Projekt składa się z aplikacji mobilnej typu cross-platform (**Client**) oraz skalowalnego backendu API (**Server**) komunikującego się z bazą danych MongoDB.

Głównym celem systemu jest rejestrowanie tras, zbieranie statystyk wędrówki oraz zapewnienie niezawodności w trudnym terenie górskim dzięki wbudowanemu buforowaniu danych offline.

### 🚀 Kluczowe Funkcjonalności

- **Autoryzacja i Profil Użytkownika:**
  - Rejestracja i logowanie (JWT, bezpieczne haszowanie haseł).
  - Zarządzanie danymi profilu i statystykami wędrówek.
- **Śledzenie Trasy (Live Tracking):**
  - Rejestracja pozycji GPS dostosowana do tempa marszu górskiego (distance filter optymalizujący zużycie baterii).
  - Obliczanie kluczowych parametrów: czas trwania, pokonany dystans, suma podejść/przewyższenie (elevation gain).
  - Zliczanie kroków przy użyciu wbudowanych sensorów urządzenia.
- **Niezawodność w Terenie (Offline Buffering):**
  - Lokalne buforowanie punktów trasy przy braku łączności komórkowej.
  - Automatyczna synchronizacja i backup danych w API po odzyskaniu połączenia z siecią.
- **Dziennik Podróży:**
  - Przeglądanie historii wypraw wraz z podsumowaniem i zapisanym śladem GPS.

---

### 🛠️ Architektura i Technologie

#### Backend (`DziennikPlecakowy.API`)
Zrealizowany zgodnie z zasadami **Clean Architecture**:
- **Framework:** .NET (C#) / ASP.NET Core Web API
- **Baza Danych:** MongoDB (oficjalny sterownik `MongoDB.Driver`)
- **Struktura Rozwiązania:**
  - `DziennikPlecakowy.API` – kontrolery HTTP (`AuthController`, `TripController`, `UserController`), routing, konfiguracja middleware.
  - `DziennikPlecakowy.DTO` – obiekty transferu danych (`TripAddRequest`, `UserAuthRequest`, `UserRegisterRequest` i inne).
  - `DziennikPlecakowy.Models` – domena i encje (`Trip`, `GeoPoint`, `User`, `UserStat`).
  - `DziennikPlecakowy.Interfaces` – kontrakty serwisów (`IAuthService`, `ICypherService`, `IHashService`, `ITripService`, `IUserService`).
  - `DziennikPlecakowy.Services` – logika biznesowa i przetwarzanie tras (`AuthService`, `CypherService`, `HashService`, `TripService`, `UserService`).
  - `DziennikPlecakowy.Infrastructure` – implementacja dostępu do bazy (`MongoDbContext`).

#### Aplikacja Mobilna (`DziennikPlecakowy`)
- **Framework:** .NET MAUI (C#)
- **Wzorzec Projektowy:** MVVM
- **Sensory:** Geolocation API, Pedometer/Sensors

---

### 📁 Struktura Projektu

```text
DziennikPlecakowy/
├── DziennikPlecakowy/                # Aplikacja mobilna (.NET MAUI)
├── DziennikPlecakowy.API/            # Warstwa wejściowa API (Controllers, Program.cs)
├── DziennikPlecakowy.DTO/            # Klasy transferowe (DTO)
├── DziennikPlecakowy.Infrastructure/ # Kontekst MongoDB (MongoDbContext.cs)
├── DziennikPlecakowy.Interfaces/     # Interfejsy serwisów
├── DziennikPlecakowy.Models/         # Encje domenowe (GeoPoint, Trip, User, UserStat)
├── DziennikPlecakowy.Services/       # Serwisy aplikacyjne i logika biznesowa
└── DziennikPlecakowy.sln             # Główne rozwiązanie Visual Studio
```

---

### ⚙️ Uruchomienie Projektu

#### 1. Wymagania
- .NET 8 SDK (lub nowszy)
- Visual Studio (z obsługą *ASP.NET and web development* oraz *.NET Multi-platform App UI development*)
- Baza danych MongoDB (lokalnie lub MongoDB Atlas)

#### 2. Konfiguracja i Start API
1. Sklonuj repozytorium:
   ```bash
   git clone [https://github.com/kacperoskyyy/DziennikPlecakowy.git](https://github.com/kacperoskyyy/DziennikPlecakowy.git)
   cd DziennikPlecakowy
   ```
2. Skonfiguruj parametry bazy w `DziennikPlecakowy.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "MongoDb": "mongodb://localhost:27017"
     },
     "DatabaseName": "DziennikPlecakowyDb"
   }
   ```
3. Uruchom API:
   ```bash
   dotnet run --project DziennikPlecakowy.API
   ```

#### 3. Uruchomienie Aplikacji Mobilnej
1. Otwórz `DziennikPlecakowy.sln` w Visual Studio.
2. Ustaw `DziennikPlecakowy` jako projekt startowy.
3. Wskaż docelowy adres API w konfiguracji aplikacji mobilnej (`http://10.0.2.2:<port>` w przypadku emulatora Androida).
4. Wybierz emulator / urządzenie fizyczne i uruchom projekt (`F5`).

---

### 👤 Autor
- **Kacper** ([@kacperoskyyy](https://github.com/kacperoskyyy))

### 📄 Licencja
Projekt udostępniony na licencji [MIT](LICENSE).

---
---

<a name="english-version"></a>
## 🇬🇧 English Version

**Dziennik Plecakowy** (*Backpack Journal*) is a mobile application built for mountain hiking enthusiasts and outdoor activity tracking. The system consists of a cross-platform mobile client and a scalable ASP.NET Core backend backed by MongoDB.

Its primary goal is to log hiking routes, track key endurance metrics, and ensure rock-solid data persistence even when hiking in remote mountain areas with poor or no cellular reception.

### 🚀 Key Features

- **Authentication & User Profiles:**
  - Secure registration and login flows (JWT, password hashing).
  - Profile data management and comprehensive activity logs.
- **Live Route Tracking:**
  - GPS route logging tuned for mountain walking pace (distance filtering to preserve battery life).
  - Real-time hike statistics: duration, distance traveled, and elevation gain.
  - Step counter driven by native device motion sensors.
- **Offline-First Mountain Resilience:**
  - Local route buffering when cellular connectivity drops.
  - Automatic synchronization and backup to the cloud once network connectivity is restored.
- **Hike Journal:**
  - History view enabling users to inspect past trips and GPS paths.

---

### 🛠️ Architecture & Tech Stack

#### Backend (`DziennikPlecakowy.API`)
Structured around **Clean Architecture** patterns:
- **Framework:** .NET (C#) / ASP.NET Core Web API
- **Database:** MongoDB (official `MongoDB.Driver`)
- **Solution Layers:**
  - `DziennikPlecakowy.API` – HTTP controllers (`AuthController`, `TripController`, `UserController`), middleware, application bootstrapping.
  - `DziennikPlecakowy.DTO` – Request & response models (`TripAddRequest`, `UserAuthRequest`, `UserRegisterRequest`, etc.).
  - `DziennikPlecakowy.Models` – Core domain entities (`Trip`, `GeoPoint`, `User`, `UserStat`).
  - `DziennikPlecakowy.Interfaces` – Service abstractions (`IAuthService`, `ICypherService`, `IHashService`, `ITripService`, `IUserService`).
  - `DziennikPlecakowy.Services` – Business logic and trip processing (`AuthService`, `CypherService`, `HashService`, `TripService`, `UserService`).
  - `DziennikPlecakowy.Infrastructure` – Database integration (`MongoDbContext`).

#### Mobile Client (`DziennikPlecakowy`)
- **Framework:** .NET MAUI (C#)
- **Pattern:** MVVM (Model-View-ViewModel)
- **Device Sensors:** Geolocation API, Native Pedometer / Step Sensor

---

### ⚙️ Getting Started

#### Prerequisites
- .NET 8 SDK (or newer)
- Visual Studio (with *ASP.NET and web development* and *.NET Multi-platform App UI development* workloads)
- MongoDB instance (local or MongoDB Atlas)

#### Running the Backend
1. Clone the repository:
   ```bash
   git clone [https://github.com/kacperoskyyy/DziennikPlecakowy.git](https://github.com/kacperoskyyy/DziennikPlecakowy.git)
   cd DziennikPlecakowy
   ```
2. Update connection string in `DziennikPlecakowy.API/appsettings.json`.
3. Start the API service:
   ```bash
   dotnet run --project DziennikPlecakowy.API
   ```

#### Running the Mobile Client
1. Open `DziennikPlecakowy.sln` in Visual Studio.
2. Set `DziennikPlecakowy` as the startup project.
3. Configure the backend base URL (use `http://10.0.2.2:<port>` for Android emulators).
4. Select your target device / emulator and run the solution (`F5`).

---

### 👤 Author
- **Kacper** ([@kacperoskyyy](https://github.com/kacperoskyyy))

### 📄 License
This project is licensed under the [MIT](LICENSE) License.
