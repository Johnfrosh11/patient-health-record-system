using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientHealthRecord.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserNamesForExistingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing users with default first and last names based on their usernames
            migrationBuilder.Sql(@"
                UPDATE Users 
                SET FirstName = CASE 
                    WHEN Username = 'admin' THEN 'Admin'
                    WHEN Username = 'doctor' THEN 'Doctor'
                    WHEN Username = 'nurse' THEN 'Nurse'
                    WHEN Username = 'receptionist' THEN 'Receptionist'
                    ELSE 'User'
                END,
                LastName = CASE 
                    WHEN Username = 'admin' THEN 'User'
                    WHEN Username = 'doctor' THEN 'Smith'
                    WHEN Username = 'nurse' THEN 'Jones'
                    WHEN Username = 'receptionist' THEN 'Brown'
                    ELSE 'Name'
                END
                WHERE FirstName = '' OR LastName = ''
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
