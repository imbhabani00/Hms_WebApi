using Dapper;
using Hms.Domains;
using Hms.Domains.Dashboard;
using Hms.Service.Request.Dashboard;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Hms.Repository
{
    #region Interface
    public interface IDashboardRepository
    {
        Task<GenericResult> AddDashboardDetails(Dashboard dashboard, int userId, int tenantId);
    }
    #endregion

    public class DashboardRepository : BaseRepository , IDashboardRepository
    {
        #region Properties
        private readonly IConfiguration configuration;
        #endregion

        #region Constructor
        public DashboardRepository(IConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }
        #endregion

        #region  AddDashboardDetails
        public async Task<GenericResult> AddDashboardDetails(Dashboard dashboard, int userId, int tenantId)
        {
            var result = new GenericResult();
            using (var dbConnection = CreateConnection())
            {
                var dynamicParameter = new DynamicParameters();

                dynamicParameter.Add("@Id", dashboard.Id);
                dynamicParameter.Add("@LoggedInUserId", userId);
                dynamicParameter.Add("@TenantId", tenantId);
                dynamicParameter.Add("@IPAddress", dashboard.IPAddress);

                dynamicParameter.Add("@Departments", dashboard.Departments);
                dynamicParameter.Add("@PatientsServed", dashboard.PatientsServed);
                dynamicParameter.Add("@PatientSatisfaction", dashboard.PatientSatisfaction);
                dynamicParameter.Add("@YearsExperience", dashboard.YearsExperience);

                dynamicParameter.Add("@TotalDoctors", dashboard.TotalDoctors);
                dynamicParameter.Add("@TotalPatients", dashboard.TotalPatients);
                dynamicParameter.Add("@AvailableBeds", dashboard.AvailableBeds);
                dynamicParameter.Add("@ICUSeats", dashboard.ICUSeats);
                dynamicParameter.Add("@WardSeats", dashboard.WardSeats);
                dynamicParameter.Add("@NursesOnDuty", dashboard.NursesOnDuty);
                dynamicParameter.Add("@MedicineItems", dashboard.MedicineItems);
                dynamicParameter.Add("@Ambulances", dashboard.Ambulances);

                dynamicParameter.Add("@CardiologyPatients", dashboard.CardiologyPatients);
                dynamicParameter.Add("@NeurologyPatients", dashboard.NeurologyPatients);
                dynamicParameter.Add("@OrthopedicsPatients", dashboard.OrthopedicsPatients);
                dynamicParameter.Add("@PediatricsPatients", dashboard.PediatricsPatients);
                dynamicParameter.Add("@GeneralPatients", dashboard.GeneralPatients);

                dynamicParameter.Add("@OccupiedBeds", dashboard.OccupiedBeds);
                dynamicParameter.Add("@AvailableBedsChart", dashboard.AvailableBedsChart);
                dynamicParameter.Add("@MaintenanceBeds", dashboard.MaintenanceBeds);

                dynamicParameter.Add("@JanAdmissions", dashboard.JanAdmissions);
                dynamicParameter.Add("@FebAdmissions", dashboard.FebAdmissions);
                dynamicParameter.Add("@MarAdmissions", dashboard.MarAdmissions);
                dynamicParameter.Add("@AprAdmissions", dashboard.AprAdmissions);
                dynamicParameter.Add("@MayAdmissions", dashboard.MayAdmissions);
                dynamicParameter.Add("@JunAdmissions", dashboard.JunAdmissions);

                dynamicParameter.Add("@Antibiotics", dashboard.Antibiotics);
                dynamicParameter.Add("@PainRelief", dashboard.PainRelief);
                dynamicParameter.Add("@Vitamins", dashboard.Vitamins);
                dynamicParameter.Add("@EmergencyDrugs", dashboard.EmergencyDrugs);
                dynamicParameter.Add("@OtherMedicines", dashboard.OtherMedicines);

                dynamicParameter.Add("@NewId", DbType.Int32, direction: ParameterDirection.Output);
                dynamicParameter.Add("@ReturnValue", DbType.Int32, direction: ParameterDirection.ReturnValue);

                dbConnection.Open();
                await dbConnection.ExecuteAsync(
                    "[dbo].[Dashboard_SaveAllData]",
                    dynamicParameter,
                    commandType: CommandType.StoredProcedure);

                result.NewId = dynamicParameter.Get<int?>("@NewId");
                result.ReturnValue = dynamicParameter.Get<int?>("@ReturnValue");
            }
            return result;
        }
        #endregion
    }
}
