using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MediaFramework.LowLevel
{
    public enum JobLogType
    { 
        Log,
        Warning,
        Error
    }

    public struct JobLog
    {
        public int tag;
        public JobLogType type;
        public FixedString128Bytes message;
    }

    [BurstCompatible]
    public unsafe struct JobLogger : IDisposable, IEnumerable<JobLog>
    {
        public int Errors { get; private set; }

        UnsafeList<int> m_Tags;
        UnsafeList<JobLogType> m_JobLogs;
        UnsafeList<FixedString128Bytes> m_JobLogMessages;

        public bool IsCreated => m_JobLogs.IsCreated && m_JobLogMessages.IsCreated;

        public int Length => m_JobLogs.Length;

        public JobLogger(int capacity, Allocator allocator)
        {
            m_Tags = new UnsafeList<int>(capacity, allocator);
            m_JobLogs = new UnsafeList<JobLogType>(capacity, allocator);
            m_JobLogMessages = new UnsafeList<FixedString128Bytes>(capacity, allocator);
            Errors = 0;
        }

        public void LogError(in FixedString128Bytes message) => LogError(0, message);

        public void LogError(int tag, in FixedString128Bytes message)
        {
            Errors++;
            m_Tags.Add(tag);
            m_JobLogs.Add(JobLogType.Error);
            m_JobLogMessages.Add(message);
        }

        public IEnumerator<JobLog> GetEnumerator()
        {
            for (int i = 0; i < m_JobLogs.Length; i++)
            {
                yield return new JobLog
                {
                    type = m_JobLogs[i],
                    tag = m_Tags[i],
                    message = m_JobLogMessages[i]
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            m_Tags.Clear();
            m_JobLogs.Clear();
            m_JobLogMessages.Clear();
            Errors = 0;
        }

        public void Dispose()
        {
            m_Tags.Dispose();
            m_JobLogs.Dispose();
            m_JobLogMessages.Dispose();
        }
    }
}
