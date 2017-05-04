using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZTEChallenge;
namespace TestGUI
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int start = 0;
        int end = 17;


        List<Grid> nodeGridList = new List<Grid>();
        List<Ellipse> nodeList = new List<Ellipse>();
        List<Line> lineList = new List<Line>();
        List<TextBlock> textList = new List<TextBlock>();
        public Map newMap;

        public MainWindow()
        {
            InitializeComponent();
            InitMap();
            Init();
            InitMust();
            SearchUsefulMatrix();

        }

        /// <summary>
        /// 设置取消必须经过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lineMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var a = (Line)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            string lineName = a.Name.Substring(1);
            string[] fromTo = lineName.Split('_');
            int from = Convert.ToInt32(fromTo[0]);
            int to = Convert.ToInt32(fromTo[1]);

            //正常路径
            if (a.Stroke == Brushes.LightSkyBlue)
            {
                newMap.AddMustPassEitherWayPath(from, to);
                a.Stroke = Brushes.Green;
            }
            //必经
            else if (a.Stroke == Brushes.Green)
            {
                newMap.RemoveMustPassEitherWayPath(from, to);
                a.Stroke = Brushes.LightSkyBlue;
            }
            //不通
            else if (a.Stroke == Brushes.Red)
            {
                newMap.RemoveMustNotPassAnyWayPath(from, to);
                newMap.AddMustPassEitherWayPath(from, to);
                a.Stroke = Brushes.Green;
            }

        }
        /// <summary>
        /// 设置取消禁止经过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lineMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            var a = (Line)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            string lineName = a.Name.Substring(1);
            string[] fromTo = lineName.Split('_');
            int from = Convert.ToInt32(fromTo[0]);
            int to = Convert.ToInt32(fromTo[1]);
            //正常路径
            if (a.Stroke == Brushes.LightSkyBlue)
            {
                newMap.AddMustNotPassAnyWayPath(from, to);
                a.Stroke = Brushes.Red;
            }
            //必经
            else if (a.Stroke == Brushes.Green)
            {
                newMap.RemoveMustPassEitherWayPath(from, to);
                newMap.AddMustNotPassAnyWayPath(from, to);
                a.Stroke = Brushes.Red;
            }
            //不通
            else if (a.Stroke == Brushes.Red)
            {
                newMap.RemoveMustNotPassAnyWayPath(from, to);
                a.Stroke = Brushes.LightSkyBlue;
            }

            WriteLine("改变了禁止路径，重新分析拓补图.");
            SearchUsefulMatrix();

        }
        /// <summary>
        /// 设置取消必须经过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nodeMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var a = (Ellipse)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            int num = Convert.ToInt32(a.Name.Substring(1));
            //当前是正常点
            if (a.Fill == Brushes.LightSkyBlue)
            {
                a.Fill = Brushes.Green;
                newMap.AddMustPassPoint(num);
            }
            //是必经点
            else if (a.Fill == Brushes.Green)
            {
                a.Fill = Brushes.LightSkyBlue;
                newMap.MustPassPoints.Remove(num);
            }
            //是起点或终点
            else if (a.Fill == Brushes.Yellow)
            {
                if (num == start)
                {
                    start = -1;
                    textList[num].Text = num.ToString();
                    a.Fill = Brushes.Green;
                    newMap.AddMustPassPoint(num);
                }
                if (num == end)
                {
                    end = -1;
                    textList[num].Text = num.ToString();
                    a.Fill = Brushes.Green;
                    newMap.AddMustPassPoint(num);
                }
            }


        }
        /// <summary>
        /// 设为起点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nodeMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            var a = (Ellipse)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            int num = Convert.ToInt32(a.Name.Substring(1));
            if (start != -1)
            {
                textList[start].Text = start.ToString();
                nodeList[start].Fill = Brushes.LightSkyBlue;
            }


            //当前是正常点
            if (a.Fill == Brushes.LightSkyBlue)
            {
                a.Fill = Brushes.Yellow;
                textList[num].Text = "S";
                start = num;
            }
            //是必经点
            else if (a.Fill == Brushes.Green)
            {
                newMap.MustPassPoints.Remove(num);
                a.Fill = Brushes.Yellow;
                textList[num].Text = "S";
                start = num;
            }
            //是起点或终点
            else if (a.Fill == Brushes.Yellow)
            {
                if (num == start)
                {
                    textList[num].Text = "S";
                    a.Fill = Brushes.Green;
                    start = num;
                }
                if (num == end)
                {
                    end = -1;
                    textList[num].Text = "S";
                    a.Fill = Brushes.Green;
                    start = num;
                }
            }
        }
        /// <summary>
        /// 设为终点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nodeMenuItem3_Click(object sender, RoutedEventArgs e)
        {
            var a = (Ellipse)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            int num = Convert.ToInt32(a.Name.Substring(1));
            if (end != -1)
            {
                textList[end].Text = end.ToString();
                nodeList[end].Fill = Brushes.LightSkyBlue;
            }


            //当前是正常点
            if (a.Fill == Brushes.LightSkyBlue)
            {
                a.Fill = Brushes.Yellow;
                textList[num].Text = "E";
                start = num;
            }
            //是必经点
            else if (a.Fill == Brushes.Green)
            {
                newMap.MustPassPoints.Remove(num);
                a.Fill = Brushes.Yellow;
                textList[num].Text = "E";
                start = num;
            }
            //是起点或终点
            else if (a.Fill == Brushes.Yellow)
            {
                if (num == start)
                {
                    textList[num].Text = "E";
                    a.Fill = Brushes.Green;
                    end = num;
                }
                if (num == end)
                {
                    end = -1;
                    textList[num].Text = "E";
                    a.Fill = Brushes.Green;
                    end = num;
                }
            }

        }
        private ContextMenu LineMenu { get; set; }
        private ContextMenu NodeMenu { get; set; }
        private void InitContextMenu()
        {
            LineMenu = new ContextMenu();
            NodeMenu = new ContextMenu();

            MenuItem lineMenuItem1 = new MenuItem() { Header = "设置/取消必需经过" };
            lineMenuItem1.Click += lineMenuItem1_Click;
            MenuItem lineMenuItem2 = new MenuItem() { Header = "设置/取消禁止经过" };
            lineMenuItem2.Click += lineMenuItem2_Click;
            LineMenu.Items.Add(lineMenuItem1);
            LineMenu.Items.Add(lineMenuItem2);
            foreach (Line l in lineList)
            {
                l.ContextMenu = LineMenu;
            }

            MenuItem nodeMenuItem1 = new MenuItem() { Header = "设置/取消必须经过" };
            nodeMenuItem1.Click += nodeMenuItem1_Click;
            MenuItem nodeMenuItem2 = new MenuItem() { Header = "设为起点" };
            nodeMenuItem2.Click += nodeMenuItem2_Click;
            MenuItem nodeMenuItem3 = new MenuItem() { Header = "设为终点" };
            nodeMenuItem3.Click += nodeMenuItem3_Click;
            NodeMenu.Items.Add(nodeMenuItem1);
            NodeMenu.Items.Add(nodeMenuItem2);
            NodeMenu.Items.Add(nodeMenuItem3);
        }
        private void Init()
        {
            //添加combobox
            FillCombobox();
            //初始化菜单
            InitContextMenu();
            //初始化节点
            for (int i = 0; i < 18; i++)
            {
                Ellipse node = (Ellipse)grid.FindName("n" + i.ToString());
                node.ContextMenu = NodeMenu;
                node.Cursor = Cursors.Hand;
                Grid nodeGrid = (Grid)node.Parent;
                TextBlock text = (TextBlock)nodeGrid.Children[1];
                text.Name = "T" + i.ToString();
                textList.Add(text);
                text.IsHitTestVisible = false;
                nodeGridList.Add(nodeGrid);
                nodeList.Add(node);
                Console.WriteLine(text.Text);
            }
            //初始化线
            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < 18; j++)
                {
                    if (i == j) { continue; }
                    if (newMap.Matrix[i, j].Paths.Count > 0)
                    {
                        Line newLine = new Line();
                        newLine.Stroke = Brushes.LightSkyBlue;
                        newLine.X1 = ((Grid)nodeList[i].Parent).Margin.Left + 20;
                        newLine.Y1 = ((Grid)nodeList[i].Parent).Margin.Top + 20;
                        newLine.X2 = ((Grid)nodeList[j].Parent).Margin.Left + 20;
                        newLine.Y2 = ((Grid)nodeList[j].Parent).Margin.Top + 20;
                        newLine.Name = "L" + i.ToString() + "_" + j.ToString();
                        newLine.Cursor = Cursors.Hand;
                        newLine.StrokeThickness = 5;
                        newLine.ContextMenu = LineMenu;
                        grid.Children.Add(newLine);
                        grid.RegisterName(newLine.Name,newLine);
                        Panel.SetZIndex((UIElement)nodeList[i].Parent, 1);
                        lineList.Add(newLine);


                        TextBlock newText = new TextBlock();
                        newText.IsHitTestVisible = false;
                        newText.Margin = new Thickness() { Left = newLine.X1 / 2 + newLine.X2 / 2, Top = newLine.Y1 / 2 + newLine.Y2 / 2 };
                        newText.Text = newMap.Matrix[i, j].Paths[0].Distance.ToString();
                        grid.Children.Add(newText);


                    }

                }
            }
        }

        private void WriteLine(string content)
        {

            textBox.AppendText(content + "\r\n");
            textBox.ScrollToEnd();
        }
        private void Write(string content)
        {

            textBox.AppendText(content);
            textBox.ScrollToEnd();
        }

        private void SearchUsefulMatrix()
        {

            WriteLine("-----正在分析拓补图...-----");
            long time0 = DateTime.Now.Ticks;
            newMap.ExecSearchUsefulMatrix();
            WriteLine("分析结束.用时: " + ((DateTime.Now.Ticks - time0) / 10000).ToString() + "毫秒.");
        }

        private void InitMap()
        {
            WriteLine("-----正在从文件中初始化...-----");
            long time0 = DateTime.Now.Ticks;
            newMap = new Map("TestData/input.txt");
            WriteLine("初始化结束.用时: " + ((DateTime.Now.Ticks - time0) / 10000).ToString() + "毫秒.");
        }

        private void SearchAllTargetPaths()
        {
            WriteLine("-----正在搜索要求路径...-----");
            long time0 = DateTime.Now.Ticks;
            
            newMap.ExecSearchAllTargetPaths(start, end);
            WriteLine("搜索结束.用时: " + ((DateTime.Now.Ticks - time0) / 10000).ToString() + "毫秒.");
            WriteLine("全部满足特殊点要求的最优路径:");
            foreach (var p in newMap.TargetPathsContainer.Paths)
            {
                WriteLine(p.PathStr + "; 节点数:" + (p.Step + 1).ToString() + "; 距离:" + p.Distance.ToString());
            }
        }
        //0
        private void PrintAllInfo(int from, int to)
        {
            WriteLine("-----" + from.ToString() + "到" + to.ToString() + "全部有用路径:-----");
            foreach (ZTEChallenge.Path p in newMap.Matrix[from, to].Paths)
            {
                WriteLine(p.PathStr + "; 节点数:" + (p.Step + 1).ToString() + "; 距离:" + p.Distance.ToString());
            }
        }

        private void PrintMinDistance(int from, int to)
        {
            var p = newMap.Matrix[from, to].MinDistancePath;
            WriteLine("-----" + from.ToString() + "到" + to.ToString() + "距离最短路径:-----");
            WriteLine(p.PathStr + "; 节点数:" + (p.Step + 1).ToString() + "; 距离:" + p.Distance.ToString());
        }

        private void PrintMinStep(int from, int to)
        {
            var p = newMap.Matrix[from, to].MinStepPath;
            WriteLine("-----" + from.ToString() + "到" + to.ToString() + "最少步数路径:-----");
            WriteLine(p.PathStr + "; 节点数:" + (p.Step + 1).ToString() + "; 距离:" + p.Distance.ToString());
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            switch (comboBoxType.SelectedIndex)
            {
                case 0:
                    PrintAllInfo(comboBoxFrom.SelectedIndex, comboBoxTo.SelectedIndex);
                    break;
                case 1:
                    PrintMinDistance(comboBoxFrom.SelectedIndex, comboBoxTo.SelectedIndex);
                    break;
                case 2:
                    PrintMinStep(comboBoxFrom.SelectedIndex, comboBoxTo.SelectedIndex);
                    break;
                default: break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SearchAllTargetPaths();
        }


        private void FillCombobox()
        {
            for (int i = 0; i < 18; i++)
            {
                comboBoxFrom.Items.Add(new ComboBoxItem() { Content = i, VerticalAlignment = VerticalAlignment.Center });
                comboBoxTo.Items.Add(new ComboBoxItem() { Content = i, VerticalAlignment = VerticalAlignment.Center });
            }
            comboBoxType.Items.Add(new ComboBoxItem() { Content = "全部有用路径", VerticalAlignment = VerticalAlignment.Center });
            comboBoxType.Items.Add(new ComboBoxItem() { Content = "距离最短路径", VerticalAlignment = VerticalAlignment.Center });
            comboBoxType.Items.Add(new ComboBoxItem() { Content = "最少步数路径", VerticalAlignment = VerticalAlignment.Center });

            comboBoxFrom.SelectedIndex = 0;
            comboBoxTo.SelectedIndex = 17;
            comboBoxType.SelectedIndex = 0;
        }

        private void InitMust()
        {
            Line l1, l2;
            newMap.AddMustNotPassAnyWayPath(11, 12);
            l1 = (Line)grid.FindName("L11_12");
            l1.Stroke = Brushes.Red;
            l2 = (Line)grid.FindName("L12_11");
            l2.Stroke= Brushes.Red;
            

            newMap.AddMustPassPoint(7);
            nodeList[7].Fill = Brushes.Green;
            newMap.AddMustPassPoint(12);
            nodeList[12].Fill = Brushes.Green;
            newMap.AddMustPassEitherWayPath(2, 4);
            l1 = (Line)grid.FindName("L2_4");
            l1.Stroke = Brushes.Green;
            l2 = (Line)grid.FindName("L4_2");
            l2.Stroke = Brushes.Green;

            newMap.AddMustPassEitherWayPath(13, 14);
            l1 = (Line)grid.FindName("L13_14");
            l1.Stroke = Brushes.Green;
            l2 = (Line)grid.FindName("L14_13");
            l2.Stroke = Brushes.Green;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            newMap.Cut = true;
            
        }
        private void checkBox_UnChecked(object sender, RoutedEventArgs e)
        {
            newMap.Cut = false;

        }
    }
}
