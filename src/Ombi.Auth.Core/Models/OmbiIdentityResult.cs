using System.Collections.Generic;

namespace Ombi.Auth.Core.Models
{
    public class OmbiIdentityResult
    {
        public List<string> Errors { get; set; }
        public bool Successful { get; set; }
    }
}