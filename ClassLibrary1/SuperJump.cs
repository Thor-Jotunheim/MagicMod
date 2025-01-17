using BepInEx;
using HarmonyLib;
using System;
using System.Diagnostics;
using UnityEngine;

namespace SuperJump
{
    [BepInPlugin("com.SuperJump", "SuperJump", "0.0.1")]
    [BepInProcess("valheim.exe")]
    public class SuperJump : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.SuperJump");

        void Awake()
        {
            harmony.PatchAll();
        }


        [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
        class Jump_Patch
        {
            static void Prefix(ref float ___m_jumpForce)
            {
                UnityEngine.Debug.Log($"Jump force: {___m_jumpForce}");
                ___m_jumpForce = 15; // default 10 I think, bolto had 8
                UnityEngine.Debug.Log($"Modified jump force: {___m_jumpForce}");

            }
        }


    }
}