using System.Collections.Generic;

namespace ZTEChallenge
{
    public class Path
    {
        public Path(int from, int to, int distance)
        {
            From = from;
            To = to;
            Distance = distance;
            Step = 1;
            if (to == from) { Step = 0; }
            PathStrNotIncludeTo = from.ToString() + " ";

        }
        public Path(Path p1, Path p2)
        {
            From = p1.From;
            To = p2.To;
            Distance = p1.Distance + p2.Distance;
            Step = p1.Step + p2.Step;
            PathStrNotIncludeTo = p1.PathStrNotIncludeTo + p2.PathStrNotIncludeTo;
        }
        public Path(Path p)
        {
            From = p.From;
            To = p.To;
            Distance = p.Distance;
            Step = p.Step;
            PathStrNotIncludeTo = p.PathStrNotIncludeTo;
        }
        public int From { get; set; }
        public int To { get; set; }
        public int Distance { get; set; }
        public int Step { get; set; }
        public string PathStrNotIncludeTo { get; set; }

        public string PathStr
        {
            get
            {
                return PathStrNotIncludeTo + To.ToString() + " ";
            }
        }
        public bool ContainPath(Path path)
        {
            return PathStr.Contains(path.PathStr);
        }
        public bool ContainPoint(int point)
        {
            return PathStr.Contains(point.ToString() + " ");
        }
        public bool IsEquals(Path path)
        {
            return PathStr == path.PathStr;
        }
        //当前在List<path>中是无用路径
        public bool IsUseless(List<Path> paths)
        {
            foreach (Path path in paths)
            {
                if (IsUseless(path)) { return true; }
            }
            return false;
        }
        //权值均大于另外一条路径且不同时相等的为无用路径
        public bool IsUseless(Path path)
        {
            return (Distance >= path.Distance && Step > path.Step) ||
                (Distance > path.Distance && Step >= path.Step) ? true : false;
        }

        //Distance优先,比较两个路径.Distance相同,Step少,返回true.Distance,Step均相同且不是相等路径返回true.
        public bool LessValueOfDistance(Path path)
        {
            if (path == null) { return true; }
            if (Distance > path.Distance) { return false; }
            if (Distance == path.Distance && Step > path.Step) { return false; }
            if (IsEquals(path)) { return false; }
            return true;
        }
        //Step优先,比较两个路径.Step相同,Distance少,返回true.Distance,Step均相同且不是相等路径返回true.
        public bool LessValueOfStep(Path path)
        {
            if (path == null) { return true; }
            if (Step > path.Step) { return false; }
            if (Step == path.Step && Distance > path.Distance) { return false; }
            if (IsEquals(path)) { return false; }
            return true;
        }
        
        //权值均相同但是不是同一条路径返回true
        public bool EqualsInValue(Path path)
        {
            return Distance == path.Distance && Step == path.Step && !IsEquals(path) ? true : false;
        }


        




    }
}
