using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Cassandra;
using System.Xml.Linq;
using System.CodeDom;
using System.Threading;

class Program
{
        static void Main(string[] args)
    {
        var results = ParseArgs(args);
        if(results is null){
           return;
        }
        var dataBaseTester = new CassandraTester(results.Item1, results.Item2);
        int[] wholeSizes = {1_000, 10_000, 100_000}; // ilosc punktow czasowych
        int batchSize = 1; // ilosc punktow czasowych wysylanych na raz do db 
        foreach(var wholeSize in wholeSizes){
            for(int i = 0; i<3; i++){
                dataBaseTester.DropTableIfExists();
                dataBaseTester.CreateTable();
                dataBaseTester.BulkLoadTest(wholeSize, batchSize);
            }
            Thread.Sleep(10000);

            int[] loopCounts = {1_000};
            foreach(int loops in loopCounts){
                for(int i = 0; i<3; i++){
                    dataBaseTester.BulkReadTest(loops);
                    Thread.Sleep(3000);
                }
            }
            
            Console.WriteLine("\n\n\n");
        }
        
        dataBaseTester.DropTableIfExists();
        dataBaseTester.CreateTable();
        dataBaseTester.BulkLoadTest(250_00, 1, 50);
        for(int i = 0; i<3; i++){
            bool flag = false;
            for(int j = 0; j<2; j++){
                dataBaseTester.AggregationTest(1, flag);
                flag = !flag;
            }
        }
               
    }

   
    static public Tuple<List<string>, List<string>> ParseArgs(string[] args){
        List<string> ipAddresses = new List<string>();
        List<string> ports = new List<string>();

        // Check if there are command-line arguments
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program.exe -a <ip1> <ip2> ... -p <port1> <port2> ...");
            return null;
        }
        string name = args[0];
       

       

        // Process command-line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-a" && i + 1 < args.Length)
            {
                ipAddresses.Add(args[i + 1]);
            }
            else if (args[i] == "-p" && i + 1 < args.Length)
            {
                int x =  int.Parse(args[i+1]);

                ports.Add(x.ToString());
            }
        }
        return new Tuple<List<string>, List<string>>(ipAddresses, ports);
    }
}