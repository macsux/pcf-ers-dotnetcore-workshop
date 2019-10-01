using System.Data;
using System.Linq;
using Articulate.Models;
using Articulate.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Articulate.Repositories
{
    public class AttendeeContext : DbContext
    {
        private static IDbConnection _persistentConn;
        protected AttendeeContext()
        {
        }

        public AttendeeContext(DbContextOptions options) : base(options)
        {
            // if we're using Sqlite memory mode, hold connection open for duration of app or schema will get recycled
            var sqlite = options.Extensions.OfType<Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal.SqliteOptionsExtension>().FirstOrDefault();
            if (sqlite != null)
            {
                _persistentConn = this.Database.GetDbConnection();                
                _persistentConn.Open();
            }
        }

        public DbSet<Attendee> Attendee { get; set; }
    }
}