﻿using System;
using System.Threading;

namespace TensorFlowNET.UnitTest
{
    public delegate void MultiThreadedTestDelegate(int threadid);

    /// <summary>
    ///     Creates a synchronized eco-system of running code.
    /// </summary>
    public class MultiThreadedUnitTestExecuter : IDisposable
    {
        public int ThreadCount { get; }
        public Thread[] Threads { get; }
        private readonly SemaphoreSlim barrier_threadstarted;
        private readonly ManualResetEventSlim barrier_corestart;
        private readonly SemaphoreSlim done_barrier2;

        public Action<MultiThreadedUnitTestExecuter> PostRun { get; set; }

        #region Static

        public static void Run(int threadCount, MultiThreadedTestDelegate workload)
        {
            if (workload == null) throw new ArgumentNullException(nameof(workload));
            if (threadCount <= 0) throw new ArgumentOutOfRangeException(nameof(threadCount));
            new MultiThreadedUnitTestExecuter(threadCount).Run(workload);
        }

        public static void Run(int threadCount, params MultiThreadedTestDelegate[] workloads)
        {
            if (workloads == null) throw new ArgumentNullException(nameof(workloads));
            if (workloads.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(workloads));
            if (threadCount <= 0) throw new ArgumentOutOfRangeException(nameof(threadCount));
            new MultiThreadedUnitTestExecuter(threadCount).Run(workloads);
        }

        public static void Run(int threadCount, MultiThreadedTestDelegate workload, Action<MultiThreadedUnitTestExecuter> postRun)
        {
            if (workload == null) throw new ArgumentNullException(nameof(workload));
            if (postRun == null) throw new ArgumentNullException(nameof(postRun));
            if (threadCount <= 0) throw new ArgumentOutOfRangeException(nameof(threadCount));
            new MultiThreadedUnitTestExecuter(threadCount) {PostRun = postRun}.Run(workload);
        }

        #endregion


        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public MultiThreadedUnitTestExecuter(int threadCount)
        {
            if (threadCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(threadCount));
            ThreadCount = threadCount;
            Threads = new Thread[ThreadCount];
            done_barrier2 = new SemaphoreSlim(0, threadCount);
            barrier_corestart = new ManualResetEventSlim();
            barrier_threadstarted = new SemaphoreSlim(0, threadCount);
        }

        public void Run(params MultiThreadedTestDelegate[] workloads)
        {
            if (workloads == null)
                throw new ArgumentNullException(nameof(workloads));
            if (workloads.Length != 1 && workloads.Length % ThreadCount != 0)
                throw new InvalidOperationException($"Run method must accept either 1 workload or n-threads workloads. Got {workloads.Length} workloads.");

            if (ThreadCount == 1)
            {
                workloads[0](0);
                return;
            }

            //thread core
            void ThreadCore(MultiThreadedTestDelegate core, int threadid)
            {
                barrier_threadstarted.Release(1);
                barrier_corestart.Wait();

                //workload
                core(threadid);

                done_barrier2.Release(1);
            }

            //initialize all threads
            if (workloads.Length == 1)
            {
                var workload = workloads[0];
                for (int i = 0; i < ThreadCount; i++)
                {
                    var i_local = i;
                    Threads[i] = new Thread(() => ThreadCore(workload, i_local));
                }
            } else
            {
                for (int i = 0; i < ThreadCount; i++)
                {
                    var i_local = i;
                    var workload = workloads[i_local % workloads.Length];
                    Threads[i] = new Thread(() => ThreadCore(workload, i_local));
                }
            }

            //run all threads
            for (int i = 0; i < ThreadCount; i++) Threads[i].Start();
            //wait for threads to be started and ready
            for (int i = 0; i < ThreadCount; i++) barrier_threadstarted.Wait();

            //signal threads to start
            barrier_corestart.Set();

            //wait for threads to finish
            for (int i = 0; i < ThreadCount; i++) done_barrier2.Wait();

            //checks after ended
            PostRun?.Invoke(this);
        }

        public void Dispose()
        {
            barrier_threadstarted.Dispose();
            barrier_corestart.Dispose();
            done_barrier2.Dispose();
        }
    }
}