using AppointmentScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppointmentScheduler.Models
{
    internal class AppointmentInfo
    {
        [JsonPropertyName("doctorId")]
        public int DoctorId { get; set; }

        [JsonPropertyName("personId")]
        public int PersonId { get; set; }

        [JsonPropertyName("appointmentTime")]
        public DateTime AppointmentTime { get; set; }

        [JsonPropertyName("isNewPatientAppointment")]
        public bool IsNewPatientAppointment { get; set; }
    }
}
