# 專案說明  
令使用者可以設定薪資的低標和最低高標、多個欲排除的關鍵字、多個欲排除的公司名稱，  
來進一步過濾從104返回的頁面內容，令顯示的工作項目更加符合期望。  
※目標網址的開頭必須是 https://www.104.com.tw/jobs/search/  
&emsp;  
# 使用技術  
01.使用 MVC 架構  
02.使用 HttpClient 爬取網頁  
03.使用 AngleSharp 解析網頁  
04.使用 Asynchronous 來提升爬取的效率  
05.使用 Session 存取過濾後的工作項目  
06.使用 Entity Framework Code First 搭建資料庫  
07.使用 ASP.NET Identity 搭建會員系統  
08.使用 Google API 讓使用者可以用 Google 帳戶登入  
09.使用 NLog 協助除錯  
10.使用 X.PagedList.Mvc.Core 實現分頁  
11.使用 CsvHelper 匯出或匯入資料  
12.使用 UnitTest 測試部分 Function 的功能  
&emsp;  
# 開發環境  
Win10(64bit) + Visual Studio 2019 + .NET Core 3.1 MVC + IIS 10 + MSTestv2  
&emsp;  
# 首頁  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_01.PNG?raw=true)  
&emsp;  
# 登入  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_02.PNG?raw=true)  
&emsp;  
# 設定列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
# 過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
# 點擊工作的 Title 可以連結到 104 的應徵頁面  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  
