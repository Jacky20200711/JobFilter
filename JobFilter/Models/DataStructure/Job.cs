using System;

namespace JobFilter.Models.DataStructure
{
    [Serializable]
    public class Job
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Link { get; set; }
        public string Area { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public string PartialContent { get; set; }
        public string WageRange { get; set; }
        public int MinimumWage { get; set; }
    }
}
