using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using Cassandra;
using System.Xml.Linq;
using System.CodeDom;

using Microsoft.VisualBasic;
using Models;
using Newtonsoft.Json;
using CsvHelper;

class Program
{
        static void Main(string[] args)
    {
        var results = ParseArgs(args);
        if(results is null){
           return;
        }
        var dataBaseTester = new DataBaseTesterFactory().ContactPointsAddr(results.Item1).Ports(results.Item2).DataBase(results.Item3).Build(); 
        
       
    }

    static public Tuple<List<string>, List<string>, DatabaseType> ParseArgs(string[] args){
        List<string> ipAddresses = new List<string>();
        List<string> ports = new List<string>();

        // Check if there are command-line arguments
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program.exe -a <ip1> <ip2> ... -p <port1> <port2> ...");
            return null;
        }
        string name = args[0];
        DatabaseType type = DatabaseType.Cassandra;

        if(name == "cassandra"){
            type = DatabaseType.Cassandra;
        }
        else if(name == "mongo"){
            type = DatabaseType.MongoDb;
        }
        else if(name == "elastic"){
            type = DatabaseType.ElasticSearch;
        }
        else{
            Console.WriteLine("Unrecognized database type!");
            return null;
        }

        // Process command-line arguments
        for (int i = 1; i < args.Length; i++)
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
        return new Tuple<List<string>, List<string>, DatabaseType>(ipAddresses, ports, type);
    }
}