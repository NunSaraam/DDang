using UnityEngine;

namespace Animal
{
    // 닭·황소가 같이 쓰는 연출 도우미 (코드로 만든 큐브, 색 입히기, 방향 계산)
    public static class AnimalFx
    {
        // 콜라이더 없는 큐브. 플레이어 이동·공격 판정에 걸리지 않음
        public static GameObject CreateCube(string name, Transform parent, Material material, Color color)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.name = name;
            if (parent != null) cube.transform.SetParent(parent, false);
            Tint(cube.GetComponent<Renderer>(), material, color);
            return cube;
        }

        // 머티리얼은 공유하고 색만 바꿈
        public static void Tint(Renderer rend, Material material, Color color)
        {
            rend.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);                    // URP
            block.SetColor("_Color", color);                        // 기본 셰이더
            block.SetFloat("_Smoothness", 0.2f);
            rend.SetPropertyBlock(block);
        }

        // 인스펙터에서 머티리얼을 넣지 않았을 때 쓰는 반투명 머티리얼 (URP Unlit). 다 쓰면 Destroy 해야 함
        public static Material CreateTransparentMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return new Material(Shader.Find("Sprites/Default"));

            var mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return mat;
        }

        // 아무 방향을 상하좌우 중 가까운 칸 방향으로 (KnockBack과 같은 규칙)
        public static Vector2Int SnapToCellDir(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) < 0.001f && Mathf.Abs(direction.z) < 0.001f) return Vector2Int.right;
            return Mathf.Abs(direction.x) >= Mathf.Abs(direction.z)
                ? new Vector2Int((int)Mathf.Sign(direction.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(direction.z));
        }

        public static Vector3 ToWorldDir(Vector2Int dir) => new Vector3(dir.x, 0f, dir.y);

        // 바닥 기준으로 카메라가 있는 쪽 방향 (두 화면 카메라 각도가 같으므로 하나만 봄). 기본 각도 (30, -90, 0)이면 +x
        public static Vector3 DirectionToCamera()
        {
            Vector3 dir = Vector3.right;
            var cam = Object.FindFirstObjectByType<Cameras.PlayerDualCamera>();
            if (cam != null)
            {
                Vector3 f = -cam.transform.forward;
                f.y = 0f;
                if (f.sqrMagnitude > 0.01f) dir = f.normalized;
            }
            return dir;
        }

        // 맵 가운데에서 away 방향으로 끝까지 간 칸(inset만큼 안쪽). 그 칸이 장애물이면 가장 가까운 바닥
        public static Vector2Int FarEdgeCell(GridGenerator grid, Vector3 away, int inset)
        {
            int halfX = grid.gridSizeX / 2, halfZ = grid.gridSizeZ / 2;
            var target = new Vector2Int(
                Mathf.Clamp(halfX + Mathf.RoundToInt(away.x * (halfX - inset)), 0, grid.gridSizeX - 1),
                Mathf.Clamp(halfZ + Mathf.RoundToInt(away.z * (halfZ - inset)), 0, grid.gridSizeZ - 1));
            return grid.GetNearestFloor(target);
        }
    }
}
