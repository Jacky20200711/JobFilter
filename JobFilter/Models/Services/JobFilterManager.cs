using JobFilter.Models.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFilter.Models.Services
{
    public static class JobFilterManager
    {
        public static void GetFilterJobs(
            List<Job> FilterJobs, 
            List<Job> UnfilteredJobs, 
            int MinimumWage, 
            string ExcludeWord,
            string IgnoreCompany)
        {
            // ...
        }
    }
}
