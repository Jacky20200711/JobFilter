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
        private List<string> TagsContent = new List<string>();    // 用來存放解析頁面時取得的標籤內容
        private List<Job> Jobs = new List<Job>();                 // 用來存放解析頁面時取得的工作列表
        private string _url;                  // 用來存放這隻爬蟲所負責的頁面網址
        private bool EncounterError = false;  // 用來記錄爬取與解析頁面的過程中，是否中途出錯
        private string PageContent = null;    // 用來存放爬取的頁面內容
        private string CrawlResult = null;    // 用來判斷是否完成爬取頁面的任務
        private string ExtractResult = null;  // 用來判斷是否完成解析頁面的任務

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
                    // 從遇到第一個數字開始，持續擷取遇到的數字
                    HasFindDigit = true;
                    chars.Add(C);
                }
                else
                {
                    // 若在遇到第一個數字後遇到了非數字則跳出
                    if (HasFindDigit) break;
                }
            }

            // 沒寫數字則代表面議(最低月薪以40000表示)
            return chars.Count == 0 ? 40000 : int.Parse(string.Join("", chars));
        }

        public int GetMinWage(string TargetSection)
        {
            // 若薪資內容為年薪且只有一個數字，將它除以12即為最低月薪
            int firstNum = GetFirstNum(TargetSection);
            return TargetSection.Contains("年薪") ? firstNum / 12 : firstNum;
        }

        public int GetMaxWage(string TargetSection)
        {
            // 若面議則令最高月薪返回最大值，確保最低月薪為40000以上的工作不會被過濾掉
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
                // 從 '~' 所在的索引擷取第二個數字
                int result = GetFirstNum(TargetSection.Substring(index));
                return TargetSection.Contains("年薪") ? result / 12 : result;
            }
        }

        public string GetValueBetweenTwoString(string TargetSection, string S1, string S2)
        {
            int IndexOfS1 = TargetSection.IndexOf(S1);                          // 搜尋S1的索引
            int IndexOfS2 = TargetSection.IndexOf(S2, IndexOfS1 + S1.Length);   // 從S1後面搜尋S2的索引

            if (IndexOfS1 < 0)
            {
                return "";
            }
            
            int startIndex = IndexOfS1 + S1.Length;

            // 若抓不到右界，則返回 "S1到尾端的字元" + "..."
            if (IndexOfS2 < 0)
            {
                return TargetSection.Substring(startIndex, TargetSection.Length - startIndex) + "...";
            }
            // 若抓得到右界，則返回 S1 和 S2 之間的字元
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
                    int Index = tag.IndexOf(TargetStr);

                    /*
                        若104搜尋到的工作不到20個(即不滿一頁)，則在 "搜尋結果太少" 的提示下方會出現推薦工作，
                        觀察頁面代碼後，發現該提示和各工作一樣，都被夾帶在其中一組<article>，假設該提示出現在第X組，
                        那麼當程式解析第X組的工作名稱時會抓不到東西，可以利用這一點來避免解析到下方的推薦工作!
                    */

                    // 檢查當前這組 <article> 是否可以擷取到工作名稱，若不能則結束任務
                    if (Index > -1)
                    {
                        Index += TargetStr.Length;
                    }
                    else
                    {
                        break;
                    }
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
                    if(PartialContent.Length > 32)
                    {
                        PartialContent = PartialContent.Substring(0, 32) + "..."; // 將顯示字數限制在一行以內
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
            try
            {
                // 爬取指定頁面
                HttpClient httpClient = new HttpClient();
                var responseMessage = await httpClient.GetAsync(_url);

                // 確認請求頁面的狀態
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // 讀取頁面的內容
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
