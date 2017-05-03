using System.Collections.Generic;

namespace ZTEChallenge
{
    public class PathsContainer
    {
        public PathsContainer(int from, int to)
        {
            FromPoint = from;
            ToPoint = to;
            Paths = new List<Path>();
        }

        public PathsContainer(PathsContainer pc)
        {
            FromPoint = pc.FromPoint;
            ToPoint = pc.ToPoint;
            Paths = new List<Path>(pc.Paths);
        }

        public int FromPoint { get; set; }
        public int ToPoint { get; set; }
        public List<Path> Paths { get; set; }


        //可以添加一些方法和属性
        public void RemoveAllExceptMinPath()
        {
            for(int i = Paths.Count; i > 0; i--)
            {
                if (Paths[i-1] != MinDistancePath && Paths[i - 1] != MinStepPath)
                {
                    Paths.RemoveAt(i-1);
                }
            }
        }
        public Path MinDistancePath { get; set; }
        public Path MinStepPath { get; set; }
    }
}
