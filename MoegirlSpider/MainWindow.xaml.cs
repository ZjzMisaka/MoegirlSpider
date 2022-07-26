using HtmlAgilityPack;
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

namespace MoegirlSpider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(@"https://mzh.moegirl.org.cn/zh-hans/日本2022年夏季动画");
            HtmlNodeCollection? titleList = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/h2/span[@class='mw-headline']");
            titleList?.RemoveAt(0);
            titleList?.RemoveAt(titleList.Count - 1);
            HtmlNodeCollection? linkList = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']//dl/dd/a");
        }
    }
}
