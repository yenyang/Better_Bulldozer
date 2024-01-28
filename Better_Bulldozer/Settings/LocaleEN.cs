// <copyright file="LocaleEN.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Better_Bulldozer.Settings
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.IO.AssetDatabase.Internal;

    /// <summary>
    /// Localization for <see cref="BetterBulldozerMod"/> mod in English.
    /// </summary>
    public class LocaleEN : IDictionarySource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleEN"/> class.
        /// </summary>
        public LocaleEN()
        {
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { "YY_BETTER_BULLDOZER.ToolMode", "Tool Mode" },
                { "YY_BETTER_BULLDOZER.RaycastSurfacesButton", "Target Surfaces" },
                { "YY_BETTER_BULLDOZER_DESCRIPTION.RaycastSurfacesButton", "Makes the bulldozer EXCLUSIVELY target surfaces and spaces inside or outside of buildings so you can remove them in one click. You must turn this off to bulldoze anything else." },
                { "YY_BETTER_BULLDOZER.RaycastMarkersButton", "Target Markers" },
                { "YY_BETTER_BULLDOZER_DESCRIPTION.RaycastMarkersButton", "Shows and EXCLUSIVELY targets markers and invisible roads. With this enabled you can demolish invisible networks, invisible parking decals, various spots, points, and spawners, but SAVE FIRST! You cannot demolish these within buildings." },
                { "YY_BETTER_BULLDOZER.GameplayManipulationButton", "Gameplay Manipulation" },
                { "YY_BETTER_BULLDOZER_DESCRIPTION.GameplayManipulationButton", "Allows you to use the bulldozer on moving objects such as vehicles or cims." },
                { "YY_BETTER_BULLDOZER.BypassConfirmationButton", "Bypass Confirmation" },
                { "YY_BETTER_BULLDOZER_DESCRIPTION.BypassConfirmationButton", "Disables the prompt for whether you are sure you want to demolish a building." },
            };
        }

        /// <inheritdoc/>
        public void Unload()
        {
        }
    }
}