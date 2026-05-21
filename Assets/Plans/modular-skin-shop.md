# Project Overview
- **Game Title:** Modular Skin Shop System
- **High-Level Concept:** A decoupled, plug-and-play skin shop for Unity projects using 3D prefabs as skins.
- **Players:** Single-player (can be extended).
- **Inspiration:** Standard mobile/indie game skin shops.
- **Render Pipeline:** Any (Modular).
- **Input System:** Any (Modular).

# Game Mechanics
## Core Gameplay Loop
1. Player opens the shop UI.
2. Player scrolls through available skins (provided by `SkinLibrarySO`).
3. If a skin is locked:
    - Check if player has enough currency via `IShopEconomy`.
    - If yes, spend currency and unlock skin via `IShopPersistence`.
4. If a skin is unlocked:
    - Select the skin.
    - Shop triggers `OnSkinSelected` event.
    - External project scripts (e.g., `PlayerCharacter`) react to the event and swap the 3D model.

## Controls and Input Methods
- Mouse/Touch: UI interaction (Scroll, Click).

# UI
- **Shop Window:** A Canvas with a ScrollView containing a Grid Layout Group.
- **Skin Item UI:**
    - Icon image.
    - Name text.
    - Price text (or "Owned" / "Selected").
    - Button to buy/select.
- **Balance Display:** Simple text showing current currency.

# Key Assets & Context
## Data Structures
- `SkinSO`: `string ID`, `string DisplayName`, `GameObject Prefab`, `Sprite Icon`, `int Price`.
- `SkinLibrarySO`: `List<SkinSO> Skins`.

## Interfaces (The Bridge)
- `IShopEconomy`: `int GetBalance()`, `bool CanAfford(int amount)`, `void Spend(int amount)`.
- `IShopPersistence`: `bool IsUnlocked(string id)`, `void Unlock(string id)`, `string GetActiveId()`, `void SetActive(string id)`.

## Logic
- `SkinShopManager`: Core logic, manages events (`OnSkinSelected`, `OnSkinPurchased`).
- `SkinShopUI`: Populates UI items and handles user interaction.
- `SkinShopItemUI`: Individual button logic.

# Implementation Steps
1. **Core Data & Interfaces:**
    - Create `SkinSO` and `SkinLibrarySO` scripts.
    - Create `IShopEconomy` and `IShopPersistence` interface definitions.
2. **Logic Layer:**
    - Implement `SkinShopManager` which holds the library and uses interfaces.
    - Add events: `OnSkinSelected(SkinSO)`, `OnSkinPurchased(SkinSO)`.
3. **UI Layer:**
    - Create `SkinShopItemUI` script to update button state (Buy vs Select).
    - Create `SkinShopUI` script to spawn items from the library.
4. **Prefabs & Assets:**
    - Create a basic "SkinItem" prefab (Image, Text, Button).
    - Create a "ShopCanvas" prefab.
5. **Bridge & Integration (Example):**
    - Create a sample `PlayerSkinApplyer` that listens to `OnSkinSelected` and swaps a prefab on a target Transform.
    - Create a sample `MockShopBridge` that implements interfaces using `PlayerPrefs` and a hardcoded balance.

# Verification & Testing
- **Unit Test:** Create a mock bridge, verify that clicking "Buy" reduces balance and unlocks the skin.
- **Visual Test:** Verify that the UI correctly updates labels when switching between "Buy" and "Select".
- **Integration Test:** Ensure that selecting a skin triggers the model swap in a sample scene.