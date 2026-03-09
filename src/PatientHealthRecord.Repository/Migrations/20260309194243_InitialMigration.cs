using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientHealthRecord.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_audit_log",
                columns: table => new
                {
                    audit_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_audit_log", x => x.audit_log_id);
                },
                comment: "System audit logs");

            migrationBuilder.CreateTable(
                name: "t_permission",
                columns: table => new
                {
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_permission", x => x.permission_id);
                },
                comment: "System permissions");

            migrationBuilder.CreateTable(
                name: "t_role",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_role", x => x.role_id);
                },
                comment: "System roles");

            migrationBuilder.CreateTable(
                name: "t_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user", x => x.user_id);
                },
                comment: "System users");

            migrationBuilder.CreateTable(
                name: "t_role_permission",
                columns: table => new
                {
                    role_permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_role_permission", x => x.role_permission_id);
                    table.ForeignKey(
                        name: "FK_t_role_permission_t_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "t_permission",
                        principalColumn: "permission_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_role_permission_t_role_role_id",
                        column: x => x.role_id,
                        principalTable: "t_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Role-Permission junction table");

            migrationBuilder.CreateTable(
                name: "t_health_record",
                columns: table => new
                {
                    health_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    diagnosis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    treatment_plan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    medical_history = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_health_record", x => x.health_record_id);
                    table.ForeignKey(
                        name: "FK_t_health_record_t_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Patient health records");

            migrationBuilder.CreateTable(
                name: "t_refresh_token",
                columns: table => new
                {
                    refresh_token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    revoked_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_refresh_token", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "FK_t_refresh_token_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "JWT refresh tokens");

            migrationBuilder.CreateTable(
                name: "t_user_role",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user_role", x => x.user_role_id);
                    table.ForeignKey(
                        name: "FK_t_user_role_t_role_role_id",
                        column: x => x.role_id,
                        principalTable: "t_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_user_role_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User-Role junction table");

            migrationBuilder.CreateTable(
                name: "t_access_request",
                columns: table => new
                {
                    access_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    health_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requesting_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    request_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    access_start_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    access_end_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_access_request", x => x.access_request_id);
                    table.ForeignKey(
                        name: "FK_t_access_request_t_health_record_health_record_id",
                        column: x => x.health_record_id,
                        principalTable: "t_health_record",
                        principalColumn: "health_record_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_access_request_t_user_requesting_user_id",
                        column: x => x.requesting_user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_access_request_t_user_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Time-bound access requests");

            migrationBuilder.CreateIndex(
                name: "ix_access_request_date",
                table: "t_access_request",
                column: "request_date");

            migrationBuilder.CreateIndex(
                name: "ix_access_request_org_active",
                table: "t_access_request",
                columns: new[] { "organization_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_access_request_record_status",
                table: "t_access_request",
                columns: new[] { "health_record_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_access_request_user_status",
                table: "t_access_request",
                columns: new[] { "requesting_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_t_access_request_reviewed_by",
                table: "t_access_request",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity",
                table: "t_audit_log",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_org_timestamp",
                table: "t_audit_log",
                columns: new[] { "organization_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_action",
                table: "t_audit_log",
                columns: new[] { "user_id", "action" });

            migrationBuilder.CreateIndex(
                name: "ix_health_record_created_by",
                table: "t_health_record",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_health_record_created_date",
                table: "t_health_record",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_health_record_org_active",
                table: "t_health_record",
                columns: new[] { "organization_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_health_record_patient_name",
                table: "t_health_record",
                column: "patient_name");

            migrationBuilder.CreateIndex(
                name: "ix_permission_name_unique",
                table: "t_permission",
                column: "permission_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_token_unique",
                table: "t_refresh_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_valid",
                table: "t_refresh_token",
                columns: new[] { "user_id", "is_revoked", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_role_name_unique",
                table: "t_role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_unique",
                table: "t_role_permission",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_role_permission_permission_id",
                table: "t_role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_created_date",
                table: "t_user",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_user_email_unique",
                table: "t_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_org_active",
                table: "t_user",
                columns: new[] { "organization_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_username_unique",
                table: "t_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_user_role_role_id",
                table: "t_user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_unique",
                table: "t_user_role",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_access_request");

            migrationBuilder.DropTable(
                name: "t_audit_log");

            migrationBuilder.DropTable(
                name: "t_refresh_token");

            migrationBuilder.DropTable(
                name: "t_role_permission");

            migrationBuilder.DropTable(
                name: "t_user_role");

            migrationBuilder.DropTable(
                name: "t_health_record");

            migrationBuilder.DropTable(
                name: "t_permission");

            migrationBuilder.DropTable(
                name: "t_role");

            migrationBuilder.DropTable(
                name: "t_user");
        }
    }
}
