using BenTools.Mathematics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoronoiGL
{
    class VoronoiGL
    {
        private const float DEG2RAD = 3.14159f / 180f;
        private static Random Randomizer = new Random();

        [STAThread]
        static void Main(string[] args)
        {
            List<Vector> DataPoints = new List<Vector>();
            List<Vector> PointsDirection = new List<Vector>();
            List<Color> PointsColor = new List<Color>();

            for (Int32 i = 0; i < 200; i++)
            {
                DataPoints.Add(new Vector(Randomizer.NextDouble(), Randomizer.NextDouble()));
                PointsDirection.Add(new Vector(Randomizer.NextDouble() * (Randomizer.Next(10) > 5 ? -1 : 1),
                                               Randomizer.NextDouble() * (Randomizer.Next(10) > 5 ? -1 : 1)));
                PointsColor.Add(Color.FromArgb(200+(byte)Randomizer.Next(35), 200+(byte)Randomizer.Next(35), 200+(byte)Randomizer.Next(35)));
                Double s = Math.Sqrt(0.0000001 / PointsDirection.Last().SquaredLength);
                PointsDirection.Last().Multiply(s);
            }

            using (GameWindow game = new GameWindow(800, 800))
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, 1, 0, 1, 0.0, 4.0);
                    GL.ClearColor(Color.Ivory);

                    VoronoiGraph vorGraph = Fortune.ComputeVoronoiGraph(DataPoints);
                    DrawPolyEdges(vorGraph, DataPoints, PointsColor);
                    DrawPoints(DataPoints);
                    DrawEdges(vorGraph);
                    game.SwapBuffers();

                    for (Int32 j = 0; j < DataPoints.Count; j++)
                    {
                        DataPoints[j][0] += PointsDirection[j][0];
                        DataPoints[j][1] += PointsDirection[j][1];
                    }
                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }

        static void DrawPoints(List<Vector> dataPoints)
        {
            Color pointColor = Color.Black;

            foreach (Vector v in dataPoints)
            {
                DrawCircle(0.0025f, (float)v[0], (float)v[1], pointColor);
            }
        }

        static void DrawCircle(float radius, float pX, float pY, Color color)
        {
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(color);

            for (int i = 0; i <= 360; i++)
            {
                float degInRad = i * DEG2RAD;
                GL.Vertex2(Math.Cos(degInRad) * radius + pX, Math.Sin(degInRad) * radius + pY);
            }

            GL.End();
        }

        static void DrawEdges(VoronoiGraph vorGraph)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Black);

            foreach (VoronoiEdge ve in vorGraph.Edges)
            {
                if (Double.IsInfinity(ve.VVertexA[0]))
                {
                    Double s = Math.Sqrt(2 / ve.DirectionVector.SquaredLength);
                    ve.DirectionVector.Multiply(s);
                    GL.Vertex2(ve.DirectionVector[0] + ve.VVertexB[0], ve.VVertexB[1] + ve.DirectionVector[1]);
                    GL.Vertex2(ve.VVertexB[0], ve.VVertexB[1]);
                }
                else if (Double.IsInfinity(ve.VVertexB[0]))
                {
                    Double s = Math.Sqrt(2 / ve.DirectionVector.SquaredLength);
                    ve.DirectionVector.Multiply(s);
                    GL.Vertex2(ve.VVertexA[0], ve.VVertexA[1]);
                    GL.Vertex2(ve.DirectionVector[0] + ve.VVertexA[0], ve.VVertexA[1] + ve.DirectionVector[1]);
                }
                else
                {
                    GL.Vertex2(ve.VVertexA[0], ve.VVertexA[1]);
                    GL.Vertex2(ve.VVertexB[0], ve.VVertexB[1]);
                }
            }

            GL.End();
        }

        static void DrawPolyEdges(VoronoiGraph vorGraph, List<Vector> dataPoints, List<Color> pointsColor)
        {
            for (Int32 i=0; i<dataPoints.Count; i++)
            {   
                Vector point = dataPoints[i];
                List<VoronoiEdge> pointEdges = vorGraph.Edges.Where(x => x.LeftData == point || x.RightData == point).ToList();

                GL.Begin(PrimitiveType.Polygon);
                //  GL.Color3((byte)Randomizer.Next(255), (byte)Randomizer.Next(255), (byte)Randomizer.Next(255));
                GL.Color3(pointsColor[i]);

              //  if (pointEdges.Any(x => x.VVertexA == Fortune.VVInfinite || x.VVertexA == Fortune.VVUnkown ||
               //     x.VVertexB == Fortune.VVInfinite || x.VVertexB == Fortune.VVUnkown)) continue;

                foreach (VoronoiEdge ve in pointEdges)
                {
                    if (Double.IsInfinity(ve.VVertexA[0]))
                    {
                        Double s = Math.Sqrt(2 / ve.DirectionVector.SquaredLength);
                        ve.DirectionVector.Multiply(s);
                        GL.Vertex2(ve.DirectionVector[0] + ve.VVertexB[0], ve.VVertexB[1] + ve.DirectionVector[1]);
                        GL.Vertex2(ve.VVertexB[0], ve.VVertexB[1]);
                    }
                    else if (Double.IsInfinity(ve.VVertexB[0]))
                    {
                        Double s = Math.Sqrt(2 / ve.DirectionVector.SquaredLength);
                        ve.DirectionVector.Multiply(s);
                        GL.Vertex2(ve.VVertexA[0], ve.VVertexA[1]);
                        GL.Vertex2(ve.DirectionVector[0] + ve.VVertexA[0], ve.VVertexA[1] + ve.DirectionVector[1]);
                    }
                    else
                    {
                        GL.Vertex2(ve.VVertexA[0], ve.VVertexA[1]);
                        GL.Vertex2(ve.VVertexB[0], ve.VVertexB[1]);
                    }
                }

                GL.End();
            }
        }
    }
}
