using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppointmentScheduler.Models
{
    internal class AppointmentRequest
    {
        [JsonPropertyName("requestId")]
        public int RequestId { get; set; }

        [JsonPropertyName("personId")]
        public int PersonId { get; set; }

        [JsonPropertyName("preferredDays")]
        public List<DateTime>? PreferredDays { get; set; }

        [JsonPropertyName("preferredDocs")]
        public List<int>? PreferredDocs { get; set; }

        [JsonPropertyName("isNew")]
        public bool IsNew { get; set; }
    }
}
