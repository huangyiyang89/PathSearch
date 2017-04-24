using System.Collections.Generic;

namespace ZTEChallenge
{
    class PathsContainer
    {
        public PathsContainer(int from, int to)
        {
            FromPoint = from;
            ToPoint = to;
            Paths = new List<Path>();
        }

        public int FromPoint { get; set; }
        public int ToPoint { get; set; }
        public List<Path> Paths { get; set; }


        //可以添加一些方法和属性
        public int MinDistance
        {
            get
            {
                int min = 9999;
                foreach (Path p in Paths)
                {
                    min = p.Distance < min ? p.Distance : min;

                }
                return min;
            }

        }
        public int MinStep
        {
            get
            {
                int min = 9999;
                foreach (Path p in Paths)
                {
                    min = p.Distance < min ? p.Step : min;

                }
                return min;
            }
        }

    }
}
