"""
복셀 모델 생성 도구 (난입 동물용)

사용법:
    pip install numpy matplotlib
    python voxel_model.py            # Tiger.obj, Tiger.mtl, preview.png 생성

새 동물 추가:
    1. PAL에 필요한 색을 추가 (번호: (이름, (r, g, b)))
    2. build_tiger()를 참고해 build_<동물>() 작성. 좌표: x=폭, y=위, z=앞(+z가 머리 방향)
    3. __main__에서 build/preview/export 호출
    4. preview.png로 모양 확인 후 .obj/.mtl 두 파일을 함께 Assets/2.GameResource 모델 폴더에 넣기

규칙:
    - 원점 = 발바닥 중앙(바닥 y=0, x·z 중심). 프리팹에서 자식 모델 Position (0,0,0)으로 두면 바닥 위에 섬
    - 1복셀 = 0.055 유닛 (호랑이 기준 길이 약 1.2칸). 동물마다 크기 비교해서 조정
    - 바깥에 드러난 면만 내보내므로 면 수가 적음
"""
import numpy as np

# 색 팔레트
PAL = {
 1: ("Orange", (0.96, 0.58, 0.22)),
 2: ("Black",  (0.15, 0.12, 0.12)),
 3: ("White",  (0.98, 0.95, 0.88)),
 4: ("Pink",   (0.95, 0.55, 0.60)),
 5: ("Navy",   (0.24, 0.29, 0.42)),
}

def build_penguin():
    # 펭귄 전용 진한 색 (PAL을 덮어씀: 주황, 검정, 흰색, 분홍, 남색)
    PAL.update({1: ("Orange", (1.00, 0.52, 0.08)), 2: ("Black", (0.07, 0.06, 0.10)),
                3: ("White", (1.00, 0.98, 0.94)), 4: ("Pink", (1.00, 0.42, 0.55))})
    PAL[5] = ("Navy", (0.10, 0.16, 0.36))
    W, H, L = 11, 18, 9
    v = np.zeros((W, H, L), dtype=int)
    def box(x0,x1,y0,y1,z0,z1,c): v[x0:x1+1, y0:y1+1, z0:z1+1] = c
    def put(x,y,z,c): v[x,y,z] = c

    box(2,4,0,0,3,7,1); box(6,8,0,0,3,7,1)      # 발
    box(1,9,1,7,1,6,5)                          # 몸통 (짧게)
    for y in (1,7):
        for x in (1,9):
            for z in (1,6): put(x,y,z,0)
    box(2,8,1,6,6,6,3); box(3,7,2,5,7,7,3)      # 배
    box(0,0,3,6,2,4,5); put(0,2,3,5)            # 날개
    box(10,10,3,6,2,4,5); put(10,2,3,5)
    box(4,6,1,1,0,0,5)                          # 꼬리
    box(1,9,8,16,1,7,5)                         # 머리 (크게)
    for y in (8,16):
        for x in (1,9):
            for z in (1,7): put(x,y,z,0)
    box(1,1,16,16,1,7,0); box(9,9,16,16,1,7,0)
    put(5,17,4,5); put(5,17,5,5)                # 머리 위 삐침 털
    box(2,8,9,13,7,7,3)                         # 하트 모양 얼굴
    box(3,4,14,14,7,7,3); box(6,7,14,14,7,7,3)
    box(3,4,11,12,7,7,2); box(6,7,11,12,7,7,2)  # 큰 눈 (2×2)
    put(2,10,7,4); put(8,10,7,4)                # 볼
    box(4,6,10,10,8,8,1)                        # 부리
    return v

def build_tiger():
    # 낮고 긴 몸, 작은 머리, 날카로운 눈 + 화난 눈썹, 송곳니, 볼 갈기, 굵은 줄무늬
    PAL.clear(); PAL.update({1: ("Orange", (0.98, 0.50, 0.10)), 2: ("Black", (0.08, 0.06, 0.07)),
        3: ("White", (1.00, 0.97, 0.90)), 4: ("Eye", (0.80, 0.95, 0.20))})
    v = np.zeros((10, 14, 26), dtype=int)
    def box(x0,x1,y0,y1,z0,z1,c): v[x0:x1+1, y0:y1+1, z0:z1+1] = c
    def put(x,y,z,c): v[x,y,z] = c

    for lx in (1,7):                            # 다리 + 흰 발
        for lz in (5,16):
            box(lx,lx+1,0,3,lz,lz+2,1); box(lx,lx+1,0,0,lz,lz+2,3)
    box(1,8,4,8,4,19,1)                         # 몸통 (낮고 김)
    box(2,7,4,4,6,17,3)                         # 배
    for z in (6,9,12,15):                       # 비스듬한 줄무늬
        box(2,7,8,8,z,z,2)
        for sx in (1,8):
            put(sx,8,z,2); put(sx,7,z,2); put(sx,6,z+1,2); put(sx,5,z+1,2)
    box(2,7,6,12,19,24,1)                       # 머리
    box(1,1,6,8,20,23,3); box(8,8,6,8,20,23,3)  # 볼 갈기
    box(2,2,6,7,24,24,3); box(7,7,6,7,24,24,3)
    box(3,6,6,8,25,25,3)                        # 주둥이
    box(4,5,8,8,25,25,2)                        # 코
    put(4,6,25,2); put(5,6,25,2)                # 입
    put(3,5,25,3); put(6,5,25,3)                # 송곳니
    put(3,10,24,4); put(6,10,24,4)              # 눈
    put(4,10,24,2); put(5,10,24,2)              # 미간
    put(2,11,24,2); put(3,11,24,2); put(6,11,24,2); put(7,11,24,2)   # 화난 눈썹
    put(2,12,24,2); put(7,12,24,2)
    put(4,12,24,2); put(5,12,24,2)              # 이마 줄무늬
    box(3,6,12,12,21,21,2); box(3,6,12,12,23,23,2)   # 머리 위 줄무늬
    box(2,7,9,9,20,20,2)                        # 머리 옆 줄무늬
    box(2,2,13,13,20,21,1); put(2,13,21,2)      # 귀
    box(7,7,13,13,20,21,1); put(7,13,21,2)
    box(4,5,7,7,1,3,1); box(4,5,8,11,0,0,1)     # 꼬리
    box(4,5,9,9,0,0,2); box(4,5,7,7,2,2,2)
    box(4,5,12,12,0,1,2)                        # 꼬리 끝
    return v

def build_chicken():
    PAL.clear(); PAL.update({1: ("White", (1.00, 0.98, 0.94)), 2: ("Red", (0.95, 0.18, 0.18)),
        3: ("Yellow", (1.00, 0.74, 0.08)), 4: ("Black", (0.07, 0.06, 0.08)), 5: ("Pink", (1.00, 0.50, 0.60))})
    v = np.zeros((9, 15, 10), dtype=int)
    def box(x0,x1,y0,y1,z0,z1,c): v[x0:x1+1, y0:y1+1, z0:z1+1] = c
    def put(x,y,z,c): v[x,y,z] = c

    box(2,3,0,0,4,6,3); box(5,6,0,0,4,6,3)      # 발
    box(2,3,1,1,4,4,3); box(5,6,1,1,4,4,3)      # 다리
    box(1,7,2,7,1,7,1)                          # 몸통
    for y in (2,7):
        for x in (1,7):
            for z in (1,7): put(x,y,z,0)
    box(3,5,6,9,0,0,1); put(4,10,0,1)           # 꼬리 깃
    box(0,0,3,6,2,5,1); box(8,8,3,6,2,5,1)      # 날개
    box(2,6,8,12,4,8,1)                         # 머리
    for x in (2,6):
        for z in (4,8): put(x,12,z,0)
    box(4,4,13,13,5,7,2); put(4,14,5,2); put(4,14,7,2)   # 볏
    box(3,5,10,10,9,9,3); put(4,9,9,3)          # 부리
    put(4,8,9,2)                                # 턱밑 볏
    put(3,11,8,4); put(5,11,8,4)                # 눈
    put(2,10,8,5); put(6,10,8,5)                # 볼
    return v

def build_bull():
    # 솟은 어깨 혹, 숙인 머리, 바깥으로 휘었다 앞으로 꺾이는 큰 뿔, 빨간 눈, 짙은 적갈색
    PAL.clear(); PAL.update({1: ("Body", (0.36, 0.17, 0.11)), 2: ("Dark", (0.12, 0.08, 0.07)),
        3: ("Horn", (0.98, 0.94, 0.82)), 4: ("Muzzle", (0.62, 0.40, 0.32)), 5: ("RedEye", (1.00, 0.18, 0.10)),
        6: ("Gold", (1.00, 0.80, 0.15)), 7: ("HornTip", (0.30, 0.25, 0.22))})
    v = np.zeros((14, 17, 24), dtype=int)
    def box(x0,x1,y0,y1,z0,z1,c): v[x0:x1+1, y0:y1+1, z0:z1+1] = c
    def put(x,y,z,c): v[x,y,z] = c

    for lx in (3,9):                            # 다리 + 발굽
        for lz in (3,14):
            box(lx,lx+1,0,4,lz,lz+2,1); box(lx,lx+1,0,0,lz,lz+2,2)
    box(3,10,5,10,1,9,1)                        # 엉덩이
    box(2,11,5,13,9,17,1)                       # 어깨·가슴
    for x in (2,11):
        for z in (9,17): put(x,13,z,0)
    box(4,9,14,14,10,15,1); box(4,9,15,15,11,13,1)   # 어깨 혹
    box(6,7,8,10,0,0,1); box(6,7,5,7,0,0,2)     # 꼬리 + 술
    box(4,9,5,11,17,21,1)                       # 머리 (숙임)
    box(4,9,5,7,22,22,4)                        # 주둥이
    put(5,7,22,2); put(8,7,22,2)                # 콧구멍
    box(6,7,4,4,22,22,6); put(5,5,23,6); put(8,5,23,6); box(6,7,4,4,23,23,6)   # 코뚜레
    put(5,9,21,5); put(8,9,21,5)                # 빨간 눈
    put(4,10,21,2); put(5,10,21,2); put(8,10,21,2); put(9,10,21,2)   # 눈썹
    box(6,7,11,11,18,21,2)                      # 앞머리
    box(1,3,10,11,19,20,3); box(10,12,10,11,19,20,3)   # 뿔: 옆으로
    box(0,1,12,14,19,20,3); box(12,13,12,14,19,20,3)   # 위로
    box(0,1,15,15,20,21,3); box(12,13,15,15,20,21,3)   # 앞으로 꺾임
    box(0,0,16,16,21,22,7); box(13,13,16,16,21,22,7)   # 끝
    put(3,9,18,1); put(10,9,18,1)               # 귀
    return v

def export_obj(v, name, scale):
    W,H,L = v.shape
    cx, cz = W/2, L/2
    verts, faces = [], {k:[] for k in PAL}
    normals = [(1,0,0),(-1,0,0),(0,1,0),(0,-1,0),(0,0,1),(0,0,-1)]
    # 각 면의 네 꼭짓점 (반시계, 바깥에서 볼 때)
    quads = {
     0:[(1,0,0),(1,1,0),(1,1,1),(1,0,1)],
     1:[(0,0,1),(0,1,1),(0,1,0),(0,0,0)],
     2:[(0,1,0),(0,1,1),(1,1,1),(1,1,0)],
     3:[(0,0,0),(1,0,0),(1,0,1),(0,0,1)],
     4:[(0,0,1),(1,0,1),(1,1,1),(0,1,1)],
     5:[(1,0,0),(0,0,0),(0,1,0),(1,1,0)],
    }
    def solid(x,y,z):
        return 0<=x<W and 0<=y<H and 0<=z<L and v[x,y,z]>0
    for x in range(W):
        for y in range(H):
            for z in range(L):
                c = v[x,y,z]
                if not c: continue
                for i,(nx,ny,nz) in enumerate(normals):
                    if solid(x+nx,y+ny,z+nz): continue
                    idx=[]
                    for (a,b,d) in quads[i]:
                        verts.append(((x+a-cx)*scale,(y+b)*scale,(z+d-cz)*scale))
                        idx.append(len(verts))
                    faces[c].append((idx,i+1))
    with open(f"{name}.obj","w") as f:
        f.write(f"mtllib {name}.mtl\no {name}\n")
        for p in verts: f.write("v %.4f %.4f %.4f\n"%p)
        for n in normals: f.write("vn %d %d %d\n"%n)
        for c,fl in faces.items():
            if not fl: continue
            f.write(f"usemtl {name}_{PAL[c][0]}\n")
            for idx,n in fl:
                f.write("f "+" ".join(f"{i}//{n}" for i in idx)+"\n")
    with open(f"{name}.mtl","w") as f:
        for c,(nm,col) in PAL.items():
            f.write(f"newmtl {name}_{nm}\nKa 0 0 0\nKd %.3f %.3f %.3f\nKs 0 0 0\nd 1\nillum 1\n\n"%col)
    return sum(len(x) for x in faces.values())

def preview(v, path):
    import matplotlib; matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    W,H,L = v.shape
    filled = v>0
    colors = np.empty(v.shape, dtype=object)
    for k,(n,c) in PAL.items(): colors[v==k] = matplotlib.colors.to_hex(c)
    # matplotlib 축: (x, z, y)로 바꿔서 y를 위로
    f2 = np.transpose(filled,(0,2,1)); c2 = np.transpose(colors,(0,2,1))
    fig = plt.figure(figsize=(12,4))
    for i,(el,az) in enumerate([(20,60),(15,100),(25,200)]):
        ax = fig.add_subplot(1,3,i+1, projection='3d')
        ax.voxels(f2, facecolors=c2, edgecolor=(0,0,0,0.15))
        ax.set_box_aspect((W,L,H)); ax.view_init(el,az); ax.axis('off')
    plt.tight_layout(); plt.savefig(path, dpi=90)

if __name__=="__main__":
    import sys
    name = sys.argv[1] if len(sys.argv) > 1 else "Tiger"     # python voxel_model.py Penguin
    v = {"Tiger": build_tiger, "Penguin": build_penguin,
         "Chicken": build_chicken, "Bull": build_bull}[name]()
    preview(v,"preview.png")
    print("faces", export_obj(v,name,0.055))
