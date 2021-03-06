﻿using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MonsterTrainModdingAPI.Managers;
using MonsterTrainModdingAPI.Interfaces;
using System.Linq;

namespace MonsterTrainModdingAPI.Patches
{
    /// <summary>
    /// Provides managers with the references they need to function.
    /// </summary>
    [HarmonyPatch(typeof(SaveManager), "Initialize")]
    class SaveManagerInitializationPatch
    {
        public static SaveManager SaveManager;
        static void Postfix(SaveManager __instance)
        {
            SaveManagerInitializationPatch.SaveManager = __instance;

            CustomCardManager.SaveManager = __instance;
            CustomCharacterManager.SaveManager = __instance;
            CustomCollectableRelicManager.SaveManager = __instance;
            CustomClassManager.SaveManager = __instance;
            CustomMapNodePoolManager.SaveManager = __instance;
            var chara = __instance.GetAllGameData().GetAllCharacterData()[0];
            CustomCharacterManager.TemplateCharacter = chara;
            CustomAssetManager.InitializeAssetConstructors();
        }
    }

    /// <summary>
    /// At this point, the API is fully set up.
    /// Initialize all API mods by calling their methods.
    /// </summary>
    [HarmonyPatch(typeof(AssetLoadingManager), "Start")]
    class AssetLoadingManagerInitializationPatch
    {
        static void Postfix()
        {
            List<IInitializable> initializables =
                PluginManager.Plugins.Values.ToList()
                    .Where((plugin) => (plugin is IInitializable))
                    .Select((plugin) => (plugin as IInitializable))
                    .ToList();
            initializables.ForEach((initializable) => initializable.Initialize());
        }
    }
}
