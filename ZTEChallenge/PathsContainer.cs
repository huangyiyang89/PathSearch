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

        public int FromPoint { get; set; }
        public int ToPoint { get; set; }
        public List<Path> Paths { get; set; }


        //可以添加一些方法和属性
       

        public Path MinDistancePath { get; set; }
        public Path MinStepPath { get; set; }
    }
}
