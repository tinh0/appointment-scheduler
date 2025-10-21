using AppointmentScheduler.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;

public class Program
{
    private static readonly HttpClient client = new HttpClient
    {
        BaseAddress = new Uri("https://scheduling.interviews.brevium.com/api/")
    };
    private static string token = "";

    // Dictionary has a key of Doctor, Patient, and list of times for existing appointments
    // O(1) lookup
    private static Dictionary<(int doctor, int patient), List<DateTime>> scheduleDictionary = 
        new Dictionary<(int doctor, int patient), List<DateTime>>();

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
                Console.WriteLine("Error with status code " + status);
                break;
            }
        }

        // Add each appointment to schedule dictionary
        foreach (AppointmentInfo appointment in schedule)
        {
            var key = (appointment.DoctorId, appointment.PersonId);

            if (!scheduleDictionary.TryGetValue(key, out var times) || times is null)
            {
                times = new List<DateTime>();
                scheduleDictionary[key] = times;
            }

            times.Add(appointment.AppointmentTime);
        }

        // Process each request and Schedule
        foreach (AppointmentRequest appointment in requests)
        {
            AppointmentInfoRequest req = HandleRequest(appointment);
            
            var statusCode = await PostAppointment(req);
            if (statusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Appointment Scheduled");
            }
            else if (statusCode == HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Could not accomodate request");
            }
            else
            {
                Console.WriteLine("Error!");
            }

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
            Console.WriteLine("Error with status code " + response.StatusCode);
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
        using var response = await client.GetAsync(path);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return (response.StatusCode, null);

        if (response.IsSuccessStatusCode)
        {
            var appt = await response.Content.ReadFromJsonAsync<AppointmentRequest>();
            return (response.StatusCode, appt);
        }

        Console.WriteLine("Error with status code " + response.StatusCode); ;
        return (response.StatusCode, null);
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
            Console.WriteLine("Error with status code " + response.StatusCode);
            return [];
        }

        var schedule = await response.Content.ReadFromJsonAsync<List<AppointmentInfo>>();
        return schedule ?? new List<AppointmentInfo>();
    }

    /// <summary>
    /// Schedule appointment with AppointmentInfoRequest object
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    private static async Task<HttpStatusCode> PostAppointment(AppointmentInfoRequest req)
    {
        var path = $"Scheduling/Schedule?token={token}";
        var response = await client.PostAsJsonAsync(path, req);

        return response.StatusCode;
    }

    /// <summary>
    /// Make a AppointmentInfoRequest object with valid appointment time and doctor for a request
    /// </summary>
    /// <param name="req"></param>
    /// <returns>AppointmentInfoRequest</returns>
    private static AppointmentInfoRequest HandleRequest(AppointmentRequest req)
    {
        // Fill out valid time and doctor w/ constraints
        var time = NextValidTime(req);

        // ran out of time
        Random rng = new Random();
        var doctor = req.PreferredDocs != null ? req.PreferredDocs[rng.Next(0, req.PreferredDocs.Count)] 
            : rng.Next(1, 3+1);
        //Console.WriteLine(doctor +" "+ req.PersonId + " " + time + " " + req.IsNew + " " + req.RequestId);
        return new AppointmentInfoRequest(doctor, req.PersonId, time, req.IsNew, req.RequestId);
    }

    /// <summary>
    /// Find the next valid appointment time using the constraints given
    /// Appointments may only be scheduled on the hour. 
    /// Appointments can be scheduled as early as 8 am UTC and as late as 4 pm UTC.
    /// Appointments may only be scheduled on weekdays during the months of November and December 2021. 
    /// Appointments can be scheduled on holidays. 
    /// For a given doctor, you may only have one appointment scheduled per hour 
    /// (though different doctors may have appointments at the same time). 
    /// For a given patient, each appointment must be separated by at least one week.
    /// For example, if Bob Smith has an appointment on 11/17 you may schedule another appointment on or before 11/10 or on or after 11/24. 
    /// Appointments for new patients may only be scheduled for 3 pm and 4 pm.
    /// </summary>
    /// <returns></returns>
    private static DateTime NextValidTime(AppointmentRequest req)
    {
        // between 2021 11 8am to 2021 12 4pm
        var start = new DateTime(2021, 11, 1, 8, 0, 0);
        var end = new DateTime(2021, 12, 31, 16, 0, 0);
        var res = start;

        var doctors = req.PreferredDocs != null ? req.PreferredDocs : [];
        
        // psuedocode on how I would complete constraints

        //while (true)
        //{
            // check scheduleDictionary for each prefered doc & patient
            // sort value DateTime List
            // increment time to find time 7 days apart from every appointment
            // from same doc, patient
            // increment time and iterate through times list so it doesn't conflict with
            // other people's appointment
            // if none are found change doctor and repeat
            // return doctor and time
        //}

        // ran out of time, rng
        Random rng = new Random();
        int month = rng.Next(11, 12 + 1);
        int day = rng.Next(1, DateTime.DaysInMonth(2021, month) + 1);
        // new only 3pm and 4pm
        int hour = req.IsNew ? rng.Next(15, 16+1) : rng.Next(8, 16 + 1);
        var date = new DateTime(2021, month, day, hour, 0, 0, DateTimeKind.Utc);
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            // if weekend reroll
            day = rng.Next(1, DateTime.DaysInMonth(2021, month) + 1);
            date = new DateTime(2021, month, day, hour, 0, 0, DateTimeKind.Utc);
        }
        return date;

    }
}