public interface IDatabaseBenchmark{
    public void SetupDB();
    public void ResetDB();
    public void BulkLoad(int timePointsCount, int chunkSize);
    public void SequentialReadTest(int readCount);
    public void AggregationTest(int queryCount);
}