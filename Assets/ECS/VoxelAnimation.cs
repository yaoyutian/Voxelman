using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

// Jobified voxel animation system

[UpdateAfter(typeof(VoxelBufferSystem))]
class VoxelAnimationSystem : JobComponentSystem
{
    [Unity.Burst.BurstCompile]
    struct VoxelAnimation : IJobProcessComponentData<Voxel, LocalToWorld>
    {
        public float dt;

        public void Execute([ReadOnly] ref Voxel voxel, ref LocalToWorld matrix)
        {
            // Per-instance random number
            var hash = new XXHash(voxel.ID);
            var rand1 = hash.Value01(1);
            var rand2 = hash.Value01(2);

            // Extract the current position/scale.
            var pos = matrix.Value.c3.xyz;
            var scale = matrix.Value.c0.x;

            // Move/Shrink.
            pos += new float3(0.1f, -2.0f, 0.3f) * (rand2 + 0.1f) * dt;
            scale *= math.lerp(0.9f, 0.98f, rand1);

            // Build a new matrix.
            matrix = new LocalToWorld {
                Value = new float4x4(
                    new float4(scale, 0, 0, 0),
                    new float4(0, scale, 0, 0),
                    new float4(0, 0, scale, 0),
                    new float4(pos.x, pos.y, pos.z, 1)
                )
            };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new VoxelAnimation() { dt = UnityEngine.Time.deltaTime };
        return job.Schedule(this, inputDeps);
    } 
}
