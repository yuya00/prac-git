﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
	private EnemyNear           enemy_near;
	private EnemySoundDetect    enemy_sounddetect;
	private GameObject          player_obj;

	private bool				player_touch_flg;
	private bool				shot_touch_flg;
	private Vector3				dist_to_player;
	private const int           FAINT_TIME	 = 180;     //気絶時間
	private int                 spin_timer = 0;
	private int                 SPIN_INTERVAL_TIME = 90;
	private const float			FALL_Y_MAX       = -100.0f; // 最大落下地点(リスポーン用)

	private float easing_timer = 0;

    private SoundManager sound;

    // キャラ指定
    private SoundManager.CHARA_TYPE ENEMY_SE = SoundManager.CHARA_TYPE.ENEMY;

    // 音の種類指定
    private SoundManager.SE_TYPE FIND_SE = SoundManager.SE_TYPE.ENEMY_FIND;
    private SoundManager.SE_TYPE SHOT_SE = SoundManager.SE_TYPE.ENEMY_SHOT;


    //壁判定Ray ---------------------------------------------
    [System.Serializable]
	public class WallRay : BoxCastAdjustBase {
		//public float length;		//20.0f
		//public float up_limit;	//1.9f	2.7f
		//public float down_limit;	//3.0f	2.5f

		public const int ANGLE_MAG = 3; //角度調整

		[Header("Rayの角度")]	//0.0f 未使用
		public float angle;

		[System.NonSerialized]	//壁との距離保存用
		public float dist_right, dist_left;

		[System.NonSerialized]	//壁との当たり判定
		public bool hit_right_flg, hit_left_flg;

		[System.NonSerialized]	//めり込み判定
		public bool cavein_right_flg, cavein_left_flg;

		[System.NonSerialized]	//両方のRayが当たった回数
		public int both_count;

		[System.NonSerialized]	//両方のRayが当たった回数判定
		public bool both_flg;

		[Header("向き変更の速さ")]
		public float spd;       //1.5f

		public void Clear() {
			dist_right = 0;
			dist_left = 0;
			hit_right_flg = false;
			hit_left_flg = false;
			cavein_right_flg = false;
			cavein_left_flg = false;
			both_count = 0;
			both_flg = false;
		}

	}
	[Header("壁判定Ray")]
	public WallRay wall_ray;


	//穴判定Ray ---------------------------------------------
	[System.Serializable]
	public class HoleRay : RayBase {
		//public float length;    //100.0f

		[Header("Rayの始点")]		//11.0f
		public float startLength;

		[Header("Rayの角度")]		//00.0f 未使用
		public float angle;

		//[System.NonSerialized] //穴との距離保存用
		//public float dist_right, dist_left;

		[System.NonSerialized]		//穴との当たり判定
		public bool hit_right_flg, hit_left_flg;

		//[System.NonSerialized] //両方のRayが当たった回数
		//public int both_count;

		//[System.NonSerialized] //両方のRayが当たった回数判定
		//public bool both_flg;

		[Header("向き変更の速さ")]	//15.0f
		public float speed;

		public void Clear() {
			hit_right_flg = false;
			hit_left_flg = false;
		}

	}
	[Header("穴判定Ray")]
	public HoleRay hole_ray;


	//状態エフェクト -----------------------------------
	[System.Serializable]
	public struct ConditionEffect {

		//[System.NonSerialized]
		//public const int MAX = 5;

		public  GameObject warning;
		public  GameObject find;
		public  GameObject away;
		public  GameObject faint;
		[System.NonSerialized]
		public  GameObject obj_attach;

		[System.NonSerialized]
		public  GameObject[] obj_attach2;


		[System.NonSerialized]
		public GameObject obj_entity;

		[System.NonSerialized]
		public GameObject[] obj_entity2;


		//[System.NonSerialized]
		//public GameObject[] obj_entity;
	}
	[Header("状態エフェクト")]
	public ConditionEffect condition_eff;


	//待機 ---------------------------------------------
	[System.Serializable]
	public struct WaitAct {
		[Header("首振り前待機")]
		public int wait_time;			//200
		[Header("首振り前待機ランダム幅")]
		public int wait_random;			//0
		[Header("首振り時間")]
		public int swing_time;			//50
		[Header("首振り時間ランダム幅")]
		public int swing_random;		//0
		[Header("首振り速さ")]
		public int swing_spd;			//80
		[Header("首振り間の間隔")]
		public int swing_space_time;    //15

		//首振りの行動種類
		public enum Enum_Swing {
			SWING,  //首振り
			WAIT    //待機
		}
		[System.NonSerialized]
		public Enum_Swing enum_swing;
	}
	[Header("待機行動")]
	public WaitAct wait_act;

	//首振りランダム値設定(待機行動) -------------------
	private struct OnceRondom {
		public int   num;
		public bool  isfinish;
	}
	private OnceRondom once_random;


	//警戒 ---------------------------------------------
	[System.Serializable]
	public struct WarningAct {
		/*
		[SerializeField, Header("首振り速さ")]
		public int swing_spd;           //100
		[SerializeField, Header("首振り時間")]
		public int swing_time;          //10
		[SerializeField, Header("首振り間の間隔")]
		public int swing_space_time;    //15
		// */
	}
	[Header("警戒行動")]
	public WarningAct warning_act;


	//逃走 ---------------------------------------------
	[System.Serializable]
	public struct AwayAct {
		//音探知範囲*mag分離れたら止まる
		public const float MAG = 2.0f;

		[Header("振り向く間隔")]
		public int lookback_interval;

		[Header("振り向いている時間")]
		public int lookback_time;

		//振り向きstate
		public enum Enum_LookBack {
			NORMAL,
			LOOKBACK
		}
		[System.NonSerialized]
		public Enum_LookBack enum_lookback;

		//敵行動の種類 -----------------------------
		//クラスの外に出した方がいい？
		[System.Serializable]
		public struct Kind {

			//インスペクター用
			[Header("どれか一つだけチェック")]
			public bool normal;
			public bool curve;
			public bool jump;
			public bool zigzag;
			[Space(8)]
			public bool armar;
			public bool shot;
			public bool spin;

			//前の状態保存
			[System.NonSerialized]
			public bool normal_front;

			[System.NonSerialized]
			public bool curve_front;

			[System.NonSerialized]
			public bool jump_front;

		}
		[Header("種類")]
		public Kind kind;

		//逃走通常、カーブ
		[System.Serializable]
		public struct Curve {

			[System.NonSerialized]　		//符号反転用
			public float        one;

			[Header("向き切り替え時間")]	//120
			public int          interval;

			[Header("曲がる速さ")]			//0.04  0.5
			public float        spd;
		}
		public Curve normal;
		public Curve curve;

		//逃走ジャンプ
		[System.Serializable]
		public struct Jump {

			[System.NonSerialized]
			public bool flg;

			[Header("ジャンプ力")]			//22
			public int	power;

			[Header("切り替え時間")]		//20
			public int	time;
		}
		public Jump jump;

	}
	[Header("逃走行動")]
	public AwayAct away_act;


	//反撃ショット --------------------------------------
	[System.Serializable]
	public struct BreakShotAct {

		[System.NonSerialized]
		public bool flg;

		public GameObject obj;

		[System.NonSerialized]
		public int serve_state;

		//ショットの位置調整
		public const int MAG = 5;

		[Header("ショット前待機")]	//10
		public int front_time;

		[Header("ショット後待機")]	//30
		public int back_time;
	}
	[Header("反撃ショット行動")]
	public BreakShotAct breakshot_act;


	//ジャンプ判定Ray ----------------------------------
	[System.Serializable]
	public class JumpRay : BoxCastAdjustBase {
		//public float length;            //4.3f
		//public float uplimit_height;    //2.0f
		//public float downlimit_height;  //3.9f

		[System.NonSerialized]			//壁との当たり判定
		public bool flg;

		[Header("ジャンプ力")]
		public float power;				//22.0f

		[Header("事前判定の長さ")]
		public float advance_length;    //23.0f

		[System.NonSerialized]			//壁との事前当たり判定
		public bool advance_flg;
	}
	[Header("ジャンプRay")]
	public JumpRay jump_ray;


	//崖ジャンプRay ---------------------------------------------
	[System.Serializable]
	public class CliffJumpRay : RayBase {
		//public float length;		//12

		[Header("Rayの始点")]		//1.5
		public float startLength;

		[Header("ジャンプ力")]		//12
		public float power;

	}
	[Header("崖ジャンプRay")]
	public CliffJumpRay cliffjump_ray;


	//気絶して目が回る ---------------------------------------------
	[System.Serializable]
	private struct FaintGiddy {

		[System.NonSerialized] //方向保存
		public Vector3 forward,upward;

		//半径
		public const float RADIUS_SIN = 1.5f;
		public const float RADIUS_COS = 1.8f;

		//回転の速さ
		public const float ROTATE_SPD = 5.0f;

	}
	private FaintGiddy faint_giddy;



	[Header("ショットへの耐久度"),SerializeField]
	private int shot_to_defense = 3;
	//当たったショットの強さ保存
	private int shot_scale_power;





	//敵モデルの種類
	enum EnumModel {
		STILL,  //幼体
		GROWN   //成体
	}
	EnumModel enum_model;


	//状態の種類
	enum Enum_State{
		WAIT,     //待機
		WARNING,  //警戒
		FIND,     //発見(回転)
		AWAY,     //逃走
		BREAK,    //ショット破壊
		FAINT,    //気絶
		WRAP,     //捕獲
		END       //消去
	}
	Enum_State enum_state;
	Enum_State old_state;


	//状態内の行動種類
	public enum Enum_Act {
		CLEAR,		//初期化
		WAIT,		//待機
		SWING,		//首振り
		SWING2,		//首振り2
		SWING3,		//首振り3
		WARNING1,	//警戒1
		WARNING2,	//警戒2
		RUN,		//走る
		JUMP,       //ジャンプ
		SPIN,		//回転
		BREAK,		//破壊
		FAINT,      //気絶
		END         //終了
	}
	Enum_Act enum_act;
	Enum_Act old_act;



	//敵別の行動(逃走ベース、反撃など)
	//逃走ベースの種類
	enum Enum_AwayKind {
		NORMAL,
		CURVE,
		JUMP,
		ZIGZAG,

		ARMAR,
		SHOT,
		SPIN
	}
	Enum_AwayKind enum_awaykind;





	//汎用タイマーの種類
	enum Enum_Timer {
		WAIT,           //待機
		WAIT_SWING,     //待機首振り
		WARNIG,			//警戒
		LOOKBACK,       //振り向き
		EACH_ACT,       //敵別行動
		FAINT           //気絶
	}
	Enum_Timer enum_timer;


}