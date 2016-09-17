using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundles
{
   
    //TODO add AssetBundleBuild objects for each bundle and a method of importing asset names and targeting bundles
    //TODO I only moved the asset settings issues from one location to another by extracting the asset bundles.  
    //users that want custom assets just have to set everything that they want appropriately or use the default bundle that's already packaged.
    //slight edits are not welcome. :(

    private static readonly string bundleOutputPath="../FireBoltUnity/AssetBundles/newAssets/";
  //  private static readonly string bundleInputPath = "../FireBoltUnity/AssetBundles/";

    [MenuItem("Asset Bundles/Log Asset Bundle Names")]
    static void LogAssetBundleNames()
    {
        foreach (var s in AssetDatabase.GetAllAssetBundleNames())
        {
            Debug.Log("Asset Bundle: " + s);
        }
    }

    [MenuItem("Asset Bundles/Log Assets")]
    static void LogAssets()
    {
        foreach (var bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            var paths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            foreach (var path in paths)
            {
                Debug.Log("Asset: " + path);
            }
        }        
    }

    [MenuItem("Asset Bundles/Build Asset Bundles")]
    static void BuildAllAssetBundles()
    {
        Debug.Log("Building all asset bundles");

        if (!Directory.Exists(bundleOutputPath))
            Directory.CreateDirectory(bundleOutputPath);

        BuildPipeline.BuildAssetBundles(bundleOutputPath,BuildAssetBundleOptions.UncompressedAssetBundle);
        Debug.Log("Asset bundle build complete @ "+ bundleOutputPath);
    }

    [MenuItem("Asset Bundles/Remove Unused Names")]
    static void RemoveUnusedNames()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }
	
    //[MenuItem("Asset Bundles/Load bundles to editor")]
    //static void LoadAssetFromAssetBundle()
    //{
    //    AssetDatabase.LoadAllAssetsAtPath(bundleInputPath);
    //   StartCoroutine( DownloadAssetBundle(bundleInputPath));
    //}

    //IEnumerator DownloadAssetBundle(string url)
    //{
    //    yield return StartCoroutine(AssetBundleManager.downloadAssetBundle(url, version));
    //    bundle = AssetBundleManager.getAssetBundle(url, version);
    //    GameObject obj = Instantiate(bundle.LoadAsset("ExampleObject"),Vector3.zero,Quaternion.identity) as GameObject;
    //    // Unload the AssetBundles compressed contents to conserve memory
    //    bundle.Unload(false);
    //}
    

}
