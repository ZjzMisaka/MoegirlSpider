using CustomizableMessageBox;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
            doc = web.Load(@"https://mzh.moegirl.org.cn/zh-hans/日本2022年春季动画");
            nameList = doc?.DocumentNode?.SelectNodes(@"//div[@class='mw-parser-output']/h2/span[@class='mw-headline']");
            nameList?.RemoveAt(0);
            nameList?.RemoveAt(nameList.Count - 1);
        }

        private MySqlConnection GetConn()
        {
            MySqlConnectionStringBuilder connStringBuilder = new MySqlConnectionStringBuilder();
            MySqlConnection conn = new MySqlConnection(connStringBuilder.ToString());

            return conn;
        }

        private bool CheckDbExist(string name)
        {
            MySqlConnection conn = GetConn();
            try
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM `WP_ANIME` WHERE `NAME` = '" + name + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    if (rdr.GetInt32(0) == 0)
                    {
                        conn.Close();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomizableMessageBox.MessageBox.Show("select failed. \n" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return true;
        }

        private string ConnectValues(params string[] str)
        {
            for (int i = 0; i < str.Count(); ++i)
            {
                str[i] = str[i].Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
            }
            return "(" + String.Join(", ", str) + ")";
        }

        private string EmptyToNull(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            return str;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (index >= 0)
            {
                if (!CheckValues())
                {
                    return;
                }
                MySqlConnection conn = GetConn();
                if (Status.Text == "Insert")
                {
                    try
                    {
                        conn.Open();
                        string sql = "INSERT INTO `WP_ANIME` " +
                            "(`name`, `original_name`, `type`, `introduction`, `staff`, `cast`, `total_episodes`, `production_company`, `opening_song`, `ending_song`, `charactor_song`, `insert_song`, `achievement`, `other`, `premiere_date`, `title_img_external_link`, `official_website`) " +
                            "VALUES " + ConnectValues("@P1", "@P2", "@P3", "@P4", "@P5", "@P6", "@P7", "@P8", "@P9", "@P10", "@P11", "@P12", "@P13", "@P14", "@P15", "@P16", "@P17");
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@P1", EmptyToNull(Name.Text));
                        cmd.Parameters.AddWithValue("@P2", EmptyToNull(OriginalName.Text));
                        cmd.Parameters.AddWithValue("@P3", EmptyToNull(Type.SelectedIndex.ToString()));
                        cmd.Parameters.AddWithValue("@P4", EmptyToNull(Introduction.Text));
                        cmd.Parameters.AddWithValue("@P5", EmptyToNull(Staff.Text));
                        cmd.Parameters.AddWithValue("@P6", EmptyToNull(Cast.Text));
                        cmd.Parameters.AddWithValue("@P7", EmptyToNull(TotalEpisodes.Text));
                        cmd.Parameters.AddWithValue("@P8", EmptyToNull(ProductionCompany.Text));
                        cmd.Parameters.AddWithValue("@P9", EmptyToNull(OpeningSong.Text));
                        cmd.Parameters.AddWithValue("@P10", EmptyToNull(EndingSong.Text));
                        cmd.Parameters.AddWithValue("@P11", EmptyToNull(CharactorSong.Text));
                        cmd.Parameters.AddWithValue("@P12", EmptyToNull(InsertSong.Text));
                        cmd.Parameters.AddWithValue("@P13", EmptyToNull(Achievement.Text));
                        cmd.Parameters.AddWithValue("@P14", EmptyToNull(Other.Text));
                        cmd.Parameters.AddWithValue("@P15", EmptyToNull(PremiereDate.Text));
                        cmd.Parameters.AddWithValue("@P16", EmptyToNull(TitleImgExternalLink.Text));
                        cmd.Parameters.AddWithValue("@P17", EmptyToNull(OfficialWebsite.Text));
                        int count = cmd.ExecuteNonQuery();
                        if (count == 0)
                        {
                            throw new Exception("count = 0");
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomizableMessageBox.MessageBox.Show("insert failed. \n" + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
                else
                {
                    try
                    {
                        conn.Open();
                        string sql = "UPDATE `WP_ANIME` " +
                            "SET `name` = @P1, " +
                            "`original_name` = @P2, " +
                            "`type` = @P3, " +
                            "`introduction` = @P4, " +
                            "`staff` = @P5, " +
                            "`cast` = @P6, " +
                            "`total_episodes` = @P7, " +
                            "`production_company` = @P8, " +
                            "`opening_song` = @P9, " +
                            "`ending_song` = @P10, " +
                            "`charactor_song` = @P11, " +
                            "`insert_song` = @P12, " +
                            "`achievement` = @P13, " +
                            "`other` = @P14, " +
                            "`premiere_date` = @P15, " +
                            "`title_img_external_link` = @P16, " +
                            "`official_website` = @P17 " +
                            "WHERE `name` = @P1";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@P1", EmptyToNull(Name.Text));
                        cmd.Parameters.AddWithValue("@P2", EmptyToNull(OriginalName.Text));
                        cmd.Parameters.AddWithValue("@P3", EmptyToNull(Type.SelectedIndex.ToString()));
                        cmd.Parameters.AddWithValue("@P4", EmptyToNull(Introduction.Text));
                        cmd.Parameters.AddWithValue("@P5", EmptyToNull(Staff.Text));
                        cmd.Parameters.AddWithValue("@P6", EmptyToNull(Cast.Text));
                        cmd.Parameters.AddWithValue("@P7", EmptyToNull(TotalEpisodes.Text));
                        cmd.Parameters.AddWithValue("@P8", EmptyToNull(ProductionCompany.Text));
                        cmd.Parameters.AddWithValue("@P9", EmptyToNull(OpeningSong.Text));
                        cmd.Parameters.AddWithValue("@P10", EmptyToNull(EndingSong.Text));
                        cmd.Parameters.AddWithValue("@P11", EmptyToNull(CharactorSong.Text));
                        cmd.Parameters.AddWithValue("@P12", EmptyToNull(InsertSong.Text));
                        cmd.Parameters.AddWithValue("@P13", EmptyToNull(Achievement.Text));
                        cmd.Parameters.AddWithValue("@P14", EmptyToNull(Other.Text));
                        cmd.Parameters.AddWithValue("@P15", EmptyToNull(PremiereDate.Text));
                        cmd.Parameters.AddWithValue("@P16", EmptyToNull(TitleImgExternalLink.Text));
                        cmd.Parameters.AddWithValue("@P17", EmptyToNull(OfficialWebsite.Text));
                        int count = cmd.ExecuteNonQuery();
                        if (count == 0)
                        {
                            throw new Exception("count = 0");
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomizableMessageBox.MessageBox.Show("insert failed. \n" + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            string name = GetNextName();
            if (String.IsNullOrWhiteSpace(name))
            {
                return;
            }
            if (CheckDbExist(name))
            {
                Next(false, name);
            }
            else 
            {
                Next(true, name);
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            string name = GetNextName();
            if (String.IsNullOrWhiteSpace(name))
            {
                return;
            }
            if(CheckDbExist(name))
            {
                Next(false, name);
            }
            else
            {
                Next(true, name);
            }
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

        private string GetNextName()
        {
            ++index;

            if (index >= nameList?.Count)
            {
                return "";
            }
            string? name = nameList?[index].InnerText.Trim();

            return name;
        }

        private void Next(bool isInsert, string name, string pLink = "")
        {
            foreach (UIElement ele in MainGrid.Children)
            {
                if (ele.GetType() == typeof(TextBox))
                {
                    ((TextBox)ele).Text = "";
                }
            }
            Type.SelectedIndex = 0;

            if (isInsert)
            {
                Status.Text = "Insert";
            }
            else
            {
                Status.Text = "Update";
            }

            Data data = new Data();

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

            string outStaff = "";
            string outCast = "";
            HtmlNodeCollection? staffAndCastNodeCollection = nameList?[index].SelectNodes(@"../following::h3");
            if (staffAndCastNodeCollection != null && staffAndCastNodeCollection.Count > 0)
            {
                foreach (HtmlNode node in staffAndCastNodeCollection)
                {
                    if (node.InnerText == "STAFF")
                    {
                        outStaff = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                    }
                    else if (node.InnerText == "CAST")
                    {
                        outCast = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                    }
                }
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

            if (isInsert)
            {
                HtmlDocument? animeDoc = web?.Load(nowLink);
                if (animeDoc.Text.StartsWith("File not found.") || animeDoc.Text.Contains("萌百娘找不到这个页面"))
                {
                    CustomizableMessageBox.MessageBox.Show(
                        Prefab.GetPropertiesSetter(PropertiesSetterName.Black),
                        new List<Object> {
                        new TextBox(),
                        "retry", new RoutedEventHandler((sender, eventArgs) => {
                            CustomizableMessageBox.MessageBox.CloseNow();
                            Next(isInsert, name, ((TextBox)CustomizableMessageBox.MessageBox.ButtonList[0]).Text);
                        }),
                        "skip", new RoutedEventHandler((sender, eventArgs) => {
                            CustomizableMessageBox.MessageBox.CloseNow();
                            string name = GetNextName();
                            if (String.IsNullOrWhiteSpace(name))
                            {
                                return;
                            }
                            if(CheckDbExist(name))
                            {
                                Next(false, name);
                            }
                            else
                            {
                                Next(true, name);
                            }
                        }),
                        "close"
                        }, name + " Failed", "");

                    return;
                }

                HtmlNodeCollection? originalNameNodes = animeDoc?.DocumentNode?.SelectNodes(@"//table/tbody/tr/td/span[@lang='ja']");
                if (originalNameNodes != null && originalNameNodes.Count > 0)
                {
                    string? origName = originalNameNodes[0].InnerText.Trim();
                    data.OriginalName = origName;
                    OriginalName.Text = data.OriginalName;
                }

                HtmlNodeCollection? introductionNodes = animeDoc?.DocumentNode?.SelectNodes(@"//h2");
                if (introductionNodes != null)
                {
                    foreach (HtmlNode node in introductionNodes)
                    {
                        if (node.InnerText.Contains("剧情简介"))
                        {
                            data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                            break;
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(data.Introduction))
                {
                    HtmlNodeCollection? introductionNodesH3 = animeDoc?.DocumentNode?.SelectNodes(@"//h3");
                    if (introductionNodesH3 != null)
                    {
                        foreach (HtmlNode node in introductionNodesH3)
                        {
                            if (node.InnerText.Contains("剧情简介"))
                            {
                                data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                                break;
                            }
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(data.Introduction))
                {
                    if (introductionNodes != null)
                    {
                        foreach (HtmlNode node in introductionNodes)
                        {
                            if (String.IsNullOrWhiteSpace(data.Introduction))
                            {
                                if (node.InnerText.Trim().StartsWith("剧情"))
                                {
                                    data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
                if (String.IsNullOrWhiteSpace(data.Introduction))
                {
                    HtmlNodeCollection? introductionNodesH3 = animeDoc?.DocumentNode?.SelectNodes(@"//h3");
                    if (introductionNodesH3 != null)
                    {
                        foreach (HtmlNode node in introductionNodesH3)
                        {
                            if (String.IsNullOrWhiteSpace(data.Introduction))
                            {
                                if (node.InnerText.Trim().StartsWith("剧情"))
                                {
                                    data.Introduction = node.SelectNodes(@"./following::div")[0].InnerText.Trim();
                                    break;
                                }
                            }
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
                if (String.IsNullOrWhiteSpace(data.Staff) || data.Staff.Split("\n").Length < outStaff.Split("\n").Length)
                {
                    data.Staff = outStaff;
                }
                Staff.Text = data.Staff;

                try
                {
                    data.Cast = animeDoc?.DocumentNode?.SelectNodes(@"//span[@id='CAST']/../following::ul")[0].InnerText.Trim();
                }
                catch
                { }
                if (String.IsNullOrWhiteSpace(data.Cast) || data.Cast.Split("\n").Length < outCast.Split("\n").Length)
                {
                    data.Cast = outCast;
                }
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
                        if (node.InnerText.Contains("制作公司："))
                        {
                            data.ProductionCompany = node.InnerText.Trim().Substring(node.InnerText.Trim().IndexOf("：") + 1);
                            break;
                        }
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

                HtmlNodeCollection? musicNodes = animeDoc?.DocumentNode?.SelectNodes(@"//h4/span[text() = '相关音乐' ]");
                HtmlNode musicNode = null;
                string musicStr = "";
                if (musicNodes != null && musicNodes.Count > 0)
                {
                    musicNode = musicNodes[0].ParentNode;
                }
                while (musicNode != null && musicNode.NextSibling != null && musicNode.NextSibling.Name != "h4" && musicNode.NextSibling.Name != "h3" && musicNode.NextSibling.Name != "h2")
                {
                    musicNode = musicNode.NextSibling;
                    if (!String.IsNullOrWhiteSpace(musicStr))
                    {
                        musicStr = musicStr + "\n";
                    }
                    musicStr = musicStr + musicNode.InnerText;
                }
                string newMusicStr = "";
                foreach (string str in musicStr.Trim().Split("\n"))
                {
                    if (String.IsNullOrWhiteSpace(str.Trim()))
                    {
                        continue;
                    }
                    if (!String.IsNullOrWhiteSpace(newMusicStr))
                    {
                        newMusicStr = newMusicStr + "\n";
                    }
                    newMusicStr = newMusicStr + str.Trim();
                }

                OpeningSong.Text = newMusicStr;
                EndingSong.Text = newMusicStr;
                CharactorSong.Text = newMusicStr;
                InsertSong.Text = newMusicStr;


                bool premiereDateFound = false;
                HtmlNodeCollection? premiereDateNodes = animeDoc?.DocumentNode?.SelectNodes(@"//td");
                if (premiereDateNodes != null)
                {
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
            }
            else
            {
                MySqlConnection conn = GetConn();
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM `WP_ANIME` WHERE `NAME` = '" + data.Name + "';";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        Name.Text = rdr.GetValue(rdr.GetOrdinal("name")).ToString();
                        OriginalName.Text = rdr.GetValue(rdr.GetOrdinal("original_name")).ToString();
                        Type.SelectedIndex = int.Parse(rdr.GetValue(rdr.GetOrdinal("type")).ToString());
                        Introduction.Text = rdr.GetValue(rdr.GetOrdinal("introduction")).ToString();
                        Staff.Text = rdr.GetValue(rdr.GetOrdinal("staff")).ToString();
                        Cast.Text = rdr.GetValue(rdr.GetOrdinal("cast")).ToString();
                        TotalEpisodes.Text = rdr.GetValue(rdr.GetOrdinal("total_episodes")).ToString();
                        ProductionCompany.Text = rdr.GetValue(rdr.GetOrdinal("production_company")).ToString();
                        OpeningSong.Text = rdr.GetValue(rdr.GetOrdinal("opening_song")).ToString();
                        EndingSong.Text = rdr.GetValue(rdr.GetOrdinal("ending_song")).ToString();
                        CharactorSong.Text = rdr.GetValue(rdr.GetOrdinal("charactor_song")).ToString();
                        InsertSong.Text = rdr.GetValue(rdr.GetOrdinal("insert_song")).ToString();
                        Achievement.Text = rdr.GetValue(rdr.GetOrdinal("achievement")).ToString();
                        Other.Text = rdr.GetValue(rdr.GetOrdinal("other")).ToString();
                        PremiereDate.Text = rdr.GetValue(rdr.GetOrdinal("premiere_date")).ToString();
                        TitleImgExternalLink.Text = rdr.GetValue(rdr.GetOrdinal("title_img_external_link")).ToString();
                        OfficialWebsite.Text = rdr.GetValue(rdr.GetOrdinal("official_website")).ToString();
                    }
                }
                catch (Exception ex)
                {
                    CustomizableMessageBox.MessageBox.Show("select failed. \n" + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private bool CheckValues()
        {
            // 名字, 介绍, 工作人员, 声优表, 制作公司, 首播日期不能为空
            if (String.IsNullOrWhiteSpace(Name.Text) ||
                String.IsNullOrWhiteSpace(Introduction.Text) ||
                String.IsNullOrWhiteSpace(Staff.Text) ||
                String.IsNullOrWhiteSpace(Cast.Text) ||
                String.IsNullOrWhiteSpace(ProductionCompany.Text) ||
                String.IsNullOrWhiteSpace(PremiereDate.Text))
            {
                CustomizableMessageBox.MessageBox.Show(Prefab.GetPropertiesSetter(PropertiesSetterName.Black), "名字, 介绍, 工作人员, 声优表, 制作公司, 首播日期不能为空");
                return false;
            }

            if (!int.TryParse(TotalEpisodes.Text, out _))
            {
                CustomizableMessageBox.MessageBox.Show(Prefab.GetPropertiesSetter(PropertiesSetterName.Black), "集数不能为非数字");
                return false;
            }

            if (!DateTime.TryParse(PremiereDate.Text, out _))
            {
                CustomizableMessageBox.MessageBox.Show(Prefab.GetPropertiesSetter(PropertiesSetterName.Black), "日期格式不对");
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
