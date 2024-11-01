using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MyTrueGear;
using System.Numerics;
using System;
using UnityEngine;
using SLZ.Marrow;
using Il2CppSystem.Collections.Generic;
using SLZ.Marrow.Interaction;
using SLZ.Marrow.Combat;
using SLZ.Marrow.Data;
using SLZ.Bonelab;


namespace BoneLab_TrueGear
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;

        private static TrueGearMod _TrueGear = null;

        private static bool isHeartBeat = false;

        private static string leftItem = null;
        private static string rightItem = null;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;

            HarmonyLib.Harmony.CreateAndPatchAll(typeof(Plugin));
            _TrueGear = new TrueGearMod();
            _TrueGear.Play("HeartBeat");

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");            
        }

        private static void CheckHeartBeat(float currHealth,float maxHealth)
        {
            if (currHealth <= maxHealth * 0.33f)
            {
                _TrueGear.StartHeartBeat();
            }
            else
            {
                _TrueGear.StopHeartBeat();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Gun), "OnFire")]
        private static void Gun_OnFire_Postfix(Gun __instance)
        {
            if (__instance == null)
            {
                return;
            }
            if (__instance.triggerGrip == null)
            {
                return;
            }
            if (__instance.AmmoCount() == 0)
            {
                return;
            }
            bool isTwoHand = (__instance.triggerGrip.attachedHands.Count > 1f);
            Log.LogInfo(__instance.triggerGrip.attachedHands.Count);
            List<Hand>.Enumerator enumerator = __instance.triggerGrip.attachedHands.GetEnumerator();
            bool isLeftHand = true;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.handedness == Handedness.RIGHT)
                {
                    isLeftHand = false;
                }
            }
            if (isTwoHand)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftHandPistolShoot");
                Log.LogInfo("RightHandPistolShoot");
                _TrueGear.Play("LeftHandPistolShoot");
                _TrueGear.Play("RightHandPistolShoot");
                return;
            }
            if (isLeftHand)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftHandPistolShoot");
                _TrueGear.Play("LeftHandPistolShoot");
            }
            else
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightHandPistolShoot");
                _TrueGear.Play("RightHandPistolShoot");
            }
            Log.LogInfo(__instance.gameObject.name);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Gun), "OnMagazineInserted")]
        private static void Gun_OnMagazineInserted_Postfix(Gun __instance)
        {
            if (__instance.gameObject.name.Contains(leftItem))
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftReloadAmmo");
                _TrueGear.Play("LeftReloadAmmo");
            }
            else
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightReloadAmmo");
                _TrueGear.Play("RightReloadAmmo");
            }

        }

        [HarmonyPostfix, HarmonyPatch(typeof(Gun), "OnMagazineRemoved")]
        private static void Gun_OnMagazineRemoved_Postfix(Gun __instance)
        {
            if (__instance.gameObject.name.Contains(leftItem))
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftMagazineEjected");
                _TrueGear.Play("LeftMagazineEjected");
            }
            else
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightMagazineEjected");
                _TrueGear.Play("RightMagazineEjected");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Gun), "SlideRelease")]
        private static void Gun_SlideRelease_Postfix(Gun __instance)
        {
            if (__instance.gameObject.name.Contains(leftItem))
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftDownReload");
                _TrueGear.Play("LeftDownReload");
            }
            else
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightDownReload");
                _TrueGear.Play("RightDownReload");
            }
        }



        [HarmonyPostfix, HarmonyPatch(typeof(Player_Health), "Death")]
        private static void Player_Health_Death_Postfix(Player_Health __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("PlayerDeath2");
            Log.LogInfo(__instance.curr_Health);
            _TrueGear.Play("PlayerDeath");
            _TrueGear.StopHeartBeat();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Player_Health), "UpdateHealth")]
        private static void Player_Health_Update_Postfix(Player_Health __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("UpdateHealth");
            Log.LogInfo(__instance.curr_Health);
            Log.LogInfo(__instance.max_Health);
            if (__instance.curr_Health <= __instance.max_Health * 0.33f)
            {
                _TrueGear.StartHeartBeat();
            }
            else if (__instance.curr_Health > __instance.max_Health * 0.33f)
            {
                _TrueGear.StopHeartBeat();
            }
            if (__instance.curr_Health <= 0)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("PlayerDeath1");
                _TrueGear.Play("PlayerDeath");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerDamageReceiver), "ReceiveAttack")]
        private static void PlayerDamageReceiver_ReceiveAttack_Postfix(PlayerDamageReceiver __instance, Attack attack)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo(__instance.bodyPart);
            Log.LogInfo(attack.attackType);
            switch (attack.attackType)
            {
                case AttackType.Fire:
                    Log.LogInfo("FireDamage");
                    _TrueGear.Play("FireDamage");
                    return;
            }
            Log.LogInfo("PoisonDamage");
            _TrueGear.Play("PoisonDamage");
            Log.LogInfo(__instance.health.curr_Health);
            Log.LogInfo(__instance.health.max_Health);
            CheckHeartBeat(__instance.health.curr_Health, __instance.health.max_Health);

        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventorySlotReceiver), "OnHandGrab")]
        private static void InventorySlotReceiver_OnHandGrab_Postfix(InventorySlotReceiver __instance, Hand hand)
        {
            if (hand == null)
            {
                return;
            }
            Log.LogInfo("----------------------------------");
            if (__instance.isInUIMode)
            {
                Log.LogInfo("ChestSlotOutputItem");
                _TrueGear.Play("ChestSlotOutputItem");
                return;
            }
            if (__instance.slotType == SlotType.SIDEARM)
            {
                Log.LogInfo("LeftChestSlotOutputItem");
                Log.LogInfo("RightChestSlotOutputItem");
                _TrueGear.Play("LeftChestSlotOutputItem");
                _TrueGear.Play("RightChestSlotOutputItem");
                return;
            }
            Log.LogInfo("BackSlotOutputItem");
            _TrueGear.Play("BackSlotOutputItem");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventorySlotReceiver), "OnHandDrop")]
        private static void InventorySlotReceiver_OnHandDrop_Postfix(InventorySlotReceiver __instance, IGrippable host)
        {
            if (__instance == null)
            {
                return;
            }
            if (host == null)
            {
                return;
            }
            if (host.GetLastHand() == null)
            {
                return;
            }
            Log.LogInfo("----------------------------------");
            if (__instance.isInUIMode)
            {
                Log.LogInfo("ChestSlotInputItem");
                _TrueGear.Play("ChestSlotInputItem");
                return;
            }
            if (__instance.slotType == SlotType.SIDEARM)
            {
                Log.LogInfo("LeftChestSlotInputItem");
                Log.LogInfo("RightChestSlotInputItem");
                _TrueGear.Play("LeftChestSlotInputItem");
                _TrueGear.Play("RightChestSlotInputItem");
                return;
            }
            Log.LogInfo("BackSlotInputItem");
            _TrueGear.Play("BackSlotInputItem");
        }



        [HarmonyPostfix, HarmonyPatch(typeof(AmmoInventory), "AddCartridge", new Type[] { typeof(AmmoGroup), typeof(int) })]
        private static void AmmoInventory_AddCartridge_Postfix(AmmoInventory __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("LeftHipSlotInputItem");
            _TrueGear.Play("LeftHipSlotInputItem");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryAmmoReceiver), "OnHandGrab")]
        private static void InventoryAmmoReceiver_OnHandGrab_Postfix(InventoryAmmoReceiver __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("LeftHipSlotOutputItem");
            _TrueGear.Play("LeftHipSlotOutputItem");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Haptor), "Haptic_Hit")]
        private static void Haptor_Haptic_Hit_Postfix(Haptor __instance, float amp)
        {
            if (amp < 0.5f)
            {
                return;
            }
            if (__instance.device_Controller.handedness == Handedness.LEFT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftHandMeleeHit");
                _TrueGear.Play("LeftHandMeleeHit");
            }
            else if (__instance.device_Controller.handedness == Handedness.RIGHT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightHandMeleeHit");
                _TrueGear.Play("RightHandMeleeHit");
            }

            Log.LogInfo(amp);
            Log.LogInfo(__instance.device_Controller.handedness);
        }





        [HarmonyPostfix, HarmonyPatch(typeof(Hand), "AttachObject")]
        private static void Hand_AttachObject_Postfix(Hand __instance, GameObject objectToAttach)
        {
            string[] names = objectToAttach.gameObject.name.Split('_');

            if (__instance.handedness == Handedness.LEFT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftHandPickupItem");
                _TrueGear.Play("LeftHandPickupItem");
                leftItem = names[0];
            }
            else if (__instance.handedness == Handedness.RIGHT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightHandPickupItem");
                _TrueGear.Play("RightHandPickupItem");
                rightItem = names[0];
            }

            Log.LogInfo(names[0]);
            Log.LogInfo(__instance.handedness);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Hand), "DetachObject")]
        private static void Hand_DetachObject_Postfix(Hand __instance)
        {
            if (__instance.handedness == Handedness.LEFT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("LeftHandDropItem");
                leftItem = null;
            }
            else if (__instance.handedness == Handedness.RIGHT)
            {
                Log.LogInfo("----------------------------------");
                Log.LogInfo("RightHandDropItem");
                rightItem = null;
            }
        }






        [HarmonyPostfix, HarmonyPatch(typeof(Health), "Respawn")]
        private static void Health_Respawn_Postfix(Health __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("Respawn");
            Log.LogInfo(__instance.curr_Health);
            Log.LogInfo(__instance.max_Health);
            CheckHeartBeat(__instance.curr_Health, __instance.max_Health);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Health), "SetFullHealth")]
        private static void Health_SetFullHealth_Postfix(Health __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("HealthSetFullHealth");
            Log.LogInfo(__instance.curr_Health);
            Log.LogInfo(__instance.max_Health);
            CheckHeartBeat(__instance.curr_Health, __instance.max_Health);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Player_Health), "SetFullHealth")]
        private static void Health_Player_Health_Postfix(Health __instance)
        {
            Log.LogInfo("----------------------------------");
            Log.LogInfo("Player_HealthSetFullHealth");
            Log.LogInfo(__instance.curr_Health);
            Log.LogInfo(__instance.max_Health);
            CheckHeartBeat(__instance.curr_Health, __instance.max_Health);
        }

    }
}
