using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using static System.Console;

namespace DisposeExampleClient
{
    class Program
    {
        public static IHubProxy tempHub;
        static void Main(string[] args)
        {
            int selection,controlNewHub=0;

            var qsData = new Dictionary<string, string>();
            qsData.Add("ClientName", "Yıldız Petrol A.Ş.");

            var connection = new HubConnection("http://localhost:8090", qsData);
            var mainHub = connection.CreateHubProxy("MainHub");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    WriteLine("\nMainHub subscription not started !!!");
                else
                    WriteLine("MainHub subscription started...");
            }).Wait();

            

            var connectionNewHub = new HubConnection("http://localhost:8091", qsData);

            do
            {
                WriteLine("\n[1] - Open TempHub");
                WriteLine("[2] - Close TempHub");
                WriteLine("[3] - Get log record");
                WriteLine("[0] - Exit");
                Write("please make your choice : ");
                selection = Convert.ToInt32(ReadLine());
                

                switch (selection)
                {
                    case 1:
                        controlNewHub++;
                        Clear();
                        mainHub.Invoke<string>("OpenNewHub", "http://localhost:8091").ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                                WriteLine("\nNew hub create message could not be sent !!!");
                            else
                                WriteLine("\nNew hub create message sent");
                        });
                        
                        mainHub.On<string>("newHubOpened", (m) =>
                        {
                            
                            tempHub = connectionNewHub.CreateHubProxy("TempHub");

                            connectionNewHub.Start().ContinueWith(task2 =>
                            {
                                if (task2.IsFaulted)
                                    WriteLine("\nTempHub subscription not started !!!");
                                else
                                    WriteLine("\nTemphub subscription started...");
                            }).Wait();
                        });
                        break;
                    case 2:
                        connectionNewHub.Stop();
                        mainHub.Invoke("CloseNewHub").ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                                WriteLine("\nTempHub Closed...");
                        });
                        break;
                    case 3:
                        
                        tempHub.Invoke<string>("LogRequest").ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                                WriteLine("\nLog request message could not sent !!!");
                            else
                            {
                                WriteLine("\nLog request message sent...");
                            }
                        });

                        tempHub.On("printLogs", (log) =>
                        {
                            WriteLine(log);
                        });
                        break;
                    case 0:
                        connection.Stop();
                        if (controlNewHub!=0)
                            connectionNewHub.Stop();
                        Environment.Exit(0);
                        break;
                    default:
                        WriteLine("Wrong Chose !!! Try Again");
                        break;
                }
            } while (selection != 0);
        }
    }
}