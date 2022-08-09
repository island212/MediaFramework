using System;
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
    public struct JobLog
    {
        public LogType Type;
        public int MetaData1;
        public int MetaData2;
        public int MetaData3;
    }

    [BurstCompatible]
    public unsafe struct JobLogger : IDisposable
    {
        public readonly Allocator Allocator;

        UnsafeList<JobLog> m_JobLogs;
        UnsafeList<FixedString128Bytes> m_JobLogMessages;

        public bool IsCreated => m_JobLogs.IsCreated;

        public int Length => m_JobLogs.Length;

        public JobLogger(int capacity, Allocator allocator)
        {
            Allocator = allocator;

            m_JobLogs = new UnsafeList<JobLog>(capacity, allocator);
            m_JobLogMessages = new UnsafeList<FixedString128Bytes>(capacity, allocator);
        }

        public void Log(in JobLog header, in FixedString128Bytes message)
        {
            m_JobLogs.Add(header);
            m_JobLogMessages.Add(message);
        }

        public void Log(LogType type, in FixedString128Bytes message)
        {
            m_JobLogs.Add(new JobLog { Type = type });
            m_JobLogMessages.Add(message);
        }

        public void Log(LogType type, int meta, in FixedString128Bytes message)
        {
            m_JobLogs.Add(new JobLog 
            { 
                Type = type,
                MetaData1 = meta,
            });
            m_JobLogMessages.Add(message);
        }

        public void Log(LogType type, int meta1, int meta2, in FixedString128Bytes message)
        {
            m_JobLogs.Add(new JobLog
            {
                Type = type,
                MetaData1 = meta1,
                MetaData2 = meta2,
            });
            m_JobLogMessages.Add(message);
        }

        public readonly ref JobLog HeaderAt(int index)
        {
            return ref m_JobLogs.ElementAt(index);
        }

        public readonly ref FixedString128Bytes MessageAt(int index)
        {
            return ref m_JobLogMessages.ElementAt(index);
        }

        public void Clear()
        {
            m_JobLogs.Clear();
        }

        public void Dispose()
        {
            m_JobLogs.Dispose();
        }
    }
}
