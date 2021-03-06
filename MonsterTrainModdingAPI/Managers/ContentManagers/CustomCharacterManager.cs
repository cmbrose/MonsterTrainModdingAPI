﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BepInEx.Logging;
using MonsterTrainModdingAPI.Builders;
using HarmonyLib;
using UnityEngine;
using ShinyShoe;
using UnityEngine.AddressableAssets;

namespace MonsterTrainModdingAPI.Managers
{
    /// <summary>
    /// Handles registration and storage of custom character data.
    /// </summary>
    public class CustomCharacterManager
    {
        /// <summary>
        /// Maps custom character IDs to their respective CharacterData.
        /// </summary>
        public static IDictionary<string, CharacterData> CustomCharacterData { get; } = new Dictionary<string, CharacterData>();
        /// <summary>
        /// Maps custom character IDs to their respective Sprite Loading Information.
        /// </summary>
        public static IDictionary<string, CustomAssetManager.AssetBundleLoadingInfo> CharacterSpriteBundleData { get; } = new Dictionary<string, CustomAssetManager.AssetBundleLoadingInfo>();
        /// <summary>
        /// Maps custom character IDs to their respective Skeleton Animation Loading Information
        /// </summary>
        public static IDictionary<string, CustomAssetManager.AssetBundleLoadingInfo> CharacterSkeletonAnimationBundleData { get; } = new Dictionary<string, CustomAssetManager.AssetBundleLoadingInfo>();
        /// <summary>
        /// FallbackData contains a default character prefab which is cloned to create custom characters.
        /// Essential for custom character art. Set during game startup.
        /// </summary>
        public static CharacterData TemplateCharacter { get; set; }
        /// <summary>
        /// Static reference to the game's SaveManager, which is necessary to register new characters.
        /// </summary>
        public static SaveManager SaveManager { get; set; }

        /// <summary>
        /// Register a custom character with the manager, allowing it to show up in game.
        /// </summary>
        /// <param name="data">The custom character data to register</param>
        /// <param name="SpriteInfo">The Information Used to Load an AssetBundle</param>
        /// <param name="SkeletonAnimationInfo"></param>
        public static bool RegisterCustomCharacter(CharacterData data, CustomAssetManager.AssetBundleLoadingInfo SpriteInfo = null, CustomAssetManager.AssetBundleLoadingInfo SkeletonAnimationInfo = null)
        {
            if (SpriteInfo != null)
            {
                CharacterSpriteBundleData.Add(data.GetID(), SpriteInfo);
            }
            if (SkeletonAnimationInfo != null)
            {
                CharacterSkeletonAnimationBundleData.Add(data.GetID(), SkeletonAnimationInfo);
            }
            CustomCharacterData.Add(data.GetID(), data);
            SaveManager.GetAllGameData().GetAllCharacterData().Add(data);
            return true;
        }

        /// <summary>
        /// Get the custom character data corresponding to the given ID
        /// </summary>
        /// <param name="characterID">ID of the custom character to get</param>
        /// <returns>The custom character data for the given ID</returns>
        public static CharacterData GetCharacterDataByID(string characterID)
        {
            if (CustomCharacterData.ContainsKey(characterID))
            {
                return CustomCharacterData[characterID];
            }
            return null;
        }

        /// <summary>
        /// Maps custom subtypes IDs to their respective SubtypeData.
        /// </summary>
        public static IDictionary<string, SubtypeData> CustomSubtypeData { get; } = new Dictionary<string, SubtypeData>();
        /// <summary>
        /// Registers a subtype, making it available for localization
        /// </summary>
        /// <param name="ID">The key used for assigning the subtype, and for its localization</param>
        public static void RegisterSubtype(string ID)
        {
            CustomSubtypeData.Add(ID, new SubtypeDataBuilder { _Subtype = ID }.Build());
        }
    }
}
