﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdactionShot : MonoBehaviour
{
    [Header("カメラGUIの表示")]
    public bool gui_on;

    public GameObject enemy;
    public GameObject parent;


    public float scale_spd = 2.0f;      // 大きくなる速度
    public float scale_multi = 1.8f;    // 初めに大きくなる速度を何倍するか

    private bool shot_hit_fg;           // ショットの当たり判定保管用

    private int state;                  // 大きくするステート切り替え
    private int prodaction_state;       // 演出切り替え

    private const float SCALE_MAX = 5.5f;   // 親との大きさの差
    private const float SCALE_MIN = 5.0f;   // 親との大きさの差

    // ステートの定数
    private const int FIRST_PRODACTION = 0;
    private const int SECOND_PRODACTION = 1;
    private const int SCALE_UP = 0;
    private const int SCALE_DOWN = 1;
    private const int PARENT = 1;


    // Start is called before the first frame update
    void Start()
    {
        state = SCALE_UP;
        prodaction_state = FIRST_PRODACTION;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 親の位置に移動
        transform.position = enemy.transform.position;

        if (enemy.GetComponent<Enemy>().ShotTouchFlg) shot_hit_fg = true;

        if (shot_hit_fg)
        {
            gameObject.SetActive(true);
            Prodaction();
        }
    }

    void Prodaction()
    {
        switch (prodaction_state)
        {
            case FIRST_PRODACTION:
                ProdactionFirst();
                break;
            case SECOND_PRODACTION:
                // 当たってから親のステートが1(小さくする)になるまで処理が呼ばれる
                if (StateCheck()) ProdactionSecond();
                break;
        }
    }

    void ProdactionFirst()
    {
        ScaleChenge(scale_spd * scale_multi);
        if (ScaleLimitMax())
        {
            prodaction_state = SECOND_PRODACTION;
        }
    }

    void ProdactionSecond()
    {
        switch (state)
        {
            case SCALE_UP:
                ScaleChenge(scale_spd);
                if (ScaleLimitMax()) state = SCALE_DOWN;
                break;
            case SCALE_DOWN:
                ScaleChenge(-scale_spd);
                if (ScaleLimitMin()) state = SCALE_UP;
                break;
        }
    }

    void ScaleChenge(float spd)
    {
        transform.localScale = new Vector3(
            transform.localScale.x + spd * Time.deltaTime,
            transform.localScale.y + spd * Time.deltaTime,
            transform.localScale.z + spd * Time.deltaTime);
    }

    // 大きさの制限
    bool ScaleLimitMax()
    {
        if (transform.localScale.x - enemy.transform.localScale.x > SCALE_MAX)
        {
            return true;
        }
        return false;
    }

    bool ScaleLimitMin()
    {
        if (transform.localScale.x - enemy.transform.localScale.x < SCALE_MIN)
        {
            return true;
        }
        return false;
    }

    // ステートが1じゃ無いときにtrue
    bool StateCheck()
    {
        int parent_state = parent.GetComponent<EnemyProdaction>().State;
        if (parent_state == PARENT) return false;
        return true;
    }

    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box");

            //uGUIスクロールビュー用
            Vector2 leftScrollPos = Vector2.zero;

            // スクロールビュー
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));
            GUILayout.Box("Camera");


            #region ここに追加

            //GUILayout.TextArea("state\n" + state);
            //GUILayout.TextArea("StateCheck()\n" + StateCheck());
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     

            // スペース
            GUILayout.Space(200);
            GUILayout.Space(10);
            #endregion


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }


}
