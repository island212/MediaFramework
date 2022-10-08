using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MediaFramework.LowLevel
{
    [BurstCompatible]
    public unsafe struct JobLogger
    {
        public int Errors { get; private set; }

        public int Length => m_LogTypes.Length;

        public bool IsCreated => m_LogTypes.IsCreated;

        UnsafeList<int> m_LogTags;
        UnsafeList<LogType> m_LogTypes;
        UnsafeList<FixedString128Bytes> m_LogMessages;

        public JobLogger(int capacity, Allocator allocator)
        {
            m_LogTags = new UnsafeList<int>(capacity, allocator);
            m_LogTypes = new UnsafeList<LogType>(capacity, allocator);
            m_LogMessages = new UnsafeList<FixedString128Bytes>(capacity, allocator);
            Errors = 0;
        }

        public void LogWarning(int tag, in FixedString128Bytes message)
        {
            LogWithType(LogType.Warning, tag, message);
        }

        public void Trace(int tag, in FixedString128Bytes message)
        {
            LogWithType(LogType.Log, 0, message);
        }

        public void Log(int tag, in FixedString128Bytes message)
        {
            LogWithType(LogType.Log, tag, message);
        }

        public void LogError(int tag, in FixedString128Bytes message)
        {
            Errors++;
            LogWithType(LogType.Error, tag, message);
        }

        public ref readonly LogType LogTypeAt(int index)
            => ref m_LogTypes.ElementAt(index);

        public ref readonly int LogTagAt(int index)
            => ref m_LogTags.ElementAt(index);

        public ref readonly FixedString128Bytes MessageAt(int index)
            => ref m_LogMessages.ElementAt(index);

        [NotBurstCompatible]
        public void PrintAll()
        {
            for (int i = 0; i < Length; i++)
            {
                switch (m_LogTypes[i])
                {
                    case LogType.Error:
                        UnityEngine.Debug.LogError(m_LogTags[i] != 0 ? $"Tag={m_LogTags[i]} {m_LogMessages[i]}" : m_LogMessages[i]);
                        break;
                    case LogType.Assert:
                        UnityEngine.Debug.LogAssertion(m_LogTags[i] != 0 ? $"Tag={m_LogTags[i]} {m_LogMessages[i]}" : m_LogMessages[i]);
                        break;
                    case LogType.Warning:
                        UnityEngine.Debug.LogWarning(m_LogTags[i] != 0 ? $"Tag={m_LogTags[i]} {m_LogMessages[i]}" : m_LogMessages[i]);
                        break;
                    case LogType.Log:
                        UnityEngine.Debug.Log(m_LogTags[i] != 0 ? $"Tag={m_LogTags[i]} {m_LogMessages[i]}" : m_LogMessages[i]);
                        break;
                }
            }
        }

        public void Clear()
        {
            m_LogTags.Clear();
            m_LogTypes.Clear();
            m_LogMessages.Clear();
            Errors = 0;
        }

        public void Dispose()
        {
            m_LogTags.Dispose();
            m_LogTypes.Dispose();
            m_LogMessages.Dispose();
        }

        internal void LogWithType(LogType type, int tag, in FixedString128Bytes message)
        {
            m_LogTags.Add(tag);
            m_LogTypes.Add(type);
            m_LogMessages.Add(message);

            Assert.IsTrue(m_LogTags.Length == m_LogTypes.Length && m_LogTypes.Length == m_LogMessages.Length, "All list length should be equals");
        }
    }
}
