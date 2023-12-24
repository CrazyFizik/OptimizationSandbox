using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OptimizationSandbox.Tasks.COA
{
    public static class Deafult
    {
        public static Design CraeteDeafult()
        {
            //channels
            Channel channelA = new Channel(
                Color.Green,
                8,
                10,
                new Vector2(10, 10),
                new Vector2(10, 90),
                0,
                2);

            Channel channelB = new Channel(
                Color.Purple,
                10,
                9,
                new Vector2(90, 10),
                new Vector2(90, 90),
                0,
                2);

            Channel[] channels = new Channel[2];
            channels[0] = channelA;
            channels[1] = channelB;

            //Borders
            Vector2[] borderVertexes = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0, 100),
                new Vector2(100,100),
                new Vector2(100,0)
            };
            Border border = new Border(borderVertexes);

            //Obstacles
            Screw screw = new Screw(Color.WhiteSmoke, new Vector2(50, 90), 8);

            Vertexes vertexes = new Vertexes();
            vertexes.Add(new Vector2(0, 0));
            vertexes.Add(new Vector2(0, 40));
            vertexes.Add(new Vector2(10, 40));
            vertexes.Add(new Vector2(10, 0));
            Polygon polygon = new Polygon(Color.WhiteSmoke, new Vector2(45, 30), vertexes);

            Obstacle[] obstacles = new Obstacle[]
            {
                screw,
                polygon
            };

            Design design = new Design(channels, obstacles, border);

            return design;
        }

        public static Design CraeteDeafult2()
        {
            //channels
            int[][] ccTXA = new int[][]
            {
                new int[2] {0 ,2 },
                new int[2] {0, 3 },
                new int[2] {3, 5 },
                new int[2] {5, 7 }
            };
            Channel TXA = new Channel(
                Color.Green,
                8,
                20.5f,
                new Vector2(64.2f, 53.8f),
                new Vector2(112.3f, 124.5f),
                0,
                4,
                ccTXA,
                0,
                8);

            int[][] ccRXA = new int[][]
            {
                new int[2] {0 ,2 },
                new int[2] {0, 3 }
            };
            Channel RXA = new Channel(
                Color.Purple,
                6,
                17.5f,
                new Vector2(129.5f, 65.4f),
                new Vector2(185.6f, 118.1f),
                0,
                5,
                ccRXA,
                0,
                7);

            int[][] ccRXB = new int[][]
            {
                new int[2] {1 ,3 },
                new int[2] {1, 4 }
            };
            Channel RXB = new Channel(
                Color.Yellow,
                6,
                18,
                new Vector2(241.2f, 30),
                new Vector2(241.1f, 130.9f),
                0,
                5,
                ccRXB,
                0,
                7);

            List<Channel> list = new List<Channel>();
            list.Add(TXA);
            list.Add(RXA);
            list.Add(RXB);
            Channel[] channels = list.ToArray();

            //Channel[] channels = new Channel[3];
            //channels[0] = TXA;
            //channels[1] = RXA;
            //channels[2] = RXB;

            //Channel[] channels = new Channel[2];
            //channels[0] = TXA;
            //channels[1] = RXA;

            //Borders
            Vector2[] borderVertexes = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0, 148.6f),
                new Vector2(258.6f,148.6f),
                new Vector2(258.6f,0)
            };
            Border border = new Border(borderVertexes);

            //Obstacles
            //big screw
            Screw screw1 = new Screw(Color.WhiteSmoke, new Vector2(0.2f, 87.5f), 13.5f);
            Screw screw2 = new Screw(Color.WhiteSmoke, new Vector2(258.7f, 75.0f), 13.5f);
            Screw screw3 = new Screw(Color.WhiteSmoke, new Vector2(0.2f, 0.2f), 13.5f);
            Screw screw4 = new Screw(Color.WhiteSmoke, new Vector2(258.7f, 0.2f), 13.5f);
            Screw screw5 = new Screw(Color.WhiteSmoke, new Vector2(129.0f, 0.2f), 13.5f);
            //small screw
            Screw screw6 = new Screw(Color.WhiteSmoke, new Vector2(214f, 132f), 5f);
            Screw screw7 = new Screw(Color.WhiteSmoke, new Vector2(214f, 118f), 5f);
            //polygon 1
            Vertexes vertexes1 = new Vertexes();
            vertexes1.Add(new Vector2(65.1f, 19.3f));
            vertexes1.Add(new Vector2(110.1f, 19.4f));
            vertexes1.Add(new Vector2(110.1f, 4.8f));
            vertexes1.Add(new Vector2(65.1f, 4.7f));
            Polygon polygon1 = new Polygon(Color.WhiteSmoke, new Vector2(0, 0), vertexes1);
            //polygon 2
            Vertexes vertexes2 = new Vertexes();
            vertexes2.Add(new Vector2(86.7f, 44.0f));
            vertexes2.Add(new Vector2(131.6f, 44.0f));
            vertexes2.Add(new Vector2(131.6f, 24.5f));
            vertexes2.Add(new Vector2(86.7f, 24.5f));
            Polygon polygon2 = new Polygon(Color.WhiteSmoke, new Vector2(0, 0), vertexes2);
            //polygon 3
            Vertexes vertexes3 = new Vertexes();
            vertexes3.Add(new Vector2(176.9f, 28.8f));
            vertexes3.Add(new Vector2(221.6f, 28.8f));
            vertexes3.Add(new Vector2(221.6f, 9.3f));
            vertexes3.Add(new Vector2(176.9f, 9.3f));
            Polygon polygon3 = new Polygon(Color.WhiteSmoke, new Vector2(0, 0), vertexes3);

            Obstacle[] obstacles = new Obstacle[]
            {
                screw1,
                screw2,
                screw3,
                screw4,
                screw5,
                screw6,
                screw7,
                polygon1,
                polygon2,
                polygon3
            };

            Design design = new Design(channels, obstacles, border);

            return design;
        }
    }
}
