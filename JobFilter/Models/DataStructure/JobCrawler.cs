using AngleSharp;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobFilter.Models.DataStructure
{
    public class JobCrawler
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private List<string> TagsContent = new List<string>();
        private List<Job> Jobs = new List<Job>();
        private string _url;
        private bool EncounterError = false;
        private string PageContent = null;
        private string CrawlResult = null;
        private string ExtractResult = null;

        public JobCrawler(string url)
        {
            _url = url;
        }

        public bool IsEncounterError()
        {
            return EncounterError;
        }

        public List<Job>  GetJobs()
        {
            return Jobs;
        }

        public int GetFirstNum(string TargetSection)
        {
            // 搜尋到第一個數字後開始擷取數字字元，直到出現非數字
            List<char> chars = new List<char>();
            bool HasFindDigit = false;
            foreach (char C in TargetSection)
            {
                // 忽略數字中間的千分號
                if (C == ',') continue;

                int CharCode = Convert.ToInt32(C);

                if (CharCode > 47 && CharCode < 58)
                {
                    HasFindDigit = true;
                    chars.Add(C);
                }
                else
                {
                    if (HasFindDigit) break;
                }
            }

            // 沒寫數字則代表面議(最低月薪以40000表示)
            return chars.Count == 0 ? 40000 : int.Parse(string.Join("", chars));
        }

        public int GetMinWage(string TargetSection)
        {
            int firstNum = GetFirstNum(TargetSection);
            return TargetSection.Contains("年薪") ? firstNum / 12 : firstNum;
        }

        public int GetMaxWage(string TargetSection)
        {
            // 若面議則返回最大值，確保不會被過濾掉
            if (TargetSection.Contains("面議"))
            {
                return 2147483647;
            }

            // 薪資範圍是以 '~' 做分割
            int index = TargetSection.IndexOf('~');

            // 但可能沒寫薪資範圍，而只有寫最低月薪或最低年薪
            if (index < 0)
            {
                // 若只有一個數字則返回最大值，確保已符合最低月薪的工作不會被過濾掉
                return 2147483647;
            }
            else
            {
                // 從 '~' 所在的索引擷取數字
                int result = GetFirstNum(TargetSection.Substring(index));
                return TargetSection.Contains("年薪") ? result / 12 : result;
            }
        }

        public string GetValueBetweenTwoString(string TargetSection, string S1, string S2)
        {
            int IndexOfS1 = TargetSection.IndexOf(S1);
            int IndexOfS2 = TargetSection.IndexOf(S2, IndexOfS1 + S1.Length);

            if (IndexOfS1 < 0)
            {
                return "";
            }
            
            int startIndex = IndexOfS1 + S1.Length;

            // 若抓不到右界，則返回 "左界到尾端的字元" + "..."
            if (IndexOfS2 < 0)
            {
                return TargetSection.Substring(startIndex, TargetSection.Length - startIndex) + "...";
            }
            // 若抓得到右界，則返回左界和右界中間的字元
            else
            {
                return TargetSection.Substring(startIndex, IndexOfS2 - startIndex);
            }
        }

        public void ExtractJobData()
        {
            try
            {
                // 最後兩筆不是工作的資料，所以去除掉
                TagsContent.RemoveAt(TagsContent.Count - 1);
                TagsContent.RemoveAt(TagsContent.Count - 1);

                foreach (string tag in TagsContent)
                {
                    // 確保擷取的對象為有效的字串
                    if (tag == null) continue;

                    // 擷取工作名稱
                    string TargetStr = "data-job-name";
                    int Index = tag.IndexOf(TargetStr) + TargetStr.Length;
                    string JobTitle = GetValueBetweenTwoString(tag.Substring(Index, 20), "\"", "\"");

                    // 擷取公司名稱
                    TargetStr = "data-cust-name";
                    Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                    string Company = GetValueBetweenTwoString(tag.Substring(Index, 30), "\"", "\"");

                    // 擷取工作網址
                    TargetStr = "<a";
                    Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                    string JobLink = "https:" + GetValueBetweenTwoString(tag.Substring(Index, 80), "\"", "\"");

                    // 擷取地區 & 經歷 & 學歷
                    TargetStr = "b-list-inline b-clearfix job-list-intro b-content";
                    Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                    Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                    string JobArea = GetValueBetweenTwoString(tag.Substring(Index, 10), ">", "<");

                    Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                    string JobExperience = GetValueBetweenTwoString(tag.Substring(Index, 10), ">", "<");

                    Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                    string Education = GetValueBetweenTwoString(tag.Substring(Index, 10), ">", "<");

                    // 擷取工作說明
                    TargetStr = "job-list-item__info b-clearfix b-content";
                    Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                    string PartialContent = tag.Substring(Index, 200);
                    PartialContent = PartialContent.Replace("<em class=\"b-txt--highlight\">", "");
                    PartialContent = PartialContent.Replace("</em>", "").TrimEnd().Replace("\n", " ");
                    PartialContent = GetValueBetweenTwoString(PartialContent, ">", "<");
                    if(PartialContent.Length > 36)
                    {
                        PartialContent = PartialContent.Substring(0, 36) + "..."; // 將顯示字數限制在一行以內
                    }

                    // 擷取工作薪資
                    TargetStr = "b-tag--default";
                    Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                    string JobWage = GetValueBetweenTwoString(tag.Substring(Index, 30), ">", "<");

                    // 儲存工作資料 & 計算最低月薪
                    Jobs.Add(new Job()
                    {
                        Title = JobTitle,
                        Company = Company,
                        Link = JobLink,
                        Area = JobArea,
                        Experience = JobExperience,
                        Education = Education,
                        PartialContent = PartialContent,
                        WageRange = JobWage,
                        MinimumWage = GetMinWage(JobWage),
                        MaximumWage = GetMaxWage(JobWage)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                EncounterError = true;
            }
        }

        public bool IsCrawlFinished()
        {
            return CrawlResult != null;
        }

        public async Task LoadPage()
        {
            HttpClient httpClient = new HttpClient();
            var responseMessage = await httpClient.GetAsync(_url);
            try
            {
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    PageContent = responseMessage.Content.ReadAsStringAsync().Result;
                    CrawlResult = "OK";
                }
                else
                {
                    EncounterError = true;
                    CrawlResult = "NO";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                EncounterError = true;
                CrawlResult = "NO";
            }
        }

        public bool IsExtractFinished()
        {
            return ExtractResult != null;
        }

        public async Task ExtractTags()
        {
            try
            {
                // 使用 AngleSharp 的前置設定
                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(res => res.Content(PageContent));

                // 取得被包夾在 <article> 標籤的內容
                var tags = document.QuerySelectorAll("article");
                foreach (var tag in tags)
                {
                    TagsContent.Add(tag.ToHtml());
                }

                ExtractResult = "OK";
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                EncounterError = true;
                ExtractResult = "NO";
            }
        }
    }
}
