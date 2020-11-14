using AngleSharp;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace JobFilter.Models.DataStructure
{
    public class JobCrawler
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();
        readonly List<string> TagsContent = new List<string>();
        readonly List<Job> Jobs = new List<Job>();
        readonly string _url;
        bool MissionComplete = false;
        bool EncounterError = false;
        string PageContent = null;

        public JobCrawler(string url)
        {
            _url = url;
        }

        public bool IsEncounterError()
        {
            return EncounterError;
        }

        public void SetMissionCompleteFlag(bool value)
        {
            MissionComplete = value;
        }

        public bool IsMissionComplete()
        {
            return MissionComplete;
        }

        public List<Job>  GetJobs()
        {
            return Jobs;
        }

        public void WaitingForFunctionIO(string FunctionName)
        {
            int StopCount = 0;
            int StopBound = 25;
            if (FunctionName == "LoadPage")
            {
                while (PageContent == null)
                {
                    Thread.Sleep(200);

                    // 若等候太久則停止作業
                    StopCount++;
                    if (StopCount > StopBound)
                    {
                        EncounterError = true;
                        return;
                    }
                }
            }
            else if (FunctionName == "ExtractTags")
            {
                while (TagsContent.Count < 2 && !IsMissionComplete())
                {
                    Thread.Sleep(200);

                    // 若等候太久則停止作業
                    StopCount++;
                    if (StopCount > StopBound)
                    {
                        EncounterError = true;
                        return;
                    }
                }
            }
        }

        public int GetFirstNum(string TargetSection)
        {
            // 搜尋到第一個數字後開始擷取數字字元，直到出現非數字
            List<char> chars = new List<char>();
            bool HasFindDigit = false;
            foreach (char C in TargetSection)
            {
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
            // 沒寫數字代表待遇面議(以40000表示)
            return chars.Count == 0 ? 40000 : int.Parse(string.Join("", chars));
        }

        public int GetMinWage(string TargetSection)
        {
            int result = GetFirstNum(TargetSection);

            return TargetSection.Contains("年薪") ? result / 12 : result;
        }

        public int GetMaxWage(string TargetSection)
        {
            // 若面議則返回最大值，確保不會被過濾掉
            if(TargetSection.Contains("面議"))
                return 2147483647;

            // 薪資範圍是以 '~' 做分割
            int index = TargetSection.IndexOf('~');

            // 但可能沒寫薪資範圍，而只有寫最低月薪或最低年薪
            if (index < 0)
            {
                // 若只有一個數字，用 GetMinWage 處理即可
                return GetMinWage(TargetSection);
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
            if (string.IsNullOrEmpty(TargetSection) || string.IsNullOrEmpty(S1) || string.IsNullOrEmpty(S2))
            {
                return "";
            }

            int IndexOfS1 = TargetSection.IndexOf(S1);
            int IndexOfS2 = TargetSection.IndexOf(S2, IndexOfS1 + S1.Length);

            if (IndexOfS1 < 0 || IndexOfS2 < 0 || IndexOfS1 > IndexOfS2)
            {
                return "";
            }

            return TargetSection.Substring(IndexOfS1 + S1.Length, IndexOfS2 - IndexOfS1 - S1.Length);
        }

        public void ExtractJobData()
        {
            // 解析失敗，提前結束任務
            if (TagsContent.Count < 2)
            {
                MissionComplete = true;
                _logger.Error($"解析失敗，<article>標籤數量太少");
                return;
            }

            // 最後兩筆不是工作的資料，所以去除掉
            TagsContent.RemoveAt(TagsContent.Count - 1);
            TagsContent.RemoveAt(TagsContent.Count - 1);

            foreach (string tag in TagsContent)
            {
                if (tag == null) continue;

                // 擷取工作名稱
                int AllowLength = 26;
                string TargetStr = "data-job-name";
                int Index = tag.IndexOf(TargetStr) + TargetStr.Length;
                string JobTitle = tag.Substring(Index, 200);
                JobTitle = GetValueBetweenTwoString(JobTitle, "\"", "\"");
                if(JobTitle.Length > AllowLength)
                {
                    JobTitle = JobTitle.Substring(0, AllowLength) + "...";
                }

                // 擷取公司名稱
                AllowLength = 30;
                TargetStr = "data-cust-name";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string Company = tag.Substring(Index, 50);
                Company = GetValueBetweenTwoString(Company, "\"", "\"");
                if (Company.Length > AllowLength)
                {
                    Company = Company.Substring(0, AllowLength) + "...";
                }

                // 擷取工作網址
                TargetStr = "<a";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string JobLink = tag.Substring(Index, 80);
                JobLink = "https:" + GetValueBetweenTwoString(JobLink, "\"", "\"");

                // 擷取地區 & 經歷 & 學歷
                TargetStr = "b-list-inline b-clearfix job-list-intro b-content";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                Index = tag.IndexOf("<li", Index) + 2; // 從ul移動到li
                string JobArea = tag.Substring(Index, 20);
                JobArea = GetValueBetweenTwoString(JobArea, ">", "<");

                Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                string JobExperience = tag.Substring(Index, 20);
                JobExperience = GetValueBetweenTwoString(JobExperience, ">", "<");

                Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                string Education = tag.Substring(Index, 20);
                Education = GetValueBetweenTwoString(Education, ">", "<");

                // 擷取工作說明
                AllowLength = 80; 
                TargetStr = "job-list-item__info b-clearfix b-content";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string PartialContent = tag.Substring(Index, 1000);
                PartialContent = PartialContent.Replace("<em class=\"b-txt--highlight\">", "");
                PartialContent = PartialContent.Replace("</em>", "").TrimEnd();
                PartialContent = GetValueBetweenTwoString(PartialContent, ">", "</").Replace("\n", " ");
                if (PartialContent.Length > AllowLength)
                {
                    PartialContent = PartialContent.Substring(0, AllowLength) + "...";
                }
                else if (string.IsNullOrEmpty(PartialContent))
                {
                    PartialContent = "工作內容字數過多，懶得處理XD";
                }

                // 擷取工作薪資
                TargetStr = "b-tag--default";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string JobWage = tag.Substring(Index, 30);
                JobWage = GetValueBetweenTwoString(JobWage, ">", "<");

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

        public async void LoadPage()
        {
            HttpClient httpClient = new HttpClient();

            var responseMessage = await httpClient.GetAsync(_url);

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                PageContent = responseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                PageContent = "Fail to load page.";
            }
        }

        public async void ExtractTags(string Tag)
        {
            // 使用 AngleSharp 的前置設定
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(res => res.Content(PageContent));

            // 取得所有該標籤的內容
            var tags = document.QuerySelectorAll(Tag);
            foreach (var tag in tags)
            {
                TagsContent.Add(tag.ToHtml());
            }
        }
    }
}
