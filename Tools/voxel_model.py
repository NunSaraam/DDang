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
    W, H, L = 10, 16, 23
    v = np.zeros((W, H, L), dtype=int)   # x, y(up), z(front = +z)
    OZ = 3                                # 꼬리용 z 오프셋
    def box(x0,x1,y0,y1,z0,z1,c):
        v[x0:x1+1, y0:y1+1, z0+OZ:z1+OZ+1] = c
    def put(x,y,z,c): v[x,y,z+OZ] = c

    # 다리
    for lx in (1,7):
        for lz in (1,9):
            box(lx,lx+1,0,2,lz,lz+1,1)
            box(lx,lx+1,0,0,lz,lz+1,3)          # 하얀 발
    # 몸통
    box(1,8,3,7,0,11,1)
    box(3,6,3,3,2,9,3)                          # 배
    # 몸통 줄무늬 (위 + 옆)
    for z in (1,4,7):
        box(1,8,7,7,z,z,2)
        box(1,1,5,7,z,z,2); box(8,8,5,7,z,z,2)
    # 머리
    box(0,9,5,12,10,17,1)
    box(1,8,5,6,17,17,3)                        # 아래 얼굴 흰색
    box(0,0,5,6,11,17,3); box(9,9,5,6,11,17,3)  # 볼
    # 주둥이
    box(3,6,5,7,18,18,3)
    box(4,5,7,7,18,18,4)                        # 코
    box(4,5,5,5,18,18,2)                        # 입
    # 눈
    for ex in (2,7):
        box(ex,ex,9,10,17,17,2)
        box(ex+(1 if ex==2 else -1),ex+(1 if ex==2 else -1),10,10,17,17,3)   # 눈 하이라이트
    # 이마 줄무늬
    box(4,5,11,12,17,17,2)
    # 머리 위/옆 줄무늬
    for z in (12,15):
        box(2,7,12,12,z,z,2)
        box(0,0,8,10,z,z,2); box(9,9,8,10,z,z,2)
    # 귀
    for ex in (1,7):
        box(ex,ex+1,13,14,13,14,1)
        box(ex,ex+1,13,13,14,14,3)
        put(ex if ex==1 else ex+1,14,13,2)
    # 꼬리
    box(4,5,6,6,-1,-1,1)
    box(4,5,6,6,-2,-2,2)
    box(4,5,6,9,-3,-3,1)
    box(4,5,8,8,-3,-3,2)
    box(4,5,10,11,-3,-3,2)
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
    v = {"Tiger": build_tiger, "Penguin": build_penguin}[name]()
    preview(v,"preview.png")
    print("faces", export_obj(v,name,0.055))
