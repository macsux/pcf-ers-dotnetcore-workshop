using System.Collections.Generic;
using System.Threading.Tasks;
using Articulate.Models;

namespace Articulate.Services
{
    public interface IAttendeeClient
    {
        Task<List<Attendee>> GetAll();
        Task<List<Attendee>> Add(string firstName, string lastName, string emailAddress);
        Task<List<Attendee>> DeleteAll();
    }
}