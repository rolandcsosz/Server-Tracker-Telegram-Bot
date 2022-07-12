using System.ServiceProcess;

//It represents the information about a tracked process
public class ServiceInfo
{
    public String Name { get; set; }
    public ServiceControllerStatus status { get; set; }

    public bool isInsatlled = false;

    public ServiceInfo(string name, ServiceControllerStatus status, bool isInsatlled)
    {
        Name = name;
        this.status = status;
        this.isInsatlled = isInsatlled;
    }
}


