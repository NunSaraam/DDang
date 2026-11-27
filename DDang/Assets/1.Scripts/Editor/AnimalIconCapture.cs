using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animal
{
    // 메뉴 Tools → Animal 아이콘 만들기
    // 모든 AnimalData의 프리팹을 빈 미리보기 씬에서 찍어 PNG로 저장하고, AnimalData.icon에 넣음
    public static class AnimalIconCapture
    {
        private const string OutFolder = "Assets/2.GameResource/UI/icon/Animal";
        private const int Size = 256;                                   // 기존 아이콘과 같은 크기
        private static readonly Vector3 ViewAngle = new(20f, 150f, 0f); // 살짝 위에서, 비스듬히 앞

        [MenuItem("Tools/Animal 아이콘 만들기")]
        private static void CaptureAll()
        {
            Directory.CreateDirectory(OutFolder);
            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:AnimalData"))
            {
                var data = AssetDatabase.LoadAssetAtPath<AnimalData>(AssetDatabase.GUIDToAssetPath(guid));
                if (data == null || data.prefab == null) continue;

                string path = $"{OutFolder}/{data.name}.png";
                File.WriteAllBytes(path, Render(data.prefab.gameObject));
                AssetDatabase.ImportAsset(path);
                SetupSprite(path);

                data.icon = AssetDatabase.LoadAssetAtPath<Sprite>(path); // 경고 UI에 연결
                EditorUtility.SetDirty(data);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Animal 아이콘 {count}개 완료 → {OutFolder}");
        }

        private static byte[] Render(GameObject prefab)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();         // 지금 열린 씬은 건드리지 않음
            try
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);

                var light = new GameObject("Light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                SceneManager.MoveGameObjectToScene(light.gameObject, scene);

                // 모델 크기에 맞춰 카메라 위치
                Bounds b = new(model.transform.position, Vector3.zero);
                foreach (Renderer r in model.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);

                var cam = new GameObject("Cam").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(cam.gameObject, scene);
                cam.scene = scene;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.clear;                      // 투명 배경
                cam.orthographic = true;
                cam.orthographicSize = b.extents.magnitude * 0.9f;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = b.extents.magnitude * 10f;
                cam.transform.rotation = Quaternion.Euler(ViewAngle);
                cam.transform.position = b.center - cam.transform.forward * (b.extents.magnitude * 4f);

                var rt = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                byte[] png = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);
                cam.targetTexture = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                return png;
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static void SetupSprite(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
