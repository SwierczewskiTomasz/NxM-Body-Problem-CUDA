using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Alea;
using NxMBodyProblemCUDA.Structures;

namespace NxMBodyProblemCUDA.Parser
{
    public static class Parser
    {
        public static List<AstronomicalBody> astronomicalBodyList;
        public static List<CelestialBody> celestialBodyList;
        public static List<CelestialBodyWithMass> celestialBodyWithMassList;

        [STAThread]
        public static void LoadFile()
        {
            string fileName = "results.csv";
            StreamReader streamReader = new StreamReader(fileName);
            streamReader.ReadLine();
            int c = 0;

            while (!streamReader.EndOfStream)
            {
                string name = streamReader.ReadLine();
                string[] splited = name.Split(',');
                if (splited.Length != 8)
                    continue;
                double semimajoraxis = 0;
                double.TryParse(splited[1], NumberStyles.Any, CultureInfo.InvariantCulture, out semimajoraxis);
                AstronomicalBody astronomicalBody = new AstronomicalBody("", 
                                                                        double.Parse(splited[2], CultureInfo.InvariantCulture),
                                                                        semimajoraxis, 
                                                                        null, 
                                                                        double.Parse(splited[3], CultureInfo.InvariantCulture), 
                                                                        double.Parse(splited[4], CultureInfo.InvariantCulture), 
                                                                        double.Parse(splited[5], CultureInfo.InvariantCulture),
                                                                        null, 
                                                                        null, 
                                                                        null, 
                                                                        null, 
                                                                        null, 
                                                                        null, 
                                                                        double.Parse(splited[7], CultureInfo.InvariantCulture));
                astronomicalBodyList.Add(astronomicalBody);
                c++;
                //if (c == 30000)
                //    break;
            }

            foreach(var a in astronomicalBodyList)
            {
                celestialBodyList.Add(a.CalculatePositionAndVelocity(2458200.5));
            }

            streamReader.Close();

            Console.WriteLine("Readed " + astronomicalBodyList.Count.ToString() + " astronomical objects");
        }

        [STAThread]
        public static void LoadData()
        {
            AstronomicalBody Earth = new AstronomicalBody("Earth", 0.01671123, 1.0, null, 0.0, -11.26064 / 180.0 * Math.PI,
                114.20783 / 180.0 * Math.PI, null, 102.93768193 / 180.0 * Math.PI, null, 0.0, null, 5.97219e24,
                2458236.784053135587);
            AstronomicalBody Ceres = new AstronomicalBody("Ceres", .07553461024389638, 2.767046248500289, 2.558038488592984,
                10.5935097971363 / 180 * Math.PI, 80.30991865594387 / 180 * Math.PI, 73.11534200131032 / 180 * Math.PI,
                null, null, null, 352.2304611765882 / 180 * Math.PI, null, 62.6284 / Constants.G,
                2458236.784053135587);
            //CelestialBodyWithMass sun = new CelestialBodyWithMass("Sun",
            //                                      new double[] { 1.953986474447199E+05, 9.744009844198005E+05, -1.620003654771077E+04 },
            //                                      new double[] { -1.107391391838082E-02, 7.710256418221521E-03, 2.640571877471076E-04 },
            //                                      Constants.Ms);
            CelestialBodyWithMass sun = new CelestialBodyWithMass("Sun",
                                                  new double[] { 0, 0, 0 },
                                                  new double[] { 0, 0, 0 },
                                                  Constants.Ms);


            CelestialBodyWithMass mercury = new CelestialBodyWithMass("Mercury",
                                                      new double[] { -4.486364957895508E+07, 2.671001160673119E+07, 6.220430971666429E+06 },
                                                      new double[] { -3.416348953981163E+01, -4.024628751412460E+01, -1.558960876726037E-01 },
                                                      3.302e23);

            CelestialBodyWithMass venus = new CelestialBodyWithMass("Venus",
                                                    new double[] { 7.766662574542834E+07, 7.638453294020119E+07, -3.452349884123292E+06},
                                                    new double[] { -2.454696121588775E+01, 2.495326620147037E+01, 1.758334494263146E+00 },
                                                    48.685e23);

            CelestialBodyWithMass earth = new CelestialBodyWithMass("Earth",
                                                    new double[] { -1.487962401117654E+08, -4.386446413101053E+06, -1.500019680619077E+04 },
                                                    new double[] { 5.872196941118726E-01, -2.987724300128726E+01, 9.900496841019901E-04 },
                                                    5.97219e24);

            CelestialBodyWithMass mars = new CelestialBodyWithMass("Mars", 
                                                   new double[] { -1.401645958912379E+08, -1.825059856170199E+08, -4.161836322036758E+05 },
                                                   new double[] { 2.014441443487323E+01, -1.263969014176502E+01, -7.593720622950455E-01 },
                                                   6.4185e23);

            CelestialBodyWithMass jupiter = new CelestialBodyWithMass("Jupiter",
                                                      new double[] { -5.778862090724405E+08, -5.670709785986338E+08, 1.527803496341616E+07 },
                                                      new double[] { 8.996165403937365E+00, -8.705794846141039E+00, -1.651893469256831E-01 },
                                                      1898.13e24);

            CelestialBodyWithMass saturn = new CelestialBodyWithMass("Saturn",
                                                      new double[] { 7.102428504961595E+07, -1.502969051845940E+09, 2.330506132549977E+07 },
                                                      new double[] { 9.116536416438429E+00, 4.247832031650504E-01, -3.701236973417399E-01 },
                                                      5.68319e26);

            CelestialBodyWithMass uranus = new CelestialBodyWithMass("Uranus",
                                                      new double[] { 2.628992351438188E+09, 1.395848580462184E+09, -2.887480189583957E+07 },
                                                      new double[] { -3.243683974340414E+00, 5.697396376809577E+00, 6.324869063969363E-02 },
                                                      86.8103e24);

            CelestialBodyWithMass neptune = new CelestialBodyWithMass("Neptune",
                                                      new double[] { 4.301217062217299E+09, -1.248583152597126E+09, -7.341363493258017E+07 },
                                                      new double[] { 1.479674610870903E+00, 5.252789174856398E+00, -1.420740405842398E-01 },
                                                      102.41e24);

            //CelestialBody celestialBody = Ceres.CalculatePositionAndVelocity(2458219);
            astronomicalBodyList = new List<AstronomicalBody>();
            celestialBodyList = new List<CelestialBody>();
            celestialBodyWithMassList = new List<CelestialBodyWithMass>();

            celestialBodyWithMassList.Add(sun);
            celestialBodyWithMassList.Add(mercury);
            celestialBodyWithMassList.Add(venus);
            celestialBodyWithMassList.Add(earth);
            celestialBodyWithMassList.Add(mars);
            celestialBodyWithMassList.Add(jupiter);
            celestialBodyWithMassList.Add(saturn);
            celestialBodyWithMassList.Add(uranus);
            celestialBodyWithMassList.Add(neptune);

            foreach(var c in celestialBodyWithMassList)
            {
                //km to m, and km/s to m
                c.Position *= 1000;
                c.Velocity *= 1000;
            }

            LoadFile();

            //Console.ReadKey();
        }

        public static void PreparePositionAndVelocityForCelestialBody(int _numBodies, out float4[] pos, out float4[] vel)
        {
            pos = new float4[_numBodies];
            vel = new float4[_numBodies];

            float size = 1.0f; //(float)6.0E+08;

            for (int i = 0; i < _numBodies; i++)
            {
                pos[i] = (celestialBodyList[i].Position / size).ToFloat4(1);
                vel[i] = (celestialBodyList[i].Velocity / size).ToFloat4(1);
                //vel[i] = new float4(0, 0, 0, 0);
            }
        }

        public static void PreparePositionAndVelocityForCelestialBodyWithMass(int _numMassBodies, out float4[] pos, out float4[] vel)
        {
            pos = new float4[_numMassBodies];
            vel = new float4[_numMassBodies];

            float size = 1.0f; //(float)6.0E+08;

            for (int i = 0; i < _numMassBodies; i++)
            {
                double mass = celestialBodyWithMassList[i].Mass;
                pos[i] = (celestialBodyWithMassList[i].Position / size).ToFloat4(mass);
                vel[i] = (celestialBodyWithMassList[i].Velocity / size).ToFloat4(mass);
            }
        }

        public static void PreparePositionAndVelocity(int _numBodies, int _numMassBodies, out float4[] pos, 
            out float4[] celPos, out float4[] celVel, out float4[] celMassPos, out float4[] celMassVel)
        {
            pos = new float4[_numBodies + _numMassBodies];
            celPos = new float4[_numBodies];
            celVel = new float4[_numBodies];
            celMassPos = new float4[_numMassBodies];
            celMassVel = new float4[_numMassBodies];

            PreparePositionAndVelocityForCelestialBody(_numBodies, out celPos, out celVel);
            PreparePositionAndVelocityForCelestialBodyWithMass(_numMassBodies, out celMassPos, out celMassVel);

            float size = (float)1.0E+09;

            //pos = celPos.Concat(celMassPos).ToArray();
            pos = celPos.ToArray();
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = new float4(pos[i].x / size, pos[i].y / size, pos[i].z / size + 50, pos[i].w + 1);
                //celVel[i] = new float4(0, 0, 0, 0);
            }
        }
    }
}
