using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Cassandra;
using System.Xml.Linq;
using System.CodeDom;


class Program
{
        static void Main(string[] args)
    {
        var results = ParseArgs(args);
        if(results is null){
           return;
        }
        var dataBaseTester = new CassandraTester(results.Item1, results.Item2);
        int wholeSize = 1_000;
        int batchSize = 10; // Higher values may not work (cassandra skill issue :( )
        for(int i = 0; i<5; i++){
            dataBaseTester.DropTableIfExists();
            dataBaseTester.CreateTable();
            dataBaseTester.BulkLoadTest(wholeSize, batchSize);
        }
       
        for(int i = 0; i<8; i++){
            dataBaseTester.BulkReadTest();
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