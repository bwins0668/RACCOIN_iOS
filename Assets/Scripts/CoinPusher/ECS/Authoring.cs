using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Raccoin.CoinPusher.ECS
{
    /// <summary>
    /// 硬币实体 Authoring - 复刻原版 GameCoinAuthoring + Baker
    /// </summary>
    public class GameCoinAuthoring : MonoBehaviour
    {
        public int CoinTypeId;
        public long PointValue = 1;
        public float CoinScale = 1.0f;
        public bool IsSpecial;
        public int EffectTypeId;
        public float MaxSpeed = 5.0f;
        public float DampingFactor = 0.98f;

        public class Baker : Baker<GameCoinAuthoring>
        {
            public override void Bake(GameCoinAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new CoinEntityInfo
                {
                    CoinId = 0,
                    CoinTypeId = authoring.CoinTypeId,
                    PointValue = authoring.PointValue,
                    Scale = authoring.CoinScale,
                    IsSettled = false,
                    IsSpecial = authoring.IsSpecial,
                    EffectTypeId = authoring.EffectTypeId
                });

                AddComponent(entity, new CoinEntityTransformInfo
                {
                    SpawnPosition = authoring.transform.position,
                    SpawnRotation = authoring.transform.rotation,
                    InitialVelocity = float3.zero
                });

                AddComponent(entity, new CoinAccelerInfo
                {
                    Acceleration = float3.zero,
                    MaxSpeed = authoring.MaxSpeed,
                    DampingFactor = authoring.DampingFactor
                });

                // 物理碰撞体 - 硬币使用圆柱体
                var collider = Unity.Physics.CylinderGeometry.Generate(
                    new float3(0, 0, 0),
                    quaternion.identity,
                    0.5f * authoring.CoinScale,  // radius
                    0.05f * authoring.CoinScale   // height
                );

                AddComponent(entity, new PhysicsCollider
                {
                    Value = Unity.Physics.Collider.Create(collider, new CollisionFilter
                    {
                        BelongsTo = 1u << 0,  // Coin layer
                        CollidesWith = 0xFFFFFFFF,
                        GroupIndex = 0
                    })
                });

                AddComponent(entity, PhysicsMass.CreateDynamic(
                    new MassDistribution { Mass = 1.0f },
                    new PhysicsCollider { Value = Unity.Physics.Collider.Create(collider) }
                ));
            }
        }
    }

    /// <summary>
    /// 宝箱 Authoring - 复刻原版 ChestAuthoring + Baker
    /// </summary>
    public class ChestAuthoring : MonoBehaviour
    {
        public int ChestTypeId;
        public float OpenProbability = 0.5f;

        public class Baker : Baker<ChestAuthoring>
        {
            public override void Bake(ChestAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GadgetInfo
                {
                    GadgetTypeId = authoring.ChestTypeId,
                    Lifetime = 30f,
                    IsCollected = false
                });
            }
        }
    }

    /// <summary>
    /// 洞塔 Authoring - 复刻原版 HoleTowerAuthoring + Baker
    /// </summary>
    public class HoleTowerAuthoring : MonoBehaviour
    {
        public int TowerHeight = 5;
        public CoinTowerShape Shape = CoinTowerShape.Cylinder;

        public class Baker : Baker<HoleTowerAuthoring>
        {
            public override void Bake(HoleTowerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HoleCoinTowerInfo
                {
                    TowerHeight = authoring.TowerHeight,
                    Shape = authoring.Shape,
                    IsActive = true
                });
                AddComponent(entity, new CoinTowerTag { TowerId = 0 });
            }
        }
    }

    /// <summary>
    /// 树塔 Authoring - 复刻原版 TreeTowerAuthoring + Baker
    /// </summary>
    public class TreeTowerAuthoring : MonoBehaviour
    {
        public int MaxHeight = 8;

        public class Baker : Baker<TreeTowerAuthoring>
        {
            public override void Bake(TreeTowerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HoleCoinTowerInfo
                {
                    TowerHeight = authoring.MaxHeight,
                    Shape = CoinTowerShape.Pyramid,
                    IsActive = true
                });
            }
        }
    }

    /// <summary>
    /// 物理机器 Authoring - 复刻原版 PhysicsMachineAuthoring + Baker
    /// </summary>
    public class PhysicsMachineAuthoring : MonoBehaviour
    {
        public float MachineWidth = 4.0f;
        public float MachineDepth = 6.0f;

        public class Baker : Baker<PhysicsMachineAuthoring>
        {
            public override void Bake(PhysicsMachineAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                // 机器边界碰撞体
            }
        }
    }

    /// <summary>
    /// 物理平台 Authoring - 复刻原版 PhysicsPlatformAuthoring + Baker
    /// </summary>
    public class PhysicsPlatformAuthoring : MonoBehaviour
    {
        public float3 Direction = new float3(1, 0, 0);
        public float Speed = 1.0f;
        public float Width = 4.0f;

        public class Baker : Baker<PhysicsPlatformAuthoring>
        {
            public override void Bake(PhysicsPlatformAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PhysicsPlatform
                {
                    Direction = math.normalize(authoring.Direction),
                    Speed = authoring.Speed,
                    Width = authoring.Width,
                    IsActive = true
                });
            }
        }
    }

    /// <summary>
    /// 物理推板 Authoring - 复刻原版 PhysicsPusherAuthoring + Baker
    /// </summary>
    public class PhysicsPusherAuthoring : MonoBehaviour
    {
        public float3 PushDirection = new float3(0, 0, 1);
        public float PushDistance = 1.5f;
        public float PushSpeed = 0.5f;

        public class Baker : Baker<PhysicsPusherAuthoring>
        {
            public override void Bake(PhysicsPusherAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                float3 startPos = authoring.transform.position;
                float3 endPos = startPos + math.normalize(authoring.PushDirection) * authoring.PushDistance;

                AddComponent(entity, new PhysicsPusher
                {
                    StartPosition = startPos,
                    EndPosition = endPos,
                    Speed = authoring.PushSpeed,
                    CurrentProgress = 0f,
                    IsMovingForward = true,
                    State = PusherState.MovingForward
                });
            }
        }
    }

    /// <summary>
    /// 奖品球 Authoring - 复刻原版 PrizeBallAuthoring + Baker
    /// </summary>
    public class PrizeBallAuthoring : MonoBehaviour
    {
        public int PrizeId;
        public float BallRadius = 0.3f;

        public class Baker : Baker<PrizeBallAuthoring>
        {
            public override void Bake(PrizeBallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GadgetInfo
                {
                    GadgetTypeId = authoring.PrizeId,
                    Lifetime = 60f,
                    IsCollected = false
                });
            }
        }
    }

    /// <summary>
    /// 生成实体配置 Authoring - 复刻原版 SpawnEntityConfigAuthoring + Baker
    /// </summary>
    public class SpawnEntityConfigAuthoring : MonoBehaviour
    {
        public GameObject CoinPrefab;
        public GameObject BallPrefab;
        public GameObject ChestPrefab;
        public int MaxCoinCount = 500;
        public float SpawnInterval = 0.5f;
        public Vector3 SpawnAreaMin = new Vector3(-2, 3, -1);
        public Vector3 SpawnAreaMax = new Vector3(2, 5, 1);

        public class Baker : Baker<SpawnEntityConfigAuthoring>
        {
            public override void Bake(SpawnEntityConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnEntityConfig
                {
                    CoinPrefab = GetEntity(authoring.CoinPrefab, TransformUsageFlags.Dynamic),
                    BallPrefab = GetEntity(authoring.BallPrefab, TransformUsageFlags.Dynamic),
                    ChestPrefab = GetEntity(authoring.ChestPrefab, TransformUsageFlags.Dynamic),
                    MaxCoinCount = authoring.MaxCoinCount,
                    SpawnInterval = authoring.SpawnInterval,
                    SpawnAreaMin = authoring.SpawnAreaMin,
                    SpawnAreaMax = authoring.SpawnAreaMax
                });
            }
        }
    }

    /// <summary>
    /// 生成道具配置 Authoring - 复刻原版 SpawnGadgetConfigAuthoring + Baker
    /// </summary>
    public class SpawnGadgetConfigAuthoring : MonoBehaviour
    {
        public GameObject GadgetPrefab;
        public float SpawnProbability = 0.1f;
        public int MaxGadgetCount = 5;

        public class Baker : Baker<SpawnGadgetConfigAuthoring>
        {
            public override void Bake(SpawnGadgetConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnGadgetConfig
                {
                    GadgetPrefab = GetEntity(authoring.GadgetPrefab, TransformUsageFlags.Dynamic),
                    SpawnProbability = authoring.SpawnProbability,
                    MaxGadgetCount = authoring.MaxGadgetCount
                });
            }
        }
    }

    /// <summary>
    /// 小道具 Authoring - 复刻原版 GadgetAuthoring + Baker
    /// </summary>
    public class GadgetAuthoring : MonoBehaviour
    {
        public int GadgetTypeId;
        public float Lifetime = 30f;

        public class Baker : Baker<GadgetAuthoring>
        {
            public override void Bake(GadgetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GadgetInfo
                {
                    GadgetTypeId = authoring.GadgetTypeId,
                    Lifetime = authoring.Lifetime,
                    IsCollected = false
                });
            }
        }
    }
}
