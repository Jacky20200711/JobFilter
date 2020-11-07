using AngleSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace JobFilter.Models.DataStructure
{
    public class JobCrawler
    {
        readonly List<string> TagsContent = new List<string>();
        readonly List<Job> Jobs = new List<Job>();
        readonly string _url;
        bool MissionComplete = false;
        string PageContent = null;

        public JobCrawler(string url)
        {
            _url = url;
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
            if (FunctionName == "LoadPage")
            {
                while (PageContent == null) ;
            }
            else if (FunctionName == "ExtractTags")
            {
                while (TagsContent == null) ;
            }
        }

        public string GetValueBetweenChars(string TargetSection, char C1, char C2)
        {
            List<char> chars = new List<char>();
            bool HasFindC1 = false;
            foreach (char C in TargetSection)
            {
                if (C == C1 && !HasFindC1)
                {
                    HasFindC1 = true;
                    continue;
                }

                if (HasFindC1)
                {
                    if (C != C2)
                    {
                        chars.Add(C);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return string.Join("", chars);
        }

        public int GetLowestWage(string TargetSection)
        {
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

            // 沒寫數字代表待遇面議
             return chars.Count == 0 ? 40000 : int.Parse(string.Join("", chars));
        }

        public void ExtractJobData()
        {
            HashSet<string> IgnoreCompany = new HashSet<string>();

            foreach (string tag in TagsContent)
            {
                if (tag == null) continue;

                // 擷取工作名稱
                string TargetStr = "data-job-name";
                int Index = tag.IndexOf(TargetStr) + TargetStr.Length;
                string JobTitle = tag.Substring(Index, 32) + "...";
                JobTitle = GetValueBetweenChars(JobTitle, '\"', '\"');

                // 擷取公司名稱
                TargetStr = "data-cust-name";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string Company = tag.Substring(Index, 40);
                Company = GetValueBetweenChars(Company, '\"', '\"');

                // 擷取工作網址
                TargetStr = "<a";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string JobLink = tag.Substring(Index, 70);
                JobLink = "https:" + GetValueBetweenChars(JobLink, '\"', '\"');

                // 擷取地區 & 經歷 & 學歷
                TargetStr = "b-list-inline b-clearfix job-list-intro b-content";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                Index = tag.IndexOf("<li", Index) + 2; // 從ul移動到li
                string JobArea = tag.Substring(Index, 30);
                JobArea = GetValueBetweenChars(JobArea, '>', '<');

                Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                string JobExperience = tag.Substring(Index, 20);
                JobExperience = GetValueBetweenChars(JobExperience, '>', '<');

                Index = tag.IndexOf("<li", Index) + 2; // 移動到下一個li
                string Education = tag.Substring(Index, 20);
                Education = GetValueBetweenChars(Education, '>', '<');

                // 擷取部分的工作說明
                TargetStr = "job-list-item__info b-clearfix b-content";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string PartialContent = tag.Substring(Index, 45) + "...";
                PartialContent = GetValueBetweenChars(PartialContent, '>', '<').TrimEnd();
                PartialContent = PartialContent.Replace("\n", " ");

                // 擷取工作薪資 & 最低薪資(若太低則過濾掉這筆工作)
                TargetStr = "b-tag--default";
                Index = tag.IndexOf(TargetStr, Index) + TargetStr.Length;
                string JobWage = tag.Substring(Index, 30);
                JobWage = GetValueBetweenChars(JobWage, '>', '<');

                // 儲存工作資料
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
                    MinimumWage = GetLowestWage(JobWage)
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

            // 最後兩筆不是工作的資料，所以去除掉
            TagsContent.RemoveAt(TagsContent.Count - 1);
            TagsContent.RemoveAt(TagsContent.Count - 1);
        }
    }
}
