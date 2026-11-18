using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 모델(.obj)을 새로 만들어 머티리얼 이름·개수가 바뀌었을 때, 프리팹 렌더러의 머티리얼을 새 모델 것으로 다시 연결하는 일회용 에디터 스크립트.
// 유니티가 스크립트를 불러올 때 한 번 실행됨. 다 되면 이 파일은 지워도 됨.
// 수동 실행: 상단 메뉴 DDang → 호랑이·황소 머티리얼 다시 연결
[InitializeOnLoad]
internal static class RelinkModelMaterials
{
    // 프리팹, 모델, .obj 안 usemtl 순서(= 서브메시 순서)
    private static readonly (string prefab, string model, string[] order)[] Targets =
    {
        ("Assets/3.Prefabs/Animal/Tiger.prefab", "Assets/Resourse/Tiger.obj",
            new[] { "Tiger_Orange", "Tiger_Black", "Tiger_White", "Tiger_Eye" }),
        ("Assets/3.Prefabs/Animal/Bull.prefab", "Assets/Resourse/Bull.obj",
            new[] { "Bull_Body", "Bull_Dark", "Bull_Horn", "Bull_Muzzle", "Bull_RedEye", "Bull_Gold", "Bull_HornTip" }),
    };

    static RelinkModelMaterials() => EditorApplication.delayCall += Run;

    [MenuItem("DDang/호랑이·황소 머티리얼 다시 연결")]
    private static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (var t in Targets) Relink(t.prefab, t.model, t.order);
    }

    private static void Relink(string prefabPath, string modelPath, string[] order)
    {
        Dictionary<string, Material> materials = AssetDatabase.LoadAllAssetsAtPath(modelPath)
            .OfType<Material>().ToDictionary(m => m.name, m => m);
        Mesh mesh = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Mesh>().FirstOrDefault();
        if (mesh == null || order.Any(n => !materials.ContainsKey(n)))
        {
            Debug.LogWarning($"[RelinkModelMaterials] {modelPath}에서 메시나 머티리얼을 못 찾았어요: " +
                string.Join(", ", order.Where(n => !materials.ContainsKey(n))));
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        bool changed = false;
        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (AssetDatabase.GetAssetPath(filter.sharedMesh) != modelPath) continue;
            var rend = filter.GetComponent<MeshRenderer>();
            Material[] wanted = order.Select(n => materials[n]).ToArray();
            if (filter.sharedMesh == mesh && rend.sharedMaterials.SequenceEqual(wanted)) continue;

            filter.sharedMesh = mesh;
            rend.sharedMaterials = wanted;
            changed = true;
        }

        if (changed)
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log($"[RelinkModelMaterials] 머티리얼 다시 연결: {prefabPath}");
        }
        PrefabUtility.UnloadPrefabContents(root);
    }
}
