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
   
    class gLine
    {

    }
    class gNode
    {
        Grid grid;
        TextBlock block;
        Ellipse ellipse;
        public gNode(Ellipse el)
        {
            grid = (Grid)el.Parent;
            block = (TextBlock)grid.Children[1];
        }

        public string Text { get; set; }
        public Brush Color { get; set; }

    }


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<gLine> lines = new List<gLine>();
        List<gNode> nodes = new List<gNode>();

        List<Grid> nodeGridList = new List<Grid>();
        List<Ellipse> nodeList = new List<Ellipse>();
        List<Line> lineList = new List<Line>();
        public Map newMap = new Map("TestData/input.txt");

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void lineMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var a = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
           
        }
        private void lineMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            var a = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            
        }
        private void nodeMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var a = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
            
        }
        private void nodeMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            var a = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
           
        }
        private void nodeMenuItem3_Click(object sender, RoutedEventArgs e)
        {
            var a = ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
           
        }
        private void InitContextMenu()
        {
            ContextMenu LineMenu = new ContextMenu();
            ContextMenu NodeMenu = new ContextMenu();

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
            foreach (Ellipse node in nodeList)
            {
                node.ContextMenu = NodeMenu;
            }
        }
        
        private void Init()
        {
            for (int i = 0; i < 18; i++)
            {
                Ellipse node = (Ellipse)grid.FindName("n" + i.ToString());
                Grid nodeGrid = (Grid)node.Parent;
                nodeGridList.Add(nodeGrid);
                nodeList.Add(node);
            }

            for (int i = 0; i < 18; i++)
            {
                for(int j = 0; j < 18; j++)
                {
                    if (i == j) { continue; }
                    if (newMap.Matrix[i, j].Paths.Count > 0)
                    {
                        Line newLine = new Line();
                        
                        newLine.Stroke = Brushes.LightSkyBlue;
                        newLine.StrokeThickness = 1;
                        newLine.X1 = ((Grid)nodeList[i].Parent).Margin.Left+20;
                        newLine.Y1= ((Grid)nodeList[i].Parent).Margin.Top+20;
                        newLine.X2=((Grid)nodeList[j].Parent).Margin.Left+20;
                        newLine.Y2 = ((Grid)nodeList[j].Parent).Margin.Top+20;
                        
                        grid.Children.Add(newLine);
                        Panel.SetZIndex((UIElement)nodeList[i].Parent, 111);
                    }
                  
                }
            }

           
            InitContextMenu();
        }

        void InitNodesAndLines()
        {
            for (int i = 0; i < 18; i++)
            {
                for (int j = 0; j < 18; j++)
                {
                    if (i == j) { continue; }
                    if (newMap.Matrix[i, j].Paths.Count > 0)
                    {
                        Line newLine = new Line();

                        newLine.Stroke = Brushes.LightSkyBlue;
                        newLine.StrokeThickness = 51;
                        newLine.X1 = ((Grid)nodeList[i].Parent).Margin.Left + 20;
                        newLine.Y1 = ((Grid)nodeList[i].Parent).Margin.Top + 20;
                        newLine.X2 = ((Grid)nodeList[j].Parent).Margin.Left + 20;
                        newLine.Y2 = ((Grid)nodeList[j].Parent).Margin.Top + 20;

                        grid.Children.Add(newLine);
                        
                        //Panel.SetZIndex((UIElement)nodeList[i].Parent, 1);
                    }

                }
            }
        }
    }
}
