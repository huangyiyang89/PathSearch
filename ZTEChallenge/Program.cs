using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;


namespace ZTEChallenge
{
    class Program
    {

        static void Main(string[] args)
        {
            Map newMap = new Map("TestData/input.txt");
            newMap.AddMustNotPassAnyWayPath(11, 12);
            newMap.AddMustPassPoint(7);
            newMap.AddMustPassPoint(12);
            newMap.AddMustPassEitherWayPath(2, 4);
            newMap.AddMustPassEitherWayPath(13, 14);

            //关闭保留相同权值路径可大幅提高搜索速度,但只能获得部分最优解,在不需要多个最优解时可关闭.
            //newMap.RetainSameValuePaths = false;
            long time0 = DateTime.Now.Ticks;

            newMap.ExecSearchUsefulMatrix();


            Console.WriteLine("搜索有效路径用时: " + ((DateTime.Now.Ticks - time0) / 10000).ToString() + "毫秒.");
            //打印全部点到点有效路径
            // PrintAllUsefulPaths(newMap);


          
            time0 = DateTime.Now.Ticks;
            //newMap.RemoveAlllllll();
            newMap.ExecSearchAllTargetPaths(0, 17);
            //newMap.ExecSearchAllTargetPaths3(0, 17);
            //newMap.ExecTwice();
            Console.WriteLine("搜索目标路径用时: " + ((DateTime.Now.Ticks - time0) / 10000).ToString() + "毫秒.");

            PrintTargetPaths(newMap);
            

            Console.ReadKey();
        }


        static void PrintPathList(List<Path> paths)
        {
            foreach (Path p in paths)
            {
                Console.WriteLine("Path: " + p.PathStr + " Distance=" + p.Distance.ToString() + " Step=" + p.Step.ToString());
            }
        }
        static void PrintTargetPaths(Map map)
        {
            Console.Write("必经点: ");
            if (map.MustPassPoints.Count == 0) { Console.Write("无"); }
            foreach (var p in map.MustPassPoints)
            {
                Console.Write(p.ToString() + " ");
            }
            Console.Write("  必经路径(任意单向经过): ");
            if (map.MustPassEitherWayPaths.Count == 0) { Console.Write("无"); }
            for (int i = 0; i < map.MustPassEitherWayPaths.Count; i += 2)
            {
                Console.Write(map.MustPassEitherWayPaths[i].From.ToString() + "-" + map.MustPassEitherWayPaths[i].To.ToString() + " ");
            }
            Console.Write("  禁止的路径: ");
            foreach (Path p in map.MustNotPassAnyWayPaths)
            {
                Console.Write(p.From.ToString() + "->" + p.To.ToString() + " ");
            }

            Console.WriteLine();
            Console.WriteLine("全部要求路径:");
            PrintPathList(map.TargetPathsContainer.Paths);
        }
        static void PrintAllUsefulPaths(Map map)
        {
            for (int i = 0; i < map.TableSize; i++)
            {
                for (int j = 0; j < map.TableSize; j++)
                {
                    var PathList = map.Matrix[i, j].Paths;
                    if (PathList.Count > 0)
                    {
                        Console.WriteLine("All Useful paths From:" + i.ToString() + " To:" + j.ToString());
                        PrintPathList(PathList);
                        Console.WriteLine("-----------------------------------------------------------------");
                    }
                }
            }
            Console.Write("禁止的路径: ");
            foreach (Path p in map.MustNotPassAnyWayPaths)
            {
                Console.Write(p.From.ToString() + "-" + p.To.ToString() + " ");
            }
            Console.WriteLine("");
        }
    }
}

