﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 任务_线程与同步
{
    class Program
    {
        static void Main(string[] args)
        {

            //Console.ReadLine();
            //ParallelExample.ForWithSleep(10);
            //ParallelExample.ForWithBreak(1000);
            //ParallelExample.NormalFor(10000000);
            //Console.WriteLine(ParallelExample.ForTLocal(1000000));
            //ParallelExample.ForEachTLocal(1000000);
            //ParallelExample.Invoke(100);
            //TaskExample.TaskWithMultiClass();
            //ParallelExample.ParallelWithCancellation();
            //ThreadPoolExample.ThreadTest(5);
            ThreadExample.StartThread(5);
            Console.ReadLine();
        }
    }

    public class ParallelExample
    {
        public static void NormalFor(int x)
        {
            for (int i = 0; i < x; i++)
            {
                string result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                //Console.WriteLine(result);
            }
        }

        public static void ForWithSleep(int x)
        {
            ParallelLoopResult loopResult = Parallel.For(0, x, i =>
            {
                string result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine(result);

                Thread.Sleep(100);

                result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine(result);
            });
            Console.WriteLine($"Loop is completed? {loopResult.IsCompleted}");
        }

        public static void ForWithDelay(int x)
        {
            ParallelLoopResult loopResult = Parallel.For(0, x, async i =>
            {
                string result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine(result);

                await Task.Delay(1000);

                result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine(result);
            });
            Console.WriteLine($"Loop is completed? {loopResult.IsCompleted}");
        }

        public static void ForWithBreak(int x)
        {
            ParallelLoopResult loopResult = Parallel.For(0, x, (int i, ParallelLoopState pls) =>
            {
                string result = $"{i}. task:{Task.CurrentId}  thread:{Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine(result);

                if (i > 15)
                    pls.Break();
            });
            Console.WriteLine($"Loop is completed? {loopResult.IsCompleted}");
            Console.WriteLine(loopResult.LowestBreakIteration);
        }

        public static long ForTLocal(int x)
        {
            int[] nums = Enumerable.Range(0, x).ToArray();

            long total = 0;
            Parallel.For<long>(0, nums.Length,
            () =>
             {
                 Console.WriteLine($"Thread{Thread.CurrentThread.ManagedThreadId} start.");
                 return 0;
             },
            (j, loop, subtotal) =>
             {
                 subtotal += nums[j];
                 return subtotal;
             },
            (subtotal) =>
             {
                 Interlocked.Add(ref total, subtotal);
                 Console.WriteLine($"Thread{Thread.CurrentThread.ManagedThreadId} end.");
             }
            );

            return total;

        }

        public static void ForEachTLocal(int x)
        {
            int[] nums = Enumerable.Range(0, x).ToArray();
            long total = 0;
            ParallelLoopResult result =
            Parallel.ForEach<int, long>(nums, () => 0,
            (i, pls, subtotal) =>
            {
                subtotal += nums[i];
                return subtotal;
            },
            (subtotal) =>
            {
                Interlocked.Add(ref total, subtotal);
            });
            Console.WriteLine(total);
        }

        public static void Invoke(int x)
        {
            List<Action> actionList = new List<Action>();
            for (int i = 0; i < x; i++)
            {
                int no = i;
                actionList.Add(() => { Console.WriteLine($"Aciton {no} done!"); });
            }
            Parallel.Invoke(actionList.ToArray());
        }

        public static void ParallelWithCancellation()
        {
            var cts = new CancellationTokenSource();
            cts.Token.Register(() => Console.WriteLine("*** token canceled"));
            // send a cancel after 500 ms
            cts.CancelAfter(500);
            try
            {
                ParallelLoopResult result =
                Parallel.For(0, 100, new ParallelOptions()
                {
                    CancellationToken = cts.Token,
                },
                x =>
                {
                    Console.WriteLine("loop {0} started", x);
                    int sum = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(2);
                        sum += i;
                    }
                    Console.WriteLine("loop {0} finished", x);
                });
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }


    public class TaskExample
    {
        public static void TaskWithMultiClass()
        {
            var parent = new Task(ParentTask);
            parent.Start();
            Thread.Sleep(2000);
            Console.WriteLine($"parent.Status: {parent.Status}");
            Thread.Sleep(4000);
            Console.WriteLine($"parent.Status: {parent.Status}");
        }
        static void ParentTask()
        {
            Console.WriteLine("parent task id: {0}", Task.CurrentId);
            var child = new Task(ChildTask, TaskCreationOptions.AttachedToParent);
            child.Start();
            Thread.Sleep(1000);
            Console.WriteLine($"child.Status: {child.Status}");
        }
        static void ChildTask()
        {
            Console.WriteLine("child start");
            Thread.Sleep(5000);
            Console.WriteLine("child finished");
        }
    }

    public class ThreadExample
    {
        public static void StartThread(int x)
        {
            ThreadWithPar twp = new ThreadWithPar(x);
            Thread t = new Thread(twp.ThreadMain);
            t.Start();
        }
    }

    public class ThreadWithPar
    {
        public int ID { get; set; }
        public ThreadWithPar(int newID)
        {
            ID = newID;
        }
        public void ThreadMain()
        {
            Console.WriteLine($"ID is {ID}");
        }
    }

    public class ThreadPoolExample
    {
        public static void ThreadTest(int x)
        {
            int nWorkerThreads;
            int nCompletionPortThreads;
            ThreadPool.GetMaxThreads(out nWorkerThreads, out nCompletionPortThreads);
            Console.WriteLine("Max worker threads: {0}, " +
            "I/O completion threads: {1}", nWorkerThreads, nCompletionPortThreads);
            for (int i = 0; i < x; i++)
            {
                ThreadPool.QueueUserWorkItem(JobForAThread);
            }
            Thread.Sleep(3000);
        }

        private static void JobForAThread(object state)
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"loop:{i} runing inside pooled thread:{Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}
