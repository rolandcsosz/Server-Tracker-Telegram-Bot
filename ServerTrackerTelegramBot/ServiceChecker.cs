using Newtonsoft.Json;
using Serilog;
using ServiceTrackerTelegramBot;
using System.ServiceProcess;


//This class is responsible for checking and tracking the statuses of the processes
internal class ServiceChecker
{
    //list of the tracked processes -> EnvironmentVariables.SERVICE_LIST_FILE 
    List<String> nameOfTrackedServices;

    //with the list of system processes and the list of input processes
    //Store the Service infos where the name, the status and the installation status are marked
    List<ServiceInfo> refreshedServices;

    //After checking and comparing the old and refreshed list
    //The refreshed list will be the old one
    List<ServiceInfo> oldServices;
    

    //list of all system process
    ServiceController[] systemServices;

    //File path where the tracked process list is placed
    private String SERVICE_LIST_FILE = EnvironmentVariables.SERVICE_LIST_FILE;

    public ServiceChecker()
    {
        //Init the lists
        systemServices = ServiceController.GetServices();
        nameOfTrackedServices = GetServiceNamesFromFile(SERVICE_LIST_FILE);
        oldServices = InitServiceInfos(systemServices, nameOfTrackedServices);
        refreshedServices = new List<ServiceInfo>();

    }

    //with the list of system processes and the list of input processes
    //Makes a comparison and mark the processes if changes occurred
    List<ServiceInfo> InitServiceInfos(ServiceController[] systemServices, List<String> nameOfTrackedServices)
    {

        List<ServiceInfo> services = new List<ServiceInfo>();

        foreach (String serviceName in nameOfTrackedServices)
        {
            ServiceInfo serviceInfo;

            if (Array.Exists(systemServices, element => element.ServiceName == serviceName))
            {
                ServiceController? scd = Array.Find<ServiceController>(systemServices, element => element.ServiceName == serviceName);
                if (scd != null)
                {
                    serviceInfo = new ServiceInfo(scd.ServiceName, scd.Status, true);
                    services.Add(serviceInfo);
                    continue;

                }
            }

            serviceInfo = new ServiceInfo(serviceName, ServiceControllerStatus.Stopped, false);
            services.Add(serviceInfo);

        }

        return services;

    }

    //Update the lists
    private void RefreshLists()
    {
        systemServices = ServiceController.GetServices();
        nameOfTrackedServices = GetServiceNamesFromFile(SERVICE_LIST_FILE);
        refreshedServices = InitServiceInfos(systemServices, nameOfTrackedServices);
    }

    //Return the fresh list of processes
    public List<ServiceInfo> GetServices()
    {
        RefreshLists();
        return refreshedServices;
    }

    //Check if changes occured from  the last check and return a list with those services
    //if there was 0 the list is empty but not null
    public List<ServiceInfo> CheckChangedServices()
    {
        RefreshLists();

        List<ServiceInfo> list = new List<ServiceInfo>();

        foreach (ServiceInfo serviceInfo in refreshedServices)
        {
            foreach (ServiceInfo serviceInfo1 in oldServices)
            {
                //if old service is listed on the refreshed list 
                if (serviceInfo.Name.Equals(serviceInfo1.Name))
                {
                    //check changes if occurs add to the list
                    if (serviceInfo.isInsatlled != serviceInfo1.isInsatlled)
                    {
                        list.Add(serviceInfo);
                        continue;
                    }

                    if (serviceInfo.status != serviceInfo1.status)
                    {
                        list.Add(serviceInfo);
                        continue;
                    }

                }

            }


        }
        bool included;

        //old one is missing from refreshed 
        if (refreshedServices.Count != oldServices.Count)
        {

            included = false;
            foreach (ServiceInfo serviceInfo1 in oldServices)
            {
                foreach (ServiceInfo serviceInfo in refreshedServices)
                {
                    if (serviceInfo.Name.Equals(serviceInfo1.Name))
                    {
                        included = true;

                    }

                }

                if (!included)
                {
                    list.Add(new ServiceInfo(serviceInfo1.Name, ServiceControllerStatus.Stopped, false));
                }

            }
        }

        oldServices = refreshedServices;
        return list;
    }

    //Reads the input *.json file nad return a list of string
    private List<String> GetServiceNamesFromFile(String SERVICE_LIST_FILE)
    {
        List<String>? result = new List<String>();

        StreamReader r;
        try
        {
            r = new StreamReader(SERVICE_LIST_FILE);

        }
        catch (Exception)
        {
            StreamWriter w = File.CreateText(SERVICE_LIST_FILE);
            w.WriteLine("[]");
            w.Close();
            r = new StreamReader(SERVICE_LIST_FILE);

            Log.Information(SERVICE_LIST_FILE + " created with content: '[]'");
        }

        string json = r.ReadToEnd();
        r.Close();
        try
        {
            result = JsonConvert.DeserializeObject<List<String>>(json);

        }
        catch (Exception e)
        {
            Log.Error("The content of " + SERVICE_LIST_FILE + "is possible to deserialize.");

        }





        if (result is null)
            return new List<String>();

        return result;
    }

    //Override the content of *.json with the input list of string (in JSON format)
    private void WriteServiceNamestoFile(String SERVICE_LIST_FILE, List<String> services)
    {
        String fileContent = JsonConvert.SerializeObject(services);

        System.IO.File.WriteAllText(SERVICE_LIST_FILE, fileContent);

    }

    //Add a new process name to the list and all the files
    public void AddServiceToTheList(String service)
    {
        List<String> vs = GetServiceNamesFromFile(SERVICE_LIST_FILE);
        vs.Add(service);
        WriteServiceNamestoFile(SERVICE_LIST_FILE, vs);
        oldServices = InitServiceInfos(systemServices, nameOfTrackedServices);


    }

    //Add an old process name from the list and all the files
    public void RemoveServiceFromTheList(String service)
    {
        List<String> vs = GetServiceNamesFromFile(SERVICE_LIST_FILE);
        vs.Remove(service);
        WriteServiceNamestoFile(SERVICE_LIST_FILE, vs);
        oldServices.RemoveAll(item => item.Name == service);

    }

}

