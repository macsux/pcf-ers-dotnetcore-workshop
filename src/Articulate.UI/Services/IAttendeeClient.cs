using System.Collections.Generic;
using System.Threading.Tasks;
using Articulate.Models;

namespace Articulate.Services
{
    public interface IAttendeeClient
    {
        Task<List<Attendee>> GetAll();
        Task Add(string firstName, string lastName, string emailAddress);
        Task DeleteAll();

        bool IsMigrated { get; }
        string Endpoint { get; }
        bool CanConnect { get; }
    }
}