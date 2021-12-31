using IndexedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Test
{
    class Program
    {
        class Data
        {
            public int id { get; set; }
            public string str { get; set; }
        }


        static void Main(string[] args)
        {
            Test();
        }

        public static void Test()
        {
            int TOTAL = 10000;
            int IDRANGE = 1000;

            // prepare
            List<Data> list = new List<Data>();
            IndexedList<Data> indexedList1 = new IndexedList<Data>();
            indexedList1.IndexBy(i => i.id);
            Random r = new Random();
            for (int i = 0; i < TOTAL; i++)
            {
                int n = r.Next(IDRANGE);
                Data d = new Data { id = n, str = n.ToString() };
                list.Add(d);
                indexedList1.Add(d);
            }


            IndexedList<Data> indexedList2 = list.IndexBy(i => i.id);
            Stopwatch sw;
            int ct;

            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = list.Where(j => j.id == list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("List.Where lookup " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");

            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = indexedList1.LookFor(j => j.id, list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("IndexedList1 lookup by expression " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");

            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = indexedList2.LookFor("id", list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("IndexedList2 lookup by index name " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");
            
            sw = new Stopwatch();
            sw.Start();
            indexedList2.RefreshIndexes();
            sw.Stop();
            Console.WriteLine("Refreshing the indexes costs " + sw.Elapsed.TotalMilliseconds + "ms");

            ct = 0;
            sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = indexedList2.LookFor("id", list[i].id);
                ct += d.Count();
            }
            sw.Stop();
            Console.WriteLine("IndexedList2 lookup by index name " + ct + " records, costs " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }



}
