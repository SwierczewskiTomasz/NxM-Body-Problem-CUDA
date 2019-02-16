using System;
using System.Collections.Generic;
using System.Diagnostics;
using Alea;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using NxMBodyProblemCUDA.Parser;
using NxMBodyProblemCUDA.Structures;
using NxMBodyProblemCUDA.Kernels;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace NxMBodyProblemCUDA
{
    class Program
    {
        public class SimWindow : GameWindow
        {
            private readonly int _numBodies;
            private readonly int _numMassBodies;
            private readonly float _deltaTime;
            private readonly float _softeningSquared;
            private readonly float _damping;
            private readonly Gpu _gpu;
            private LeapFrogFloat4Simulator _simulator;
            private readonly uint[] _buffers;
            private readonly DeviceMemory<float4> _posVel;
            private readonly DeviceMemory<float4> _posMassVel;
            private readonly IntPtr[] _resources;
            private readonly Stopwatch _stopwatch;
            private readonly int _fpsCalcLag;
            private int _frameCounter;
            private static int count = -1;

            static void Help()
            {
                Console.WriteLine("Press these keys:");
                Console.WriteLine("[ESC]    : Exit");
                Console.WriteLine("S        : Switch to next simulator");
            }

            void Description()
            {
                var time = _stopwatch.ElapsedMilliseconds;
                var fps = ((float)_frameCounter) * 1000.0 / ((float)time);
                //Title = $"bodies {_numBodies}, {_gpu.Device.Name} {_gpu.Device.Arch} {_gpu.Device.Cores} cores, {_simulator.Description}, fps {fps}";
                Title = "test";
                _stopwatch.Restart();
            }

            void LockPos(Action<deviceptr<float4>, deviceptr<float4>,
                                deviceptr<float4>, deviceptr<float4>,
                                deviceptr<float4>, deviceptr<float4>,
                                deviceptr<float4>, deviceptr<float4>,
                                deviceptr<float4>, deviceptr<float4>,
                                deviceptr<float4>, deviceptr<float4>> f)
            {
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[0],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[1],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[2],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[3],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[4],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[5],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[6],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[7],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[8],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[9],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[10],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_READ_ONLY));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceSetMapFlags(_resources[11],
                    (uint)CUDAInterop.CUgraphicsMapResourceFlags_enum.CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITE_DISCARD));
                CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsMapResourcesEx(12u, _resources, IntPtr.Zero));

                var bytes = IntPtr.Zero;
                var handle0 = IntPtr.Zero;
                var handle1 = IntPtr.Zero;
                var handle2 = IntPtr.Zero;
                var handle3 = IntPtr.Zero;
                var handle4 = IntPtr.Zero;
                var handle5 = IntPtr.Zero;
                var handle6 = IntPtr.Zero;
                var handle7 = IntPtr.Zero;
                var handle8 = IntPtr.Zero;
                var handle9 = IntPtr.Zero;
                var handle10 = IntPtr.Zero;
                var handle11 = IntPtr.Zero;
                unsafe
                {
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle0, &bytes,
                                                                                          _resources[0]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle1, &bytes,
                                                                                          _resources[1]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle2, &bytes,
                                                                                          _resources[2]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle3, &bytes,
                                                                                          _resources[3]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle4, &bytes,
                                                                                          _resources[4]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle5, &bytes,
                                                                                          _resources[5]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle6, &bytes,
                                                                                          _resources[6]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle7, &bytes,
                                                                                          _resources[7]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle8, &bytes,
                                                                                          _resources[8]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle9, &bytes,
                                                                                          _resources[9]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle10, &bytes,
                                                                                          _resources[10]));
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsResourceGetMappedPointer(&handle11, &bytes,
                                                                                          _resources[11]));
                }
                var celPosScaled0 = new deviceptr<float4>(handle0);
                var celPosScaled1 = new deviceptr<float4>(handle1);
                var celPos0 = new deviceptr<float4>(handle2);
                var celPos1 = new deviceptr<float4>(handle3);
                var celVel0 = new deviceptr<float4>(handle4);
                var celVel1 = new deviceptr<float4>(handle5);
                var celMassPosScaled0 = new deviceptr<float4>(handle6);
                var celMassPosScaled1 = new deviceptr<float4>(handle7);
                var celMassPos0 = new deviceptr<float4>(handle8);
                var celMassPos1 = new deviceptr<float4>(handle9);
                var celMassVel0 = new deviceptr<float4>(handle10);
                var celMassVel1 = new deviceptr<float4>(handle11);
                try
                {
                    f(celPosScaled0, celPosScaled1, celPos0, celPos1, celVel0, celVel1, celMassPosScaled0, celMassPosScaled1, celMassPos0, celMassPos1, celMassVel0, celMassVel1);
                }
                finally
                {
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsUnmapResourcesEx(12u, _resources, IntPtr.Zero));
                }
            }

            [DllImport("USER32.DLL")]
            public static extern IntPtr FindWindow(String className, String windowName);

            [DllImport("USER32.DLL", SetLastError = true)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int left, int top, int width, int height, uint flags);

            public SimWindow(Gpu gpu) : base(1920, 1080, GraphicsMode.Default, "Gravitational n-body simulation")
            {
                Parser.Parser.LoadData();

                //this.ClientSize = new Size(6144, 3160);

                _numBodies = Parser.Parser.celestialBodyList.Count;
                _numMassBodies = Parser.Parser.celestialBodyWithMassList.Count;

                _deltaTime = 1f;
                _softeningSquared = 12500000000000f;
                _gpu = gpu;

                _stopwatch = Stopwatch.StartNew();
                _fpsCalcLag = 128;
                _frameCounter = 0;

                _simulator = new LeapFrogFloat4Simulator(gpu, 256);

                _buffers = new uint[12];
                for (var i = 0; i < _buffers.Length; i++)
                {
                    _buffers[i] = 0;
                }
                GL.GenBuffers(_buffers.Length, _buffers);

                //Generate buffers for pos0 i pos1
                for (var i = 0; i < 12; i++)
                {
                    var buffer = _buffers[i];
                    GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Gpu.SizeOf<float4>() * (_numBodies)),
                                  IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    var size = 0;
                    unsafe
                    {
                        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, &size);
                    }
                    if (size != Gpu.SizeOf<float4>() * (_numBodies))
                    {
                        throw new Exception("Pixel Buffer Object allocation failed!");
                    }
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGLRegisterBufferObject(buffer));
                }

                _resources = new IntPtr[_buffers.Length];
                for (var i = 0; i < _buffers.Length; i++)
                {
                    var res = IntPtr.Zero;
                    unsafe
                    {
                        CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsGLRegisterBuffer(&res, _buffers[i], 0u));
                    }
                    _resources[i] = res;
                }

                _posVel = _gpu.AllocateDevice<float4>(_numBodies);
                _posMassVel = _gpu.AllocateDevice<float4>(_numMassBodies);

                float4[] hostPos;
                float4[] hostCelPos;
                float4[] hostCelVel;
                float4[] hostCelMassPos;
                float4[] hostCelMassVel;

                Parser.Parser.PreparePositionAndVelocity(_numBodies, _numMassBodies, out hostPos, out hostCelPos, out hostCelVel, out hostCelMassPos, out hostCelMassVel);


                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => Gpu.Copy(hostPos, 0L, _gpu, pos1, hostPos.Length));
                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => Gpu.Copy(hostCelPos, 0L, _gpu, cel1, hostCelPos.Length));
                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => Gpu.Copy(hostCelMassPos, 0L, _gpu, celMass1, hostCelMassPos.Length));
                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => Gpu.Copy(hostCelVel, 0L, _gpu, celVel1, hostCelVel.Length));
                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => Gpu.Copy(hostCelMassVel, 0L, _gpu, celMassVel1, hostCelMassVel.Length));


                Help();
                Description();
            }

            public void SwapPos()
            {
                var buffer = _buffers[0];
                _buffers[0] = _buffers[1];
                _buffers[1] = buffer;

                buffer = _buffers[2];
                _buffers[2] = _buffers[3];
                _buffers[3] = buffer;

                buffer = _buffers[4];
                _buffers[4] = _buffers[5];
                _buffers[5] = buffer;

                buffer = _buffers[6];
                _buffers[6] = _buffers[7];
                _buffers[7] = buffer;

                buffer = _buffers[8];
                _buffers[8] = _buffers[9];
                _buffers[9] = buffer;

                buffer = _buffers[10];
                _buffers[10] = _buffers[11];
                _buffers[11] = buffer;

                var resource = _resources[0];
                _resources[0] = _resources[1];
                _resources[1] = resource;

                resource = _resources[2];
                _resources[2] = _resources[3];
                _resources[3] = resource;

                resource = _resources[4];
                _resources[4] = _resources[5];
                _resources[5] = resource;

                resource = _resources[6];
                _resources[6] = _resources[7];
                _resources[7] = resource;

                resource = _resources[8];
                _resources[8] = _resources[9];
                _resources[9] = resource;

                resource = _resources[10];
                _resources[10] = _resources[11];
                _resources[11] = resource;
            }

            protected override void Dispose(bool disposing)
            {
                foreach (var resource in _resources)
                {
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGraphicsUnregisterResource(resource));
                }
                foreach (var buffer in _buffers)
                {
                    CUDAInterop.cuSafeCall(CUDAInterop.cuGLUnregisterBufferObject(buffer));
                }
                if (_buffers.Length > 0)
                {
                    GL.DeleteBuffers(_buffers.Length, _buffers);
                }
                if (disposing)
                {
                    _posVel.Dispose();
                }
                base.Dispose(disposing);
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);
                VSync = VSyncMode.Off;
                GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f); // black as the universe
                GL.Enable(EnableCap.DepthTest);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                var projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4.0f,
                                                                      (float)Width / Height, 1.0f, 64.0f);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref projection);
            }

            protected override void OnRenderFrame(FrameEventArgs e)
            {
                // var TOP = new IntPtr(0);
                // uint SHOWWINDOW = 0x0040, NOCOPYBITS = 0x0100, NOSENDCHANGING = 0x0400;
                // var hwnd = FindWindow(null, "test");
                // //SetWindowPos(hwnd, TOP, 0, 0, 6144, 3160, NOCOPYBITS | NOSENDCHANGING | SHOWWINDOW);
                // SetWindowPos(hwnd, TOP, 0, 0, 6160, 3199, NOCOPYBITS | NOSENDCHANGING | SHOWWINDOW);

                base.OnRenderFrame(e);

                _frameCounter++;
                if (_frameCounter >= _fpsCalcLag)
                {
                    Description();
                    _frameCounter = 0;
                }

                SwapPos();
                //LockPos((pos0, pos1) =>
                //    _simulator.Integrate(pos1, pos0, _vel.Ptr, _numBodies, _deltaTime, _softeningSquared, _damping));
                //newPos, oldPos, newCel, oldCel, newCelMass, oldCelMass, vel, numBodies, numMassBodies, softeningSquared, deltaTime;
                LockPos((pos0, pos1, cel0, cel1, celVel0, celVel1, celMassScaled0, celMassScaled1, celMass0, celMass1, celMassVel0, celMassVel1) => _simulator.Integrate(pos1, pos0, cel1, cel0,
                    celVel1, celVel0, celMassScaled1, celMassScaled0, celMass1, celMass0,
                    celMassVel1, celMassVel0, _numBodies, _numMassBodies, _softeningSquared, 1.0f));

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
                //GL.Clear(ClearBufferMask.None);
                var modelview = Matrix4.LookAt(OpenTK.Vector3.Zero, OpenTK.Vector3.UnitZ, OpenTK.Vector3.UnitY);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref modelview);

                GL.Color3(1.0f, 215.0f / 255.0f, 0.0f); // golden as the stars
                GL.PointSize(1.0f);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _buffers[1]);
                GL.VertexPointer(4, VertexPointerType.Float, 0, 0);
                GL.DrawArrays(PrimitiveType.Points, 0, _numBodies);

                GL.Color3(1.0f, 0.0f, 0.0f);
                GL.PointSize(5.0f);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _buffers[7]);
                GL.VertexPointer(4, VertexPointerType.Float, 0, 0);
                GL.DrawArrays(PrimitiveType.Points, 0, _numBodies);

                GL.DisableClientState(ArrayCap.VertexArray);

                GL.Finish();
                SwapBuffers();

                //Bitmap b;
                //danger:
                //try
                //{
                //    b = new Bitmap(Width, Height);
                //}
                //catch(Exception exc)
                //{
                //    goto danger;
                //}
                //var bits = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //GL.ReadPixels(0, 0, Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);
                //b.UnlockBits(bits);
                //string name = count++.ToString() + ".png";
                ////string name = DateTime.Now.ToString("hh-mm-ss-ffffff") + ".png";
                //b.Save(name, ImageFormat.Png);
                ////GL.ReadPixels(0, 0, this.Width, this.Height, PixelFormat.Rgb, PixelType.
                //if (count > 2000)
                //    throw new Exception("THE END!");
                //b.Dispose();
            }
        }

        private static void Main()
        {
            var gpu = Gpu.Default;
            gpu.Context.SetCurrent();
            using (var window = new SimWindow(gpu))
            {
                window.Run();
            }
        }
    }
}
