using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Raccoin.CoinPusher.ECS
{
    /// <summary>
    /// 硬币生成系统 - 复刻原版 SpawnCoinSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnCoinSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnEntityConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<SpawnEntityConfig>();
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // 统计当前硬币数量
            int currentCount = 0;
            foreach (var coinInfo in SystemAPI.Query<RefRO<CoinEntityInfo>>())
            {
                currentCount++;
            }

            // 如果未达到上限，按概率生成
            if (currentCount < config.MaxCoinCount)
            {
                var coinEntity = ecb.Instantiate(config.CoinPrefab);
                float3 spawnPos = new float3(
                    UnityEngine.Random.Range(config.SpawnAreaMin.x, config.SpawnAreaMax.x),
                    config.SpawnAreaMax.y,
                    UnityEngine.Random.Range(config.SpawnAreaMin.z, config.SpawnAreaMax.z)
                );
                ecb.SetComponent(coinEntity, LocalTransform.FromPosition(spawnPos));
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// 球体生成系统 - 复刻原版 SpawnBallSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnBallSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnEntityConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // 球体生成逻辑（奖品球等）
        }
    }

    /// <summary>
    /// 宝箱生成系统 - 复刻原版 SpawnChestSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnChestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnEntityConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // 宝箱生成逻辑
        }
    }

    /// <summary>
    /// 小道具生成系统 - 复刻原版 SpawnGadgetSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnGadgetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnGadgetConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<SpawnGadgetConfig>();
            // 按概率生成小道具
        }
    }

    /// <summary>
    /// 胶水生成系统 - 复刻原版 SpawnGlueSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnGlueSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 胶水生成逻辑
        }
    }

    /// <summary>
    /// 塔生成系统 - 复刻原版 SpawnTowerSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnTowerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnEntityConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // 硬币塔生成逻辑
        }
    }

    /// <summary>
    /// 硬币塔基础系统 - 复刻原版 BasicTowerSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BasicTowerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 处理硬币塔的堆叠和倒塌逻辑
            var job = new TowerUpdateJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct TowerUpdateJob : IJobEntity
        {
            public void Execute(ref HoleCoinTowerInfo towerInfo, in LocalTransform transform)
            {
                // 塔高度检测和更新
            }
        }
    }

    /// <summary>
    /// 硬币速度处理系统 - 复刻原版 CoinSpeedDealSystem
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
    public partial struct CoinSpeedDealSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new LimitCoinJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct LimitCoinJob : IJobEntity
        {
            public void Execute(CoinSpeedAspect coinSpeed)
            {
                coinSpeed.LimitSpeed();

                // 应用阻尼
                float damping = coinSpeed.AccelerInfo.ValueRO.DampingFactor;
                coinSpeed.CurrentVelocity *= damping;
            }
        }
    }

    /// <summary>
    /// 硬币位置同步系统 - 复刻原版 CoinSyncPosSystem
    /// 将 ECS 实体位置同步到 GameObject 视图
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CoinSyncPosSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 将 ECS 硬币位置同步到 MonoBehaviour 视图层
            foreach (var aspect in SystemAPI.Query<CoinSyncPosAspect>())
            {
                // 通过 Entity-GO 映射同步位置
            }
        }
    }

    /// <summary>
    /// 碰撞事件处理系统 - 复刻原版 ProcessCoinCollisionEventsSystem
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct ProcessCoinCollisionEventsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // 初始化碰撞事件缓冲区
        }

        public void OnUpdate(ref SystemState state)
        {
            // 处理硬币间碰撞事件
            // 处理硬币与推板碰撞事件
        }
    }

    /// <summary>
    /// 推板物理系统 - 复刻原版 PhysicsPusherSystem
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(Unity.Physics.Systems.PhysicsSystemGroup))]
    public partial struct PhysicsPusherSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new PhysicsPusherJob { DeltaTime = deltaTime };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct PhysicsPusherJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(PhysicsPusherAspect pusherAspect)
            {
                ref var pusher = ref pusherAspect.Pusher.ValueRW;
                ref var transform = ref pusherAspect.Transform.ValueRW;

                switch (pusher.State)
                {
                    case PusherState.MovingForward:
                        pusher.CurrentProgress += pusher.Speed * DeltaTime;
                        if (pusher.CurrentProgress >= 1.0f)
                        {
                            pusher.CurrentProgress = 1.0f;
                            pusher.State = PusherState.MovingBackward;
                        }
                        break;

                    case PusherState.MovingBackward:
                        pusher.CurrentProgress -= pusher.Speed * DeltaTime;
                        if (pusher.CurrentProgress <= 0.0f)
                        {
                            pusher.CurrentProgress = 0.0f;
                            pusher.State = PusherState.MovingForward;
                        }
                        break;

                    case PusherState.Idle:
                    case PusherState.Paused:
                        return;
                }

                // 插值位置
                float3 newPos = math.lerp(pusher.StartPosition, pusher.EndPosition, pusher.CurrentProgress);
                transform.Position = newPos;
            }
        }
    }

    /// <summary>
    /// 修改物理步长系统 - 复刻原版 ModifyPhysicsStepSystem
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    public partial struct ModifyPhysicsStepSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // 根据性能动态调整物理步长
        }
    }

    /// <summary>
    /// 清理浣熊币系统 - 复刻原版 ClearRaccoinSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ClearRaccoinSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var job = new ClearRaccoinJob { Ecb = ecb.AsParallelWriter() };
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private partial struct ClearRaccoinJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in CoinEntityInfo coinInfo, in LocalTransform transform)
            {
                // 清理掉出边界的硬币
                if (transform.Position.y < -10f)
                {
                    Ecb.DestroyEntity(sortKey, entity);
                }
            }
        }
    }
}
