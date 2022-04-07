using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ColorfulHierarchy
{
    [InitializeOnLoad]
    public static class ColorfulHierarchy
    {
        static ColorfulHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            UnityEngine.Object instance = EditorUtility.InstanceIDToObject(instanceID);

            if (instance != null)
            {
                ColorfulObject colorfulObject = (instance as GameObject).GetComponent<ColorfulObject>();

                if (colorfulObject != null)
                {
                    HierarchyItem item = new HierarchyItem(instanceID, selectionRect, colorfulObject);
                    PaintBackground(item);
                    PaintHoverOverlay(item);
                    PaintText(item);
                    PaintCollapseToggleIcon(item);
                    PaintPrefabIcon(item);
                    PaintEditPrefabIcon(item);
                }
            }
        }

        private static void PaintBackground(HierarchyItem item)
        {
            Color32 color;
            if (item.ColorfulObject.UseDefaultBackgroundColor || item.IsSelected)
            {
                color = EditorColors.GetDefaultBackgroundColor(EditorUtils.IsHierarchyFocused, item.IsSelected);
            }
            else
            {
                color = item.ColorfulObject.BackgroundColor;
            }

            EditorGUI.DrawRect(item.BackgroundRect, color);
        }

        private static void PaintHoverOverlay(HierarchyItem item)
        {
            if (item.IsHovered && !item.IsSelected)
            {
                EditorGUI.DrawRect(item.BackgroundRect, EditorColors.HoverOverlay);
            }
        }

        private static void PaintText(HierarchyItem item)
        {
            Color32 color;

            if (item.ColorfulObject.UseDefaultTextColor || item.IsSelected)
            {
                color = EditorColors.GetDefaultTextColor(EditorUtils.IsHierarchyFocused, item.IsSelected, item.GameObject.activeInHierarchy);
            }
            else
            {
                color = item.ColorfulObject.TextColor;
                color.a = item.GameObject.activeInHierarchy ? EditorColors.TextAlphaObjectEnabled : EditorColors.TextAlphaObjectDisabled;
            }

            GUIStyle labelGUIStyle = new GUIStyle
            {
                normal = new GUIStyleState { textColor = color },
                fontStyle = item.ColorfulObject.FontStyle,
                alignment = item.ColorfulObject.Alignment,
                fontSize = item.ColorfulObject.FontSize,
                font = item.ColorfulObject.Font
            };

            if (item.ColorfulObject.TextDropShadow)
            {
                EditorGUI.DropShadowLabel(item.TextRect, item.ColorfulObject.name, labelGUIStyle);
            }
            else
            {
                EditorGUI.LabelField(item.TextRect, item.ColorfulObject.name, labelGUIStyle);
            }
        }

        private static void PaintCollapseToggleIcon(HierarchyItem item)
        {
            if (item.GameObject.transform.childCount > 0)
            {
                Type sceneHierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                PropertyInfo sceneHierarchyWindow = sceneHierarchyWindowType.GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);

                int[] expandedIDs = (int[])sceneHierarchyWindowType.GetMethod("GetExpandedIDs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(sceneHierarchyWindow.GetValue(null), null);

                string iconID = expandedIDs.Contains(item.InstanceID) ? "IN Foldout on" : "IN foldout";

                GUI.DrawTexture(item.CollapseToggleIconRect, EditorGUIUtility.IconContent(iconID).image, ScaleMode.StretchToFill, true, 0f, EditorColors.CollapseIconTintColor, 0f, 0f);
            }
        }

        private static void PaintPrefabIcon(HierarchyItem item)
        {
            Texture icon = EditorGUIUtility.ObjectContent(EditorUtility.InstanceIDToObject(item.InstanceID), null).image;

            // The above does not account for the selection highlight, so we do it manually...
            if (EditorUtils.IsHierarchyFocused && item.IsSelected)
            {
                if (icon.name == "d_Prefab Icon" || icon.name == "Prefab Icon")
                {
                    icon = EditorGUIUtility.IconContent("d_Prefab On Icon").image;
                }
                else if (icon.name == "GameObject Icon") // Dark theme is fine by default here...
                {
                    icon = EditorGUIUtility.IconContent("GameObject On Icon").image;
                }
            }

            // Alpha of the icon is affected by the object's active/inactive state
            Color color = item.GameObject.activeInHierarchy ? Color.white : new Color(1f, 1f, 1f, 0.5f);

            //GUI.DrawTexture(item.PrefabIconRect, tex, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
            GUI.DrawTexture(item.PrefabIconRect, icon, ScaleMode.StretchToFill, true, 0f, color, 0f, 0f);
        }

        private static void PaintEditPrefabIcon(HierarchyItem item)
        {
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(item.GameObject) != null && PrefabUtility.IsAnyPrefabInstanceRoot(item.GameObject))
            {
                Texture icon = EditorGUIUtility.IconContent("ArrowNavigationRight").image;
                GUI.DrawTexture(item.EditPrefabIconRect, icon, ScaleMode.StretchToFill, true, 0f, EditorColors.EditPrefabIconTintColor, 0f, 0f);
            }
        }
    }
}
