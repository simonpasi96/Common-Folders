using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class CommonFolders
{
    public enum FolderType
    {
        Materials, Prefabs, Scenes, Scripts, Shaders, Textures,
        Editor, Packages, Standard_Assets
    }
    readonly static FolderType[] CommonFolderTypes = new FolderType[] { FolderType.Materials, FolderType.Prefabs, FolderType.Scenes, FolderType.Scripts, FolderType.Shaders, FolderType.Textures };


    #region MenuItems.
    const string MenuItemRoot = "Assets/Create/Common Folders/";
    const int CommonPriority = 20;
    const int SpecialPriority = 100;

    [MenuItem(MenuItemRoot + "/Materials", false, CommonPriority)]
    public static void CreateMaterialsFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Materials);
    [MenuItem(MenuItemRoot + "/Prefabs", false, CommonPriority)]
    public static void CreatePrefabsFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Prefabs);
    [MenuItem(MenuItemRoot + "/Scenes", false, CommonPriority)]
    public static void CreateScenesFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Scenes);
    [MenuItem(MenuItemRoot + "/Scripts", false, CommonPriority)]
    public static void CreateScriptsFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Scripts);
    [MenuItem(MenuItemRoot + "/Shaders", false, CommonPriority)]
    public static void CreateShadersFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Shaders);
    [MenuItem(MenuItemRoot + "/Textures", false, CommonPriority)]
    public static void CreateTexturesFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Textures);

    [MenuItem(MenuItemRoot + "/All Possible Folders", false, CommonPriority)]
    public static void CreateAllPossibleFoldersAtSelection() => CreateAllPossibleFoldersTypesAtObject(Selection.activeObject);

    [MenuItem(MenuItemRoot + "/Editor", false, SpecialPriority)]
    static void CreateEditorFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Editor);
    [MenuItem(MenuItemRoot + "/Packages", false, SpecialPriority)]
    static void CreatePackagesFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Packages);
    [MenuItem(MenuItemRoot + "/Standard Assets", false, SpecialPriority)]
    static void CreateStandardAssetsFolderAtSelection() => CreateFolder(Selection.activeObject, FolderType.Standard_Assets);
    #endregion


    #region Workers.
    /// <summary>
    /// Create a folder at an asset and move filtered assets in it.
    /// </summary>
    public static void CreateFolder(Object context, FolderType folderType, bool abortIfNoAssetsFound = false)
    {
        // Get workFolder and assetsToMove.
        string workFolder = GetObjectFolder(context);
        List<string> foundAssets = AssetDatabase.FindAssets(FolderTypeToFilter(folderType), new[] { workFolder }).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToList();
        List<string> assetsToMove = foundAssets.Where(asset => ComparePaths(Path.GetDirectoryName(asset), workFolder)).ToList();

        // Abort if abortIfNoAssetsFound is true and no files are found.
        if (abortIfNoAssetsFound && assetsToMove.Count == 0)
        {
            return;
        }

        // Create a new folder and move all assets in it.
        string newFolderPath = GetFolder(workFolder, FolderTypeToName(folderType));
        assetsToMove.ForEach(asset => AssetDatabase.MoveAsset(asset, newFolderPath + "/" + Path.GetFileName(asset)));

        // Refresh AssetDatabase.   
        AssetDatabase.Refresh();
    }

    public static void CreateAllPossibleFoldersTypesAtObject(Object activeObject)
    {
        Object workingFolder = AssetDatabase.LoadAssetAtPath(GetObjectFolder(activeObject), typeof(Object));
        Enum.GetValues(typeof(FolderType))
            .Cast<FolderType>()
            .ToList()
            .ForEach(folderType => CreateFolder(workingFolder, folderType, true));
    }
    #endregion


    [InitializeOnLoadMethod]
    static void EnforcePlacementInEditorFolder()
    {
        if (File.Exists(Application.dataPath + "/" + typeof(CommonFolders) + ".cs"))
        {
            AssetDatabase.MoveAsset("Assets/CommonFolders.cs", GetFolder("Assets", "Editor") + "/" + typeof(CommonFolders) + ".cs");
        }
    }


    #region FolderType helpers.
    static string FolderTypeToName(FolderType folderType) => folderType.ToString().Replace('_', ' ');
    static string FolderTypeToFilter(FolderType folderType) => "t:" + (CommonFolderTypes.Contains(folderType) ? folderType.ToString().TrimEnd('s') : "t:null");
    #endregion

    #region AssetDatabase helpers.
    /// <summary>
    /// Get the folder of an object in the AssetDatabase. If object is a folder, object will be returned.
    /// </summary>
    static string GetObjectFolder(Object @object)
    {
        if (@object)
        {
            string objectPath = AssetDatabase.GetAssetPath(@object);
            return AssetDatabase.IsValidFolder(objectPath) ? objectPath : Path.GetDirectoryName(objectPath);
        }
        else
        {
            return "Assets";
        }
    }

    static string GetFolder(string parentFolder, string folderName)
    {
        string folderPath = parentFolder + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            folderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(parentFolder, folderName));
        }

        return folderPath;
    }

    static bool ComparePaths(string pathA, string pathB) => Path.GetFullPath(pathA) == Path.GetFullPath(pathB);
    #endregion
}