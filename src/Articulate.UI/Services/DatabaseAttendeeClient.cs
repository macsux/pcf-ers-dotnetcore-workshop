using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Articulate.Models;
using Articulate.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Articulate.Services
{
    public class DatabaseAttendeeClient : IAttendeeClient
    {
        private readonly AttendeeContext _context;

        public DatabaseAttendeeClient(AttendeeContext context)
        {
            _context = context;
        }

        public async Task<List<Attendee>> GetAll() => await _context.Attendee.ToListAsync();

        public async Task Add(string firstName, string lastName, string emailAddress)
        {
            var attendee = new Attendee()
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = emailAddress
            };
            _context.Attendee.Add(attendee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAll()
        {
            var attendees = await _context.Attendee.ToListAsync();
            _context.Attendee.RemoveRange(attendees);
            await _context.SaveChangesAsync();
        }

        public bool IsMigrated
        {
            get 
            {
                try
                {
                    return ((IInfrastructure<IServiceProvider>) _context.Database).Instance.GetService<IHistoryRepository>().Exists();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool CanConnect
        {
            get
            {
                try
                {
                    _context.Database.OpenConnection();
                    _context.Database.CloseConnection();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public string Endpoint => _context.Database.GetDbConnection().ConnectionString;
    }
}