public class DataBaseTesterFactory{
    
    DatabaseType type;
    List<string> contactPointsAddr;
    List<string> ports;
    public DataBaseTesterFactory(){
        this.type = DatabaseType.Cassandra;
        this.contactPointsAddr = new List<string>();
        this.ports = new List<string>();
    }

    public DataBaseTesterFactory DataBase(DatabaseType type){
        this.type = type;
        return this;
    }

    public DataBaseTesterFactory ContactPointsAddr(List<string> contactPoints){
        this.contactPointsAddr = contactPoints;
        return this;
    }

    public DataBaseTesterFactory Ports(List<string> ports){
        this.ports = ports;
        return this;
    }
    
    public DataBaseTestingInterface Build(){
        if(this.type == DatabaseType.Cassandra){
            return new CassandraTester(contactPointsAddr, ports);
        }
        return new CassandraTester(contactPointsAddr, ports);
    }
}