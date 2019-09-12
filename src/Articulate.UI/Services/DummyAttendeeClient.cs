using System.Collections.Generic;
using System.Threading.Tasks;
using Articulate.Models;
#pragma warning disable 1998

namespace Articulate.Services
{
    public class DummyAttendeeClient : IAttendeeClient
    {
        public async Task<List<Attendee>> GetAll()
        {
            return new List<Attendee>();
        }

        public async Task<List<Attendee>> Add(string firstName, string lastName, string emailAddress)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<Attendee>> DeleteAll()
        {
            throw new System.NotImplementedException();
        }
    }
}