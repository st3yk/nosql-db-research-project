using Models;

public class BenchmarkDataGenerator{

    private int chunkSize;
    public int chunkCount {get;}
    private int timePointsCount;
    private DateTime startTime; 
    private int dt;
    public BenchmarkDataGenerator(int _chunkSize, int _timePointsCount, DateTime _startTime, int _dt){
        this.chunkSize = _chunkSize;
        this.timePointsCount = _timePointsCount;
        this.chunkCount = _timePointsCount/_chunkSize;
        if(_timePointsCount%_chunkSize != 0) this.chunkCount += 1;
        this.startTime = _startTime; 
	    this.dt = _dt;
    }

    public IEnumerable<List<Record>> GetDataChunk(){
    	Random random = new Random();
        DateTime timeStamp = startTime;
        Console.WriteLine(timeStamp.ToUniversalTime());
        for(int chunkNumber = 0; chunkNumber < this.chunkCount; chunkNumber++){
            List<Record> chunk = new List<Record>();
            for(int i = chunkNumber*this.chunkSize; i < (chunkNumber+1) * this.chunkSize && i < this.timePointsCount; i ++){
                for (int bs_id = 0; bs_id < 3; bs_id++){
                                
                    // data for each user belonging to the base station
                    for (int ue_id = 0; ue_id < 5; ue_id++){
                        chunk.Add(new Record{
                            timestamp = timeStamp.ToUniversalTime(),
                            ue_data = new UEData{
                                ue_id = 5*bs_id + ue_id,
                                pci = bs_id
                                }
                        });
                    }
                
                }
                timeStamp = timeStamp.AddSeconds(5);
            }
            yield return chunk;
            }
        Console.WriteLine(timeStamp.ToUniversalTime());
    }
    public IEnumerable<List<Record>> GetTimepointChunk(){
        Random random = new Random();
        DateTime timeStamp = startTime;
        Console.WriteLine(timeStamp.ToUniversalTime());
        for (int i = 0; i < this.timePointsCount; i++){
            List<Record> data = new List<Record>();
            
                for (int bs_id = 0; bs_id < 3; bs_id++){
                    // data for each user belonging to the base station
                    for (int ue_id = 0; ue_id < 5; ue_id++){
                        data.Add(new Record{
                            timestamp = timeStamp.ToUniversalTime(),
                            ue_data = new UEData{
                                ue_id = 5*bs_id + ue_id,
                                pci = bs_id
                            }
                        });
                }
                 
            }
		    timeStamp = timeStamp.AddSeconds(this.dt);

            yield return data;
        }
        
        Console.WriteLine(timeStamp.ToUniversalTime());       
    }

}
