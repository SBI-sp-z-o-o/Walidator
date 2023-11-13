#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CreateAssetBundle : MonoBehaviour
{
    [MenuItem("Assets/Build Asset Bundle")]
    static void BuildBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/Bundles", BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}
#endif
