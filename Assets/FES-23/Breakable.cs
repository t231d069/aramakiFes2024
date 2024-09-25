using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Type { plane, slash, crash, pierce }

public class Breakable : MonoBehaviour
{
	[SerializeField, Tooltip("耐久値")]
	private int durability = default;
	[Header("属性耐性")]
	[SerializeField, Tooltip("切断耐性")]
	private int slashResist = default;
	[SerializeField, Tooltip("衝撃耐性"),]
	private int crashResist = default;
	[SerializeField, Tooltip("貫通耐性"),]
	private int pierceResist = default;
	[SerializeField, Tooltip("スコア")]
	private int score = default;

    // 属性耐性の辞書
    private Dictionary<Type, int> resists = new Dictionary<Type, int>();
    // 結合しているときの結合相手のBreakerクラス
    // private Breaker connectedMainObj = null;

    private void Start()
    {
		resists.Add(Type.slash, slashResist);
		resists.Add(Type.crash, crashResist);
		resists.Add(Type.pierce, pierceResist);
    }

    /// <summary>
    /// 与えられた攻撃力と属性、自身の耐性、最終的なダメージの値を計算する。
    /// </summary>
    /// <param name="receivedATK">受ける攻撃力</param>
    /// <param name="attackType">受ける攻撃の属性</param>
    /// <returns></returns>
    private int CalcDamage(int receivedATK, Type attackType)
	{
		int damage = receivedATK - resists[attackType];
		if (damage < 0) damage = 0;
        return damage;
	}

}
