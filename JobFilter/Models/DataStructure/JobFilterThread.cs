namespace JobFilter.Models.DataStructure
{
    public class JobFilterThread
    {
        private static JobCrawler _jobCrawler;

        public JobFilterThread(JobCrawler jobCrawler)
        {
            _jobCrawler = jobCrawler;
        }

        public void DoFilter()
        {
            // 爬取網頁 & 等待爬蟲的 IO 處理完畢
            _jobCrawler.LoadPage();
            _jobCrawler.WaitingForFunctionIO("LoadPage");

            // 取得所有工作區塊(被包在<article></article>) & 等待爬蟲的 IO 處理完畢
            _jobCrawler.ExtractTags("article");
            _jobCrawler.WaitingForFunctionIO("ExtractTags");

            // 從工作區塊中，擷取出想要的資料
            _jobCrawler.ExtractJobData();
        }
    }
}
