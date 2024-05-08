using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskPilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "IsActive", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "054a41f6-e269-47dc-80f5-972dd09d1ccb", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4409), true, "Default User", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4425) },
                    { "39cce4c1-a7f1-4596-bdae-872c61999600", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4430), true, "Administrator", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4430) },
                    { "41159927-ddc3-4888-bed0-b38211bcf960", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4433), true, "Developer", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4434) },
                    { "8c34d7c7-218a-4799-9282-a5afa3f0e62e", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4436), true, "Product Owner", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4437) },
                    { "caaa6c4e-b3b3-4aaa-a966-baf6df4270c2", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4439), true, "Scrum Master", null, new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4440) }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1832bb8d-0db6-43fe-8939-0bee5016c772"), "Role" },
                    { new Guid("2e259d26-29d7-418c-815e-ce8c4f04ab87"), "Priority" },
                    { new Guid("41db43bc-df25-4570-82b0-0c757df35b4a"), "Dashboard" },
                    { new Guid("7325d9b4-e4e9-40c5-8644-be3941d2326f"), "Profile" },
                    { new Guid("7eabf33a-6427-4f7f-95ab-3d36e81383be"), "Task" },
                    { new Guid("a92e344f-4df7-4633-9ec6-ee2c1f3d62a2"), "User" },
                    { new Guid("dd0cffb5-0fd7-4ac2-8289-7ff36efdb77e"), "Status" }
                });

            migrationBuilder.InsertData(
                table: "Priorities",
                columns: new[] { "Id", "CreatedAt", "Description", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("225e896e-4117-4bba-8681-00dee0b264e4"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4672), "High", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4674) },
                    { new Guid("969885aa-ec78-4025-a72c-4a20073ec9e2"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4676), "Low", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4677) },
                    { new Guid("c0f4cf3a-8999-42c4-9c97-c3d2bae1966f"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4680), "Medium", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4681) },
                    { new Guid("cc23b7c3-77c7-49cd-83a5-8160f5785494"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4678), "Lowest", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4679) }
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "Id", "CreatedAt", "Description", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("101fe5b2-cf5a-4c62-b258-8fbb56c8f5b6"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4602), "In-Progress", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4603) },
                    { new Guid("63ad4f22-6e3d-4315-bff1-e14015c847e9"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4606), "Closed", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4607) },
                    { new Guid("6f10b490-ed56-492f-92d8-882f9a1e14bc"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4600), "New", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4601) },
                    { new Guid("adfcb548-9784-4c13-9a6a-7211d1e29f91"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4597), "Removed", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4598) },
                    { new Guid("bd67686d-f547-4d0d-93a1-97b008d1e5eb"), new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4604), "Resolved", new DateTime(2024, 5, 8, 11, 43, 50, 404, DateTimeKind.Local).AddTicks(4605) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "054a41f6-e269-47dc-80f5-972dd09d1ccb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "39cce4c1-a7f1-4596-bdae-872c61999600");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "41159927-ddc3-4888-bed0-b38211bcf960");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8c34d7c7-218a-4799-9282-a5afa3f0e62e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "caaa6c4e-b3b3-4aaa-a966-baf6df4270c2");

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("1832bb8d-0db6-43fe-8939-0bee5016c772"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("2e259d26-29d7-418c-815e-ce8c4f04ab87"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("41db43bc-df25-4570-82b0-0c757df35b4a"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("7325d9b4-e4e9-40c5-8644-be3941d2326f"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("7eabf33a-6427-4f7f-95ab-3d36e81383be"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("a92e344f-4df7-4633-9ec6-ee2c1f3d62a2"));

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: new Guid("dd0cffb5-0fd7-4ac2-8289-7ff36efdb77e"));

            migrationBuilder.DeleteData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: new Guid("225e896e-4117-4bba-8681-00dee0b264e4"));

            migrationBuilder.DeleteData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: new Guid("969885aa-ec78-4025-a72c-4a20073ec9e2"));

            migrationBuilder.DeleteData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: new Guid("c0f4cf3a-8999-42c4-9c97-c3d2bae1966f"));

            migrationBuilder.DeleteData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: new Guid("cc23b7c3-77c7-49cd-83a5-8160f5785494"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("101fe5b2-cf5a-4c62-b258-8fbb56c8f5b6"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("63ad4f22-6e3d-4315-bff1-e14015c847e9"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("6f10b490-ed56-492f-92d8-882f9a1e14bc"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("adfcb548-9784-4c13-9a6a-7211d1e29f91"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("bd67686d-f547-4d0d-93a1-97b008d1e5eb"));
        }
    }
}
