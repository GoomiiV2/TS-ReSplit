using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class Explorer : EditorWindow
{
    SimpleTreeView FileListing;
    Vector2 FIleListScrollPos;

    const float FileListingWidth = 150;

    [MenuItem(ReSplitMenus.MenuName + "/Explorer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        Explorer window = (Explorer)EditorWindow.GetWindow(typeof(Explorer));

        window.FileListing = new SimpleTreeView(new TreeViewState());

        window.Show();
    }

    // Urggggh
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            #region File listing
            EditorGUILayout.BeginHorizontal(GUILayout.Width(FileListingWidth));
            {
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("File Listing");
                    FIleListScrollPos = EditorGUILayout.BeginScrollView(FIleListScrollPos, true, true, GUILayout.Width(FileListingWidth), GUILayout.Height(position.height));
                    {
                        FileListing.OnGUI(new Rect(0, 0, FileListingWidth, position.height));
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Items
            EditorGUILayout.BeginHorizontal(GUILayout.Width(FileListingWidth));
            {
                
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Inspector
            EditorGUILayout.BeginHorizontal(GUILayout.Width(FileListingWidth));
            {
                
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawSeprator()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }
}

public class SimpleTreeView : TreeView
{
    public SimpleTreeView(TreeViewState treeViewState)
        : base(treeViewState)
    {
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
        // are created from data. Here we create a fixed set of items. In a real world example,
        // a data model should be passed into the TreeView and the items created from the model.

        // This section illustrates that IDs should be unique. The root item is required to 
        // have a depth of -1, and the rest of the items increment from that.
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        var allItems = new List<TreeViewItem>
        {
            new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
            new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
            new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
            new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
            new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
            new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
            new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
            new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
            new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
        };

        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        SetupParentsAndChildrenFromDepths(root, allItems);

        // Return root of the tree
        return root;
    }
}
