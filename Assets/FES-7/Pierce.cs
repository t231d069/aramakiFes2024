using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // 刺突属性による結合の開始
    public int Connect(Container container)
    {
        // コンテナの子オブジェクトにされるrigidbodyの破棄
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Destroy(rigidbody);

        // 自身の親をBreaker.containerにする
<<<<<<< HEAD
        this.gameObject.transform.SetParent(breaker.GetContainer().gameObject.transform);

        // Containerクラスの登録オブジェクトを自身にする
        GameObject container = breaker.GetContainer().gameObject;
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   

        // 回復する耐久値を返す
        return durabilityRecoveryAmount; 
=======
        this.gameObject.transform.SetParent(container.gameObject.transform);

        // Containerクラスの登録オブジェクトを自身にする
        container.SetRegisteredObject(this.gameObject);

        // 回復する耐久値を返す
        return durabilityRecoveryAmount;
>>>>>>> FES-7-遯∝ｱ樊�ｧ縺ｫ繧医ｋ繧ｪ繝悶ず繧ｧ繧ｯ繝医�ｮ遐ｴ螢雁�ｦ逅�
    }

    // 結合する座標の設定
    private void DecideConnectPosition()
    {

    }
}