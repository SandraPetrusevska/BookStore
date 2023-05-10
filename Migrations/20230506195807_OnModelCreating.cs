using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    public partial class OnModelCreating : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBooks_Book_Bookid",
                table: "UserBooks");

            migrationBuilder.RenameColumn(
                name: "Bookid",
                table: "UserBooks",
                newName: "BookId");

            migrationBuilder.RenameIndex(
                name: "IX_UserBooks_Bookid",
                table: "UserBooks",
                newName: "IX_UserBooks_BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBooks_Book_BookId",
                table: "UserBooks",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBooks_Book_BookId",
                table: "UserBooks");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "UserBooks",
                newName: "Bookid");

            migrationBuilder.RenameIndex(
                name: "IX_UserBooks_BookId",
                table: "UserBooks",
                newName: "IX_UserBooks_Bookid");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBooks_Book_Bookid",
                table: "UserBooks",
                column: "Bookid",
                principalTable: "Book",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
