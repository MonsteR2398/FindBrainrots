# Project Overview
- Game Title: FindBrainrots
- High-Level Concept: Exploration/Collection game where players find and collect "Brainrots" (various objects with different rarities).
- Players: Single player.
- Render Pipeline: URP.
- Input System: New Input System.

# Game Mechanics
## Core Gameplay Loop
1. Explore the world to find Brainrot objects.
2. Brainrots have different rarities, distinguished by their textures.
3. Picking up a Brainrot adds it to the player's collection and triggers a UI animation.
4. Collected Brainrots disappear from the map in subsequent sessions (synced with collection).

## Controls and Input Methods
- Character movement and interaction (ModularPickup system).

# UI
- BrainrotUI: Shows a popup/animation when a new Brainrot is found (Item Name, Icon, Rarity).
- Collection: A menu to view all found Brainrots.

# Key Asset & Context
- `CollectibleItemSO`: Data asset for a specific Brainrot item (ID, Name, Icon, Rarity, Model, Texture).
- `BrainrotMapInstance`: Script on the world object that handles visuals (swapping textures) and collection logic.
- `BrainrotDistributor`: Editor tool to assign specific items to map placeholders based on rarity.

# Implementation Steps
1. **Modify `CollectibleItemSO`**:
   - Add a `Texture2D overrideTexture` field to store the specific texture for a rarity variant.
   - Files: `Assets/Treasures/ModularCollection/Scripts/Data/CollectibleItemSO.cs`.

2. **Update `BrainrotMapInstance`**:
   - Update `UpdateVisuals()` to apply `overrideTexture` to the instantiated model's material.
   - Files: `Assets/_Project/Script/Pickups/BrainrotMapInstance.cs`.

3. **Rebuild Database Script**:
   - Create a new generation script that scans both FBX models and their corresponding texture folders.
   - For each texture in a folder (e.g., Apple Pepple), create a *separate* `CollectibleItemSO` representing that specific variant/rarity.
   - Map texture names (Blood, Gold, etc.) to specific `RaritySettingsSO` assets.
   - Files: New temporary editor script or update `BrainrotDistributor.cs`.

4. **Update `BrainrotDistributor`**:
   - Ensure it respects the new SO structure and triggers visual updates correctly.
   - Files: `Assets/_Project/Script/Editor/BrainrotDistributor.cs`.

# Verification & Testing
1. Run the database rebuild script. Verify that multiple SOs are created for "Apple Pepple" (one for each texture/rarity).
2. Use the Distributor tool to populate the scene.
3. Verify in the Editor that Brainrots on the map have the correct textures applied based on their assigned rarity.
4. Play the game, collect an item, and verify it appears in the Collection and is hidden on reload.
