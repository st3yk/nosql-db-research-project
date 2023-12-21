using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

public class ParallelDataGenerator<T>
{
    private T[] arrayOfData;
    private int[] threadControl;
    private int[] dataControl;
    private object lockObject;
    private Func<T> _function;
    private Thread[] threads;
    private int threadCount;


    public ParallelDataGenerator(int threadCount, Func<T> function) 
    {
        this._function = function ?? throw new ArgumentNullException(nameof(function));
        this.threads = new Thread[threadCount];
        this.lockObject = new object();
        this.threadControl = new int[threadCount];
        this.dataControl = new int[threadCount];
        this.arrayOfData = new T[threadCount];
        this.threadCount = threadCount;

        for (int i = 0; i < this.threadCount; i++)
        {
            int index = i;
            this.threadControl[i] = 0;
            this.dataControl[i] = 0;
            this.threads[i] = new Thread(() => Worker(index));
            this.threads[i].Start();
        }
        
    }

    public void Stop(){

        lock (lockObject)
        {
            for(int index = 0; index < this.threadCount; index++){
                Console.WriteLine($"Sent signal to stop to thread id: {index}");
                this.threadControl[index] = 1;
            }
            Monitor.PulseAll(this.lockObject);
        }

        foreach (var thread in this.threads)
        {

            thread.Join();
        }
        Console.WriteLine("All thread ended their job");

    }
    
    ~ParallelDataGenerator(){
        this.Stop();
    }
    private void Worker(int index)
    {
        Console.WriteLine($"Thread id: {index} started");
        bool flag = true;
        while (flag)
        {
            
            T newData = this._function();

            lock (this.lockObject)
            {
                while(flag){                    
                    if(this.threadControl[index] == 1){
                        Console.WriteLine($"Thread id: {index} got signal to exit");
                        flag = false;
                        break;
                    }

                    if(this.dataControl[index] == 0){
                        Console.WriteLine($"Thread id: {index} saves data");
                        this.arrayOfData[index] = newData;
                        this.dataControl[index] = 1;
                        Monitor.Pulse(this.lockObject); 
                        break;
                    }
                    else{
                        Monitor.Wait(this.lockObject); 
                    }
                }
            }
           
        }

    }

    public T Get()
    {
        T result = default(T);
        lock (this.lockObject)
        {
            while (true)
            {
                for(int index = 0; index < this.threadCount; index++)
                {
                    if (this.dataControl[index] == 1)
                    {                       
                        result = this.arrayOfData[index];
                        this.dataControl[index] = 0;
                        Console.WriteLine($"Get: returning list of index {index}");
                        Monitor.Pulse(this.lockObject);
                        return result;
                    }
                }
                Monitor.Wait(this.lockObject); // Wait for a new list to be generated
            }
        }

    }
}
