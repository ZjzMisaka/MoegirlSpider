using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
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
        string nowLink = "";

        int index = -1;
        public MainWindow()
        {
            InitializeComponent();

            web = new HtmlWeb();
            web.PreRequest = delegate (HttpWebRequest webReq)
            {
                webReq.Timeout = 999999999; // number of milliseconds
                return true;
            };
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

            Next();
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = nowLink,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void Next(string pLink = "")
        {
            foreach (UIElement ele in MainGrid.Children)
            {
                if (ele.GetType() == typeof(TextBox))
                {
                    ((TextBox)ele).Text = "";
                }
            }

            ++index;

            if (index >= nameList?.Count)
            {
                return;
            }

            Data data = new Data();

            string? name = nameList?[index].InnerText.Trim();
            data.Name = name;
            Name.Text = data.Name;
            HtmlNode? link = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/div[@class='mf-section-" + (index + 2) + " collapsible-block']/dl/dd/a")[0];

            string? path = link?.Attributes["href"].Value.Trim();

            if (String.IsNullOrWhiteSpace(pLink))
            {
                nowLink = @"https://mzh.moegirl.org.cn/zh-hans" + path;
            }
            else
            {
                nowLink = pLink;
            }
            HtmlDocument? animeDoc = web?.Load(nowLink);
            if (animeDoc.Text.StartsWith("File not found."))
            {
                TextBox tb = new TextBox();

                Button buttonRetry = new Button();
                buttonRetry.Content = "retry";
                buttonRetry.Click += (s, e) =>
                {
                    --index;
                    Next(((TextBox)CustomizableMessageBox.MessageBox.ButtonList[0]).Text);
                };

                Button buttonSkip = new Button();
                buttonRetry.Content = "skip";
                buttonSkip.Click += (s, e) =>
                {
                    Next();
                };

                CustomizableMessageBox.MessageBox.Show(new List<Object> { tb, buttonRetry, buttonSkip }, name + " Failed", "");
                
                return;
            }

            string? origName = animeDoc?.DocumentNode?.SelectNodes(@"//table/tbody/tr/td/span[@lang='ja']")[0].InnerText.Trim();
            data.OriginalName = origName;
            OriginalName.Text = data.OriginalName;

            HtmlNodeCollection? introductionNodes = animeDoc?.DocumentNode?.SelectNodes(@"//h2");
            foreach (HtmlNode node in introductionNodes)
            {
                if (node.InnerText.Contains("剧情简介"))
                {
                    data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                    break;
                }
                if (String.IsNullOrWhiteSpace(data.Introduction))
                {
                    if (node.InnerText.Contains("剧情"))
                    {
                        data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                        break;
                    }
                }
            }
            if (String.IsNullOrWhiteSpace(data.Introduction))
            {
                introductionNodes = animeDoc?.DocumentNode?.SelectNodes(@"//div[@class='poem']");
                if (introductionNodes != null && introductionNodes.Count > 0)
                {
                    data.Introduction = introductionNodes[0].InnerText.Trim();
                }
            }
            Introduction.Text = data.Introduction;
            try
            {
                data.Staff = animeDoc?.DocumentNode?.SelectNodes(@"//span[@id='STAFF']/../following::ul")[0].InnerText.Trim();
            }
            catch
            { }
            Staff.Text = data.Staff;

            try
            {
                data.Cast = animeDoc?.DocumentNode?.SelectNodes(@"//span[@id='CAST']/../following::ul")[0].InnerText.Trim();
            }
            catch
            { }
            Cast.Text = data.Cast;

            HtmlNodeCollection? totalEpisodesNodes = animeDoc?.DocumentNode?.SelectNodes(@"//td");
            bool totalEpisodesFound = false;
            foreach (HtmlNode node in totalEpisodesNodes)
            {
                if (node.InnerText.Contains("话数") || node.InnerText.Contains("集数"))
                {
                    totalEpisodesFound = true;
                }
                else if (totalEpisodesFound)
                {
                    data.TotalEpisodes = node.InnerText;
                    break;
                }
            }
            if (String.IsNullOrWhiteSpace(data.TotalEpisodes))
            {
                foreach (Match match in Regex.Matches(animeDoc?.DocumentNode?.InnerHtml, "[全共][0 - 9]+[话集]"))
                {
                    data.TotalEpisodes = System.Text.RegularExpressions.Regex.Replace(match.Value, @"[^0-9]+", "");
                    if (String.IsNullOrWhiteSpace(data.TotalEpisodes))
                    {
                        data.TotalEpisodes = match.Value;
                    }
                    break;
                }
            }
            TotalEpisodes.Text = data.TotalEpisodes;
        }
    }
}
