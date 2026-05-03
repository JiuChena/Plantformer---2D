using System;
using System.Collections.Generic;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        public ScriptableStats _stats;
        private ScriptableStats _originalStats;
        private Rigidbody2D _rb;
        public CapsuleCollider2D _col;
        
        private Dictionary<string, HoldablePropBase> _holdableProps = new Dictionary<string, HoldablePropBase>();
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        private MoveGround _currentPlatform;
        private int _facing = 1;
        private int _wallDirection;
        private float _wallStickTimeLeft;
        private float _wallJumpLockTimeLeft;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public Vector2 GroundNormal => _groundNormal;
        public float GroundAngle => _groundSignedAngle;
        public bool Grounded => _grounded;
        public float LastFallDistance => _lastFallDistance;
        public ScriptableStats Stats => _stats;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action AirJumped;
        public event Action Dashed;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            // 运行时克隆 ScriptableStats，保护原始资产不被修改
            if (_stats != null)
            {
                _originalStats = _stats;
                _stats = Instantiate(_originalStats);
            }
        }

        private void OnDestroy()
        {
            // 恢复原始 ScriptableStats 引用并销毁运行时副本
            if (_originalStats != null)
            {
                Destroy(_stats);
                _stats = _originalStats;
            }
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            if (!_stats.InputEnable)
            {
                _frameInput = default;
                return;
            }

            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                DashDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (_frameInput.DashDown) _dashToConsume = true;
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            DashCoolDownHandle();
            
            HandleDash();
            if (_dashing)
            {
                HandleBodyRotation();
                ApplyMovement();
                return;
            }

            HandleWalls();
            HandleJump();
            HandleDirection();
            HandleGravity();
            HandleBodyRotation();
            
            ApplyMovement();
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;
        private RaycastHit2D _groundHit;
        private Vector2 _groundNormal = Vector2.up;
        private Vector2 _downhillDirection;
        private float _groundAngle;
        private float _groundSignedAngle;
        private bool _onSteepSlope;
        private bool _wallLeftHit;
        private bool _wallRightHit;
        private bool _wallSliding;
        private bool _steepSlopeAhead;
        private float _fallStartY;
        private float _lastFallDistance;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            var castAngle = _rb.rotation;
            _groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, castAngle, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool groundHit = _groundHit.collider != null && !_groundHit.collider.isTrigger;
            var ceilingHitResult = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, castAngle, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = ceilingHitResult.collider != null && !ceilingHitResult.collider.isTrigger;
            var wallLeftResult = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, castAngle, Vector2.left, _stats.WallDetectionDistance, ~_stats.PlayerLayer);
            _wallLeftHit = wallLeftResult.collider != null && !wallLeftResult.collider.isTrigger;
            var wallRightResult = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, castAngle, Vector2.right, _stats.WallDetectionDistance, ~_stats.PlayerLayer);
            _wallRightHit = wallRightResult.collider != null && !wallRightResult.collider.isTrigger;
            _wallDirection = _wallLeftHit ? -1 : _wallRightHit ? 1 : 0;

            UpdateGroundInfo(groundHit);
            CheckSteepSlopeAhead();

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                _airJumpsRemaining = _stats.AirJumps;
                _dashUsable = true;
                _wallSliding = false;
                _wallStickTimeLeft = 0f;
                _lastFallDistance = Mathf.Max(0f, _fallStartY - transform.position.y);
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                _fallStartY = transform.position.y;
                GroundedChanged?.Invoke(false, 0);
            }

            // 检测是否站在可移动地面上，读取平台速度供ApplyMovement使用
            if (_grounded && _groundHit.collider != null)
                _currentPlatform = _groundHit.collider.GetComponentInParent<MoveGround>();
            else
                _currentPlatform = null;

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        private void UpdateGroundInfo(bool groundHit)
        {
            if (!groundHit)
            {
                _groundNormal = Vector2.up;
                _groundSignedAngle = 0f;
                _groundAngle = 0f;
                _onSteepSlope = false;
                _downhillDirection = Vector2.zero;
                return;
            }

            var averagedNormal = Vector2.zero;
            var hitCount = 0;

            AddGroundNormalProbe(-1f, ref averagedNormal, ref hitCount);
            AddGroundNormalProbe(1f, ref averagedNormal, ref hitCount);

            if (hitCount > 0)
            {
                averagedNormal.Normalize();
                _groundNormal = averagedNormal;
            }
            else
            {
                _groundNormal = _groundHit.normal;
            }

            _groundSignedAngle = Vector2.SignedAngle(Vector2.up, _groundNormal);
            _groundAngle = Mathf.Abs(_groundSignedAngle);
            _onSteepSlope = _groundAngle > _stats.MaxWalkableSlopeAngle;
            _downhillDirection = GetDownhillDirection(_groundNormal);
        }

        private void AddGroundNormalProbe(float horizontalSign, ref Vector2 accumulatedNormal, ref int hitCount)
        {
            var center = (Vector2)transform.TransformPoint(_col.offset);
            var right = (Vector2)transform.right;
            var up = (Vector2)transform.up;
            var halfWidth = _col.size.x * 0.5f * Mathf.Abs(transform.lossyScale.x);
            var horizontalOffset = halfWidth * _stats.SlopeSensorSpread * horizontalSign;
            var origin = center + right * horizontalOffset + up * _stats.SlopeSensorStartHeight;
            var hit = Physics2D.Raycast(origin, Vector2.down, _stats.SlopeSensorDistance, ~_stats.PlayerLayer);

            if (!hit || hit.collider.isTrigger) return;

            var hitAngle = Mathf.Abs(Vector2.SignedAngle(Vector2.up, hit.normal));
            if (hitAngle > _stats.MaxWalkableSlopeAngle) return;

            accumulatedNormal += hit.normal;
            hitCount++;
        }

        private void CheckSteepSlopeAhead()
        {
            _steepSlopeAhead = false;
            if (!_grounded) return;

            var center = (Vector2)_col.bounds.center;
            var forwardOffset = Vector2.right * (_facing * _stats.SteepSlopeAheadDetectionRange);
            var origin = center + forwardOffset;

            var hit = Physics2D.Raycast(origin, Vector2.down, _col.bounds.extents.y + 0.5f, ~_stats.PlayerLayer);

            if (hit.collider != null && !hit.collider.isTrigger)
            {
                var angle = Mathf.Abs(Vector2.SignedAngle(Vector2.up, hit.normal));
                if (angle > _stats.MaxWalkableSlopeAngle)
                {
                    _steepSlopeAhead = true;
                }
            }
        }

        #region Walls

        private void HandleWalls()
        {
            if (!_stats.AllowWallInteraction || _grounded || _wallDirection == 0)
            {
                _wallSliding = false;
                _wallStickTimeLeft = 0f;
                return;
            }

            if (_frameVelocity.y > 0f)
            {
                _wallSliding = false;
                return;
            }

            var pressingIntoWall = _frameInput.Move.x == _wallDirection;
            var releasedTowardsWall = _frameInput.Move.x == 0 && _wallStickTimeLeft > 0f;

            if (pressingIntoWall)
            {
                _wallStickTimeLeft = _stats.WallStickTime;
                _wallSliding = true;
            }
            else if (releasedTowardsWall)
            {
                _wallStickTimeLeft = Mathf.Max(0f, _wallStickTimeLeft - Time.fixedDeltaTime);
                _wallSliding = true;
            }
            else
            {
                _wallStickTimeLeft = 0f;
                _wallSliding = false;
            }
        }

        #endregion
        
        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;
        private int _airJumpsRemaining;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump(false);
            else if (_stats.AllowWallInteraction && _wallSliding) ExecuteWallJump();
            else if (_stats.AllowAirJump && _stats.AirJumps > 0 && _airJumpsRemaining > 0)
            {
                _airJumpsRemaining--;
                ExecuteJump(true);
            }

            _jumpToConsume = false;
        }

        private void ExecuteJump(bool isAirJump)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;

            if (_frameInput.Move.x != 0) _facing = (int)Mathf.Sign(_frameInput.Move.x);

            if (isAirJump) AirJumped?.Invoke();
            else Jumped?.Invoke();
        }

        private void ExecuteWallJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _wallSliding = false;
            _wallStickTimeLeft = 0f;

            // 以玩家输入决定水平跳跃方向，无输入时默认远离墙壁
            var inputX = _frameInput.Move.x;
            var horizontalDir = inputX != 0 ? (int)inputX : -_wallDirection;

            _frameVelocity = new Vector2(horizontalDir * _stats.WallJumpHorizontalPower, _stats.WallJumpPower);
            _facing = horizontalDir != 0 ? (int)Mathf.Sign(horizontalDir) : _facing;

            // 仅当跳跃方向朝向墙壁时才锁定输入，防止立即重新抓墙；远离墙壁则无需锁定
            _wallJumpLockTimeLeft = horizontalDir == _wallDirection ? _stats.WallJumpControlLockTime : 0f;

            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_wallJumpLockTimeLeft > 0f)
            {
                _wallJumpLockTimeLeft = Mathf.Max(0f, _wallJumpLockTimeLeft - Time.fixedDeltaTime);
                return;
            }

            if (_grounded && _onSteepSlope && _frameVelocity.y <= 0f)
            {
                if (_frameInput.Move.x != 0 && Mathf.Sign(_frameInput.Move.x) == Mathf.Sign(_downhillDirection.x))
                {
                    _facing = (int)Mathf.Sign(_frameInput.Move.x);
                }

                // Steep slopes should always push the player downhill instead of allowing uphill acceleration.
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
                return;
            }

            if (_frameInput.Move.x != 0) _facing = (int)Mathf.Sign(_frameInput.Move.x);

            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Dash

        private bool _dashToConsume;
        private bool _dashUsable = true;
        private bool _dashing;
        private float _dashTimeLeft;
        private float _lastDashTime = float.MinValue;

        private void HandleDash()
        {
            if (_dashing)
            {
                _dashTimeLeft -= Time.fixedDeltaTime;
                if (_dashTimeLeft <= 0f) _dashing = false;
                return;
            }

            if (!_dashToConsume) return;

            var canDash = _dashUsable &&
                          (_grounded ? _stats.AllowGroundDash : true) &&
                          _time >= _lastDashTime + _stats.DashCooldown;

            if (canDash) ExecuteDash();

            _dashToConsume = false;
        }

        private void ExecuteDash()
        {
            _dashUsable = false;
            _dashing = true;
            _dashTimeLeft = _stats.DashDuration;
            _lastDashTime = _time;
            _jumpToConsume = false;
            _endedJumpEarly = false;

            var dashDirection = _frameInput.Move;
            if (dashDirection == Vector2.zero) dashDirection = new Vector2(_facing, 0f);
            if (_grounded && _onSteepSlope && Vector2.Dot(dashDirection.normalized, _downhillDirection) <= 0f)
            {
                dashDirection = _downhillDirection;
            }
            dashDirection.Normalize();

            _frameVelocity = dashDirection * _stats.DashSpeed;
            Dashed?.Invoke();
        }

        private void DashCoolDownHandle()
        {
            if(_grounded && _time - _lastDashTime >= _stats.DashCooldown) _dashUsable = true;
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _onSteepSlope && _frameVelocity.y <= 0f)
            {
                _frameVelocity = Vector2.MoveTowards(_frameVelocity, _downhillDirection * _stats.SteepSlopeSlideSpeed, _stats.SteepSlopeSlideAcceleration * Time.fixedDeltaTime);
                return;
            }

            if (_wallSliding)
            {
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.WallSlideSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
                return;
            }

            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void HandleBodyRotation()
        {
            var targetAngle = (_grounded && !_steepSlopeAhead) ? _groundSignedAngle : 0f;
            var nextAngle = Mathf.MoveTowardsAngle(_rb.rotation, targetAngle, _stats.SlopeAlignmentSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(nextAngle);
            _rb.angularVelocity = 0f;
        }

        private void ApplyMovement()
        {
            var platformVel = _currentPlatform != null ? _currentPlatform.PlatformVelocity : Vector2.zero;
            _rb.velocity = _frameVelocity + platformVel;
        }

        /// <summary>
        /// 封禁玩家操作：禁用输入并立即清零速度，使角色停止运动。
        /// </summary>
        public void DisableInput()
        {
            _stats.InputEnable = false;
            _frameVelocity = Vector2.zero;
            _rb.velocity = Vector2.zero;
        }

        /// <summary>
        /// 解除玩家操作封禁，恢复正常输入响应。
        /// </summary>
        public void EnableInput()
        {
            _stats.InputEnable = true;
        }

        #region Holdable Props

        public void AddProp(string name, HoldablePropBase prop)
        {
            if (string.IsNullOrEmpty(name) || prop == null) return;
            _holdableProps[name] = prop;
        }

        public void RemoveProp(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            _holdableProps.Remove(name);
        }

        public T GetProp<T>(string name) where T : HoldablePropBase
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_holdableProps.TryGetValue(name, out HoldablePropBase prop))
            {
                return prop as T;
            }
            return null;
        }

        #endregion

        private static Vector2 GetDownhillDirection(Vector2 surfaceNormal)
        {
            var downhill = Vector2.Perpendicular(surfaceNormal).normalized;
            if (downhill.y > 0f) downhill = -downhill;
            return downhill;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public event Action AirJumped;
        public event Action Dashed;
        public Vector2 FrameInput { get; }
        public Vector2 GroundNormal { get; }
        public float GroundAngle { get; }
        public bool Grounded { get; }
        public float LastFallDistance { get; }
    }
}
