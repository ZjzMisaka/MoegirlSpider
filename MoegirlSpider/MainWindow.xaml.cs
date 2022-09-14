using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        HtmlNodeCollection? nameList = null;
        HtmlDocument? doc = null;
        HtmlWeb? web = null;

        int index = -1;
        public MainWindow()
        {
            InitializeComponent();

            web = new HtmlWeb();
            doc = web.Load(@"https://mzh.moegirl.org.cn/zh-hans/日本2022年夏季动画");
            nameList = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/h2/span[@class='mw-headline']");
            nameList?.RemoveAt(0);
            nameList?.RemoveAt(nameList.Count - 1);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (index >= 0)
            { 
                // insert sql
            }

            ++index;

            Data data = new Data();

            string? name = nameList?[index].InnerText.Trim();
            data.Name = name;
            Name.Text = data.Name;
            HtmlNode? link = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/div[@class='mf-section-" + (index + 2) + " collapsible-block']/dl/dd/a")[0];

            string? path = link?.Attributes["href"].Value.Trim();
            HtmlDocument? animeDoc = web?.Load(@"https://mzh.moegirl.org.cn/zh-hans" + path);

            string? origName = animeDoc?.DocumentNode?.SelectNodes(@"//table/tbody/tr/td/span[@lang='ja']")[0].InnerText.Trim();
            data.OriginalName = origName;
            OriginalName.Text = data.OriginalName;

            data.Introduction = animeDoc?.DocumentNode?.SelectNodes(@"//div[@class='poem']")[0].InnerText.Trim();
            Introduction.Text = data.Introduction;
            data.Staff = animeDoc?.DocumentNode?.SelectNodes(@"//h4/span[@id='STAFF']/../following::ul")[0].InnerText.Trim();
            Staff.Text = data.Staff;
            data.Cast = animeDoc?.DocumentNode?.SelectNodes(@"//h4/span[@id='CAST']/../following::ul")[0].InnerText.Trim();
            Cast.Text = data.Cast;

            HtmlNodeCollection? nodes = animeDoc?.DocumentNode?.SelectNodes(@"//td");
            foreach (HtmlNode node in nodes)
            {
                if (node.InnerText == "总话数")
                {
                    data.TotalEpisodes = node.ParentNode.ChildNodes[1].InnerText;
                }
            }
            if (String.IsNullOrWhiteSpace(data.TotalEpisodes))
            {
                foreach (Match match in Regex.Matches(animeDoc?.DocumentNode?.InnerHtml, "[全共][0 - 9]*[话集]"))
                {
                    data.TotalEpisodes = System.Text.RegularExpressions.Regex.Replace(match.Value, @"[^0-9]+", "");
                    break;
                }
            }
            TotalEpisodes.Text = data.TotalEpisodes;
        }
    }
}
