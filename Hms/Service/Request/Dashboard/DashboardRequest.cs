namespace Hms.Service.Request.Dashboard
{
    public class DashboardRequest
    {
        public int? Id { get; set; }
        public string? IPAddress { get; set; }
        public int Departments { get; set; }
        public int PatientsServed { get; set; }
        public decimal PatientSatisfaction { get; set; }
        public int YearsExperience { get; set; }

        // Section 2: Hospital Statistics
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int AvailableBeds { get; set; }
        public int ICUSeats { get; set; }
        public int WardSeats { get; set; }
        public int NursesOnDuty { get; set; }
        public int MedicineItems { get; set; }
        public int Ambulances { get; set; }

        // Section 3: Chart Data - Department Patient Distribution
        public int CardiologyPatients { get; set; }
        public int NeurologyPatients { get; set; }
        public int OrthopedicsPatients { get; set; }
        public int PediatricsPatients { get; set; }
        public int GeneralPatients { get; set; }

        // Bed Occupancy Status
        public int OccupiedBeds { get; set; }
        public int AvailableBedsChart { get; set; }
        public int MaintenanceBeds { get; set; }

        // Monthly Patient Admissions
        public int JanAdmissions { get; set; }
        public int FebAdmissions { get; set; }
        public int MarAdmissions { get; set; }
        public int AprAdmissions { get; set; }
        public int MayAdmissions { get; set; }
        public int JunAdmissions { get; set; }

        // Medicine Stock Distribution
        public int Antibiotics { get; set; }
        public int PainRelief { get; set; }
        public int Vitamins { get; set; }
        public int EmergencyDrugs { get; set; }
        public int OtherMedicines { get; set; }
    }
}
