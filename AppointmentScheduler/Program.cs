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

    // Dictionary has a key of Doctor, Appointment Time, and value of AppointmentInfo
    // O(1) lookup
    private static Dictionary<(int, DateTime), AppointmentInfo> scheduleDictionary = 
        new Dictionary<(int, DateTime), AppointmentInfo>();

    public async static Task Main(string[] args)
    {
        // Get api token from secrets
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        token = config["Api:Token"] ?? throw new Exception("Missing Api Token");

        Console.WriteLine("Start of Brevium Take Home Assessment");

        // Initialize lists and fill
        await StartScheduling();
        List<AppointmentRequest> requests = new List<AppointmentRequest>();
        List<AppointmentInfo> schedule = await GetSchedule();


        // Get requests until there are none remaining
        while (true)
        {
            var (status, appointment) = await GetNextAppointmentRequest();
            if (status == HttpStatusCode.NoContent)
            {
                Console.WriteLine("No more appointments (204). Stopping.");
                break;
            }

            if (status == HttpStatusCode.OK && appointment is not null)
            {
                requests.Add(appointment);
            }
            else
            {
                Console.Error.WriteLine($"Error {(int)status} {status}.");
                break;
            }
        }

        // Add each appointment to schedule dictionary
        foreach (AppointmentInfo appointment in schedule)
        {
            scheduleDictionary.Add((appointment.DoctorId, appointment.AppointmentTime), appointment);
        }

        foreach (AppointmentRequest appointment in requests)
        {
            AppointmentInfoRequest req = HandleRequest(appointment);
        }
    }

    /// <summary>
    /// Initialize Scheduling Requests
    /// </summary>
    /// <returns></returns>
    private static async Task StartScheduling()
    {
        var path = $"Scheduling/Start?token={token}";
        using var response = await client.PostAsync(path, content: null);
        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine(response.StatusCode);
            return;
        }
        Console.WriteLine("Successfully Started Scheduling");
    }

    /// <summary>
    /// Get next appointment request from scheduler
    /// </summary>
    /// <returns> (StatusCode, AppointmentRequest)</returns>
    private static async Task<(HttpStatusCode status, AppointmentRequest? appt)> GetNextAppointmentRequest()
    {
        var path = $"Scheduling/AppointmentRequest?token={token}";
        using var resp = await client.GetAsync(path);

        if (resp.StatusCode == HttpStatusCode.NoContent)
            return (resp.StatusCode, null);

        if (resp.IsSuccessStatusCode)
        {
            var appt = await resp.Content.ReadFromJsonAsync<AppointmentRequest>();
            return (resp.StatusCode, appt);
        }

        Console.Error.WriteLine($"Request failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
        return (resp.StatusCode, null);
    }

    /// <summary>
    /// Get existing appointments on the schedule
    /// </summary>
    /// <returns></returns>
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
        return schedule ?? new List<AppointmentInfo>();
    }

    /// <summary>
    /// Make a AppointmentInfoRequest object with valid appointment time and doctor for a request
    /// </summary>
    /// <param name="req"></param>
    /// <returns>AppointmentInfoRequest</returns>
    private static AppointmentInfoRequest HandleRequest(AppointmentRequest req)
    {
        return new AppointmentInfoRequest();
    }
}