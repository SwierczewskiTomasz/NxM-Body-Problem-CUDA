using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxMBodyProblemCUDA.Structures
{
    public class AstronomicalBody
    {
        public string Name { get; }
        public double Eccentricity { get; }
        public double? SemimajorAxis { get; }
        public double? Periapsis { get; }
        public double Inclination { get; }
        public double? LongitudeOfTheAscendingNode { get; }
        public double? ArgumentOfPeriapsis { get; }
        public double? TrueAnomally { get; }
        public double? LongitudeOfPeriapsis { get; }
        public double? MeanMotion { get; }
        public double? MeanAnomally { get; }
        public double? MeanLongitude { get; }
        public double? Mass { get; }
        public double? TimeOfPeriapsis { get; }


        public AstronomicalBody(string name, double eccentricity, double? semimajorAxis, double? periapsis, double inclination,
            double? longitudeOfTheAscendingNode, double? argumentOfPeriapsis, double? trueAnomally, double? longitudeOfPeriapsis,
            double? meanMotion, double? meanAnomally, double? meanLongitude, double? mass, double timeOfPeriapsis)
        {
            Name = name;
            Eccentricity = eccentricity;
            SemimajorAxis = semimajorAxis;
            Periapsis = periapsis;
            Inclination = inclination;
            LongitudeOfTheAscendingNode = longitudeOfTheAscendingNode;
            ArgumentOfPeriapsis = argumentOfPeriapsis;
            TrueAnomally = trueAnomally;
            LongitudeOfPeriapsis = longitudeOfPeriapsis;
            MeanMotion = meanMotion;
            MeanAnomally = meanAnomally;
            MeanLongitude = meanLongitude;
            Mass = mass;
            TimeOfPeriapsis = timeOfPeriapsis;
        }

        public CelestialBody CalculatePositionAndVelocity(double epoch)
        {
            double x, y, z;
            double vx, vy, vz;

            double O = (double)LongitudeOfTheAscendingNode;
            O /= 180 / Math.PI;
            //double w = (double)LongitudeOfPeriapsis;
            double w = (double)ArgumentOfPeriapsis;
            w /= 180 / Math.PI;
            double i = Inclination;
            i /= 180 / Math.PI;

            double a = (double)SemimajorAxis * Constants.au;
            double e = Eccentricity;
            double E;
            double Px, Py, Pz, Qx, Qy, Qz;
            double k = Constants.G;
            double m = Constants.Ms + (Mass == null ? 0.0 : (double)Mass);
            double n; //must be in radians on time
            double r;
            double M;

            //Px = Math.Cos(w) * Math.Cos(O) - Math.Sin(w) * Math.Sin(O) * Math.Cos(i);
            //Py = Math.Cos(w) * Math.Sin(O) + Math.Sin(w) * Math.Cos(O) * Math.Cos(i);
            //Pz = Math.Sin(w) * Math.Sin(i);

            //Qx = -Math.Sin(w) * Math.Cos(O) - Math.Cos(w) * Math.Sin(O) * Math.Cos(i);
            //Qy = -Math.Sin(w) * Math.Sin(O) + Math.Cos(w) * Math.Cos(O) * Math.Cos(i);
            //Qz = Math.Cos(w) * Math.Sin(i);

            double[,] R = new double[3, 3];

            R[0, 0] = Math.Cos(w) * Math.Cos(O) - Math.Sin(w) * Math.Sin(O) * Math.Cos(i);
            R[0, 1] = -Math.Cos(O) * Math.Sin(w) - Math.Cos(w) * Math.Sin(O) * Math.Cos(i);
            R[0, 2] = Math.Sin(O) * Math.Sin(i);

            R[1, 0] = Math.Sin(O) * Math.Cos(w) + Math.Cos(O) * Math.Sin(w) * Math.Cos(i);
            R[1, 1] = -Math.Sin(w) * Math.Sin(O) + Math.Cos(w) * Math.Cos(O) * Math.Cos(i);
            R[1, 2] = -Math.Cos(w) * Math.Sin(i);

            R[2, 0] = Math.Sin(w) * Math.Sin(i);
            R[2, 1] = Math.Cos(w) * Math.Sin(i);
            R[2, 2] = Math.Cos(i);

            double period = 2 * Math.PI * Math.Sqrt(Math.Pow(a, 3) / (k * m));
            double peroidInDays = period / 86400;

            n = 2 * Math.PI / period;

            double M0;
            M0 = n * (epoch - (double)TimeOfPeriapsis) * 86400;
            M0 = M0 % (2 * Math.PI);
            if (M0 < 0)
                M0 += 2 * Math.PI;
            M = M0;
            //E = M + e * Math.Sin(M) + e * e * Math.Sin(2 * M) / 2;
            double Eprev = 0;
            E = M;
            while (Math.Abs(E - Eprev) > 0.000001f)
            {
                Eprev = E;
                E = E - (E - e * Math.Sin(E) - M) / (1 - e * Math.Cos(E));
            }


            r = a * (1 - e * Math.Cos(E));

            //x = a * Px * (Math.Cos(E) - e) + a * Qx * Math.Sqrt(1 - e * e) * Math.Sin(E);
            //y = a * Py * (Math.Cos(E) - e) + a * Qy * Math.Sqrt(1 - e * e) * Math.Sin(E);
            //z = a * Pz * (Math.Cos(E) - e) + a * Qz * Math.Sqrt(1 - e * e) * Math.Sin(E);

            //vx = a * n / r * (-a * Px * Math.Sin(E) + a * Qx * Math.Sqrt(1 - e * e) * Math.Cos(E));
            //vy = a * n / r * (-a * Py * Math.Sin(E) + a * Qy * Math.Sqrt(1 - e * e) * Math.Cos(E));
            //vz = a * n / r * (-a * Pz * Math.Sin(E) + a * Qz * Math.Sqrt(1 - e * e) * Math.Cos(E));

            x = R[0, 0] * r * Math.Cos(E) + R[0, 1] * r * Math.Sin(E);
            y = R[1, 0] * r * Math.Cos(E) + R[1, 1] * r * Math.Sin(E);
            z = R[2, 0] * r * Math.Cos(E) + R[2, 1] * r * Math.Sin(E);

            vx = Math.Sqrt(m * Constants.G / (a * (1 - e * e))) * (-R[0, 0] * Math.Sin(E) + R[0, 1] * (e + Math.Cos(E)));
            vy = Math.Sqrt(m * Constants.G / (a * (1 - e * e))) * (-R[1, 0] * Math.Sin(E) + R[1, 1] * (e + Math.Cos(E)));
            vz = Math.Sqrt(m * Constants.G / (a * (1 - e * e))) * (-R[2, 0] * Math.Sin(E) + R[2, 1] * (e + Math.Cos(E)));

            CelestialBody celestialBody = new CelestialBody(Name, new double[3] { x, y, z }, new double[3] { vx, vy, vz });
            return celestialBody;
        }
    }
}
