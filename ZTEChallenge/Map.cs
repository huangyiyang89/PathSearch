using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZTEChallenge
{
    class Map
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

        }

        public int Infinity { get; set; }

        /// <summary>
        /// 临接矩阵,每个点是一个PathContainer实例.
        /// </summary>
        /// 
        public PathsContainer[,] Matrix { get; private set; }

        public PathsContainer TargetPathsContainer { get; set; }

        public int TableSize { get; private set; }

        public List<Path> MustPassEitherWayPaths { get; private set; }

        public List<int> MustPassPoints { get; private set; }

        /// <summary>
        /// 是否在搜索有用路径时保留相同权值的路径,关闭可提高搜索速度以及查找目标路径的速度,默认开启.
        /// </summary>
        /// 
        public bool RetainSameValuePaths { get; set; }


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

        public void ExecSearchUsefulPaths()
        {

            //忽略MustNotPass路径
            foreach (Path p in MustNotPassAnyWayPaths)
            {
                Matrix[p.From, p.To].Paths.Remove(p);
            }

            for (int k = 0; k < TableSize; k++)
            {
                for (int i = 0; i < TableSize; i++)
                {
                    for (int j = 0; j < TableSize; j++)
                    {
                        //对i->k   k->j中全部路径做比较
                        for (int p1 = 0; p1 < Matrix[i, k].Paths.Count; p1++)
                        {
                            for (int p2 = 0; p2 < Matrix[k, j].Paths.Count; p2++)
                            {
                                //k点不为起点或终点
                                if (i == k || j == k) { continue; }

                                //起点终点是一个点
                                if (i == j) { continue; }

                                Path newPath = new Path(Matrix[i, k].Paths[p1], Matrix[k, j].Paths[p2]);

                                bool shouldAdd = true;
                                #region 条件判断
                                for (int p = Matrix[i, j].Paths.Count; p > 0; p--)
                                {
                                    var eachPath = Matrix[i, j].Paths[p - 1];
                                    if (newPath.Distance >= eachPath.Distance && newPath.Step >= eachPath.Step) { shouldAdd = false; }
                                    //是否保留相同权值和步数的路径,保留会影响搜索速度,但能找到多个最优解,如果存在多个最优解的话.
                                    if (newPath.EqualsInValue(eachPath) && RetainSameValuePaths) { shouldAdd = true; }
                                    //借此循环删除List中无用路径
                                    if (eachPath.IsUseless(newPath)) { Matrix[i, j].Paths.Remove(eachPath); }
                                }
                                #endregion
                                if (shouldAdd){Matrix[i, j].Paths.Add(newPath);}
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

        public void ExecSearchAllTargetPaths(int s, int e)
        {
            TargetPathsContainer = new PathsContainer(s, e);
            TargetPathsContainer.Paths.Clear();
            List<int> ShouldPassPoints = GetShouldPassPoints();
            DfsAllMustPassPaths(s, e, ShouldPassPoints);
        }

        public void ExecSearchMinDistanceTargetPaths(int s,int e)
        {

        }
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
        private void DfsAllMustPassPaths(int from, int end, List<int> RemainPoints, Path currentPath = null)
        {
            if (RemainPoints.Count == 0)
            {
                foreach (Path endPath in Matrix[from, end].Paths)
                {
                    var newPath = new Path(currentPath, endPath);

                    bool shouldAdd = true;
                    //判断是否经过必经路径
                    for (int i = 0; i < MustPassEitherWayPaths.Count; i = i + 2)
                    {
                        if (!(newPath.ContainPath(MustPassEitherWayPaths[i]) || newPath.ContainPath(MustPassEitherWayPaths[i + 1])))
                        {
                            shouldAdd = false;
                        }
                    }

                    
                    for(int i = TargetPathsContainer.Paths.Count; i > 0; i--)
                    {
                        var path = TargetPathsContainer.Paths[i-1];
                        if (newPath.IsUseless(path)) { shouldAdd = false; }
                        if (path.IsUseless(newPath)) { TargetPathsContainer.Paths.Remove(path); }
                    }   
                     
                    if (shouldAdd)
                    {
                        TargetPathsContainer.Paths.Add(newPath);
                    }
                    
                }

                return;
            }
            for (int i = 0; i < RemainPoints.Count; i++)
            {
                for (int j = 0; j < Matrix[from, RemainPoints[i]].Paths.Count; j++)
                {
                    Path newPath = currentPath == null ? Matrix[from, RemainPoints[i]].Paths[j] : new Path(currentPath, Matrix[from, RemainPoints[i]].Paths[j]);


                    List<int> nextLastPoints = new List<int>(RemainPoints);

                    int nextPoint = Matrix[from, RemainPoints[i]].Paths[j].To;
                    nextLastPoints.RemoveAt(i);
                    DfsAllMustPassPaths(nextPoint, end, nextLastPoints, newPath);
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

                return csvData.Tables[0]; ;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }

                if (command != null)
                {
                    command.Dispose();
                }

                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }
        }
        private void InitMatrix(int size)
        {
            Matrix = new PathsContainer[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        Matrix[i, j] = new PathsContainer(i, j);
                        Matrix[i, j].Paths.Add(new Path(i, j, 0));
                    }
                    else
                    {
                        Matrix[i, j] = new PathsContainer(i, j);
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
                DataTable dt = CsvConvertToTable("C:\\", "topo.csv");
                TableSize = (int)dt.Compute("max(F2)", "") + 1;
                InitMatrix(TableSize);
                foreach (DataRow row in dt.Rows)
                {
                    int from = Convert.ToInt32(row.ItemArray[1]);
                    int to = Convert.ToInt32(row.ItemArray[2]);
                    int distance = Convert.ToInt32(row.ItemArray[3]);
                    Matrix[from, to].Paths.Add(new Path(from, to, distance));
                }

            }
            catch
            {
                return false;
            }
            return true;
        }
      
        #endregion
    }
}
