using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Slash : MonoBehaviour
{
    [SerializeField, Tooltip("残り切断可能回数")]
    private int _numberOfCanSlash = 0;
    [SerializeField, Tooltip("切断された後のオブジェクト")]
    private GameObject _cutObjectPrefab;
    [SerializeField, Tooltip("切断面用のマテリアル（任意）")]
    private Material _cutSurfaceMaterial; 

    // オブジェクト切断時に呼び出すイベント登録
    public UnityEvent onSlashEvent; 
    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;

    // アクセサ
    public void SetNumberOfCanSlash(int vlaue)
    {
        _numberOfCanSlash = vlaue;
    }
    public void SetCutObjectPrefab(GameObject prefab)
    {
        _cutObjectPrefab = prefab;
    }

    /// <summary>
    /// カッター（切断する平面）を作成する
    /// </summary>
    /// <param name="worldPoint">平面の座標</param>
    private Plane CalcCutterPlane(Vector3 worldPoint, Vector3 moveDirection)
    {
        // カッターの法線ベクトルをワールド空間で計算
        Vector3 worldNormal = Vector3.Cross(transform.forward.normalized, moveDirection).normalized;
        Debug.DrawRay(worldPoint, worldNormal, Color.green, 2, false);
        // 平面の距離を計算：平面の法線ベクトルからワールド空間の任意の点への距離
        float worldDistance = Vector3.Dot(worldNormal, worldPoint);
        // 断面を相手のワールド座標で設定
        return new Plane(worldNormal, worldDistance);
    }

    /// <summary>
    /// 切断クラスの呼び出し時にはじめに呼び出され、ActSubdivideに切断させる
    /// </summary>
    /// <param name="breaker">攻撃した側の情報</param>
    /// <returns></returns>
    public void CallSlash(Breaker breaker)
    {
        if (_numberOfCanSlash <= 0)
        {
            Destroy(this.gameObject);
            // 破壊時のイベントを呼び出す
            onBreakEvent?.Invoke();
            return;
        }
        // 切断された後のオブジェクトに割り当てるマテリアルの作成
        Material[] materials = this.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
        Material[] newMaterials;
        // 断面のマテリアルを持っていない場合、追加する
        bool canAddCutSurfaceMaterial = false;
        if (_cutSurfaceMaterial != null)
        {
            if(materials[materials.Length-1].name != _cutSurfaceMaterial.name)
            {
                canAddCutSurfaceMaterial = true;
            }
        }
        if (canAddCutSurfaceMaterial)
        {
            newMaterials = new Material[materials.Length + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[materials.Length] = _cutSurfaceMaterial;
        }
        else
        {
            newMaterials = materials;
        }

        // メッシュを計算に必要な参照を取得
        Transform transform = this.gameObject.transform;
        Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        Plane cutter = CalcCutterPlane(breaker.GetContactPoint(), breaker.GetMoveDirection());
        // 切断された後のオブジェクトに割り当てるメッシュを計算する。
        (Mesh rightMesh, Mesh leftMesh) = ActSubdivide4.Subdivide(mesh, transform, cutter, canAddCutSurfaceMaterial);

        // 失敗
        if (rightMesh == null || leftMesh == null)
        {
            Debug.Log("メッシュの計算ができませんでした。");
            onBreakEvent?.Invoke();
            return;
        }
        // 生成したオブジェクトと干渉しないようにColliderを無効化
        this.gameObject.GetComponent<Collider>().enabled = false;
        // 切断された後のオブジェクトを生成する
        if (rightMesh != null)
        {
            CreateCutObject(transform, rightMesh, newMaterials);
        }
        if (leftMesh != null)
        {
            CreateCutObject(transform, leftMesh, newMaterials);
        }
        // 切られた元のオブジェクトを破棄する
        Destroy(this.gameObject);
        
        // 切断時のイベントを呼び出す
        onSlashEvent?.Invoke();

    }

    /// <summary>
    /// 切断された後のオブジェクトを生成する
    /// </summary>
    /// <param name="originTransform">元オブジェクトのTransform</param>
    /// <param name="newMesh">作成したメッシュ</param>
    /// <param name="newMaterials">割り当てるマテリアル</param>
    /// <returns></returns>
    public void CreateCutObject(Transform originTransform, Mesh newMesh, Material[] newMaterials)
    {
        GameObject polygonInfo_subject = Instantiate(_cutObjectPrefab, originTransform.position, originTransform.rotation, null);
        // Scaleの設定
        polygonInfo_subject.transform.localScale = originTransform.localScale;
        // Meshの設定
        polygonInfo_subject.GetComponent<MeshFilter>().mesh = newMesh;
        // マテリアルの設定
        polygonInfo_subject.GetComponent<MeshRenderer>().sharedMaterials = newMaterials;
        // MeshColliderの設定
        MeshCollider meshCollider = polygonInfo_subject.GetComponent<MeshCollider>();
        if(meshCollider)
        {
            meshCollider.sharedMesh = newMesh;
        }
        // Slashの再設定
        Slash slash = polygonInfo_subject.GetComponent<Slash>();
        slash?.SetNumberOfCanSlash(_numberOfCanSlash-1);
        slash?.SetCutObjectPrefab(_cutObjectPrefab);
    }
}
