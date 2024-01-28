// <copyright file="BulldozeToolSystemInitializeRaycastPatch.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

namespace Better_Bulldozer.Patches
{
    using Better_Bulldozer.Systems;
    using Game.Areas;
    using Game.Common;
    using Game.Net;
    using Game.Rendering;
    using Game.Tools;
    using HarmonyLib;
    using Unity.Entities;

    /// <summary>
    /// Patches Bulldoze Tool System Inititialize Raycast to add Markers as something to raycast.
    /// </summary>
    [HarmonyPatch(typeof(BulldozeToolSystem), "InitializeRaycast")]
    public class BulldozeToolSystemInitializeRaycastPatch
    {
        /// <summary>
        /// Patches Bulldoze Tool System Inititialize Raycast to add Markers as something to raycast.
        /// </summary>
        public static void Postfix()
        {
            ToolRaycastSystem toolRaycastSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolRaycastSystem>();
            BetterBulldozerUISystem betterBulldozerUISystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<BetterBulldozerUISystem>();
            RenderingSystem renderingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<RenderingSystem>();
            if (renderingSystem.markersVisible && betterBulldozerUISystem.SelectedRaycastTarget == BetterBulldozerUISystem.RaycastTarget.Markers)
            {
                toolRaycastSystem.typeMask = TypeMask.Net | TypeMask.StaticObjects;
                toolRaycastSystem.netLayerMask = Layer.MarkerPathway | Layer.MarkerTaxiway;
                toolRaycastSystem.raycastFlags = RaycastFlags.Markers | RaycastFlags.Decals;
            }
            else if (betterBulldozerUISystem.SelectedRaycastTarget == BetterBulldozerUISystem.RaycastTarget.Surfaces)
            {
                toolRaycastSystem.typeMask = TypeMask.Areas;
                toolRaycastSystem.areaTypeMask = AreaTypeMask.Surfaces | AreaTypeMask.Spaces;
                toolRaycastSystem.raycastFlags |= RaycastFlags.SubElements;
            }
        }
    }
}
