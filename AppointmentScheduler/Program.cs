using AppointmentScheduler.Models;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

public class Program
{
    private static readonly HttpClient client = new HttpClient
    {
        BaseAddress = new Uri("https://scheduling.interviews.brevium.com/api/")
    };
    private static string token = "";
    public async static Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        token = config["Api:Token"] ?? throw new Exception("Missing Api Token");

        Console.WriteLine("Start of Brevium Take Home Assessment");
        await StartScheduling();
        List<AppointmentRequest> appointments = new List<AppointmentRequest>();
        List<AppointmentInfo> schedule = await GetSchedule();
        var newAppointment = await GetNextAppointmentRequest();
        appointments.Add(newAppointment);
        foreach (AppointmentRequest appointment in appointments)
        {
            Console.WriteLine(appointment.RequestId);
            Console.WriteLine(appointment.PersonId);
            Console.WriteLine(appointment.PreferredDays);
            Console.WriteLine(appointment.PreferredDocs);
            Console.WriteLine(appointment.IsNew);
        }

        foreach (AppointmentInfo appointment in schedule)
        {
            Console.WriteLine(appointment.DoctorId);
            Console.WriteLine(appointment.PersonId);
            Console.WriteLine(appointment.AppointmentTime);
            Console.WriteLine(appointment.IsNewPatientAppointment);
        }
    }

    private static async Task StartScheduling()
    {
        var path = $"Scheduling/Start?token={token}";
        using var response = await client.PostAsync(path, content: null);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine("Successfully Started Scheduling");
    }

    private static async Task<AppointmentRequest> GetNextAppointmentRequest()
    {
        var path = $"Scheduling/AppointmentRequest?token={token}";
        var response = await client.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Error with status code {}", response.StatusCode);
            return new AppointmentRequest();
        }

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentRequest>();
        return appointment;
    }

    private static async Task<List<AppointmentInfo>> GetSchedule()
    {
        var path = $"Scheduling/Schedule?token={token}";
        var response = await client.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Error with status code {}", response.StatusCode);
            return [];
        }

        var schedule = await response.Content.ReadFromJsonAsync<List<AppointmentInfo>>();
        return schedule;
    }
}