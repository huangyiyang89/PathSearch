﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZTEChallenge
{
    public class Map
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath">解析的文件路径，支持csv,txt</param>
        public Map(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath.ToLower());
            if (extension == ".csv")
            {
                InitializeFromCsv(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileName(filePath));
            }
            else if (extension == ".txt")
            {
                InitializeFromTxt(filePath);
            }
            else
            {
                throw new Exception("不支持的文件类型");
            }
            Infinity = 9999;
            MustPassPoints = new List<int>();
            MustPassEitherWayPaths = new List<Path>();
            MustNotPassAnyWayPaths = new List<Path>();
            RetainSameValuePaths = true;
            Cut = false;

        }

        public int Infinity { get; set; }


        public PathsContainer[,] OriginalMatrix { get; private set; }
        /// <summary>
        /// 临接矩阵,每个点是一个PathContainer实例.
        /// </summary>
        public PathsContainer[,] Matrix { get; private set; }

        public PathsContainer[,] TargetMatrix { get; private set; }

        public PathsContainer TargetPathsContainer { get; set; }

        public int TableSize { get; private set; }

        public List<Path> MustPassEitherWayPaths { get; private set; }

        public List<int> MustPassPoints { get; private set; }

        /// <summary>
        /// 是否在搜索有用路径时保留相同权值的路径,关闭可提高搜索速度以及查找目标路径的速度,默认开启.
        /// </summary> 
        public bool RetainSameValuePaths { get; set; }
        public bool Cut { get; set; }

        public List<Path> MustNotPassAnyWayPaths { get; set; }

        /// <summary>
        /// 添加一条必经路径,从任意一端走均算经过.
        /// </summary>
        /// <param name="pointA">点A</param>
        /// <param name="pointB">点B</param>
        /// 
        public void AddMustPassEitherWayPath(int pointA, int pointB)
        {
            try
            {
                MustPassEitherWayPaths.Add(Matrix[pointA, pointB].Paths[0]);
                MustPassEitherWayPaths.Add(Matrix[pointB, pointA].Paths[0]);
            }
            catch
            {
                throw new Exception("未找到该路径.");
            }
        }

        public void RemoveMustPassEitherWayPath(int pointA, int pointB)
        {
            try
            {
                var path1 = MustPassEitherWayPaths.Where(p => p.From == pointA && p.To == pointB).FirstOrDefault();
                MustPassEitherWayPaths.Remove(path1);
                var path2 = MustPassEitherWayPaths.Where(p => p.From == pointB && p.To == pointA).FirstOrDefault();
                MustPassEitherWayPaths.Remove(path2);
            }
            catch
            {
                throw new Exception("未找到该路径.");
            }
        }

        public void AddMustPassPoint(int point)
        {
            if (!MustPassPoints.Contains(point))
            {
                MustPassPoints.Add(point);
            }
        }

        public void AddMustNotPassAnyWayPath(int pointA, int pointB)
        {
            try
            {
                MustNotPassAnyWayPaths.Add(Matrix[pointA, pointB].Paths[0]);
                MustNotPassAnyWayPaths.Add(Matrix[pointB, pointA].Paths[0]);
            }
            catch
            {
                throw new Exception("未找到该路径.");
            }
        }
        public void RemoveMustNotPassAnyWayPath(int pointA, int pointB)
        {
            try
            {
                var path1 = MustNotPassAnyWayPaths.Where(p => p.From == pointA && p.To == pointB).FirstOrDefault();
                MustPassEitherWayPaths.Remove(path1);
                var path2 = MustNotPassAnyWayPaths.Where(p => p.From == pointB && p.To == pointA).FirstOrDefault();
                MustPassEitherWayPaths.Remove(path2);
            }
            catch
            {
                throw new Exception("未找到该路径.");
            }
        }
        public void ExecSearchUsefulMatrix()
        {
            //从原始临接矩阵复制
            foreach (var r in OriginalMatrix)
            {
                Matrix[r.FromPoint, r.ToPoint] = new PathsContainer(r);
            }

            //忽略MustNotPass路径
            foreach (Path p in MustNotPassAnyWayPaths)
            {
                Matrix[p.From, p.To].Paths.RemoveAll(tp => tp.From == p.From && tp.To == p.To);
            }

            for (int k = 0; k < TableSize; k++)
            {
                for (int i = 0; i < TableSize; i++)
                {
                    if (i == k) { continue; }
                    for (int j = 0; j < TableSize; j++)
                    {
                        if (k == j || i == j) { continue; }
                        //对i->k   k->j中全部路径做比较
                        for (int p1 = 0; p1 < Matrix[i, k].Paths.Count; p1++)
                        {
                            for (int p2 = 0; p2 < Matrix[k, j].Paths.Count; p2++)
                            {
                                //k点不为起点或终点
                                //if (i == k || j == k) { continue; }

                                //起点终点是一个点
                                //if (i == j) { continue; }

                                Path newPath = new Path(Matrix[i, k].Paths[p1], Matrix[k, j].Paths[p2]);

                                bool shouldAdd = true;
                                #region 条件判断
                                for (int p = Matrix[i, j].Paths.Count; p > 0; p--)
                                {
                                    var eachPath = Matrix[i, j].Paths[p - 1];
                                    if (newPath.Distance >= eachPath.Distance && newPath.Step >= eachPath.Step) { shouldAdd = false; }
                                    //是否保留相同权值和步数的路径,保留会影响搜索速度,但能找到多个最优解(如果存在).
                                    if (newPath.EqualsInValue(eachPath) && RetainSameValuePaths) { shouldAdd = true; }
                                    //保存最小Distance路径
                                    if (newPath.LessValueOfDistance(Matrix[i, j].MinDistancePath)) { Matrix[i, j].MinDistancePath = newPath; }
                                    //保存最小Step路径
                                    if (newPath.LessValueOfStep(Matrix[i, j].MinStepPath)) { Matrix[i, j].MinStepPath = newPath; }
                                    //借此循环删除List中无用路径
                                    if (eachPath.IsUseless(newPath)) { Matrix[i, j].Paths.Remove(eachPath); }
                                }
                                #endregion
                                if (shouldAdd) { Matrix[i, j].Paths.Add(newPath); }
                            }
                        }
                    }
                }
            }

            //将MustNotPass路径重新放回
            foreach (Path p in MustNotPassAnyWayPaths)
            {
                Matrix[p.From, p.To].Paths.Insert(0, p);
            }
        }
        //public void ExecTwice()
        //{
        //    TargetMatrix = GetTargetMatrix(Matrix);
        //    List<int> shouldPassPoints = GetShouldPassPoints();

        //    foreach (int k in shouldPassPoints)
        //    {
        //        foreach (int i in shouldPassPoints)
        //        {
        //            if (k == i) { continue; }
        //            foreach (int j in shouldPassPoints)
        //            {
        //                if (j == i) { continue; }
        //                bool next = false;
        //                foreach (Path p in TargetMatrix[i, j].Paths)
        //                {
        //                    if (p.ContainPoint(k)) { next = true; }
        //                }
        //                if (next) { continue; }
        //                for (int p1 = 0; p1 < TargetMatrix[i, k].Paths.Count; p1++)
        //                {
        //                    for (int p2 = 0; p2 < TargetMatrix[k, j].Paths.Count; p2++)
        //                    {
        //                        //k点不为起点或终点
        //                        if (i == k || j == k) { continue; }

        //                        //起点终点是一个点
        //                        if (i == j) { continue; }

        //                        Path newPath = new Path(TargetMatrix[i, k].Paths[p1], TargetMatrix[k, j].Paths[p2]);

        //                        bool shouldAdd = true;
        //                        #region 条件判断
        //                        //for (int p = Matrix[i, j].Paths.Count; p > 0; p--)
        //                        //{
        //                        //    var eachPath = Matrix[i, j].Paths[p - 1];
        //                        //    if (newPath.Distance >= eachPath.Distance && newPath.Step >= eachPath.Step) { shouldAdd = false; }
        //                        //    //是否保留相同权值和步数的路径,保留会影响搜索速度,但能找到多个最优解(如果存在).
        //                        //    if (newPath.EqualsInValue(eachPath) && RetainSameValuePaths) { shouldAdd = true; }
        //                        //    //借此循环删除List中无用路径
        //                        //    if (eachPath.IsUseless(newPath)) { Matrix[i, j].Paths.Remove(eachPath); }
        //                        //    //保存最小Distance路径
        //                        //    if (newPath.LessValueOfDistance(Matrix[i, j].MinDistancePath)) { Matrix[i, j].MinDistancePath = newPath; }
        //                        //    //保存最小Step路径
        //                        //    if (newPath.LessValueOfStep(Matrix[i, j].MinStepPath)) { Matrix[i, j].MinStepPath = newPath; }

        //                        //}
        //                        #endregion
        //                        if (shouldAdd) { TargetMatrix[i, j].Paths.Add(newPath); }
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    for (int k = 0; k < TableSize; k++)
        //    {
        //        for (int i = 0; i < TableSize; i++)
        //        {
        //            for (int j = 0; j < TableSize; j++)
        //            {
        //                if (TargetMatrix[i, j] == null || TargetMatrix[i, k] == null || TargetMatrix[k, j] == null) { continue; }
        //                //对i->k   k->j中全部路径做比较
        //                for (int p1 = 0; p1 < TargetMatrix[i, k].Paths.Count; p1++)
        //                {
        //                    for (int p2 = 0; p2 < TargetMatrix[k, j].Paths.Count; p2++)
        //                    {
        //                        //k点不为起点或终点
        //                        if (i == k || j == k) { continue; }

        //                        //起点终点是一个点
        //                        if (i == j) { continue; }

        //                        Path newPath = new Path(TargetMatrix[i, k].Paths[p1], TargetMatrix[k, j].Paths[p2]);

        //                        bool shouldAdd = true;
        //                        #region 条件判断

        //                        #endregion
        //                        if (shouldAdd) { TargetMatrix[i, j].Paths.Add(newPath); }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public void ExecSearchAllTargetPaths(int s, int e)
        {
            TargetPathsContainer = new PathsContainer(s, e);
            TargetPathsContainer.Paths.Clear();
            //List<int> ShouldPassPoints = GetShouldPassPoints();
            //if (ShouldPassPoints.Count == 0)
            //{
            //    TargetPathsContainer.Paths.AddRange(Matrix[s, e].Paths);
            //    return;
            //}
            //ShouldPassPoints.Remove(e);
            //ShouldPassPoints.Remove(s);
            List<int> mustPassPoints = new List<int>(MustPassPoints);
            List<Path> mustPassPaths = new List<Path>(MustPassEitherWayPaths);
            DfsAllMustPassPaths(s, e, mustPassPoints, mustPassPaths);
        }

        //public void ExecSearchAllTargetPaths3(int s, int e)
        //{

        //    //PathsContainer[,] matrix=GetTargetMatrix(Matrix);
        //    TargetMatrix = GetTargetMatrix(Matrix);
        //    TargetPathsContainer = new PathsContainer(s, e);
        //    TargetPathsContainer.Paths.Clear();
        //    List<List<int>> mustPassPoints = GetMustPassPoinsOrderByBothValue(s, Matrix);
        //    DfsNew(s, e, mustPassPoints);

        //}

        //public void DfsNew(int from, int end, List<List<int>> RemainPoints, Path currentPath = null)
        //{
        //    if (RemainPoints.Count == 0)
        //    {
        //        foreach (Path endPath in TargetMatrix[from, end].Paths)
        //        {
        //            var newPath = new Path(currentPath, endPath);

        //            bool shouldAdd = true;
        //            //判断是否经过必经路径
        //            for (int i = 0; i < MustPassEitherWayPaths.Count; i = i + 2)
        //            {
        //                if (!(newPath.ContainPath(MustPassEitherWayPaths[i]) || newPath.ContainPath(MustPassEitherWayPaths[i + 1])))
        //                {
        //                    shouldAdd = false;
        //                }
        //            }


        //            for (int i = TargetPathsContainer.Paths.Count; i > 0; i--)
        //            {
        //                var path = TargetPathsContainer.Paths[i - 1];
        //                if (newPath.IsUseless(path)) { shouldAdd = false; }
        //                if (path.IsUseless(newPath)) { TargetPathsContainer.Paths.Remove(path); }
        //            }

        //            if (shouldAdd)
        //            {
        //                TargetPathsContainer.Paths.Add(newPath);
        //            }

        //        }

        //        return;
        //    }

        //    foreach (int to in RemainPoints[0])
        //    {
        //        foreach (Path path in TargetMatrix[from, to].Paths)
        //        {
        //            Path newPath = currentPath == null ? new Path(path) : new Path(currentPath, path);
        //            List<List<int>> newRemainPoints = RemainPoints.ToList();
        //            if (RemainPoints.Count == 1)
        //            {
        //                newRemainPoints.RemoveAt(0);
        //            }
        //            else
        //            {
        //                newRemainPoints[0].Remove(to);
        //            }
        //            DfsNew(to, end, newRemainPoints, currentPath);
        //        }

        //    }

        //}

        #region 私有方法
        /// <summary>
        /// 将MustPass路径的点加入mustpasspoints得到新的list
        /// </summary>
        /// <returns></returns>
        private List<int> GetShouldPassPoints()
        {
            List<int> newList = new List<int>(MustPassPoints);
            foreach (Path p in MustPassEitherWayPaths)
            {

                if (!newList.Contains(p.From))
                {
                    newList.Add(p.From);
                }
                if (!newList.Contains(p.To))
                {
                    newList.Add(p.To);
                }
            }
            return newList;

        }
        private List<int> GetShouldPassPoints(List<int> points, List<Path> paths)
        {
            List<int> newList = new List<int>(points);
            foreach (Path p in paths)
            {

                if (!newList.Contains(p.From))
                {
                    newList.Add(p.From);
                }
                if (!newList.Contains(p.To))
                {
                    newList.Add(p.To);
                }
            }
            return newList;
        }
        private void DfsAllMustPassPaths(int from, int end, List<int> remainPoints, List<Path> remainPaths, Path currentPath = null)
        {
            if (remainPaths.Count == 0 && remainPoints.Count == 0)
            {
                foreach (Path endPath in Matrix[from, end].Paths)
                {
                    var newPath =currentPath==null?new Path(endPath):new Path(currentPath, endPath);
                    bool shouldAdd = true;

                    //判断是否经过必经路径
                    //for (int i = 0; i < MustPassEitherWayPaths.Count; i = i + 2)
                    //{
                    //    if (!(newPath.ContainPath(MustPassEitherWayPaths[i]) || newPath.ContainPath(MustPassEitherWayPaths[i + 1])))
                    //    {
                    //        shouldAdd = false;
                    //    }
                    //}

                    for (int i = TargetPathsContainer.Paths.Count; i > 0; i--)
                    {

                        var path = TargetPathsContainer.Paths[i - 1];
                        if (newPath.IsUseless(path)) { shouldAdd = false; }
                        if (newPath.IsEquals(path)) { shouldAdd = false; }
                        if (shouldAdd == true && path.IsUseless(newPath))
                        {
                            TargetPathsContainer.Paths.Remove(path);
                        }
                    }


                    if (shouldAdd)
                    {
                        TargetPathsContainer.Paths.Add(newPath);
                    }

                }

                return;
            }

            #region 必经点
            //对于每个剩余必经点
            for (int i = 0; i < remainPoints.Count; i++)
            {
                if (currentPath != null) { if (currentPath.ContainPoint(remainPoints[i])) { continue; } }


                if (currentPath != null && Cut)
                {
                    int lastJoinPoint = currentPath.lastJoinPoint;
                    //下一个点到终点步数和距离同时小于当前点的(相对于终点走了回头路) && 上一个点到该点的距离和步数同时小于到下一个点的距离
                    if (Matrix[from, end].MinDistancePath.Distance < Matrix[remainPoints[i], end].MinDistancePath.Distance &&
                        Matrix[from, end].MinStepPath.Step < Matrix[remainPoints[i], end].MinStepPath.Step &&

                        Matrix[lastJoinPoint, from].MinDistancePath.Distance < Matrix[lastJoinPoint, remainPoints[i]].MinDistancePath.Distance &&
                        Matrix[lastJoinPoint, from].MinStepPath.Step < Matrix[lastJoinPoint, remainPoints[i]].MinStepPath.Step
                        )
                    {

                        continue;
                    }
                }


                //起始点是必经点
                if (remainPoints[i] == from)
                {
                    Path newPath = new Path(currentPath);
                    int nextPoint = from;
                    List<int> nextRemainPoints = new List<int>(remainPoints);
                    nextRemainPoints.Remove(remainPoints[i]);
                    List<Path> nextRemainPaths = new List<Path>(remainPaths);
                    DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);
                }
                else
                {
                    for (int j = 0; j < Matrix[from, remainPoints[i]].Paths.Count; j++)
                    {
                        Path newPath = currentPath == null ? Matrix[from, remainPoints[i]].Paths[j] : new Path(currentPath, Matrix[from, remainPoints[i]].Paths[j]);

                        int nextPoint = Matrix[from, remainPoints[i]].Paths[j].To;

                        List<int> nextRemainPoints = new List<int>(remainPoints);
                        nextRemainPoints.Remove(remainPoints[i]);

                        List<Path> nextRemainPaths = new List<Path>(remainPaths);

                        DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);
                    }
                }

            }
            #endregion

            //对于每条必经路径
            for (int i = remainPaths.Count; i > 0; i -= 2)
            {

                Path mustPassPath = remainPaths[i - 1];
                Path mustPassPathElse = remainPaths[i - 2];

                if (currentPath != null)
                {
                    if (currentPath.ContainPath(remainPaths[i - 1]) || currentPath.ContainPath(remainPaths[i - 2])) { continue; }
                }

                if (from == mustPassPath.From)
                {
                    Path newPath = currentPath == null ? new Path(mustPassPath) : new Path(currentPath, mustPassPath);

                    int nextPoint = mustPassPath.To;
                    List<Path> nextRemainPaths = new List<Path>(remainPaths);
                    nextRemainPaths.Remove(mustPassPath);
                    nextRemainPaths.Remove(mustPassPathElse);
                    List<int> nextRemainPoints = new List<int>(remainPoints);
                    DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);
                }
                else
                {
                    foreach (Path toMustPassPath in Matrix[from, mustPassPath.From].Paths)
                    {
                        Path newPath = currentPath == null ? new Path(toMustPassPath) : new Path(currentPath, toMustPassPath);
                        newPath = new Path(newPath, mustPassPath);
                        int nextPoint = mustPassPath.To;
                        List<Path> nextRemainPaths = new List<Path>(remainPaths);
                        nextRemainPaths.Remove(mustPassPath);
                        nextRemainPaths.Remove(mustPassPathElse);
                        List<int> nextRemainPoints = new List<int>(remainPoints);
                        DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);

                    }
                }
                if (from == mustPassPathElse.From)
                {
                    Path newPath = currentPath == null ? new Path(mustPassPathElse) : new Path(currentPath, mustPassPathElse);
                    int nextPoint = mustPassPathElse.To;
                    List<Path> nextRemainPaths = new List<Path>(remainPaths);
                    nextRemainPaths.Remove(mustPassPath);
                    nextRemainPaths.Remove(mustPassPathElse);
                    List<int> nextRemainPoints = new List<int>(remainPoints);
                    DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);
                }
                else
                {
                    foreach (Path toMustPassPath in Matrix[from, mustPassPathElse.From].Paths)
                    {
                        Path newPath = currentPath == null ? new Path(toMustPassPath) : new Path(currentPath, toMustPassPath);
                        newPath = new Path(newPath, mustPassPathElse);
                        int nextPoint = mustPassPathElse.To;
                        List<Path> nextRemainPaths = new List<Path>(remainPaths);
                        nextRemainPaths.Remove(mustPassPath);
                        nextRemainPaths.Remove(mustPassPathElse);
                        List<int> nextRemainPoints = new List<int>(remainPoints);
                        DfsAllMustPassPaths(nextPoint, end, nextRemainPoints, nextRemainPaths, newPath);
                    }
                }
            }
        }
        private DataTable CsvConvertToTable(string folderPath, string fileName)
        {
            OleDbConnection connection = null;
            OleDbCommand command = null;
            OleDbDataAdapter adapter = null;

            try
            {
                connection = new OleDbConnection
                {
                    ConnectionString = string.Format(@"
                Provider=Microsoft.Jet.OLEDB.4.0;
                Data Source={0};
                Extended Properties='Text;FMT=Delimited;HDR=No;CharacterSet=65001;'",
                    folderPath
                    )
                };

                connection.Open();

                command = new OleDbCommand
                {
                    Connection = connection,
                    CommandText = string.Format("SELECT * FROM {0}", fileName)
                };

                adapter = new OleDbDataAdapter
                {
                    SelectCommand = command
                };

                DataSet csvData = new DataSet();

                adapter.Fill(csvData, "Csv");
                return csvData.Tables[0];

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            //finally
            //{
            //    if (connection != null)
            //    {
            //        connection.Close();
            //        connection.Dispose();
            //    }

            //    if (command != null)
            //    {
            //        command.Dispose();
            //    }

            //    if (adapter != null)
            //    {
            //        adapter.Dispose();
            //    }
            //    return false;
            //}

        }
        private void InitMatrix(int size)
        {
            Matrix = new PathsContainer[size, size];
            OriginalMatrix = new PathsContainer[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        Matrix[i, j] = new PathsContainer(i, j);
                        OriginalMatrix[i, j] = new PathsContainer(i, j);
                        //Matrix[i, j].Paths.Add(new Path(i, j, 0));
                        //OriginalMatrix[i, j].Paths.Add(new Path(i, j, 0));
                    }
                    else
                    {
                        Matrix[i, j] = new PathsContainer(i, j);
                        OriginalMatrix[i, j] = new PathsContainer(i, j);
                    }

                }
            }
        }
        private bool InitializeFromTxt(string filePath)
        {
            string inputFile = null;
            if (!File.Exists(filePath))
            {
                return false;
            }
            inputFile = File.ReadAllText(filePath);
            string[] output = Regex.Split(inputFile, ",|(;)");

            #region 计算TableSize
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == ";")
                {
                    TableSize = i;
                    break;
                }
            }
            #endregion

            InitMatrix(TableSize);
            int x = 0, y = 0;
            for (int j = 0; j < output.Length; j++)
            {
                if (!String.IsNullOrWhiteSpace(output[j]))
                {
                    if (output[j] != ";")
                    {
                        var distance = Convert.ToInt32(output[j]);
                        if (distance > 0 && distance < 999)
                        {
                            Matrix[x, y].Paths.Add(new Path(x, y, distance));
                            OriginalMatrix[x, y].Paths.Add(new Path(x, y, distance));
                        }
                        y++;
                    }
                    if (output[j] == ";")
                    {
                        x++;
                        y = 0;
                    }
                }
            }
            return true;
        }
        private bool InitializeFromCsv(string path, string fileName)
        {
            try
            {
                DataTable dt = CsvConvertToTable(Directory.GetCurrentDirectory() + "\\" + path, fileName);
                TableSize = (int)dt.Compute("max(F2)", "") + 1;
                InitMatrix(TableSize);
                foreach (DataRow row in dt.Rows)
                {
                    int from = Convert.ToInt32(row.ItemArray[1]);
                    int to = Convert.ToInt32(row.ItemArray[2]);
                    int distance = Convert.ToInt32(row.ItemArray[3]);
                    Matrix[from, to].Paths.Add(new Path(from, to, distance));
                    OriginalMatrix[from, to].Paths.Add(new Path(from, to, distance));
                }

            }
            catch
            {
                return false;
            }
            return true;
        }



        //public void RemoveAlllllll()
        //{
        //    List<int> ShouldPassPoints = GetShouldPassPoints();
        //    foreach (int p1 in ShouldPassPoints)
        //    {
        //        List<Path> usefulPaths = new List<Path>();

        //        foreach (int p2 in ShouldPassPoints)
        //        {
        //            usefulPaths.AddRange(Matrix[p1, p2].Paths);
        //        }

        //        Path MinDistancePath = null;
        //        Path MinStepPath = null;
        //        foreach (int p2 in ShouldPassPoints)
        //        {
        //            if (p1 == p2) { continue; }
        //            var path = Matrix[p1, p2].MinDistancePath;
        //            if (MinDistancePath == null) { MinDistancePath = path; }
        //            else if (path.LessValueOfDistance(MinDistancePath))
        //            {
        //                MinDistancePath = path;
        //            }
        //            path = Matrix[p1, p2].MinStepPath;
        //            if (MinStepPath == null)
        //            {
        //                MinStepPath = path;
        //            }
        //            else if (path.LessValueOfStep(MinStepPath))
        //            {
        //                MinStepPath = path;
        //            }
        //        }
        //        for (int i = usefulPaths.Count; i > 0; i--)
        //        {
        //            if (usefulPaths[i - 1].IsUseless(MinDistancePath) || usefulPaths[i - 1].IsUseless(MinStepPath))
        //            {

        //                Matrix[usefulPaths[i - 1].From, usefulPaths[i - 1].To].Paths.Remove(usefulPaths[i - 1]);
        //                usefulPaths.RemoveAt(i - 1);
        //            }
        //        }
        //    }


        //}

        //private void DfsAllMustPassPaths2(int from, int end, List<int> RemainPoints, Path currentPath = null)
        //{

        //    if (RemainPoints.Count == 0)
        //    {
        //        foreach (Path endPath in Matrix[from, end].Paths)
        //        {
        //            var newPath = new Path(currentPath, endPath);

        //            bool shouldAdd = true;
        //            //判断是否经过必经路径
        //            for (int i = 0; i < MustPassEitherWayPaths.Count; i = i + 2)
        //            {
        //                if (!(newPath.ContainPath(MustPassEitherWayPaths[i]) || newPath.ContainPath(MustPassEitherWayPaths[i + 1])))
        //                {
        //                    shouldAdd = false;
        //                }
        //            }


        //            for (int i = TargetPathsContainer.Paths.Count; i > 0; i--)
        //            {
        //                var path = TargetPathsContainer.Paths[i - 1];
        //                if (newPath.IsUseless(path)) { shouldAdd = false; }
        //                if (path.IsUseless(newPath)) { TargetPathsContainer.Paths.Remove(path); }
        //            }

        //            if (shouldAdd)
        //            {
        //                TargetPathsContainer.Paths.Add(newPath);
        //            }

        //        }

        //        return;
        //    }



        //    var usefulPaths = new List<Path>();
        //    foreach (int point in RemainPoints)
        //    {
        //        usefulPaths.AddRange(Matrix[from, point].Paths.ToList());
        //    }



        //    Path MinDistancePath = null;
        //    Path MinStepPath = null;
        //    foreach (int p in RemainPoints)
        //    {
        //        var path = Matrix[from, p].MinDistancePath;
        //        if (MinDistancePath == null || path.LessValueOfDistance(MinDistancePath))
        //        {
        //            MinDistancePath = path;
        //        }
        //        path = Matrix[from, p].MinStepPath;
        //        if (MinStepPath == null || path.LessValueOfStep(MinStepPath))
        //        {
        //            MinStepPath = path;
        //        }
        //    }
        //    for (int i = usefulPaths.Count; i > 0; i--)
        //    {
        //        if (usefulPaths[i - 1].IsUseless(MinDistancePath) || usefulPaths[i - 1].IsUseless(MinStepPath))
        //        {
        //            usefulPaths.RemoveAt(i - 1);
        //        }
        //    }

        //    foreach (Path p in usefulPaths)
        //    {
        //        Path newPath = currentPath == null ? p : new Path(currentPath, p);
        //        List<int> nextLastPoints = new List<int>(RemainPoints);
        //        nextLastPoints.Remove(p.From);
        //        DfsAllMustPassPaths(p.To, end, nextLastPoints, newPath);
        //    }

        //}

        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        //private PathsContainer[,] GetTargetMatrix(PathsContainer[,] matrix)
        //{

        //    List<int> shouldPassPoints = GetShouldPassPoints();
        //    PathsContainer[,] ret = new PathsContainer[matrix.GetLength(0), matrix.GetLength(0)];
        //    foreach (int from in shouldPassPoints)
        //    {
        //        foreach (int to in shouldPassPoints)
        //        {
        //            ret[from, to] = new PathsContainer(matrix[from, to]);
        //        }
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 同时移除了无用路径
        /// </summary>
        /// <param name="start"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        //private List<List<int>> GetMustPassPoinsOrderByBothValue(int start, PathsContainer[,] matrix)
        //{
        //    List<PathsContainer> AllContainer = new List<PathsContainer>();
        //    List<List<int>> ret = new List<List<int>>();
        //    List<int> shouldPassPoints = GetShouldPassPoints();
        //    foreach (int p in shouldPassPoints)
        //    {
        //        AllContainer.Add(matrix[start, p]);

        //    }
        //    AllContainer.OrderByDescending(container => container.MinDistancePath.Distance)/*.ThenBy(container => container.MinStepPath.Step)*/;
        //    for (int i = 0; i < AllContainer.Count; i++)
        //    {
        //        if (i == 0)
        //        {
        //            ret.Add(new List<int>());
        //            ret[0].Add(AllContainer[0].ToPoint);
        //        }
        //        else if (AllContainer[i - 1].MinDistancePath.Distance < AllContainer[i].MinDistancePath.Distance && AllContainer[i - 1].MinStepPath.Step < AllContainer[i].MinStepPath.Step)
        //        {
        //            ret.Add(new List<int>());
        //            ret[ret.Count - 1].Add(AllContainer[i].ToPoint);
        //            matrix[start, i - 1].RemoveAllExceptMinPath();
        //        }
        //        else
        //        {
        //            ret[ret.Count - 1].Add(AllContainer[i].ToPoint);
        //        }
        //    }

        //    return ret;
        //}
    }
    #endregion
}
