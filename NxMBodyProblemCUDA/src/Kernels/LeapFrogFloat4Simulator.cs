using System;
using Alea;
using Alea.CSharp;
using NUnit.Framework;
using NxMBodyProblemCUDA.Parser;
using NxMBodyProblemCUDA.Structures;

namespace NxMBodyProblemCUDA.Kernels
{
    class LeapFrogFloat4Simulator
    {
        [GpuParam] private readonly Constant<int> _blockSize;
        private readonly string _description;
        private readonly Gpu _gpu;

        public LeapFrogFloat4Simulator(Gpu gpu, int blockSize)
        {
            _gpu = gpu;
            _blockSize = Gpu.Constant(blockSize);
            _description = $"LeapFrogSimulator({blockSize})";
        }

        public void LeapFrogStepWithMass(deviceptr<float4> newCelMassPos, deviceptr<float4> oldCelMassPos,
            deviceptr<float4> newCelMassVel, deviceptr<float4> oldCelMassVel, deviceptr<float4> newCelMassScaled, deviceptr<float4> oldCelMassScaled,
            int numMassBodies, float deltaTime, float softeningSquared, int numTiles)
        {
            var index = threadIdx.x + blockIdx.x * _blockSize.Value;

            if (index >= numMassBodies)
                return;

            var position = oldCelMassPos[index];
            var accel = ComputeBodyAccel(softeningSquared, position, oldCelMassPos, numTiles, numMassBodies);

            var velocity = oldCelMassVel[index];

            var g = 6.67408e-11f;

            velocity.x = velocity.x + accel.x * deltaTime * g;
            velocity.y = velocity.y + accel.y * deltaTime * g;
            velocity.z = velocity.z + accel.z * deltaTime * g;

            position.x = position.x + velocity.x * deltaTime;
            position.y = position.y + velocity.y * deltaTime;
            position.z = position.z + velocity.z * deltaTime;

            var scaledPosition = position;
            float scale = (float)5.0E+10;
            scaledPosition.x = position.x / scale;
            scaledPosition.y = position.y / scale;
            scaledPosition.z = position.z / scale + 50;
            scaledPosition.w = 1;
            newCelMassScaled[index] = scaledPosition;

            newCelMassPos[index] = position;
            newCelMassVel[index] = velocity;
        }

        public void LeapFrogStepWithoutMass(deviceptr<float4> newPos, deviceptr<float4> oldPos,
            deviceptr<float4> newCelPos, deviceptr<float4> oldCelPos, deviceptr<float4> celMassPos,
            deviceptr<float4> newCelVel, deviceptr<float4> oldCelVel, deviceptr<float4> newCelMassVel, deviceptr<float4> oldCelMassVel,
            int numBodies, int numMassBodies, float deltaTime, float softeningSquared, int numTiles)
        {
            var index = threadIdx.x + blockIdx.x * _blockSize.Value;

            if (index >= numBodies)
                return;

            var position = oldCelPos[index];
            var accel = ComputeBodyAccel(softeningSquared, position, celMassPos, numTiles, numMassBodies);

            var velocity = oldCelVel[index];

            var g = 6.67408e-11f;


            velocity.x = velocity.x + accel.x * deltaTime * g;
            velocity.y = velocity.y + accel.y * deltaTime * g;
            velocity.z = velocity.z + accel.z * deltaTime * g;

            position.x = position.x + velocity.x * deltaTime;
            position.y = position.y + velocity.y * deltaTime;
            position.z = position.z + velocity.z * deltaTime;



            newCelPos[index] = position;

            var scaledPosition = position;
            float scale = (float)5.0E+10;
            scaledPosition.x = position.x / scale;
            scaledPosition.y = position.y / scale;
            scaledPosition.z = position.z / scale + 50;
            scaledPosition.w = 1;
            newPos[index] = scaledPosition;
            newCelVel[index] = velocity;
        }

        public void LeapFrogStep(deviceptr<float4> newPos, deviceptr<float4> oldPos, deviceptr<float4> newCelPos,
            deviceptr<float4> oldCelPos, deviceptr<float4> newCelMassPos, deviceptr<float4> oldCelMassPos,
            deviceptr<float4> newCelVel, deviceptr<float4>oldCelVel, deviceptr<float4> newCelMassVel, deviceptr<float4> oldCelMassVel,
            deviceptr<float4> newCelMassScaled, deviceptr<float4> oldCelMassScaled,
            int numBodies, int numMassBodies, float softeningSquared, float deltaTime)
        {
            //N - number of bodies with mass
            //M - number of bodies without mass - later called bodies

            //O(N^2)
            var numBlocks = Common.DivUp(numMassBodies, _blockSize.Value);
            var numTiles = Common.DivUp(numMassBodies, _blockSize.Value);
            var lp = new LaunchParam(numBlocks, _blockSize.Value);
            _gpu.Launch(LeapFrogStepWithMass, lp, newCelMassPos, oldCelMassPos, newCelMassVel, oldCelMassVel, newCelMassScaled, oldCelMassPos, numMassBodies, 10000f, softeningSquared, numTiles);

            //O(N*M)
            numBlocks = Common.DivUp(numBodies, _blockSize.Value);
            numTiles = Common.DivUp(numBodies, _blockSize.Value);
            lp = new LaunchParam(numBlocks, _blockSize.Value);
            _gpu.Launch(LeapFrogStepWithoutMass, lp, newPos, oldPos, newCelPos, oldCelPos, newCelMassPos, newCelVel, oldCelVel, newCelMassVel, oldCelMassVel, numBodies, numMassBodies, 10000f, softeningSquared, numTiles);

        }

        public float3 ComputeBodyAccel(float softeningSquared, float4 bodyPos, deviceptr<float4> bodies,
                               int numTiles, int secondMax)
        {
            var sharedPos = __shared__.Array<float4>(_blockSize.Value);
            var acc = new float3(0.0f, 0.0f, 0.0f);

            if (secondMax < numTiles)
                numTiles = secondMax;

            for (var tile = 0; tile < numTiles; tile++)
            {
                sharedPos[threadIdx.x] = bodies[tile * blockDim.x + threadIdx.x];

                DeviceFunction.SyncThreads();

                // This is the "tile_calculation" function from the GPUG3 article.
                for (var counter = 0; counter < _blockSize.Value && counter < secondMax; counter++)
                {
                    acc = BodyBodyInteraction(softeningSquared, acc, bodyPos, sharedPos[counter]);
                }

                DeviceFunction.SyncThreads();
            }
            return (acc);
        }

        public static float3 BodyBodyInteraction(float softeningSquared, float3 ai, float4 bi, float4 bj)
        {
            // r_ij  [3 FLOPS]
            var r = new float3(bj.x - bi.x, bj.y - bi.y, bj.z - bi.z);

            // distSqr = dot(r_ij, r_ij) + EPS^2  [6 FLOPS]
            var distSqr = r.x * r.x + r.y * r.y + r.z * r.z + softeningSquared;

            // invDistCube =1/distSqr^(3/2)  [4 FLOPS (2 mul, 1 sqrt, 1 inv)]
            var invDist = LibDevice.__nv_rsqrtf(distSqr);
            var invDistCube = invDist * invDist * invDist;

            // s = m_j * invDistCube [1 FLOP]
            var s = bj.w * invDistCube;

            // a_i =  a_i + s * r_ij [6 FLOPS]
            return (new float3(ai.x + r.x * s, ai.y + r.y * s, ai.z + r.z * s));
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public void Integrate(deviceptr<float4> newPos, deviceptr<float4> oldPos, deviceptr<float4> newCel,
            deviceptr<float4> oldCel, deviceptr<float4> celVel0, deviceptr<float4> celVel1,
            deviceptr<float4> newCelMassScaled, deviceptr<float4> oldCelMassScaled,
            deviceptr<float4> newCelMass, deviceptr<float4> oldCelMass, deviceptr<float4> celMassVel0, deviceptr<float4> celMassVel1,
            int numBodies, int numMassBodies, float softeningSquared, float deltaTime)
        {
            //IntegrateNbodySystem(newPos, oldPos, vel, numBodies, deltaTime, softeningSquared, damping);
            LeapFrogStep(newPos, oldPos, newCel, oldCel, newCelMass, oldCelMass, celVel0, celVel1, celMassVel0, celMassVel1, newCelMassScaled, oldCelMassScaled, 
                numBodies, numMassBodies, softeningSquared, deltaTime);
        }
    }
}
