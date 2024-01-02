using Models;

public class BenchmarkDataGenerator{

    private int chunkSize;
    public int chunkCount {get;}
    private int timePointsCount;
    private DateTime startTime; 
    public BenchmarkDataGenerator(int _chunkSize, int _timePointsCount, DateTime _startTime){
        this.chunkSize = _chunkSize;
        this.timePointsCount = _timePointsCount;
        this.chunkCount = _timePointsCount/_chunkSize;
        if(_timePointsCount%_chunkSize != 0) this.chunkCount += 1;
        this.startTime = _startTime; 
    }
    public List<Record> GenerateData(){
        List<Record> data = new List<Record>();
        Random random = new Random();
        DateTime timestamp = startTime;
        Console.WriteLine($"Started generating {timePointsCount*18} records...");
        for (int dt = 0; dt < timePointsCount; dt++){
            // data for each base station
            for (int bs_id = 0; bs_id < 3; bs_id++){
                data.Add(new Record{
                    timestamp = startTime.AddSeconds(dt).ToUniversalTime(),
                    bs_data = new BSData{
                        bs_id = bs_id
                    }
                });
                // data for each user belonging to the base station
                for (int ue_id = 0; ue_id < 5; ue_id++){
                    data.Add(new Record{
                        timestamp = startTime.AddSeconds(dt).ToUniversalTime(),
                        ue_data = new UEData{
                            ue_id = 3*bs_id + ue_id,
                            pci = bs_id
                        }
                    });
                }
            }
            
            timestamp = timestamp.AddSeconds(1);
            if(dt%1_000 == 0 && dt != 0) Console.WriteLine($"{dt*18} records generated...");
        }
        Console.WriteLine("Generating finished.\n");
        return data;
    
    }

    public IEnumerable<List<Record>> GetDataChunk(){
        Random random = new Random();
        DateTime timeStamp = startTime;
        Console.WriteLine(timeStamp.ToUniversalTime());
        for (int chunkIdx = 0; chunkIdx < this.chunkCount; chunkIdx++){
            List<Record> data = new List<Record>();
            for (int dt = chunkIdx * this.chunkSize; dt < (chunkIdx+1) * this.chunkSize && dt < this.timePointsCount; dt++){
                timeStamp = timeStamp.AddSeconds(1);
                // data for each base station
                for (int bs_id = 0; bs_id < 3; bs_id++){
                    
                    data.Add(new Record{
                        timestamp = timeStamp.ToUniversalTime(),
                        bs_data = new BSData{
                            bs_id = bs_id
                        }
                    });
                    
                    // data for each user belonging to the base station
                    for (int ue_id = 0; ue_id < 5; ue_id++){
                        data.Add(new Record{
                            timestamp = timeStamp.ToUniversalTime(),
                            ue_data = new UEData{
                                ue_id = 3*bs_id + ue_id,
                                pci = bs_id
                            }
                        });
                    }
                }
            }
            yield return data;
        }
        
        Console.WriteLine(timeStamp.ToUniversalTime());       
    }

}