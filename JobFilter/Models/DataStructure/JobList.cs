using System;
using System.Collections.Generic;

namespace JobFilter.Models.DataStructure
{
    [Serializable]
    public class JobList : ICollection<Job>
    {
        public List<Job> JobItems;

        public JobList()
        {
            JobItems = new List<Job>();
        }

        public bool IsReadOnly => ((ICollection<Job>)JobItems).IsReadOnly;

        public int Count => ((ICollection<Job>)JobItems).Count;

        public void Add(Job item)
        {
            ((ICollection<Job>)JobItems).Add(item);
        }

        public void Clear()
        {
            ((ICollection<Job>)JobItems).Clear();
        }

        public bool Contains(Job item)
        {
            return ((ICollection<Job>)JobItems).Contains(item);
        }

        public void CopyTo(Job[] array, int arrayIndex)
        {
            ((ICollection<Job>)JobItems).CopyTo(array, arrayIndex);
        }


        public bool Remove(Job item)
        {
            return ((ICollection<Job>)JobItems).Remove(item);
        }

        #region IEnumerator

        IEnumerator<Job> IEnumerable<Job>.GetEnumerator()
        {
            return JobItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return JobItems.GetEnumerator();
        }

        #endregion
    }
}
