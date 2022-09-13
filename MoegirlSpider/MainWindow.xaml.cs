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
            HtmlNodeCollection? nameList = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/h2/span[@class='mw-headline']");
            nameList?.RemoveAt(0);
            nameList?.RemoveAt(nameList.Count - 1);

            for (int i = 0; i < nameList?.Count; ++i)
            {
                Data data = new Data();

                string name = nameList[i].InnerText;
                data.Name = name;
                HtmlNode? link = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/div[@class='mf-section-" + (i + 2) + " collapsible-block']/dl/dd/a")[0];

                string? path = link?.Attributes["href"].Value;
                HtmlDocument animeDoc = web.Load(@"https://mzh.moegirl.org.cn/zh-hans" + path);

                string? origName = animeDoc?.DocumentNode?.SelectNodes(@"//table/tbody/tr/td/span[@lang='ja']")[0].InnerText;
                data.OriginalName = origName;


            }
        }
    }
}
