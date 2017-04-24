using System.Collections.Generic;

namespace ZTEChallenge
{
    class Path
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
        public bool IsUseless(List<Path> paths)
        {
            foreach (Path path in paths)
            {
                if (IsUseless(path)) { return true; }
            }
            return false;
        }
        public bool IsUseless(Path path)
        {
            return (Distance >= path.Distance && Step > path.Step) ||
                (Distance > path.Distance && Step >= path.Step) ? true : false;
        }
        public bool EqualsInValue(Path path)
        {
            return Distance == path.Distance && Step == path.Step && !IsEquals(path) ? true : false;
        }
    }
}
