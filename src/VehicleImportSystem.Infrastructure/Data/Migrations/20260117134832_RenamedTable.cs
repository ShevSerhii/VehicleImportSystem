using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleImportSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalculationRecords_CarBrands_BrandId",
                table: "CalculationRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CalculationRecords_CarModels_ModelId",
                table: "CalculationRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalculationRecords",
                table: "CalculationRecords");

            migrationBuilder.RenameTable(
                name: "CalculationRecords",
                newName: "CustomsCalculation");

            migrationBuilder.RenameIndex(
                name: "IX_CalculationRecords_UserDeviceId_CreatedAt",
                table: "CustomsCalculation",
                newName: "IX_CustomsCalculation_UserDeviceId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CalculationRecords_ModelId",
                table: "CustomsCalculation",
                newName: "IX_CustomsCalculation_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CalculationRecords_BrandId",
                table: "CustomsCalculation",
                newName: "IX_CustomsCalculation_BrandId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomsCalculation",
                table: "CustomsCalculation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomsCalculation_CarBrands_BrandId",
                table: "CustomsCalculation",
                column: "BrandId",
                principalTable: "CarBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomsCalculation_CarModels_ModelId",
                table: "CustomsCalculation",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomsCalculation_CarBrands_BrandId",
                table: "CustomsCalculation");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomsCalculation_CarModels_ModelId",
                table: "CustomsCalculation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomsCalculation",
                table: "CustomsCalculation");

            migrationBuilder.RenameTable(
                name: "CustomsCalculation",
                newName: "CalculationRecords");

            migrationBuilder.RenameIndex(
                name: "IX_CustomsCalculation_UserDeviceId_CreatedAt",
                table: "CalculationRecords",
                newName: "IX_CalculationRecords_UserDeviceId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CustomsCalculation_ModelId",
                table: "CalculationRecords",
                newName: "IX_CalculationRecords_ModelId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomsCalculation_BrandId",
                table: "CalculationRecords",
                newName: "IX_CalculationRecords_BrandId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalculationRecords",
                table: "CalculationRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CalculationRecords_CarBrands_BrandId",
                table: "CalculationRecords",
                column: "BrandId",
                principalTable: "CarBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CalculationRecords_CarModels_ModelId",
                table: "CalculationRecords",
                column: "ModelId",
                principalTable: "CarModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
