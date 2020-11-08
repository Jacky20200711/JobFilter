# 專案說明  
令使用者可以設定目標網址、最低月薪、多個欲排除的關鍵字、多個欲排除的公司，  
來進一步過濾從目標網址返回的工作列表，令顯示的工作內容更符合期望。  
※目標網址的開頭必須是 https://www.104.com.tw/jobs/search/  
  
# 使用技術  
01.使用 MVC 架構  
02.使用 HttpClient 爬取網頁  
03.使用 AngleSharp 解析網頁  
04.使用 Multi-thread & 異步來增進爬取的效率  
05.使用 Session 存取過濾後的工作項目  
06.使用 ASP.NET Identity 搭建會員系統  
07.使用 Google API 讓使用者可以用 Google 帳戶登入  
08.使用 NLog 協助除錯  
09.使用 X.PagedList.Mvc.Core 實現分頁  
10.使用 CsvHelper 匯出或匯入資料  
11.使用 UnitTest 測試部分 Function 的功能  
  
# 開發環境  
Win10(64bit) + Visual Studio 2019 + .NET Core 3.1 MVC + IIS 10 + MSTestv2  
&emsp;  
&emsp;  
# 首頁  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_01.PNG?raw=true)  
&emsp;  
&emsp;  
&emsp;  
# 登入  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_02.PNG?raw=true)  
&emsp;  
&emsp;  
&emsp;  
# 設定列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
&emsp;  
&emsp;  
# 過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
&emsp;  
&emsp;  
# 點擊工作的 Title 可以連結到 104 的應徵頁面  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  
&emsp;  
&emsp  
