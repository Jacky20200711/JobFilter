# 專案說明  
用途為簡單的工作過濾器，  
令使用者可以設定目標網址、薪資水準、欲排除的公司名稱與職稱關鍵字，  
來進一步過濾從104返回的工作列表，令顯示的工作列表更加符合期望。  
※目標網址的開頭必須是 https://www.104.com.tw/jobs/search/  
※若查詢的結果有多個分頁，只會嘗試解析前6頁的內容。  
&emsp;  
# 使用技術  
01.使用 MVC 架構  
02.使用 HttpClient 爬取網頁  
03.使用 AngleSharp 解析網頁  
04.使用 Asynchronous 提升爬取與解析的效率  
05.使用 Session 存取過濾後的工作項目  
06.使用 Entity Framework Code First 存取資料庫  
07.使用 ASP.NET Identity 搭建會員系統  
08.使用 Microsoft.AspNetCore.Authentication.Google 實現 Google 登入  
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
# 新增設定  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_03.PNG?raw=true)  
&emsp;  
# 設定列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_04.PNG?raw=true)  
&emsp;  
# 過濾後的工作列表  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_05.PNG?raw=true)  
&emsp;  
# 點選職稱可以連結到應徵頁面  
![image](https://github.com/Jacky20200711/JobFilter/blob/master/DEMO_06.PNG?raw=true)  
&emsp;  
