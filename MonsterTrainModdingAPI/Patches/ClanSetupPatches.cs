using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MonsterTrainModdingAPI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterTrainModdingAPI.Patches
{
    class ClanSetupPatches
    {
        /// <summary>
        /// This patch reimplements the functionality of SaveManager.LoadClassAndSubclass, which is called and then retconned away
        /// It also extends that functionality for covenants.
        /// It only applies to custom classes.
        /// </summary>
        [HarmonyPatch(typeof(GameStateManager), "StartRun")]
        public class ClanCardSetupPatch
        {
            static void Postfix(ref GameStateManager __instance, RunType runType,
                                string sharecode,
                                ClassData mainClass,
                                ClassData subClass,
                                int ascensionLevel)
            {
                if (ascensionLevel == 0) { return; }
                if (CustomClassManager.CustomClassData.ContainsKey(mainClass.GetID()))
                {
                    foreach (CardData cardData in mainClass.CreateMainClassStartingDeck())
                        SaveManagerInitializationPatch.SaveManager.AddCardToDeck(cardData);
                    if (ascensionLevel >= 6) { SaveManagerInitializationPatch.SaveManager.AddCardToDeck(mainClass.CreateMainClassStartingDeck()[0]); }
                    if (ascensionLevel >= 13) { SaveManagerInitializationPatch.SaveManager.AddCardToDeck(mainClass.CreateMainClassStartingDeck()[0]); }
                    //SaveManagerInitializationPatch.SaveManager.AddCardToDeck(mainClass.GetStartingChampionCard());
                }

                if (CustomClassManager.CustomClassData.ContainsKey(subClass.GetID()))
                {
                    foreach (CardData cardData in subClass.CreateSubClassStartingDeck())
                        SaveManagerInitializationPatch.SaveManager.AddCardToDeck(cardData);
                    if (ascensionLevel >= 8) { SaveManagerInitializationPatch.SaveManager.AddCardToDeck(mainClass.CreateMainClassStartingDeck()[0]); }
                    if (ascensionLevel >= 15) { SaveManagerInitializationPatch.SaveManager.AddCardToDeck(mainClass.CreateMainClassStartingDeck()[0]); }
                }
            }
        }

        /// <summary>
        /// Identifies the necessary card frame for the class and installs it through ridiculous means.
        /// </summary>
        [HarmonyPatch(typeof(CardFrameUI), "SetUpFrame")]
        public class ClanCardFramePatch
        {
            static void Postfix(ref CardFrameUI __instance, CardState cardState, List<AbstractSpriteSelector> ___spriteSelectors)
            {
                try
                {
                    if (cardState.GetLinkedClassID() == null) { return; }
                    List<Sprite> cardFrame;
                    if (CustomClassManager.CustomClassFrame.TryGetValue(cardState.GetLinkedClassID(), out cardFrame))
                    {
                        foreach (AbstractSpriteSelector spriteSelector in ___spriteSelectors)
                        {
                            switch (spriteSelector)
                            {
                                case ClassSpriteSelector classSpriteSelector:

                                    foreach (var image in classSpriteSelector.gameObject.GetComponents<Image>())
                                    {
                                        image.sprite = cardState.GetCardType() == CardType.Monster ? cardFrame[0] : cardFrame[1];
                                    }
                                    continue;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    API.Log(BepInEx.Logging.LogLevel.Error, "TryGetValue is a dumb function.");
                }
            }
        }

        // This patch fixes display on card upgrade trees
        [HarmonyPatch(typeof(ChampionUpgradeRewardData), "GetUpgradeTree")]
        public class CUSCanInit
        {
            static CardUpgradeTreeData Postfix(CardUpgradeTreeData ret, ref ChampionUpgradeRewardData __instance, SaveManager saveManager)
            {
                if (CustomClassManager.CustomClassData.ContainsKey(saveManager.GetMainClass().GetID()))
                {
                    return saveManager.GetMainClass().GetUpgradeTree();
                }

                return ret;
            }
        }

        // This patch adds in the cusom icon for a clan. We could theoretically add these to VictoryUI's ClassIconMapping list, which seems better in theory. 
        // In practice, we have no way to guarantee the existence of VictoryUI in the scene at the time of Class instantiation, and no way to serialize the class mapping in advance of 
        [HarmonyPatch(typeof(RewardItemUI), "TryOverrideDraftIcon")]
        public class CustomClanRewardDraftIcon
        {
            static void Prefix(RewardItemUI __instance, ref Sprite mainClassIcon, ref Sprite subClassIcon)
            {
                DraftRewardData draftRewardData;
                if ((object)(draftRewardData = (__instance.rewardState?.RewardData as DraftRewardData)) != null)
                {
                    if (draftRewardData.ClassType == RunState.ClassType.MainClass)
                    {
                        string mainClass = CustomClassManager.SaveManager.GetMainClass().GetID();
                        if (CustomClassManager.CustomClassDraftIcons.ContainsKey(mainClass))
                            CustomClassManager.CustomClassDraftIcons.TryGetValue(mainClass, out mainClassIcon);
                    }
                    else if (draftRewardData.ClassType == RunState.ClassType.SubClass)
                    {
                        string subClass = CustomClassManager.SaveManager.GetSubClass().GetID();
                        if (CustomClassManager.CustomClassDraftIcons.ContainsKey(subClass))
                            CustomClassManager.CustomClassDraftIcons.TryGetValue(subClass, out subClassIcon);
                    }
                }
            }
        }

        // This patch displays custom characters on the clan select screen
        [HarmonyPatch(typeof(ClassSelectionScreen), "RefreshCharacters")]
        public class ClassSelectionScreenCustomCharacters
        {
            static void Prefix(ref bool __state, ref ClassSelectCharacterDisplay[] ___characterDisplays)
            {
                __state = false;
                if (___characterDisplays == null)
                {
                    __state = true;
                }
            }

            static void Postfix(ref bool __state, ref ClassSelectCharacterDisplay[] ___characterDisplays, ref Transform ___charactersRoot)
            {
                if (__state)
                {
                    int customClassCount = CustomClassManager.CustomClassData.Values.Count;
                    int totalClassCount = CustomClassManager.SaveManager.GetAllGameData().GetAllClassDatas().Count;
                    int vanillaClassCount = totalClassCount - customClassCount;

                    // "totalClassCount + 1" to account for the random slot
                    var characterDisplaysNew = new ClassSelectCharacterDisplay[(totalClassCount + 1) * 2];

                    var characterDisplay = ___characterDisplays[0];

                    Debug.Log(___characterDisplays.Length);
                    int i;
                    for (i = 0; i < ___characterDisplays.Length; i++)
                    {
                        int clanIndex = (int)AccessTools.Field(typeof(ClassSelectCharacterDisplay), "clanIndex").GetValue(___characterDisplays[i]);
                        if (clanIndex == vanillaClassCount + 1)
                        { // Change index of random clan select display to account for custom classes
                            AccessTools.Field(typeof(ClassSelectCharacterDisplay), "clanIndex").SetValue(___characterDisplays[i], totalClassCount + 1);
                        }
                        characterDisplaysNew[i] = ___characterDisplays[i];
                    }

                    var customClasses = CustomClassManager.CustomClassData.Values;

                    int j = 0;
                    foreach (ClassData customClassData in customClasses)
                    {
                        Debug.Log(j + "  " + i + "  " + customClassCount + "  " + characterDisplaysNew.Length);
                        
                        // After vanilla clans, but before random slot
                        // Note that each clan has two entries in __state, hence "(j / 2)"
                        int clanIndex = vanillaClassCount + j + 1;

                        var customMainCharacterDisplay = GameObject.Instantiate(characterDisplay);

                        var oldCharacterState = customMainCharacterDisplay.GetComponentInChildren<CharacterState>();
                        var characterStateObject = oldCharacterState.gameObject;
                        var customStateObject = CustomCharacterManager.CreateCharacterGameObject("TestMod_Character_BlueEyes");
                        characterStateObject.transform.parent = characterStateObject.transform.parent;
                        GameObject.Destroy(characterStateObject);

                        customMainCharacterDisplay.gameObject.SetActive(false);
                        characterDisplaysNew[i] = customMainCharacterDisplay;
                        AccessTools.Field(typeof(ClassSelectCharacterDisplay), "clanIndex").SetValue(customMainCharacterDisplay, clanIndex);
                        customMainCharacterDisplay.name = customClassData.GetID() + "_Main";

                        i++;

                        var customSubCharacterDisplay = GameObject.Instantiate(characterDisplay);
                        customSubCharacterDisplay.transform.parent = ___charactersRoot;
                        customSubCharacterDisplay.gameObject.SetActive(false);
                        characterDisplaysNew[i] = customSubCharacterDisplay;
                        AccessTools.Field(typeof(ClassSelectCharacterDisplay), "clanIndex").SetValue(customSubCharacterDisplay, clanIndex);
                        customSubCharacterDisplay.name = customClassData.GetID() + "_Sub";

                        j++;
                    }

                    ___characterDisplays = characterDisplaysNew;
                }
            }
        }
    }
}
