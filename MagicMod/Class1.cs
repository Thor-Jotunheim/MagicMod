using BepInEx;
using HarmonyLib;
using System;
using System.Diagnostics;
using UnityEngine;

namespace VodosModCustom
{
    [BepInPlugin("com.Vodos.VodosModCustom", "Vodos Mod Custom", "0.0.1")]
    [BepInProcess("valheim.exe")]
    public class VodosModCustom : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.Vodos.VodosModCustom");

        void Awake()
        {
            harmony.PatchAll();
        }


        [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
        class Jump_Patch
        {
            static void Prefix(ref float ___m_jumpForce)
            {
                Debug.Log($"Jump force: {___m_jumpForce}");
                ___m_jumpForce = 15; // default 10 I think, bolto had 8
                Debug.Log($"Modified jump force: {___m_jumpForce}");

            }
        }


    }
}