// <copyright file="BetterBulldozerPlugin.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

#if BEPINEX

namespace Better_Bulldozer
{
    using BepInEx;
    using Game;
    using Game.Common;
    using HarmonyLib;

    /// <summary>
    /// Mod entry point for BepInEx configuaration.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, "Better Bulldozer", "1.0.0")]
    [HarmonyPatch]
    public class BetterBulldozerPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// A static instance of the IMod for mod entry point.
        /// </summary>
        internal static BetterBulldozerMod _mod;

        /// <summary>
        /// Patches and Injects mod into game via Harmony.
        /// </summary>
        public void Awake()
        {
            _mod = new ();
            _mod.OnLoad();
            _mod.Logger.Info($"{nameof(BetterBulldozerPlugin)}.{nameof(Awake)}");
            Harmony.CreateAndPatchAll(typeof(BetterBulldozerPlugin).Assembly, MyPluginInfo.PLUGIN_GUID);
        }

        [HarmonyPatch(typeof(SystemOrder), nameof(SystemOrder.Initialize), new[] { typeof(UpdateSystem) })]
        [HarmonyPostfix]
        private static void InjectSystems(UpdateSystem updateSystem)
        {
            _mod.Logger.Info($"{nameof(BetterBulldozerPlugin)}.{nameof(InjectSystems)}");
            _mod.OnCreateWorld(updateSystem);
        }
    }
}
#endif