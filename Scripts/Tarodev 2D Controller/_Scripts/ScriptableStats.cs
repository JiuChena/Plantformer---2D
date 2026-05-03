using UnityEngine;
using UnityEngine.Serialization;

namespace TarodevController
{
    [CreateAssetMenu]
    public class ScriptableStats : ScriptableObject
    {
        [Header("层级设置")] 
        [Tooltip("设置玩家角色所在的物理层")]
        public LayerMask PlayerLayer;
        
        [Header("输入设置")]
        [Tooltip("是否允许读取玩家输入。设为 false 时角色将忽略所有输入（移动、跳跃、冲刺等），可用于过场动画或对话期间禁用操控")]
        public bool InputEnable = true;
        
        [Tooltip("使所有输入值 snapped 到整数。防止手柄慢走。建议设为 true 以确保手柄和键盘的输入一致性。")]
        public bool SnapInput = true;
        
        [Tooltip("爬上梯子或 ledge 所需的最小输入值。避免使用手柄时意外攀爬。取值范围 0.01-0.99"), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;
        
        [Tooltip("识别左右移动所需的最小输入值。避免粘性手柄导致的漂移。取值范围 0.01-0.99"), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;
        
        [Header("移动设置")] 
        [Tooltip("水平移动的最大速度（单位：单位/秒）。控制角色在地面和空中的最高移动速度")]
        public float MaxSpeed = 14;
        
        [Tooltip("水平加速度（单位：单位/秒²）。数值越大，角色加速到最大速度越快")]
        public float Acceleration = 120;
        
        [Tooltip("地面减速度（单位：单位/秒²）。角色在地面停止输入时的减速速率，数值越大停得越快")]
        public float GroundDeceleration = 60;
        
        [Tooltip("空中减速度（单位：单位/秒²）。角色在空中停止输入时的减速速率，通常比地面小以保持空中惯性")]
        public float AirDeceleration = 30;
        
        [Tooltip("恒定向下的地面吸附力（单位：单位/秒）。取值范围 0 到 -10。帮助角色在斜坡上保持稳定，防止滑动")]
        public float GroundingForce = -1.5f;
        
        [Tooltip("地面和天花板检测的距离（单位：单位）。取值范围 0-0.5。数值过大会导致悬空检测为着地，过小会导致检测不稳定")]
        public float GrounderDistance = 0.05f;

        [Header("坡度限制")]
        [Tooltip("角色可以正常站立和上坡的最大坡度（单位：度）。超过这个角度会自动下滑，并且不能继续上坡"), Range(0f, 89f)]
        public float MaxWalkableSlopeAngle = 45f;

        [Tooltip("站在超过最大坡度的斜面上时，沿斜面向下滑落的目标速度（单位：单位/秒）")]
        public float SteepSlopeSlideSpeed = 10f;

        [Tooltip("站在超过最大坡度的斜面上时，向下滑落的加速度（单位：单位/秒²）")]
        public float SteepSlopeSlideAcceleration = 60f;

        [Tooltip("用于计算坡面平均法线的左右脚采样间距比例。1 表示接近碰撞体最左右两侧"), Range(0.1f, 1f)]
        public float SlopeSensorSpread = 0.8f;

        [Tooltip("坡面采样射线起点距离角色中心的向上偏移（单位：单位）。用于让射线从角色身体内部向下发射")]
        public float SlopeSensorStartHeight = 0.1f;

        [Tooltip("坡面采样射线长度（单位：单位）。需要略大于角色脚底到地面的距离")]
        public float SlopeSensorDistance = 1f;

        [Tooltip("角色本体和碰撞体对齐坡面的旋转速度（单位：度/秒）")]
        public float SlopeAlignmentSpeed = 720f;

        [Tooltip("角色前方探测陡坡的水平距离（单位：单位）。在此范围内如果检测到超过最大可行走坡度的地形，将禁用角色的坡面旋转对齐")]
        public float SteepSlopeAheadDetectionRange = 1f;
        
        [Header("跳跃设置")] 
        [Tooltip("跳跃时立即施加的垂直速度（单位：单位/秒）。数值越大跳得越高")]
        public float JumpPower = 36;
        
        [Tooltip("垂直下落的最大速度（单位：单位/秒）。限制角色下落的最快速度，防止穿过平台")]
        public float MaxFallSpeed = 40;
        
        [Tooltip("空中重力加速度（单位：单位/秒²）。控制角色在空中下落时的加速速率，数值越大下落越快")]
        public float FallAcceleration = 110;
        
        [Tooltip("提前松开跳跃键时的重力倍数。松开跳跃键后下落速度会乘以这个倍数，实现短跳效果")]
        public float JumpEndEarlyGravityModifier = 3;
        
        [Tooltip("Coyote Time 的持续时间（单位：秒）。离开平台边缘后仍可跳跃的宽容时间，让跳跃手感更友好")]
        public float CoyoteTime = .15f;
        
        [Tooltip("跳跃缓冲时间（单位：秒）。在接触地面前输入跳跃会被缓冲，落地后立即起跳，提升操作流畅度")]
        public float JumpBuffer = .2f;

        [Tooltip("触发落地音效所需的最小下落距离（单位：单位）。下落距离低于此值时不会播放落地音效，适合过滤掉走下小台阶等场景的音效")]
        public float MinLandingSoundFallDistance = 0.05f;
        
        [Header("空中选项")]
        [Tooltip("是否允许空中跳跃（二段跳）。设为 false 时玩家只能在地面跳跃，AirJumps 参数将被忽略")]
        public bool AllowAirJump = true;
        
        [Tooltip("落地前可用的额外跳跃次数。设为 0 表示只能在地面跳跃，1 表示可以二段跳，以此类推")]
        public int AirJumps = 1;
        
        [Header("墙壁交互")]
        [Tooltip("是否启用墙壁交互功能（墙壁滑行和墙壁跳跃）。设为 false 时玩家无法进行任何墙壁交互")]
        public bool AllowWallInteraction = true;
        
        [Tooltip("墙壁检测距离（单位：单位）。从角色边缘向外检测墙壁的距离，用于墙壁滑行和墙壁跳跃")]
        public float WallDetectionDistance = 0.05f;
        
        [Tooltip("沿墙壁滑行的最大下落速度（单位：单位/秒）。数值越小滑行越慢")]
        public float WallSlideSpeed = 5f;
        
        [Tooltip("松开墙壁方向键后墙壁滑行的保持时间（单位：秒）。提供操作宽容度，避免松开瞬间失去抓墙")]
        public float WallStickTime = 0.15f;
        
        [Tooltip("墙壁跳跃时的垂直速度（单位：单位/秒）。控制从墙壁跳起的高度")]
        public float WallJumpPower = 30f;
        
        [Tooltip("墙壁跳跃时的水平速度（单位：单位/秒）。控制从墙壁跳离的距离")]
        public float WallJumpHorizontalPower = 18f;
        
        [Tooltip("墙壁跳跃后水平输入锁定的时间（单位：秒）。防止墙壁跳跃后立即反向输入导致操作失误")]
        public float WallJumpControlLockTime = 0.12f;
        
        [Header("冲刺设置")]
        [Tooltip("是否允许在地面冲刺。设为 false 时只能空中冲刺")]
        public bool AllowGroundDash = false;
        
        [Tooltip("冲刺持续时间（单位：秒）。冲刺效果维持的时间长度")]
        public float DashDuration = 0.12f;
        
        [Tooltip("冲刺移动速度（单位：单位/秒）。冲刺期间角色的移动速度")]
        public float DashSpeed = 24f;
        
        [Tooltip("两次冲刺之间的最小冷却时间（单位：秒）。防止连续频繁使用冲刺")]
        public float DashCooldown = 0.2f;

        [Tooltip("冲刺残影的生成间隔（单位：秒）。值越小残影越密集，建议范围 0.02-0.06")]
        [Range(0.01f, 0.2f)] public float DashAfterImageInterval = 0.03f;

        [Tooltip("冲刺残影的存在时间（单位：秒）。残影从生成到完全消失的持续时间")]
        [Range(0.05f, 1f)] public float DashAfterImageLifetime = 0.4f;

        [Tooltip("冲刺残影的初始透明度。值越大残影越明显，范围 0-1")]
        [Range(0f, 1f)] public float DashAfterImageInitialAlpha = 0.6f;

        [Tooltip("冲刺残影的颜色。残影会以该颜色叠加初始透明度进行显示")]
        public Color DashAfterImageColor = new Color(0.5f, 0.8f, 1f, 1f);
    }
}
