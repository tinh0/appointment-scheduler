using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace AppointmentScheduler.Models
{
    internal class AppointmentInfoRequest
    {
        [JsonPropertyName("doctorId")]
        public int DoctorId { get; set; }

        [JsonPropertyName("personId")]
        public int PersonId { get; set; }

        [JsonPropertyName("appointmentTime")]
        public DateTime AppointmentTime { get; set; }

        [JsonPropertyName("isNewPatientAppointment")]
        public bool IsNewPatientAppointment { get; set; }

        [JsonPropertyName("requestId")]
        public int RequestId { get; set; }

    }
}
