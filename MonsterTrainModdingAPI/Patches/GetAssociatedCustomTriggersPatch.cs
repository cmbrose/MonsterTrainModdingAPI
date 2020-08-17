﻿using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MonsterTrainModdingAPI.Managers;

namespace MonsterTrainModdingAPI.Patches
{
    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(CardTriggerTypeMethods), "GetAssociatedCardTrigger")]
    class GetAssociatedCustomCardTriggersPatch
    {
        static void Postfix(ref CardTriggerType? __result, ref CharacterTriggerData.Trigger charTrigger)
        {
            if (__result == null)
            {
                __result = CustomTriggerManager.GetAssociatedCardTrigger(charTrigger);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(CardTriggerTypeMethods), "GetAssociatedCharacterTrigger")]
    class GetAssociatedCustomCharacterTriggersPatch
    {
        static void Postfix(ref CharacterTriggerData.Trigger? __result, ref CardTriggerType cardTrigger)
        {
            if (__result == null)
            {
                __result = CustomTriggerManager.GetAssociatedCharacterTrigger(cardTrigger);
            }
        }
    }
}
