using Microsoft.VisualStudio.TestTools.UnitTesting;

using IndexedList;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        class Data {
            public int id { get; set; }
            public string str { get; set; }
        }


        [TestMethod]
        public void TestMethod1()
        {
            int TOTAL = 10000;

            // prepare
            List<Data> list = new List<Data>();
            Random r = new Random();
            for (int i = 0; i < TOTAL; i++) {
                int n = r.Next();
                list.Add(new Data { id = n, str = n.ToString() }); 
            }
            

            IndexedList<Data> indexedList = list.IndexBy(i => i.id);
            Stopwatch sw = new Stopwatch();


            sw.Start();
            for (int i = 0; i < TOTAL; i++) {
                var d = indexedList.LookFor(j => j.id, list[i].id);
            }
            sw.Stop();
            Console.WriteLine("IndexedList lookup: " + sw.Elapsed);

            sw = new Stopwatch();
            for (int i = 0; i < TOTAL; i++)
            {
                var d = list.Where(j => j.id == list[i].id);
            }
            sw.Stop();
            Console.WriteLine("IndexedList lookup: " + sw.Elapsed);

        }
    }
}
