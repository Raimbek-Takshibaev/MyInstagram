using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyInstagram.Migrations
{
    public partial class AddedSubcribesAndSubcribersToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "SubscribersIds",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "SubscribesIds",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscribersIds",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscribesIds",
                table: "AspNetUsers");
        }
    }
}
