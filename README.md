# Dealer Tool 🏎️

Dealer Tool est une application console **C#** permettant de gérer un concessionnaire automobile.
Elle permet de gérer clients, véhicules et ventes, avec import CSV et affichage interactif en console grâce à **Spectre.Console**.

---

## Fonctionnalités ✨

* 📥 Import des clients et véhicules depuis des fichiers CSV
* 🧑‍💼 Ajouter de nouveaux clients
* 🚗 Ajouter de nouveaux véhicules
* 📋 Afficher la liste des clients et véhicules
* 💰 Enregistrer une vente et gérer l’historique
* 🔍 Rechercher clients et véhicules par identifiant
* 🎨 Interface console avec couleurs et tableaux via Spectre.Console

---

## Structure du projet 📂

```
ProjetCS/
├─ Data/
│  ├─ CSV/
│  │  ├─ clients.csv
│  │  ├─ voitures.csv
│  │  └─ voitures_old.csv
│  ├─ InterfaceRepository/
│  │  ├─ ICustomerRepository.cs
│  │  └─ IVehicleRepository.cs
│  ├─ CustomerRepository.cs
│  ├─ VehicleRepository.cs
│  ├─ DbConnection.cs
│  ├─ DealerDbContext.cs
│  └─ DateTimeUtils.cs
├─ Migrations/
│  ├─ 20251106175054_InitialCreate.cs
│  └─ DealerDbContextModelSnapshot.cs
├─ Model/
│  ├─ Customer.cs
│  └─ Vehicle.cs
├─ appsettings.json
└─ Program.cs
```

---

## Prérequis 🛠️

* .NET 7.0 SDK ou supérieur
* PostgreSQL
* Visual Studio 2022 ou Visual Studio Code
* Extensions recommandées : C# pour VSCode, Npgsql (PostgreSQL)

---

## Installation 💻

1. **Cloner le projet :**

```bash
git clone https://github.com/ton-utilisateur/dealer-tool.git
cd dealer-tool
```

2. **Configurer la base de données :**

   * Créer une base PostgreSQL.
   * Modifier `appsettings.json` :

  ```json
  "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=DealerDB;Username=postgres;Password=yourpassword"
  }
  ```

3. **Installer les dépendances :**

```bash
dotnet restore
```

4. **Compiler et exécuter :**

```bash
dotnet build
dotnet ef database update
dotnet run
```

---

## Utilisation 📝

1. Lancer l’application
2. Sélectionner une option dans le menu avec les flèches
3. Suivre les instructions pour ajouter clients, véhicules ou enregistrer des ventes
4. Les informations sont affichées dans des tableaux colorés pour plus de lisibilité

---

## Exemple de données CSV injectables📄

**Clients (`clients.csv`)**

```
Firstname%Lastname%Birthdate%Phone%Email
John%Doe%15/03/1985%0612345678%john.doe@email.com
```

**Véhicules (`voitures.csv`)**

```
Brand/Model/Year/Price/Color/Sold/PurchaseDate/CustomerEmail
Toyota/Corolla/2020/20000,00/Blanc/true/2023-06-01/john.doe@email.com
```

---

## Dépendances principales 📦

* Microsoft.EntityFrameworkCore
* Npgsql.EntityFrameworkCore.PostgreSQL
* Microsoft.Extensions.Hosting
* Spectre.Console
