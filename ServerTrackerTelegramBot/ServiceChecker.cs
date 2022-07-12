using Newtonsoft.Json;
using Serilog;
using System.ServiceProcess;

internal class ServiceChecker
{
    private List<String> nameOfTrackedServices;
    private List<ServiceInfo> oldServices;
    private List<ServiceInfo> refreshedServices;
    ServiceController[] systemServices;

    private String PATH = "services.json";

    public ServiceChecker()
    {
        systemServices = ServiceController.GetServices();
        nameOfTrackedServices = GetServiceNamesFromFile(PATH);
        oldServices = InitServiceInfos(systemServices, nameOfTrackedServices);
        refreshedServices = new List<ServiceInfo>();

    }

    private List<ServiceInfo> InitServiceInfos(ServiceController[] systemServices, List<String> nameOfTrackedServices)
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

    private void RefreshLists()
    {
        systemServices = ServiceController.GetServices();
        nameOfTrackedServices = GetServiceNamesFromFile(PATH);
        refreshedServices = InitServiceInfos(systemServices, nameOfTrackedServices);
    }

    public List<ServiceInfo> GetServices()
    {
        RefreshLists();
        return refreshedServices;
    }

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

        //if(list.Count > 0)
        // {
        //     serviceStatusChangedDelegate?.Invoke(list);

        // }
        oldServices = refreshedServices;
        return list;
    }

    private List<String> GetServiceNamesFromFile(String PATH)
    {
        List<String>? result = new List<String>();

        StreamReader r;
        try
        {
            r = new StreamReader(PATH);

        }
        catch (Exception)
        {
            StreamWriter w = File.CreateText(PATH);
            w.WriteLine("[]");
            w.Close();
            r = new StreamReader(PATH);

            Log.Information(PATH + " created with content: '[]'");
        }

        string json = r.ReadToEnd();
        r.Close();
        try
        {
            result = JsonConvert.DeserializeObject<List<String>>(json);

        }
        catch (Exception e)
        {
            Log.Error("The content of " + PATH + "is possible to deserialize.");

        }





        if (result is null)
            return new List<String>();

        return result;
    }

    private void WriteServiceNamestoFile(String PATH, List<String> services)
    {
        String fileContent = JsonConvert.SerializeObject(services);

        System.IO.File.WriteAllText(PATH, fileContent);

    }

    public void AddServiceToTheList(String service)
    {
        List<String> vs = GetServiceNamesFromFile(PATH);
        vs.Add(service);
        WriteServiceNamestoFile(PATH, vs);
        oldServices = InitServiceInfos(systemServices, nameOfTrackedServices);


    }

    public void RemoveServiceFromTheList(String service)
    {
        List<String> vs = GetServiceNamesFromFile(PATH);
        vs.Remove(service);
        WriteServiceNamestoFile(PATH, vs);
        oldServices.RemoveAll(item => item.Name == service);

    }

}

