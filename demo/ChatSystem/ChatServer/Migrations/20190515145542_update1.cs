using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatServer.Migrations
{
    public partial class update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(nullable: true),
                    NickName = table.Column<string>(nullable: true),
                    PassWord = table.Column<string>(nullable: true),
                    OnLineStatus = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    MessageId = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<long>(nullable: false),
                    MsgType = table.Column<byte>(nullable: false),
                    FromUserId = table.Column<long>(nullable: false),
                    TargetUserId = table.Column<long>(nullable: false),
                    MessageContext = table.Column<string>(nullable: false),
                    IsSend = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Message_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Message_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Message_FromUserId",
                table: "Message",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_TargetUserId",
                table: "Message",
                column: "TargetUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
