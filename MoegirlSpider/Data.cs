using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoegirlSpider
{
    public enum Type
    {
        TV版 = 0,
        剧场版 = 1,
        OVA = 2,
        OAD = 3,
        WEB = 4
    }
    internal class Data
    {
        private string? name;
        private string? originalName;
        private Type type;
        private string? introduction;
        private string? staff;
        private string? cast;
        private string? totalEpisodes;
        private string? productionCompany;
        private string? openingSong;
        private string? endingSong;
        private string? charactorSong;
        private string? insertSong;
        private string? achievement;
        private string? other;
        private string? premiereDate;
        private string? titleImgExternalLink;
        private string? officialWebsite;

        public string? Name { get => name; set => name = value; }
        public string? OriginalName { get => originalName; set => originalName = value; }
        public Type Type { get => type; set => type = value; }
        public string? Introduction { get => introduction; set => introduction = value; }
        public string? Staff { get => staff; set => staff = value; }
        public string? Cast { get => cast; set => cast = value; }
        public string? TotalEpisodes { get => totalEpisodes; set => totalEpisodes = value; }
        public string? ProductionCompany { get => productionCompany; set => productionCompany = value; }
        public string? OpeningSong { get => openingSong; set => openingSong = value; }
        public string? EndingSong { get => endingSong; set => endingSong = value; }
        public string? CharactorSong { get => charactorSong; set => charactorSong = value; }
        public string? InsertSong { get => insertSong; set => insertSong = value; }
        public string? Achievement { get => achievement; set => achievement = value; }
        public string? Other { get => other; set => other = value; }
        public string? PremiereDate { get => premiereDate; set => premiereDate = value; }
        public string? TitleImgExternalLink { get => titleImgExternalLink; set => titleImgExternalLink = value; }
        public string? OfficialWebsite { get => officialWebsite; set => officialWebsite = value; }
    }
}
