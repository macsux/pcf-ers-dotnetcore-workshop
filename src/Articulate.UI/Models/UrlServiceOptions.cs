using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace Articulate.Models
{
    public class UrlServiceOptions : AbstractServiceOptions
    {
        public UrlCredentials Credentials { get; set; }
    }
}