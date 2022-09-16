using CustomizableMessageBox;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
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
                if (!Check())
                {
                    CustomizableMessageBox.MessageBox.Show(Prefab.GetPropertiesSetter(PropertiesSetterName.Black), "名字, 介绍, 工作人员, 声优表, 制作公司, 首播日期不能为空");
                    return;
                }

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

            HtmlNodeCollection? officialWebsiteNodeCollection = nameList?[index].SelectNodes(@"../following::div");
            if (officialWebsiteNodeCollection != null && officialWebsiteNodeCollection.Count > 0)
            {
                HtmlNode? officialWebsiteNode = officialWebsiteNodeCollection[0].SelectNodes(@"./ul/li/a")?[0];
                string? officialWebsitePath = officialWebsiteNode?.Attributes["href"].Value.Trim();
                data.OfficialWebsite = officialWebsitePath;
                OfficialWebsite.Text = data.OfficialWebsite;
            }



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
            if (animeDoc.Text.StartsWith("File not found.") || animeDoc.Text.Contains("萌百娘找不到这个页面"))
            {
                CustomizableMessageBox.MessageBox.Show(
                    Prefab.GetPropertiesSetter(PropertiesSetterName.Black),
                    new List<Object> {
                        new TextBox(), 
                        "retry", new RoutedEventHandler((sender, eventArgs) => { 
                            --index;
                            CustomizableMessageBox.MessageBox.CloseNow();
                            Next(((TextBox)CustomizableMessageBox.MessageBox.ButtonList[0]).Text);
                        }),
                        "skip", new RoutedEventHandler((sender, eventArgs) => {
                            CustomizableMessageBox.MessageBox.CloseNow();
                            Next();
                        }),
                        "close"
                    }, name + " Failed", "");

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


            HtmlNodeCollection? productionCompanyNodes = animeDoc?.DocumentNode?.SelectNodes(@"//li");
            foreach (HtmlNode node in productionCompanyNodes)
            {
                if (node.InnerText.StartsWith("动画制作："))
                {
                    data.ProductionCompany = node.InnerText.Trim().Substring(node.InnerText.Trim().IndexOf("：") + 1);
                    break;
                }
                if (String.IsNullOrWhiteSpace(data.ProductionCompany))
                {
                    if (node.InnerText.Contains("制作："))
                    {
                        data.ProductionCompany = node.InnerText.Trim().Substring(node.InnerText.Trim().IndexOf("：") + 1);
                        break;
                    }
                }
            }
            ProductionCompany.Text = data.ProductionCompany;


            bool premiereDateFound = false;
            HtmlNodeCollection? premiereDateNodes = animeDoc?.DocumentNode?.SelectNodes(@"//td");
            foreach (HtmlNode node in premiereDateNodes)
            {
                if (node.InnerText.Contains("首播时间"))
                {
                    premiereDateFound = true;
                }
                else if (premiereDateFound)
                {
                    data.PremiereDate = FormatDate(node.InnerText.Trim());
                    break;
                }
            }
            PremiereDate.Text = data.PremiereDate;
        }

        private bool Check()
        {
            // 名字, 介绍, 工作人员, 声优表, 制作公司, 首播日期不能为空
            if (String.IsNullOrWhiteSpace(Name.Text) ||
                String.IsNullOrWhiteSpace(Introduction.Text) ||
                String.IsNullOrWhiteSpace(Staff.Text) ||
                String.IsNullOrWhiteSpace(Cast.Text) ||
                String.IsNullOrWhiteSpace(ProductionCompany.Text) ||
                String.IsNullOrWhiteSpace(PremiereDate.Text))
            {
                return false;
            }

            return true;
        }

        /*
         jQuery("#premiere_date").on('change', function() {
            var newDateStr = jQuery("#premiere_date").val().replace("年", "/").replace("月", "/").replace("日", "").replace(/-/g, "/");
            var newDateStrSplited = newDateStr.split('/');
            if(newDateStrSplited.length == 3) {
                if(newDateStrSplited[1].length == 1) {
                    newDateStrSplited[1] = '0' + newDateStrSplited[1];
                }
                if(newDateStrSplited[2].length == 1) {
                    newDateStrSplited[2] = '0' + newDateStrSplited[2];
                }
                jQuery("#premiere_date").val(newDateStrSplited[0] + '/' + newDateStrSplited[1] + '/' + newDateStrSplited[2]);
            }
        });
         */
        private string FormatDate(string date)
        {
            date = date.Replace("年", "/").Replace("月", "/").Replace("日", "").Replace("-", "/").Trim();
            string[] splited = date.Split("/");
            if (splited.Length == 3)
            {
                if (splited[1].Length == 1)
                {
                    splited[1] = "0" + splited[1];
                }
                if (splited[2].Length == 1)
                {
                    splited[2] = "0" + splited[2];
                }
            }
            return String.Join("/", splited);
        }

        private void PremiereDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            PremiereDate.Text = FormatDate(PremiereDate.Text);
        }
    }
}
