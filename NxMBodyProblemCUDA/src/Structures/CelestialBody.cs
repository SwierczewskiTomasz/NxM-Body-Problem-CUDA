using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxMBodyProblemCUDA.Structures
{
    public class CelestialBody
    {
        public string Name { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; set; }

        public CelestialBody(string name, double[] position, double[] velocity)
        {
            Name = name;
            if (velocity == null || velocity.Length != 3)
                throw new Exception("Velocity or Position must be vector of 3 doubles");
            if (position == null || position.Length != 3)
                throw new Exception("Velocity or Position must be vector of 3 doubles");
            Velocity = new Vector3(velocity[0], velocity[1], velocity[2]);
            Position = new Vector3(position[0], position[1], position[2]);
        }

        public CelestialBody(string name, Vector3 position, Vector3 velocity)
        {
            Name = name;
            Velocity = velocity;
            Position = position;
        }
    }

    public class CelestialBodyWithMass
    {
        public string Name { get; }
        public Vector3 Velocity { get; set; }
        public Vector3 Position { get; set; }
        public double Mass { get; }

        public CelestialBodyWithMass(string name, double[] position, double[] velocity, double mass)
        {
            Name = name;
            if (velocity == null || velocity.Length != 3)
                throw new Exception("Velocity or Position must be vector of 3 doubles");
            if (position == null || position.Length != 3)
                throw new Exception("Velocity or Position must be vector of 3 doubles");
            Velocity = new Vector3(velocity[0], velocity[1], velocity[2]);
            Position = new Vector3(position[0], position[1], position[2]);
            Mass = mass;
        }

        public CelestialBodyWithMass(string name, Vector3 position, Vector3 velocity, double mass)
        {
            Name = name;
            Velocity = velocity;
            Position = position;
            Mass = mass;
        }
    }
}
