using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Raccoin.CoinPusher.ECS
{
    // ===== ECS Components =====

    /// <summary>
    /// 硬币实体信息 - 复刻原版 CoinEntityInfo
    /// </summary>
    public struct CoinEntityInfo : IComponentData
    {
        public int CoinId;
        public int CoinTypeId;
        public long PointValue;
        public float Scale;
        public bool IsSettled;
        public bool IsSpecial;
        public int EffectTypeId;
    }

    /// <summary>
    /// 硬币实体变换信息 - 复刻原版 CoinEntityTransformInfo
    /// </summary>
    public struct CoinEntityTransformInfo : IComponentData
    {
        public float3 SpawnPosition;
        public quaternion SpawnRotation;
        public float3 InitialVelocity;
    }

    /// <summary>
    /// 硬币加速信息 - 复刻原版 CoinAccelerInfo
    /// </summary>
    public struct CoinAccelerInfo : IComponentData
    {
        public float3 Acceleration;
        public float MaxSpeed;
        public float DampingFactor;
    }

    /// <summary>
    /// 硬币速度切面 - 复刻原版 CoinSpeedAspect
    /// </summary>
    public readonly partial struct CoinSpeedAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRW<PhysicsVelocity> Velocity;
        public readonly RefRO<CoinAccelerInfo> AccelerInfo;

        public float3 CurrentVelocity
        {
            get => Velocity.ValueRO.Linear;
            set => Velocity.ValueRW.Linear = value;
        }

        public void LimitSpeed()
        {
            float maxSpeed = AccelerInfo.ValueRO.MaxSpeed;
            float speedSq = math.lengthsq(Velocity.ValueRO.Linear);
            if (speedSq > maxSpeed * maxSpeed)
            {
                Velocity.ValueRW.Linear = math.normalize(Velocity.ValueRO.Linear) * maxSpeed;
            }
        }
    }

    /// <summary>
    /// 硬币位置同步切面 - 复刻原版 CoinSyncPosAspect
    /// </summary>
    public readonly partial struct CoinSyncPosAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRO<LocalTransform> Transform;
        public readonly RefRO<CoinEntityInfo> CoinInfo;
    }

    /// <summary>
    /// 推板物理组件 - 复刻原版 PhysicsPusher
    /// </summary>
    public struct PhysicsPusher : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
        public float Speed;
        public float CurrentProgress;
        public bool IsMovingForward;
        public PusherState State;
    }

    public enum PusherState
    {
        Idle = 0,
        MovingForward = 1,
        MovingBackward = 2,
        Paused = 3
    }

    /// <summary>
    /// 推板物理切面 - 复刻原版 PhysicsPusherAspect
    /// </summary>
    public readonly partial struct PhysicsPusherAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRW<PhysicsPusher> Pusher;
        public readonly RefRW<LocalTransform> Transform;
    }

    /// <summary>
    /// 平台物理组件 - 复刻原版 PhysicsPlatform
    /// </summary>
    public struct PhysicsPlatform : IComponentData
    {
        public float3 Direction;
        public float Speed;
        public float Width;
        public bool IsActive;
    }

    /// <summary>
    /// 平台物理切面 - 复刻原版 PhysicsPlatformAspect
    /// </summary>
    public readonly partial struct PhysicsPlatformAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRW<PhysicsPlatform> Platform;
        public readonly RefRW<LocalTransform> Transform;
    }

    /// <summary>
    /// 生成实体配置 - 复刻原版 SpawnEntityConfig
    /// </summary>
    public struct SpawnEntityConfig : IComponentData
    {
        public Entity CoinPrefab;
        public Entity BallPrefab;
        public Entity ChestPrefab;
        public int MaxCoinCount;
        public float SpawnInterval;
        public float3 SpawnAreaMin;
        public float3 SpawnAreaMax;
    }

    /// <summary>
    /// 生成道具配置 - 复刻原版 SpawnGadgetConfig
    /// </summary>
    public struct SpawnGadgetConfig : IComponentData
    {
        public Entity GadgetPrefab;
        public float SpawnProbability;
        public int MaxGadgetCount;
    }

    /// <summary>
    /// 小道具信息 - 复刻原版 GadgetInfo
    /// </summary>
    public struct GadgetInfo : IComponentData
    {
        public int GadgetTypeId;
        public float Lifetime;
        public bool IsCollected;
    }

    /// <summary>
    /// 洞塔信息 - 复刻原版 HoleCoinTowerInfo
    /// </summary>
    public struct HoleCoinTowerInfo : IComponentData
    {
        public int TowerHeight;
        public CoinTowerShape Shape;
        public bool IsActive;
    }

    public enum CoinTowerShape
    {
        Cylinder = 0,
        Pyramid = 1,
        Random = 2
    }

    /// <summary>
    /// 碰撞事件队列 - 复刻原版 CollisionEventQueues
    /// </summary>
    public struct CollisionEventQueues : IComponentData
    {
        public int EventCount;
        public bool HasPendingEvents;
    }

    /// <summary>
    /// 硬币碰撞事件信息 - 复刻原版 CoinCollisionEventInfo
    /// </summary>
    public struct CoinCollisionEventInfo : IBufferElementData
    {
        public Entity CoinEntity;
        public Entity OtherEntity;
        public float3 ContactPoint;
        public float3 Normal;
        public float Impulse;
    }

    /// <summary>
    /// 推板碰撞事件信息 - 复刻原版 CoinPusherCollisionEventInfo
    /// </summary>
    public struct CoinPusherCollisionEventInfo : IBufferElementData
    {
        public Entity CoinEntity;
        public float3 PushDirection;
        public float PushForce;
    }

    /// <summary>
    /// 硬币胶水标签 - 复刻原版 CoinGlueTag
    /// </summary>
    public struct CoinGlueTag : IComponentData
    {
        public float GlueDuration;
        public float RemainingTime;
    }

    /// <summary>
    /// 硬币塔基础标签
    /// </summary>
    public struct CoinTowerTag : IComponentData
    {
        public int TowerId;
    }
}
