using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Articulate.Models;
#pragma warning disable 1998

namespace Articulate.Services
{
    public partial class ApiAttendeeClient : IAttendeeClient
    {
        public async Task<List<Attendee>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task Add(string firstName, string lastName, string emailAddress)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAll()
        {
            throw new NotImplementedException();
        }

        public bool IsMigrated => true;
        public string Endpoint => null; //_httpClient.BaseAddress.ToString();
        public bool CanConnect => true;
    }
}