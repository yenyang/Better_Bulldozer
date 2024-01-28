// <copyright file="BetterBulldozerMod.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Better_Bulldozer
{
    using System.IO;
    using Better_Bulldozer.Settings;
    using Better_Bulldozer.Systems;
    using Colossal.Logging;
    using Game;
    using Game.Modding;
    using Game.SceneFlow;

    /// <summary>
    /// Mod entry point.
    /// </summary>
    public class BetterBulldozerMod : IMod
    {
        /// <summary>
        /// Gets the install folder for the mod.
        /// </summary>
        private static string m_modInstallFolder;

        /// <summary>
        /// Gets the static reference to the mod instance.
        /// </summary>
        public static BetterBulldozerMod Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Install Folder for the mod as a string.
        /// </summary>
        public static string ModInstallFolder
        {
            get
            {
                if (m_modInstallFolder is null)
                {
                    m_modInstallFolder = Path.GetDirectoryName(typeof(BetterBulldozerPlugin).Assembly.Location);
                }

                return m_modInstallFolder;
            }
        }

        /// <summary>
        /// Gets ILog for mod.
        /// </summary>
        internal ILog Logger { get; private set; }

        /// <inheritdoc/>
        public void OnLoad()
        {
            Instance = this;
            Logger = LogManager.GetLogger("Mods_Yenyang_Better_Bulldozer", false);
            Logger.Info("Loading. . .");
        }

        /// <inheritdoc/>
        public void OnCreateWorld(UpdateSystem updateSystem)
        {
            Logger.effectivenessLevel = Level.Debug;
            Logger.Info("Handling create world");
            Logger.Info("ModInstallFolder = " + ModInstallFolder);
            LoadLocales();
            updateSystem.UpdateAt<BetterBulldozerUISystem>(SystemUpdatePhase.UIUpdate);
        }

        /// <inheritdoc/>
        public void OnDispose()
        {
            Logger.Info("Disposing..");
        }

        private void LoadLocales()
        {
            foreach (var lang in GameManager.instance.localizationManager.GetSupportedLocales())
            {
                GameManager.instance.localizationManager.AddSource(lang, new LocaleEN());
            }
        }
    }
}
