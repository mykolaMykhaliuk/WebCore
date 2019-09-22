using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppCore.Templates
{
    public static class TPL
    {
        public static void Run()
        {
            Task[] taskArray = new Task[10];

            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((Object obj) => {
                    CustomData data = obj as CustomData;
                    if (data == null)
                        return;

                    data.ThreadNum = Thread.CurrentThread.ManagedThreadId;
                },
                                                      new CustomData() { Name = i, CreationTime = DateTime.Now.Ticks });
            }
            Task.WaitAll(taskArray);
            foreach (var task in taskArray)
            {
                var data = task.AsyncState as CustomData;
                if (data != null)
                    Console.WriteLine("Task #{0} created at {1}, ran on thread #{2}.",
                                      data.Name, data.CreationTime, data.ThreadNum);
            }
        }

        class CustomData
        {
            public long CreationTime;
            public int Name;
            public int ThreadNum;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ///

        public static void Example2()
        {
            Task<Double>[] taskArray = { Task<Double>.Factory.StartNew(() => DoComputation(1.0)),
                                     Task<Double>.Factory.StartNew(() => DoComputation(100.0)),
                                     Task<Double>.Factory.StartNew(() => DoComputation(1000.0)) };

            var results = new Double[taskArray.Length];
            Double sum = 0;

            for (int i = 0; i < taskArray.Length; i++)
            {
                results[i] = taskArray[i].Result;
                Console.Write("{0:N1} {1}", results[i],
                                  i == taskArray.Length - 1 ? "= " : "+ ");
                sum += results[i];
            }
            Console.WriteLine("{0:N1}", sum);
        }

        private static Double DoComputation(Double start)
        {
            Double sum = 0;
            for (var value = start; value <= start + 10; value += .1)
                sum += value;

            return sum;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ///

        public static void Example3()
        {
            // Create the task object by using an Action(Of Object) to pass in custom data
            // to the Task constructor. This is useful when you need to capture outer variables
            // from within a loop. 
            Task[] taskArray = new Task[20];
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((Object obj) => {
                    CustomData data = obj as CustomData;
                    if (data == null)
                        return;

                    data.ThreadNum = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine("Task #{0} created at {1} on thread #{2}.",
                                     data.Name, data.CreationTime, data.ThreadNum);
                },
                                                      new CustomData() { Name = i, CreationTime = DateTime.Now.Ticks });
            }
            Task.WaitAll(taskArray);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        ///

        public static void Example4()
        {
            var displayData = Task.Factory.StartNew(() => {
                Random rnd = new Random();
                int[] values = new int[1000];
                for (int ctr = 0; ctr <= values.GetUpperBound(0); ctr++)
                    values[ctr] = rnd.Next();

                return values;
            }).
                              ContinueWith((x) => {
                                  int n = x.Result.Length;
                                  long sum = 0;
                                  double mean;

                                  for (int ctr = 0; ctr <= x.Result.GetUpperBound(0); ctr++)
                                      sum += x.Result[ctr];

                                  mean = sum / (double)n;
                                  return Tuple.Create(n, sum, mean);
                              }).
                              ContinueWith((x) => {
                                  return String.Format("N={0:N0}, Total = {1:N0}, Mean = {2:N2}",
                                                 x.Result.Item1, x.Result.Item2,
                                                 x.Result.Item3);
                              });
            Console.WriteLine(displayData.Result);
        }

        ///

        public static void Example()
        {
            var parent = Task.Factory.StartNew(() => {
                Console.WriteLine("Parent task beginning.");
                for (int ctr = 0; ctr < 100; ctr++)
                {
                    int taskNo = ctr;
                    Task.Factory.StartNew((x) => {
                        Thread.SpinWait(5000000);
                        Console.WriteLine("Attached child #{0} completed.",
                                          x);
                    },
                                          taskNo, TaskCreationOptions.AttachedToParent);
                }
            });

            parent.Wait();
            Console.WriteLine("Parent task completed.");
        }
    }
}
