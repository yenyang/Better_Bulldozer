// <copyright file="BetterBulldozerUISystem.cs" company="Yenyang's Mods. MIT License">
// Copyright (c) Yenyang's Mods. MIT License. All rights reserved.
// </copyright>

// #define VERBOSE
namespace Better_Bulldozer.Systems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Anarchy.Utils;
    using cohtml.Net;
    using Colossal.Entities;
    using Colossal.Logging;
    using Game.Areas;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.SceneFlow;
    using Game.Tools;
    using Game.UI;
    using Unity.Entities;

    /// <summary>
    /// UI system for Better Bulldozer extensions to the bulldoze tool.
    /// </summary>
    public partial class BetterBulldozerUISystem : UISystemBase
    {
        private View m_UiView;
        private ToolSystem m_ToolSystem;
        private string m_InjectedJS = string.Empty;
        private ILog m_Log;
        private RenderingSystem m_RenderingSystem;
        private PrefabSystem m_PrefabSystem;
        private bool m_BulldozeItemShown;
        private string m_LastTool;
        private List<BoundEventHandle> m_BoundEventHandles;
        private string m_BulldozeToolItemScript;
        private BulldozeToolSystem m_BulldozeToolSystem;
        private bool m_LastGamePlayManipulation;
        private bool m_LastBypassConfrimation;
        private bool m_RecordedShowMarkers;
        private bool m_PrefabIsMarker = false;
        private RaycastTarget m_RaycastTarget;
        private bool m_FirstTimeLoadingJS = true;
        private bool m_DelayOneFrameForAnarchy = true;
        private NetToolSystem m_NetToolSystem;
        private AreaTypeMask m_AreasFilter = AreaTypeMask.Surfaces;
        private string m_AreasFiltersRowScript = string.Empty;

        /// <summary>
        /// An enum to handle different raycast target options.
        /// </summary>
        public enum RaycastTarget
        {
            /// <summary>
            /// Do not change the raycast targets.
            /// </summary>
            Vanilla,

            /// <summary>
            /// Exclusively target surfaces and spaces
            /// </summary>
            Areas,

            /// <summary>
            /// Exclusively target markers.
            /// </summary>
            Markers,
        }

        /// <summary>
        /// Gets a value indicating what to raycast.
        /// </summary>
        public RaycastTarget SelectedRaycastTarget { get => m_RaycastTarget; }

        /// <summary>
        /// Gets a value indicating the filter to apply to areas.
        /// </summary>
        public AreaTypeMask AreasFilter { get => m_AreasFilter; }

        /// <inheritdoc/>
        protected override void OnCreate()
        {
            m_Log = BetterBulldozerMod.Instance.Logger;
            m_ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            m_UiView = GameManager.instance.userInterface.view.View;
            m_BulldozeToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<BulldozeToolSystem>();
            m_RenderingSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<RenderingSystem>();
            m_PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            m_NetToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<NetToolSystem>();
            ToolSystem toolSystem = m_ToolSystem; // I don't know why vanilla game did this.
            m_ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(toolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));
            m_BoundEventHandles = new ();

            m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
            m_BulldozeToolItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYBB-bulldoze-tool-mode-row.html"), "if (document.getElementById(\"YYA-bulldoze-tool-mode-item\") == null && document.getElementById(\"YYBB-bulldoze-tool-mode-item\") == null) { yyBetterBulldozer.div.className = \"item_bZY\"; yyBetterBulldozer.div.id = \"YYBB-bulldoze-tool-mode-item\"; yyBetterBulldozer.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyBetterBulldozer.entities[0] != null) { yyBetterBulldozer.entities[0].insertAdjacentElement('afterbegin', yyBetterBulldozer.div); } }");
            m_AreasFiltersRowScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYBB-Areas-Filters-row.html"), "if (document.getElementById(\"YYBB-area-filters-item\") == null) { yyBetterBulldozer.div.className = \"item_bZY\"; yyBetterBulldozer.div.id = \"YYBB-area-filters-item\"; yyBetterBulldozer.toolModeItem = document.getElementById(\"YYBB-bulldoze-tool-mode-item\"); if (yyBetterBulldozer.toolModeItem) { yyBetterBulldozer.toolModeItem.insertAdjacentElement('beforebegin', yyBetterBulldozer.div); } }");


            if (m_UiView == null)
            {
                m_Log.Info($"{nameof(BetterBulldozerUISystem)}.{nameof(OnCreate)} m_UiView == null");
            }

            base.OnCreate();
        }

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (m_UiView == null)
            {
                return;
            }

            if (m_ToolSystem.activeTool != m_BulldozeToolSystem)
            {
                if (m_BulldozeItemShown)
                {
                    UnshowBulldozeItem();
                }

                Enabled = false;
                return;
            }

            // This script creates the Anarchy object if it doesn't exist.
            UIFileUtils.ExecuteScript(m_UiView, "if (yyBetterBulldozer == null) var yyBetterBulldozer = {};");

            if (m_BulldozeItemShown == false)
            {
                if (m_DelayOneFrameForAnarchy)
                {
                    m_DelayOneFrameForAnarchy = false;
                    return;
                }

                if (m_InjectedJS == string.Empty)
                {
                    m_Log.Warn($"{nameof(BetterBulldozerUISystem)}.{nameof(OnUpdate)} m_InjectedJS is empty!!! Did you forget to include the ui.js file in the mod install folder?");
                    m_InjectedJS = UIFileUtils.ReadJS(Path.Combine(UIFileUtils.AssemblyPath, "ui.js"));
                    m_BulldozeToolItemScript = UIFileUtils.ReadHTML(Path.Combine(UIFileUtils.AssemblyPath, "YYBB-bulldoze-tool-mode-row.html"), "if (document.getElementById(\"YYA-bulldoze-tool-mode-item\") == null && document.getElementById(\"YYBB-bulldoze-tool-mode-item\") == null) { yyBetterBulldozer.div.className = \"item_bZY\"; yyBetterBulldozer.div.id = \"YYBB-bulldoze-tool-mode-item\"; yyBetterBulldozer.entities = document.getElementsByClassName(\"tool-options-panel_Se6\"); if (yyBetterBulldozer.entities[0] != null) { yyBetterBulldozer.entities[0].insertAdjacentElement('afterbegin', yyBetterBulldozer.div); } }");
                }

                // This unregisters the events.
                foreach (BoundEventHandle eventHandle in m_BoundEventHandles)
                {
                    m_UiView.UnregisterFromEvent(eventHandle);
                }

                m_BoundEventHandles.Clear();

                // This script creates the bulldozer tool mode row and sets up the buttons.
                UIFileUtils.ExecuteScript(m_UiView, m_BulldozeToolItemScript);

                // This script defines the JS functions if they are not defined.
                UIFileUtils.ExecuteScript(m_UiView, m_InjectedJS);

                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-log", (Action<string>)LogFromJS));

                if (m_FirstTimeLoadingJS)
                {
                    m_FirstTimeLoadingJS = false;
                    return;
                }

                UIFileUtils.ExecuteScript(m_UiView, "yyBetterBulldozer.toolModeLabel = document.getElementById(\"YYBB-tool-mode-label\"); if (yyBetterBulldozer.toolModeLabel) yyBetterBulldozer.toolModeLabel.innerHTML = engine.translate(yyBetterBulldozer.toolModeLabel.getAttribute(\"localeKey\"));");
                UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Bypass-Confirmation-Button\", {BoolToString(m_BulldozeToolSystem.debugBypassBulldozeConfirmation)}, \"BypassConfirmationButton\")");
                UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Gameplay-Manipulation-Button\", {BoolToString(m_BulldozeToolSystem.allowManipulation)}, \"GameplayManipulationButton\")");
                UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Raycast-Markers-Button\", {IsRaycastTargetSelected(RaycastTarget.Markers)}, \"RaycastMarkersButton\")");
                UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Raycast-Areas-Button\", {IsRaycastTargetSelected(RaycastTarget.Areas)}, \"RaycastAreasButton\")");

                if (m_RaycastTarget == RaycastTarget.Areas)
                {
                    UIFileUtils.ExecuteScript(m_UiView, m_AreasFiltersRowScript);
                    UIFileUtils.ExecuteScript(m_UiView, "yyBetterBulldozer.areaFilterLabel = document.getElementById(\"YYBB-areas-filter-label\"); if (yyBetterBulldozer.areaFilterLabel) yyBetterBulldozer.areaFilterLabel.innerHTML = engine.translate(yyBetterBulldozer.areaFilterLabel.getAttribute(\"localeKey\"));");
                    UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Surfaces-Filter-Button\", {IsAreaFilterSelected(AreaTypeMask.Surfaces)}, \"SurfacesFilterButton\")");
                    UIFileUtils.ExecuteScript(m_UiView, $"if (typeof yyBetterBulldozer.setupButton == 'function') yyBetterBulldozer.setupButton(\"YYBB-Spaces-Filter-Button\", {IsAreaFilterSelected(AreaTypeMask.Spaces)}, \"SpacesFilterButton\")");
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Surfaces-Filter-Button", (Action<bool>)SurfacesFilterToggled));
                    m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Spaces-Filter-Button", (Action<bool>)SpacesFilterToggled));
                }

                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Bypass-Confirmation-Button", (Action<bool>)BypassConfirmationToggled));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Gameplay-Manipulation-Button", (Action<bool>)GameplayManipulationToggled));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Raycast-Markers-Button", (Action<bool>)RaycastMarkersButtonToggled));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("YYBB-Raycast-Areas-Button", (Action<bool>)RaycastAreasButtonToggled));
                m_BoundEventHandles.Add(m_UiView.RegisterForEvent("CheckForElement", (Action<bool>)ElementCheck));

                m_BulldozeItemShown = true;
                m_DelayOneFrameForAnarchy = true;
            }
            else
            {
                // This script checks if bulldoze tool mode item exists. If it doesn't it triggers bulldoze tool mode being recreated.
                UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"YYBB-bulldoze-tool-mode-item\") == null) engine.trigger('CheckForElement', false);");

                if (m_RaycastTarget == RaycastTarget.Areas)
                {
                    UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"YYBB-areas-filter-label\") == null) engine.trigger('CheckForElement', false);");
                }
                else
                {
                    UIFileUtils.ExecuteScript(m_UiView, $"if (document.getElementById(\"YYBB-areas-filter-label\") != null) engine.trigger('CheckForElement', false);");
                }

                if (m_LastBypassConfrimation != m_BulldozeToolSystem.debugBypassBulldozeConfirmation)
                {
                    if (m_BulldozeToolSystem.debugBypassBulldozeConfirmation)
                    {
                        // This script finds sets Bypass-Confirmation-Button button selected if toggled using DevUI.
                        m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Bypass-Confirmation-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.add(\"selected\");");
                    }
                    else
                    {
                        // This script finds sets Bypass-Confirmation-Button button unselected if toggled using DevUI
                        m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Bypass-Confirmation-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
                    }

                    m_LastBypassConfrimation = m_BulldozeToolSystem.debugBypassBulldozeConfirmation;
                }

                if (m_LastGamePlayManipulation != m_BulldozeToolSystem.allowManipulation)
                {
                    if (m_BulldozeToolSystem.allowManipulation)
                    {
                        // This script finds sets Gameplay-Manipulation-Button button selected if toggled using DevUI.
                        m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Gameplay-Manipulation-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.add(\"selected\");");
                    }
                    else
                    {
                        // This script finds sets Gameplay-Manipulation-Button button unselected if toggled using DevUI
                        m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Gameplay-Manipulation-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
                    }

                    m_LastGamePlayManipulation = m_BulldozeToolSystem.allowManipulation;
                }
            }

            if (m_RaycastTarget == RaycastTarget.Markers && !m_RenderingSystem.markersVisible)
            {
                m_RenderingSystem.markersVisible = true;
            }

            base.OnUpdate();
        }

        /// <summary>
        /// Get a script for Destroing element by id if that element exists.
        /// </summary>
        /// <param name="id">The id from HTML or JS.</param>
        /// <returns>a script for Destroing element by id if that element exists.</returns>
        private string DestroyElementByID(string id)
        {
            return $"yyBetterBulldozer.itemElement = document.getElementById(\"{id}\"); if (yyBetterBulldozer.itemElement) yyBetterBulldozer.itemElement.parentElement.removeChild(yyBetterBulldozer.itemElement);";
        }

        /// <summary>
        /// Logs a string from JS.
        /// </summary>
        /// <param name="log">A string from JS to log.</param>
        private void LogFromJS(string log) => m_Log.Debug($"{nameof(BetterBulldozerUISystem)}.{nameof(LogFromJS)} {log}");

        /// <summary>
        /// Converts a C# bool to JS string.
        /// </summary>
        /// <param name="flag">a bool.</param>
        /// <returns>"true" or "false".</returns>
        private string BoolToString(bool flag)
        {
            if (flag)
            {
                return "true";
            }

            return "false";
        }

        /// <summary>
        /// Returns a JS string for whether the raycast target is selected or not.
        /// </summary>
        /// <param name="target">A Raycast target</param>
        /// <returns>true or false as a string.</returns>
        private string IsRaycastTargetSelected(RaycastTarget target)
        {
            if (m_RaycastTarget == target)
            {
                return "true";
            }

            return "false";
        }

        private string IsAreaFilterSelected(AreaTypeMask areaTypeMask)
        {
            if ((m_AreasFilter & areaTypeMask) == areaTypeMask)
            {
                return "true";
            }

            return "false";
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. Toggles the bypassConfirmation field of the bulldozer system.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void BypassConfirmationToggled(bool flag)
        {
            m_BulldozeToolSystem.debugBypassBulldozeConfirmation = flag;
            m_LastBypassConfrimation = flag;
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. Toggles the game playmanipulation field of the bulldozer system.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void GameplayManipulationToggled(bool flag)
        {
            m_BulldozeToolSystem.allowManipulation = flag;
            m_LastGamePlayManipulation = flag;
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. Toggles the m_RenderingSystem.MarkersVisible.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void RaycastMarkersButtonToggled(bool flag)
        {
            if (flag)
            {
                m_RaycastTarget = RaycastTarget.Markers;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Raycast-Areas-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
            }
            else
            {
                m_RaycastTarget = RaycastTarget.Vanilla;
                m_RenderingSystem.markersVisible = m_RecordedShowMarkers;
            }
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. For filtering for surfaces.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void SurfacesFilterToggled(bool flag)
        {
            if (flag)
            {
                m_AreasFilter = AreaTypeMask.Surfaces;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Spaces-Filter-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
            }
            else
            {
                m_AreasFilter = AreaTypeMask.Spaces;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Spaces-Filter-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.add(\"selected\");");
            }
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. For filtering for surfaces.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void SpacesFilterToggled(bool flag)
        {
            if (flag)
            {
                m_AreasFilter = AreaTypeMask.Spaces;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Surfaces-Filter-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
            }
            else
            {
                m_AreasFilter = AreaTypeMask.Surfaces;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Surfaces-Filter-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.add(\"selected\");");
            }
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. Toggles the m_RaycastAreas.
        /// </summary>
        /// <param name="flag">A bool for what to set the field to.</param>
        private void RaycastAreasButtonToggled(bool flag)
        {
            if (flag)
            {
                m_RaycastTarget = RaycastTarget.Areas;
                m_UiView.ExecuteScript($"yyBetterBulldozer.buttonElement = document.getElementById(\"YYBB-Raycast-Markers-Button\"); if (yyBetterBulldozer.buttonElement != null) yyBetterBulldozer.buttonElement.classList.remove(\"selected\");");
                m_RenderingSystem.markersVisible = m_RecordedShowMarkers;
            }
            else
            {
                m_RaycastTarget = RaycastTarget.Vanilla;
            }
        }

        /// <summary>
        /// C# event handler for event callback from UI JavaScript. If element YYA-anarchy-item is found then set value to true.
        /// </summary>
        /// <param name="flag">A bool for whether to element was found.</param>
        private void ElementCheck(bool flag) => m_BulldozeItemShown = flag;

        /// <summary>
        /// Handles cleaning up after the icons are no longer needed.
        /// </summary>
        private void UnshowBulldozeItem()
        {
            if (m_UiView == null)
            {
                return;
            }

            // This script destroys the bulldoze tool mode row if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYBB-bulldoze-tool-mode-item"));

            // This script destroys the area filters mode row if it exists.
            UIFileUtils.ExecuteScript(m_UiView, DestroyElementByID("YYBB-area-filters-item"));

            // This unregisters the events.
            foreach (BoundEventHandle eventHandle in m_BoundEventHandles)
            {
                m_UiView.UnregisterFromEvent(eventHandle);
            }

            m_BoundEventHandles.Clear();

            // This records that everything is cleaned up.
            m_BulldozeItemShown = false;
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            // This script creates the Anarchy object if it doesn't exist.
            UIFileUtils.ExecuteScript(m_UiView, "if (yyBetterBulldozer == null) var yyBetterBulldozer = {};");

            if (tool != m_BulldozeToolSystem)
            {
                if (m_BulldozeItemShown)
                {
                    UnshowBulldozeItem();
                }

                this.Enabled = false;
                if (m_LastTool == m_BulldozeToolSystem.toolID && m_RaycastTarget == RaycastTarget.Markers)
                {
                     m_RenderingSystem.markersVisible = m_RecordedShowMarkers;
                }

                if (tool == m_NetToolSystem && m_NetToolSystem.GetPrefab() != null)
                {
                    if (m_PrefabSystem.TryGetEntity(m_NetToolSystem.GetPrefab(), out Entity prefabEntity))
                    {
                        if (EntityManager.HasComponent<MarkerNetData>(prefabEntity))
                        {
                            m_PrefabIsMarker = true;
                            m_RecordedShowMarkers = m_RenderingSystem.markersVisible;
                        }
                    }
                    else
                    {
                        m_PrefabIsMarker = false;
                    }
                }
                else
                {
                    m_PrefabIsMarker = false;
                }
            }
            else
            {
                this.Enabled = true;
                if (m_LastTool == m_NetToolSystem.toolID && m_NetToolSystem.GetPrefab() != null)
                {
                    if (m_PrefabSystem.TryGetEntity(m_NetToolSystem.GetPrefab(), out Entity prefabEntity))
                    {
                        if (EntityManager.HasComponent<MarkerNetData>(prefabEntity))
                        {
                            m_PrefabIsMarker = true;
                        }
                    }
                    else
                    {
                        m_PrefabIsMarker = false;
                    }
                }
                else
                {
                    m_PrefabIsMarker = false;
                }

                if (!m_PrefabIsMarker || m_LastTool != m_NetToolSystem.toolID)
                {
                    m_RecordedShowMarkers = m_RenderingSystem.markersVisible;
                }

                if (m_RaycastTarget == RaycastTarget.Markers)
                {
                    m_RenderingSystem.markersVisible = true;
                }
            }

            m_LastTool = tool.toolID;
        }
    }
}
