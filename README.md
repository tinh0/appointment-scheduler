# Appointment Scheduler
Appointment scheduler that calls scheduler API and schedules appointments from given requests

# Setup
Restore packages\
dotnet restore

To set api key:\
dotnet user-secrets init\
dotnet user-secrets set "Api:Token" "YOUR_TOKEN_HERE"

# Approach
Post /api/Scheduling/Start to start scheduling\
Get /api/Scheduling/Schedule to fill out existing schedule, save to\
dictionary<(doctor, patient), appointmentTimes> to keep track of conflicts\
Get /api/Scheduling/AppointmentRequest until there are no more appointment requests\
Create new AppointmentRequestInfo from each AppointmentRequest\
(not finished) find next valid time and doctor using constraints and use them to schedule appointment\
Post each AppointmentRequestInfo to schedule the appointment
